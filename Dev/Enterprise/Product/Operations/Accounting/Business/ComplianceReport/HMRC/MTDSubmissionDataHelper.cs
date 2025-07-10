using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using TaxTypes = Enterprise.MasterFiles.Business.AccTaxRate.Types;

namespace Enterprise.Accounting.Business.ComplianceReport.HMRC
{
	public class MTDSubmissionDataHelper
	{
		public MTDSubmissionDataHelper(AccComplianceReport report)
		{
			Report = report;
			euOrgCodes = null;
			TooOldWarning = false;
		}

		public AccComplianceReport Report { get; }

		public bool HaveAllGroupMembersSubmittedReturn()
		{
			(ZInt totalMemberCompanies, ZInt totalMemberCompaniesSubmittedReturn) = GetGroupMemberCompanyInfo();
			return totalMemberCompanies == totalMemberCompaniesSubmittedReturn;
		}

		public void SetGroupMemberTotalValues(MTDSubmissionData submissionData)
		{
			Dictionary<ZString, ZDecimal> groupMemberTotalValues = GetGroupMemberTotalValues();
			if (groupMemberTotalValues.Count > 0)
			{
				SetValue(MTDSubmissionDataColumns.VatDueSales, value => submissionData.Box1_VATDue = value);
				SetValue(MTDSubmissionDataColumns.VatDueAcquisitions, value => submissionData.Box2_VATDueReverseChg = value);
				SetValue(MTDSubmissionDataColumns.VatReclaimedCurrPeriod, value => submissionData.Box4_VATReclaimed = value);
				SetValue(MTDSubmissionDataColumns.TotalValueSalesExVAT, value => submissionData.Box6_TotalSalesExVAT = value);
				SetValue(MTDSubmissionDataColumns.TotalValuePurchasesExVAT, value => submissionData.Box7_TotalPurchaseExVAT = value);
				SetValue(MTDSubmissionDataColumns.TotalValueGoodsSuppliedExVAT, value => submissionData.Box8_GoodsSalesECMembersExVAT = value);
				SetValue(MTDSubmissionDataColumns.TotalAcquisitionsExVAT, value => submissionData.Box9_GoodsPurchaseECMembersExVAT = value);
			}

			void SetValue(string key, Action<ZDecimal> setValue)
			{
				ZDecimal value = ZDecimal.Zero;
				groupMemberTotalValues.TryGetValue(key, out value);
				setValue(value);
			}
		}

		public void SetValues(MTDSubmissionData subData, MTDSubmissionData previousSubData = null)
		{
			if (previousSubData == null)
			{
				previousSubData = new MTDSubmissionData(Report.Factory);
			}

			ZDecimal[] boxes = new ZDecimal[7] { ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero };
			ZDecimal[] oldBoxes = new ZDecimal[7] { ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero, ZDecimal.Zero };

			// 0 = box1
			// 1 = box2
			// 2 = box4
			// 3 = box6
			// 4 = box7
			// 5 = box8
			// 6 = box9

			var earlistAcceptedDate = Report.ACR_DateFrom.AddYears(-4);

			foreach (AccComplianceReportLine line in Report.ReportLines.Cast<AccComplianceReportLine>())
			{
				if (line.PostDate.Date < earlistAcceptedDate)
				{
					TooOldWarning = true;
					continue;
				}

				bool isPreviousPeriod = line.PostDate.Date < Report.ACR_DateFrom;

				if (line.AH_Ledger == LedgerTypes.AccountsReceivable)
				{
					ARLine(line, ref boxes, ref oldBoxes, isPreviousPeriod);
				}
				else if (line.AH_Ledger == LedgerTypes.CashBook)
				{
					CBLine(line, ref boxes, ref oldBoxes, isPreviousPeriod);
				}
				else if (line.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					APLine(line, ref boxes, ref oldBoxes, isPreviousPeriod);
				}
			}

			boxes[3].Truncate(0);
			boxes[4].Truncate(0);
			boxes[5].Truncate(0);
			boxes[6].Truncate(0);

			oldBoxes[3].Truncate(0);
			oldBoxes[4].Truncate(0);
			oldBoxes[5].Truncate(0);
			oldBoxes[6].Truncate(0);

			subData.Box1_VATDue = boxes[0];
			subData.Box2_VATDueReverseChg = boxes[1];
			subData.Box4_VATReclaimed = boxes[2];
			subData.Box6_TotalSalesExVAT = boxes[3];
			subData.Box7_TotalPurchaseExVAT = boxes[4];
			subData.Box8_GoodsSalesECMembersExVAT = boxes[5];
			subData.Box9_GoodsPurchaseECMembersExVAT = boxes[6];

			previousSubData.Box1_VATDue = oldBoxes[0];
			previousSubData.Box2_VATDueReverseChg = oldBoxes[1];
			previousSubData.Box4_VATReclaimed = oldBoxes[2];
			previousSubData.Box6_TotalSalesExVAT = oldBoxes[3];
			previousSubData.Box7_TotalPurchaseExVAT = oldBoxes[4];
			previousSubData.Box8_GoodsSalesECMembersExVAT = oldBoxes[5];
			previousSubData.Box9_GoodsPurchaseECMembersExVAT = oldBoxes[6];

			subData.IsReadOnly = true;
			previousSubData.IsReadOnly = true;
		}

