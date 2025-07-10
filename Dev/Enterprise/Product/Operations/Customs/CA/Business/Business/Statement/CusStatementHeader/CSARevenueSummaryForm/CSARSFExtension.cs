using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public static class CSARSFExtension
	{
		public static CusStatementLine GetOrCreateNewPaymentLine(this CusStatementHeader rsf, ZString paymentType)
		{
			var line = rsf.StatementLines.Cast<CusStatementLine>().Where(x => x.B3_EntryType == paymentType)?.FirstOrDefault();
			if (line == null)
			{
				using (rsf.SuspendSettingHasChanges())
				{
					line = rsf.StatementLines.AddNew();
					line.B3_EntryType = paymentType;
				}
			}
			return line;
		}

		public static CusStatementLineCharge GetOrCreateNewTransactionLineCharge(this CusStatementLine line, ZString chargeType, bool createIfNotExist = true)
		{
			return line.Charges.Cast<CusStatementLineCharge>().Where(x => x.B4_ChargeType == chargeType)?.FirstOrDefault()
				?? (createIfNotExist ? CreateNewCharge(line, chargeType) : null);
		}

		static CusStatementLineCharge CreateNewCharge(CusStatementLine line, ZString chargeType)
		{
			var result = line.Charges.AddNew();
			result.B4_ChargeType = chargeType;
			return result;
		}

		public static CusStatementLine GetStatementLineDependsOnPaymentType(this CusStatementHeader rsf, ZString payment)
		{
			CusStatementLine result = null;
			switch (payment)
			{
				case CSARSFPaymentTypes.Codes.Debit:
					result = rsf.DebitLine;
					break;
				case CSARSFPaymentTypes.Codes.Credit:
					result = rsf.CreditLine;
					break;
				case CSARSFPaymentTypes.Codes.Interim:
					result = rsf.InterimLine;
					break;
				default:
					break;
			}
			return result;
		}

		public static bool IsCalculatedAutomatically(this CSARSFPayment payment)
		{
			var code = payment.CodeID;
			return payment.Factory.GetCachedValue(code, () =>
			{
				return code == CSARSFDebitCodes.Codes._490101 || code == CSARSFDebitCodes.Codes._490102
						|| code == CSARSFDebitCodes.Codes._491211 || code == CSARSFDebitCodes.Codes._491212
						|| code == CSARSFDebitCodes.Codes._49011 || code == CSARSFDebitCodes.Codes._49475
						|| code == CSARSFCreditCodes.Codes._49017 || code == CSARSFCreditCodes.Codes._49018;
			});
		}

		public static CodeDescriptionPairList GetPaymentCodeList(BusinessObjectFactory factory, ZString paymentType)
		{
			return factory.GetCachedValue(string.Format("CSARSF|{0}", paymentType), () =>
			{
				switch (paymentType)
				{
					case CSARSFPaymentTypes.Codes.Debit:
						return new CSARSFDebitCodes();
					case CSARSFPaymentTypes.Codes.Credit:
						return new CSARSFCreditCodes();
					case CSARSFPaymentTypes.Codes.Interim:
						return new CSARSFInterimPaymentCodes();
					default:
						return new CodeDescriptionPairList();
				}
			});
		}

		public static ZDBOnlyQuery GetCSARSFDeclarationQuery(this CusStatementHeader rsf)
		{
			var option = OrgImpAddInfo.Get(rsf.Importer).ZO_AccountingTimeOption;
			var startDate = rsf.B2_PeriodStartDate;
			var endDate = rsf.B2_PeriodEndDate;

			var companyPK = rsf.Company.PK;
			var subQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
			subQuery.AddToFilter(GlbBranchSchema.GB_GC, companyPK);

			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddSubQuery(JobDeclarationSchema.JE_GB, subQuery, JoinCondition.And);
			query.AddToFilter(JobDeclarationSchema.JE_OH_Importer, rsf.B2_OH_Importer);

			var b3xQuery = new ZDBOnlyQuery(typeof(JobDeclaration));
			b3xQuery.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.XTypeEntry);

			var b3xSubQuery = new ZDBOnlySubQuery(typeof(GenAddOnColumn), GenAddOnColumnSchema.XA_ParentID);
			b3xSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Name, CAAddInfoSchema.CA_B2AcceptedDate.Name);
			b3xSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.GreaterThanOrEqualTo, startDate.ToZDateTime().SqlFormat);
			b3xSubQuery.AddToFilter(GenAddOnColumnSchema.XA_Data, SQLComparisonOperator.LessThanOrEqualTo, endDate.ToZDateTime().SqlFormat);
			b3xQuery.AddSubQuery(JobDeclarationSchema.PK, b3xSubQuery, JoinCondition.And);

			var periodStart1 = new ZDateTime(rsf.PeriodYear, rsf.PeriodMonth, 19, 00, 00, 00);
			var periodEnd1 = new ZDateTime(rsf.PeriodYear, rsf.PeriodMonth, 18, 23, 59, 59).AddMonths(1);
			var periodStart2 = new ZDateTime(rsf.PeriodYear, rsf.PeriodMonth, 1, 00, 00, 00);
			var periodEnd2 = new ZDateTime(rsf.PeriodYear, rsf.PeriodMonth, 1, 23, 59, 59).AddMonths(1).AddDays(-1);
			var businessDay1 = periodEnd2;
			var businessDay2 = new ZDateTime(rsf.PeriodYear, rsf.PeriodMonth, 18, 23, 59, 59);

			var query1 = GetDeclarationQuery(businessDay1, periodStart1, periodEnd1);
			var query2 = GetDeclarationQuery(businessDay2, periodStart2, periodEnd2);
			var subQuery1 = new ZDBOnlyQuery(typeof(JobDeclaration));
			subQuery1.AddToFilter(b3xQuery);

			if (option == CSARSFAccountingOptionList.Codes.Option1)
			{
				subQuery1.AddToFilter(query1, JoinCondition.Or);
			}
			else if (option == CSARSFAccountingOptionList.Codes.Option2)
			{
				subQuery1.AddToFilter(query2, JoinCondition.Or);
			}
			else
			{
				subQuery1.AddToFilter(query1, JoinCondition.Or);
				subQuery1.AddToFilter(query2, JoinCondition.Or);
			}

			query.AddToFilter(subQuery1);
			return query;
		}

		static ZDBOnlyQuery GetDeclarationQuery(ZDateTime businessDay, ZDateTime periodStart, ZDateTime periodEnd)
		{
			var query = new ZDBOnlyQuery(typeof(JobDeclaration));
			query.AddToFilter(JobDeclarationSchema.JE_MessageType, JobMessageTypeList.Codes.Import);
			query.AddToFilter(JobDeclarationSchema.JE_EntryAuthorisationDate, SQLComparisonOperator.LessThanOrEqualTo, businessDay);
			var subQuery = new ZDBOnlySubQuery(typeof(CusEntryHeader), CusEntryHeaderSchema.CH_JE);
			subQuery.AddToFilter(CusEntryHeaderSchema.CH_MessageType, new[] { MessageTypeList.Codes.B3CUSDEC, MessageTypeList.Codes.CommercialAccountingDeclaration });
			subQuery.AddToFilter(CusEntryHeaderSchema.CH_EntryReleaseDate, SQLComparisonOperator.GreaterThanOrEqualTo, periodStart);
			subQuery.AddToFilter(CusEntryHeaderSchema.CH_EntryReleaseDate, SQLComparisonOperator.LessThanOrEqualTo, periodEnd);
			query.AddSubQuery(JobDeclarationSchema.PK, CusEntryHeaderSchema.CH_JE, subQuery, JoinCondition.And);

			return query;
		}
	}
}
