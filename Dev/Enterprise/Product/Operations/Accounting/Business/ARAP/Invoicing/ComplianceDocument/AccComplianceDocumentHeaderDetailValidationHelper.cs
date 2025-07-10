using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ComplianceReport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class AccComplianceDocumentHeaderDetailValidationHelper
	{
		public AccComplianceDocumentHeaderDetailValidationHelper(BusinessObjectFactory factory, IComplianceDocumentHeaderDetail complianceDocumentHeaderDetail)
		{
			this.ComplianceDocumentHeaderDetail = complianceDocumentHeaderDetail;
			Factory = factory;
		}

		readonly IComplianceDocumentHeaderDetail ComplianceDocumentHeaderDetail;
		readonly BusinessObjectFactory Factory;

		#region ComplianceDocumentNumber

		public ZString NotMatchINVDocumentNumberError(ZString organizationType) => Res.GetString("6B103D11-A0AE-4DBF-9F9E-30EA8F788E2C", "Document Number does not match any 'INV' Compliance Document recorded against the {0}.", organizationType);

		public ZString DuplicateNumberError = Res.GetString("CE3C3914-C676-4074-9D5D-189D5FFF3193", "This Compliance Document Number is already in use. Please enter another number.");

		public ZString ValidateDocumentNumberWithValidPrefix()
		{
			if (GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan)
			{
				var documentSubType = ComplianceDocumentHeaderDetail.DocumentSubType;
				var documentNumber = ComplianceDocumentHeaderDetail.DocumentNumber;

				if (SubTypeListForCheckDocumentNumber.Contains(ComplianceDocumentHeaderDetail.DocumentSubType))
				{
					var startWithBBRegex = new Regex(@"^[B]{2}[a-z0-9]{8}$", RegexOptions.IgnoreCase);
					var tenAlphaNumberRegex = new Regex(@"^[a-z0-9]{10}$", RegexOptions.IgnoreCase);
					var basicRegex = new Regex(@"^[a-z]{2}\d{8}$", RegexOptions.IgnoreCase);

					if ((documentSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE || documentSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE))
					{
						if (!(startWithBBRegex.IsMatch(documentNumber) || basicRegex.IsMatch(documentNumber)))
						{
							return Res.GetString("97F26763-666F-4CE5-B794-4E331ECF86B0", @"For compliance sub type TXE and TCE, the document number must be in one of the following formats:
1. 2 characters prefix followed by 8 numeric digits. E.g. TX00001001.
2. 2 characters 'BB' prefix followed by 8 alpha-numeric characters. E.g. BB12345678, BBTXIC2535.");
						}
					}
					else if ((documentSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC || documentSubType == TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD))
					{
						if (!(basicRegex.IsMatch(documentNumber) || tenAlphaNumberRegex.IsMatch(documentNumber)))
						{
							return Res.GetString("{9E6DBCEC-5E51-4FCD-A3F9-F739908DF04F}", @"For compliance sub type TDC and TCD, the document number must be in one of the following formats:
1. 2 characters prefix followed by 8 numeric digits. E.g. TX00001001.
2. 10 alpha-numeric. E.g. A1G2345678, BDTXIC2535, 1234567890.");
						}
					}
					else
					{
						return ValidateDocumentNumberWithValidPrefixBasic();
					}
				}
			}

			return ZString.Empty;
		}

		public ZString ValidateDocumentNumberWithValidPrefixBasic()
		{
			var regex = new Regex(@"^[a-z]{2}\d{8}$", RegexOptions.IgnoreCase);
			if (!regex.IsMatch(ComplianceDocumentHeaderDetail.DocumentNumber))
			{
				return Res.GetString("904AB739-E117-46C4-8114-5CCBBC7F404B", "Document Number must contains two alphabet prefix followed by eight numeric values. E.g. TX00001001");
			}
			return ZString.Empty;
		}

		public ZString ValidateDocumentNmberMatchINVForCRD(ZGuid orgPK, ZString ledger)
		{
			if (ComplianceDocumentHeaderDetail.DocumentTransactionType == TransactionTypes.CreditNote &&
				AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.Value)
			{
				var result = GetDuplicateNumberComplianceDocument(TransactionTypes.Invoice, orgPK) != null;

				var organizationType = ledger == LedgerTypes.AccountsReceivable ? Res.GetString("E56B3054-AAB9-405B-89CA-12D8008E3222", "Debtor") : Res.GetString("6AEA46AD-D38B-4C65-9178-3CCDC7EC22EA", "Creditor");
				return result ? ZString.Empty : NotMatchINVDocumentNumberError(organizationType);
			}

			return ZString.Empty;
		}

		public ZString ValidateDuplicateAPDocumentNumber(ZGuid orgPK)
		{
			var result = GlbCompany.CurrentCompany.GC_RN_NKCountryCode == Core.Constants.CountryCodes.Taiwan
				? GetDuplicateNumberComplianceDocument(ComplianceDocumentHeaderDetail.DocumentTransactionType) != null
				: GetDuplicateNumberComplianceDocument(ComplianceDocumentHeaderDetail.DocumentTransactionType, orgPK) != null;

			return result ? DuplicateNumberError : ZString.Empty;
		}

		public ZBool ShouldValidateDuplicateNumber => ComplianceDocumentHeaderDetail.DocumentLedger == LedgerTypes.AccountsPayable &&
													(ComplianceDocumentHeaderDetail.DocumentTransactionType == TransactionTypes.Invoice ||
													(ComplianceDocumentHeaderDetail.DocumentTransactionType == TransactionTypes.CreditNote &&
													!AccountingMasterFilesRegistry.Instance.CreditNoteComplianceDocumentConfiguration.Value));

		public AccComplianceDocumentHeader GetDuplicateNumberComplianceDocument(ZString transactionType, ZGuid orgPK = new ZGuid())
		{
			var query = new ZQuery();
			query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_DocumentNumber, ComplianceDocumentHeaderDetail.DocumentNumber);
			query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_TransactionType, transactionType);
			query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_Ledger, ComplianceDocumentHeaderDetail.DocumentLedger);
			if (!orgPK.IsEmpty)
			{
				query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_OH_Organisation, orgPK);
			}
			query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_GC_Company, ComplianceDocumentHeaderDetail.DocumentCompany);
			query.AddToFilter(AccComplianceDocumentHeaderSchema.PK, SQLComparisonOperator.NotEqual, ComplianceDocumentHeaderDetail.DocumentPK);
			query.AddToFilter(AccComplianceDocumentHeaderSchema.ADH_DocumentStatus, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.ComplianceDocumentStatus.Voided);

			return Factory.LoadTop1<AccComplianceDocumentHeader>(query);
		}

		public List<string> SubTypeListForCheckDocumentNumber = new List<string>() { TaiwanComplianceInfo.ComplianceSubTypeCodes.TXI, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXP, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDI, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDC, TaiwanComplianceInfo.ComplianceSubTypeCodes.TDP, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCR, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCE, TaiwanComplianceInfo.ComplianceSubTypeCodes.TCD, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXE, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXC, TaiwanComplianceInfo.ComplianceSubTypeCodes.TSX, TaiwanComplianceInfo.ComplianceSubTypeCodes.TSD, TaiwanComplianceInfo.ComplianceSubTypeCodes.TXS };

		#endregion

		public ZString ValidateDocumentDate()
		{
			ZString result = ZString.Empty;
			if (ComplianceDocumentHeaderDetail.DocumentDate.Date > ZDateTime.Today.Date)
			{
				result = Res.GetString("9D9EA8B5-9FFF-48ED-9B66-7AF8A601226F", "This date cannot be in the future.");
			}
			return result;
		}

		public ZString ValidateDocumentReportingPeriod()
		{
			ZString result = ZString.Empty;

			ZInt period = ComplianceDocumentHeaderDetail.DocumentReportingPeriod;
			if (!PeriodCalculator.IsPeriodValid(period))
			{
				result = AccountingPeriodCalculator.GetInvalidPeriodValidationError(period);
			}
			else if (ComplianceDocumentHeaderDetail.DocumentLedger == LedgerTypes.AccountsPayable)
			{
				if (PeriodCalculator.IsFuturePeriod(period))
				{
					result = Res.GetString("DB458864-FFCF-45CD-A837-F0B8A73277A2", "This period cannot be in the future.");
				}

				if (result.IsEmpty && PeriodCalculator.IsPeriodSubLedgerClosed(period))
				{
					result = PeriodValidation.SubLedgerPeriodClosedError;
				}
			}

			if (result.IsEmpty)
			{
				var query = new ZQuery(AccPeriodManagementSchema.AM_GC_Company, GlbCompany.CurrentCompany.PK);
				query.AddToFilter(AccPeriodManagementSchema.AM_Period, ComplianceDocumentHeaderDetail.DocumentReportingPeriod);
				var periodManagement = Factory.LoadTop1<AccPeriodManagement>(query);

				if (periodManagement != null)
				{
					if (Finaliser.IsInFinalisedRange(ComplianceDocumentHeaderDetail.DocumentSubType, periodManagement.AM_StartDate, periodManagement.AM_EndDate.Date, ComplianceDocumentHeaderDetail.DocumentLedger))
					{
						result = Res.GetString("A8FC6620-8DBF-4166-B940-F0DC79238C19", "The reporting period falls in a compliance report that has been finalized.");
					}
				}
			}

			return result;
		}

		ComplianceReportComplianceDocumentFinaliser Finaliser
		{
			get
			{
				if (fFinaliser == null)
				{
					fFinaliser = new ComplianceReportComplianceDocumentFinaliser(Factory);
				}
				return fFinaliser;
			}
		}
		ComplianceReportComplianceDocumentFinaliser fFinaliser;

		AccountingPeriodCalculator fPeriodCalculator;
		public AccountingPeriodCalculator PeriodCalculator
		{
			get
			{
				if (fPeriodCalculator == null)
				{
					fPeriodCalculator = new AccountingPeriodCalculator(Factory);
				}
				return fPeriodCalculator;
			}
		}

		public PeriodValidationProvider PeriodValidation
		{
			get { return periodValidation ?? (periodValidation = GetPeriodValidationProvider()); }
		}
		PeriodValidationProvider periodValidation;

		protected virtual PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new PeriodValidationProvider(Factory);
		}
	}
}