		void ARLine(AccComplianceReportLine line, ref ZDecimal[] boxes, ref ZDecimal[] oldBoxes, bool isPreviousPeriod)
		{
			if (line.AT_Type == TaxTypes.Rated
				|| line.AT_Type == TaxTypes.CapitalRated)
			{
				if (!isPreviousPeriod) { boxes[0] += line.TotalTaxAmount; }
				else { oldBoxes[0] += line.TotalTaxAmount; }

				if (!isPreviousPeriod) { boxes[3] += line.TotalExTaxAmount; }
				else { oldBoxes[3] += line.TotalExTaxAmount; }
			}
			else if (line.AT_Type == TaxTypes.Exempt
				|| line.AT_Type == TaxTypes.ReverseRated
				|| line.AT_Type == "")
			{
				if (!isPreviousPeriod) { boxes[3] += line.TotalExTaxAmount; }
				else { oldBoxes[3] += line.TotalExTaxAmount; }
			}
			if (EUOrgCodes.Contains(line.OH_Code) && line.IsGoods)
			{
				if (!isPreviousPeriod) { boxes[5] += line.GoodsExTaxAmount; }
				else { oldBoxes[5] += line.GoodsExTaxAmount; }
			}
		}

		void CBLine(AccComplianceReportLine line, ref ZDecimal[] boxes, ref ZDecimal[] oldBoxes, bool isPreviousPeriod)
		{
			if (line.AH_TransactionType == TransactionTypes.DirectReceipt)
			{
				if (line.AT_Type == TaxTypes.Rated
					|| line.AT_Type == TaxTypes.CapitalRated)
				{
					if (!isPreviousPeriod) { boxes[0] += line.TotalTaxAmount; }
					else { oldBoxes[0] += line.TotalTaxAmount; }

					if (!isPreviousPeriod) { boxes[3] += line.TotalExTaxAmount; }
					else { oldBoxes[3] += line.TotalExTaxAmount; }
				}
				else if (line.AT_Type == TaxTypes.Exempt
					|| line.AT_Type == TaxTypes.ReverseRated
					|| line.AT_Type == "")
				{
					if (!isPreviousPeriod) { boxes[3] += line.TotalExTaxAmount; }
					else { oldBoxes[3] += line.TotalExTaxAmount; }
				}
			}
			else if (line.AH_TransactionType == TransactionTypes.DirectPayment)
			{
				if (line.AT_Type == TaxTypes.ReverseRated)
				{
					if (!isPreviousPeriod) { boxes[1] -= line.TaxReverseChargeOutputAmount; }
					else { oldBoxes[1] -= line.TaxReverseChargeOutputAmount; }

					if (!isPreviousPeriod) { boxes[3] -= line.TotalExTaxAmount; }
					else { oldBoxes[3] -= line.TotalExTaxAmount; }
				}
				if (line.AT_Type != TaxTypes.NotReportable
					&& line.AT_Type != TaxTypes.ExcludedFromTheTaxBase)
				{
					if (!isPreviousPeriod) { boxes[2] -= line.TaxRecoverableAmount; }
					else { oldBoxes[2] -= line.TaxRecoverableAmount; }

					if (!isPreviousPeriod) { boxes[4] -= line.TotalExTaxAmount; }
					else { oldBoxes[4] -= line.TotalExTaxAmount; }
				}
			}
		}

