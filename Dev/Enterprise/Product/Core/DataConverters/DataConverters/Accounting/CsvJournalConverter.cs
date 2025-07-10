using System;
using System.Globalization;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataConverters.Accounting
{
	public class CsvJournalConverter
	{
		public const string Tab = "\t";

		public CsvJournalConverter(OCsvLine line, BusinessObjectFactory factory, string ledger, ZDateTime postDate)
		{
			this.Line = line;
			this.Factory = factory;
			this.Ledger = ledger;
			this.PostDate = postDate;

			ParseCsv();
			Validate();
		}

		public bool IsValid
		{
			get { return Errors.IsEmpty; }
		}

		public ZString Errors
		{
			get { return fErrors; }
		}

		public ZString Organisation
		{
			get { return fOrganisation; }
		}

		public bool IsDuplicate
		{
			get { return fIsDuplicate; }
		}

		public Journal CreateJournal()
		{
			if (!IsValid)
			{
				return null;
			}

			Type journalType = (Ledger == LedgerTypes.AccountsReceivable) ? typeof(ARJournal) : typeof(APJournal);

			Journal journal = (Journal)Factory.New(journalType);
			journal.UnhookLocalForeignAmountRecalculation();

			// The order of Set method is important. Have a careful look at them before changing.
			journal.AH_OH = OrganisationPK;
			journal.AH_RX_NKTransactionCurrency = CurrencyNK;
			journal.AH_Desc = Description;
			journal.AH_InvoiceDate = InvoiceDate;
			journal.AH_DueDate = DueDate;
			journal.AH_OSExTaxAmount = ForeignAmount;
			journal.AH_LocalExTaxAmount = InvoiceAmount;
			journal.SetDebitCreditSign();

			SetExchangeRate(journal, InvoiceAmount, ForeignAmount);
			journal.AH_Ledger = Ledger;
			journal.AH_GB = ImportFileBranchPK;
			journal.AH_GE = ImportFileDeparmentPK;
			journal.AH_PostDate = PostDate;

			journal.MarkAsNeedingValidation();
			journal.RunPreSaveValidation();
			if (journal.HasErrors())
			{
				var errors = new HumanReadableNotificationCollector(journal, true, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).GetErrors().ToUniqueMessageListString();
				SetErrorText(errors);
			}
			return journal;
		}

		#region Implementation

		readonly OCsvLine Line;
		readonly BusinessObjectFactory Factory;
		readonly string Ledger;
		readonly ZDateTime PostDate;

		ZString fOrganisation;
		ZString fErrors;
		bool fIsDuplicate;

		ZGuid OrganisationPK;
		ZString CurrencyNK;
		ZString Description;
		ZDateTime InvoiceDate;
		ZDateTime DueDate;
		ZDecimal InvoiceAmount;
		ZDecimal ForeignAmount;
		ZGuid ImportFileBranchPK;
		ZGuid ImportFileDeparmentPK;
		OrgHeader OrganisationHeader;

		#region Parse

		void ParseCsv()
		{
			if (Line.FieldValues.Length < 5)
			{
				SetErrorText(Res.GetString("9d1ec370-f61a-4f59-acc8-b46fdd39d2ee", "Incorrect format: Expected [organization, description, invoice date, due date, currency]. The line is: '{0}'", Line.ToString()) + " ");
				return;
			}

			fOrganisation = Line.FieldValues[0];
			OrganisationHeader = GetOrganisation(Organisation);
			OrganisationPK = OrganisationHeader != null ? OrganisationHeader.PK : ZGuid.Empty;
			CurrencyNK = Line.FieldValues[4];
			Description = new ZString(Line.FieldValues[1]).Trim();
			InvoiceDate = GetDateTime(Line.FieldValues[2]);
			DueDate = GetDateTime(Line.FieldValues[3]);

			InvoiceAmount = Line.FieldValues.Length > 6 ? GetAmount(Line.FieldValues[6], "Local Amount") : ZDecimal.Zero;
			ImportFileBranchPK = Line.FieldValues.Length > 7 ? GetBranchPK(Line.FieldValues[7]) : GlbBranch.CurrentBranch.PK;

			ForeignAmount = Line.FieldValues.Length > 5 ? GetAmount(Line.FieldValues[5], "Foreign Amount") : ZDecimal.Zero;
			ImportFileDeparmentPK = Line.FieldValues.Length > 8 ? GetDepartmentPK(Line.FieldValues[8]) : GlbDepartment.CurrentDepartment.PK;
		}

		ZDecimal GetAmount(string amountInString, string column)
		{
			ZDecimal amount;

			if (!ZDecimal.TryParse(amountInString, out amount))
			{
				SetErrorText(Res.GetString("266e7622-3172-44a2-ab86-205c92259d67", "{0} is in incorrect format.", column) + " ");
			}

			return amount;
		}

		/// <summary>
		/// Search for organisation will first try and match to a Deliverance Code
		/// If the organisation has not been found, the search will then try and match to a Legacy Code.
		/// Lastly if it still cannot find a match it will attempt to match on the actual organisation code
		/// </summary>
		OrgHeader GetOrganisation(string organisationCode)
		{
			ZGuid orgPK = ZGuid.Empty;
			OrgHeader enterpriseOrg = GetOrgFromDeliveranceCode(organisationCode);
			if (enterpriseOrg != null)
			{
				orgPK = enterpriseOrg.PK;
			}

			if (orgPK.IsEmpty)
			{
				enterpriseOrg = OrgHeader.FindByAccountID(Factory, organisationCode);
				if (enterpriseOrg != null)
				{
					orgPK = enterpriseOrg.PK;
				}
				else
				{
					enterpriseOrg = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, new ZString(organisationCode));
					if (enterpriseOrg != null)
					{
						orgPK = enterpriseOrg.PK;
					}
				}
			}

			return enterpriseOrg;
		}

		OrgHeader GetOrgFromDeliveranceCode(ZString orgLegacyCode)
		{
			OrgHeader org = null;
			ZQuery codeFilter = new ZQuery(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.DeliveranceCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			codeFilter.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, orgLegacyCode);
			OrgCusCode orgLegacyCodeRecord = Factory.LoadTop1<OrgCusCode>(codeFilter);
			if (orgLegacyCodeRecord != null)
			{
				org = Factory.Load<OrgHeader>(orgLegacyCodeRecord.OK_OH);
			}
			return org;
		}

		ZDateTime GetDateTime(string dateValue)
		{
			try
			{
				DateTimeFormatInfo info = new DateTimeFormatInfo();
				info.LongDatePattern = DateFormat;
				return new ZDateTime(DateTime.ParseExact(dateValue, DateFormat, info));
			}
			catch (FormatException)
			{
				return ZDateTime.Empty;
			}
		}

		const string DateFormat = "yyyyMMdd";

		ZGuid GetBranchPK(string branchCode)
		{
			GlbBranch branch = Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, new ZString(branchCode).Trim());
			if (branch != null)
			{
				return branch.PK;
			}

			return GlbBranch.CurrentBranch.PK;
		}

		void SetExchangeRate(Journal journal, ZDecimal localAmount, ZDecimal overseasAmount)
		{
			if (localAmount == overseasAmount)
			{
				journal.AH_ExchangeRate = 1;
			}
			else
			{
				int decimals = journal.TransactionCurrency.Decimals;
				ExchangeRate rate = new ExchangeRate(GlbCompany.CurrentCompany.GC_IsReciprocal, decimals, GlbCompany.CurrentCompany.PK.ToGuid());
				journal.AH_ExchangeRate = rate.GetRate(localAmount, overseasAmount);
			}
		}

		ZGuid GetDepartmentPK(string departmentCode)
		{
			GlbDepartment department = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, new ZString(departmentCode).Trim());
			if (department != null)
			{
				return department.PK;
			}

			return GlbDepartment.CurrentDepartment.PK;
		}

		ZString GetCurrencyNKForBranch(ZGuid branchPK)
		{
			GlbBranch branchOfTransaction = Factory.Load<GlbBranch>(branchPK);

			if (branchOfTransaction != null && branchOfTransaction.Company != null)
			{
				return branchOfTransaction.Company.GC_RX_NKLocalCurrency;
			}
			else
			{
				return ZString.Empty;
			}
		}

		#endregion

		#region Validate

		void SetErrorText(ZString text)
		{
			if (fErrors.IsEmpty)
			{
				fErrors = text;
			}
			else
			{
				fErrors += Tab + text;
			}
		}

		void Validate()
		{
			if (!OrganisationPK.IsValid)
			{
				SetErrorText(Res.GetString("a4c572e8-44e1-4705-a29a-e6ef86bd14de", "Organization code is invalid: ({0}).", fOrganisation) + " " );
			}

			RefCurrency currency = RefCurrency.LoadFromCurrencyCode(Factory, CurrencyNK);
			RefCurrency localCurrency = RefCurrency.LoadFromCurrencyCode(Factory, GetCurrencyNKForBranch(ImportFileBranchPK));

			if (currency == null)
			{
				SetErrorText(Res.GetString("73545253-3331-4a63-84e3-684ec0051043", "Currency code is invalid: ({0}).", (Line.FieldValues.Length > 4) ? Line.FieldValues[4] : " ") + " ");
			}

			if (Description.Length > AccTransactionHeaderSchema.AH_Desc.MaxLength)
			{
				SetErrorText(Res.GetString("864ce710-0a95-46f2-8768-23349cc2d506", "Description exceeds Max Length {0}.", AccTransactionHeaderSchema.AH_Desc.MaxLength) + " " );
			}

			var branch = Factory.Load<GlbBranch>(ImportFileBranchPK);
			if (branch?.Company.PK != GlbCompany.CurrentCompany.PK)
			{
				SetErrorText(Res.GetString("61567780-629e-4250-b68c-686eea4ef38e", "Transaction branch must be from the current company.") + " ");
			}

			if (CurrencyNK == GetCurrencyNKForBranch(ImportFileBranchPK) && InvoiceAmount != ForeignAmount)
			{
				SetErrorText(Res.GetString("51240bb7-cc0d-484c-9b3c-f9c70bc63a4f", "As Currency and Local Currency are same,  Invoice Amount and Foreign Amount must be equal.") + " ");
			}

			if (InvoiceAmount > 0 && localCurrency != null)
			{
				if (localCurrency.Decimals < InvoiceAmount.DecimalPlaces)
				{
					SetErrorText(Res.GetString("73829363-eb47-47b1-9a9b-972ff5a96279", "Local Amount has a decimal part longer than allowed. Currency settings allows {0} decimal places", GlbCompany.CurrentCompany.LocalCurrency.Decimals.ToString()) + " ");
				}
			}

			if (ForeignAmount > 0 && currency != null)
			{
				if (currency.Decimals < ForeignAmount.DecimalPlaces)
				{
					SetErrorText(Res.GetString("35bb347d-e635-4a5f-ab26-2818b45c1518", "Foreign Amount has a decimal part longer than allowed. Currency settings allows {0} decimal places", currency.Decimals.ToString()) + " ");
				}
			}

			if (InvoiceAmount < 0 && ForeignAmount > 0 || InvoiceAmount > 0 && ForeignAmount < 0)
			{
				SetErrorText(Res.GetString("6a260370-7b8d-4f91-80a3-2c4ec5f0aa60", "Local Amount must be the same sign as Foreign Amount.") + " ");
			}

			if (DuplicateUpdate(OrganisationPK, CurrencyNK, InvoiceAmount, Description))
			{
				SetErrorText(Res.GetString("7748ac71-2971-40de-8701-53e53e0f26ee", "Journal entry being loaded already exists - cannot load duplicate data. \r\nIt appears that the Balance conversion has already been run.\r\nYou cannot re-run it."));
				fIsDuplicate = true;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		bool DuplicateUpdate(ZGuid organisation, ZString currency, ZDecimal invAmount, ZString jnlDesc)
		{
			string ledgerType = "AP";
			if (Ledger == LedgerTypes.AccountsReceivable)
			{
				ledgerType = "AR";
			}

			if (ledgerType == "AP")
			{
				invAmount = invAmount * -1;
			}

			ZString filterString = @"SELECT count(*) FROM dbo.AccTransactionHeader WHERE AH_Ledger = '" + ledgerType +
				"' and AH_TransactionType = 'JNL' and AH_OH = '" + organisation +
				"' and AH_RX_NKTransactionCurrency = '" + currency +
				"' and AH_InvoiceAmount = " + invAmount +
				" and AH_Desc = @Description HAVING count(*) > 0";

			DbCommand checkDuplicateCommand = Db.Connection.Command(filterString);

			checkDuplicateCommand.AddParameterBasedOnDbColumn("@Description", jnlDesc.Left(AccTransactionHeader.Schema.AH_DescMaxLength).ToString(), AccTransactionHeaderSchema.AH_Desc);

			object result = checkDuplicateCommand.ExecuteScalar();

			return result != null && ((int)result > 0);
		}

		#endregion

		#endregion
	}
}