		void APLine(AccComplianceReportLine line, ref ZDecimal[] boxes, ref ZDecimal[] oldBoxes, bool isPreviousPeriod)
		{
			if (line.AT_Type == TaxTypes.ReverseRated)
			{
				if (!isPreviousPeriod) { boxes[1] -= line.TaxReverseChargeOutputAmount; }
				else { oldBoxes[1] -= line.TaxReverseChargeOutputAmount; }

				if (!isPreviousPeriod) { boxes[2] -= line.TaxReverseChargeInputAmount; }
				else { oldBoxes[2] -= line.TaxReverseChargeInputAmount; }

				if (!isPreviousPeriod) { boxes[3] -= line.TotalExTaxAmount; }
				else { oldBoxes[3] -= line.TotalExTaxAmount; }

				if (!isPreviousPeriod) { boxes[4] -= line.TotalExTaxAmount; }
				else { oldBoxes[4] -= line.TotalExTaxAmount; }
			}
			else if (line.AT_Type == TaxTypes.Rated
				|| line.AT_Type == TaxTypes.CapitalRated
				|| line.AT_Type == TaxTypes.Exempt
				|| line.AT_Type == "")
			{
				if (!isPreviousPeriod) { boxes[4] -= line.TotalExTaxAmount; }
				else { oldBoxes[4] -= line.TotalExTaxAmount; }
			}

			if (line.AT_Type != TaxTypes.ReverseRated
				&& line.AT_Type != TaxTypes.NotReportable
				&& line.AT_Type != TaxTypes.ExcludedFromTheTaxBase)
			{
				if (!isPreviousPeriod) { boxes[2] -= line.TaxRecoverableAmount; }
				else { oldBoxes[2] -= line.TaxRecoverableAmount; }
			}
			if (EUOrgCodes.Contains(line.OH_Code) && line.IsGoods)
			{
				if (!isPreviousPeriod) { boxes[6] -= line.GoodsExTaxAmount; }
				else { oldBoxes[6] -= line.GoodsExTaxAmount; }
			}
		}

		public bool TooOldWarning { get; private set; }

		public static string OldTxnMsg => Res.GetString("5fd0d2e6-10b9-4537-9761-b5bd8ae41b2f",
					"You cannot submit VAT errors older than 4 years along with the current VAT return. These values are not included in this VAT return.");

		HashSet<ZString> euOrgCodes;

		HashSet<ZString> EUOrgCodes
		{
			get
			{
				if (euOrgCodes == null)
				{
					euOrgCodes = new HashSet<ZString>();
					foreach (var chuckCodes in AccountingUtils.ChunksOf(Report.ReportLines.Cast<AccComplianceReportLine>().Where(x => !x.OH_Code.IsEmpty).Select(x => x.OH_Code).Distinct(), 500))
					{
						var orgs = Report.Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, chuckCodes));

						if (orgs != null)
						{
							foreach (var org in orgs)
							{
								var economicGrouping = org.Country?.RN_EconomicGrouping ?? ZString.Empty;
								if (economicGrouping == EconomicGroupList.Codes.EuropeanUnion)
								{
									euOrgCodes.Add(org.OH_Code);
								}
							}
						}
					}
				}

				return euOrgCodes;
			}
		}

		Dictionary<ZString, ZDecimal> GetGroupMemberTotalValues()
		{
			var sqlStatement = $@"
			SELECT
				ATC_ColumnName, SUM(ATC_Amount) as ATC_Amount
			FROM dbo.StmData
				INNER JOIN dbo.GlbCompany ON GC_PK = SD_Owner
				INNER JOIN dbo.AccComplianceReport ON ACR_GC_Company = SD_Owner
					AND ACR_ReportType = 'MTD'
					AND ACR_Status IN ('{AccComplianceReport.Status.ReportGenerated}', '{AccComplianceReport.Status.ReportFinalised}')
					AND ACR_DateFrom = @FromDate
					AND ACR_DateTo = @ToDate
				INNER JOIN dbo.AccTaxReturn ON ATR_ACR_ComplianceReport = ACR_PK
					AND ATR_Status IN ('{AccTaxReturn.Status.Submitted}', '{AccTaxReturn.Status.Saved}')
				INNER JOIN dbo.AccTaxReturnColumn ON ATC_ATR_AccTaxReturn = ATR_PK
					AND ATC_GroupCode = '{MTDSubmissionDataColumns.AmountsToBeSubmittedToHMRCGroup}'
			WHERE SD_Name = '{nameof(AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany)}'
				AND SD_GuidValue = @CompanyPK
				AND GC_RN_NKCountryCode = 'GB'
			GROUP BY ATC_GroupCode, ATC_ColumnName";

			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@CompanyPK", Report.ACR_GC_Company, StmDataSchema.SD_GuidValue),
				ZSqlParameter.New("@FromDate", Report.ACR_DateFrom, AccComplianceReportSchema.ACR_DateFrom),
				ZSqlParameter.New("@ToDate", Report.ACR_DateTo, AccComplianceReportSchema.ACR_DateTo)
			};

			var collection = new DynamicBusinessObjectCollection(Report.Factory);
			collection.Load(sqlStatement, sqlParameters);

			var groupMemberTotalValues = new Dictionary<ZString, ZDecimal>();
			Array.ForEach(collection.ToArray(), (item) => { groupMemberTotalValues.Add((ZString)item["ATC_ColumnName"], (ZDecimal)item["ATC_Amount"]); });
			return groupMemberTotalValues;
		}

		(ZInt TotalMemberCompanies, ZInt TotalMemberCompaniesSubmittedReturn) GetGroupMemberCompanyInfo()
		{
			var sqlStatement = $@"
			SELECT
				COUNT(*) as TotalMemberCompanies,
				COUNT(ATR_PK) as TotalMemberCompaniesSubmittedReturn
			FROM dbo.StmData
				INNER JOIN dbo.GlbCompany ON GC_PK = SD_Owner
				LEFT JOIN dbo.AccComplianceReport ON ACR_GC_Company = SD_Owner
					AND ACR_ReportType = 'MTD'
					AND ACR_Status = '{AccComplianceReport.Status.ReportFinalised}'
					AND ACR_DateFrom = @FromDate
					AND ACR_DateTo = @ToDate
				LEFT JOIN dbo.AccTaxReturn ON ATR_ACR_ComplianceReport = ACR_PK
					AND ATR_Status = '{AccTaxReturn.Status.Submitted}'
			WHERE SD_Name = '{nameof(AccountingConfigurationRegistry.Instance.ConsumptionTaxGroupReportingCompany)}'
				AND SD_GuidValue = @CompanyPK
				AND GC_RN_NKCountryCode = 'GB'";

			var sqlParameters = new ZSqlParameter[]
			{
				ZSqlParameter.New("@CompanyPK", Report.ACR_GC_Company, StmDataSchema.SD_GuidValue),
				ZSqlParameter.New("@FromDate", Report.ACR_DateFrom, AccComplianceReportSchema.ACR_DateFrom),
				ZSqlParameter.New("@ToDate", Report.ACR_DateTo, AccComplianceReportSchema.ACR_DateTo)
			};

			var collection = new DynamicBusinessObjectCollection(Report.Factory);
			collection.Load(sqlStatement, sqlParameters);

			int totalMemberCompanies = 0, totalMemberCompaniesSubmittedReturn = 0;

			if (collection.Count == 1)
			{
				totalMemberCompanies = (ZInt)collection[0]["TotalMemberCompanies"];
				totalMemberCompaniesSubmittedReturn = (ZInt)collection[0]["TotalMemberCompaniesSubmittedReturn"];
			}

			return (totalMemberCompanies, totalMemberCompaniesSubmittedReturn);
		}
	}
}
