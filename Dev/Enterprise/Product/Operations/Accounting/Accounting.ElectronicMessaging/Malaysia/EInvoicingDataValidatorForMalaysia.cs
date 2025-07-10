using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Malaysia
{
	public class EInvoicingDataValidatorForMalaysia : BaseEInvoicingDataValidator
	{
		public EInvoicingDataValidatorForMalaysia(GlbCompany company, ICountryEInvoicingObjectFactory countryFactory, bool shouldSendErrorNotificationEmail = true) : base(company, shouldSendErrorNotificationEmail)
		{
			CountryFactory = countryFactory;
		}

		readonly ICountryEInvoicingObjectFactory CountryFactory;

		protected override void RunCore(ILogger logger) => ValidateBatchedTransactions(logger);

		public override IReadOnlyCollection<ZString> ValidateTransaction(InvoicingBase transaction, AccEInvoicingTransactionPivot pivot)
		{
			var errorMessages = new List<ZString>();

			if (pivot.AIP_ActionType != EInvoicingPivotActionType.Submit)
			{
				return errorMessages;
			}

			CheckClientIdAndSecret(transaction, errorMessages);
			CheckDigitalSignature(transaction, errorMessages);
			CheckRegistryNumberForProxy(transaction, errorMessages);
			CheckRegistryNumberForOrganisation(transaction, errorMessages);
			CheckTaxExemptionMessage(transaction, errorMessages);
			CheckCreatorContactNumber(transaction, errorMessages);
			CheckPartyContactNumber(transaction, errorMessages);

			return errorMessages;
		}

		void CheckClientIdAndSecret(InvoicingBase transaction, List<ZString> errorMessages)
		{
			var eInvoicingCredentials = AccountingElectronicMessagingRegistry.Instance.MalaysiaEInvoicingCredentials.Value;
			if (eInvoicingCredentials.ClientId.IsEmpty || eInvoicingCredentials.ClientSecret.IsEmpty)
			{
				errorMessages.Add(Res.GetString("5BAE0569-C511-4ADD-A4CA-E9736B38D6DD", "Malaysia E-Invoicing Client ID and Secret must be set against the registry 'E-Invoicing Credentials'."));
			}
		}

		void CheckDigitalSignature(InvoicingBase transaction, List<ZString> errorMessages)
		{
			var passwordType = CountryFactory.Credentials.PasswordType;
			var query = new ZQuery(GlbExternalPasswordSchema.GP_GC, transaction.Company.PK);
			query.AddToFilter(GlbExternalPasswordSchema.GP_PasswordType, SQLComparisonOperator.Equal, passwordType);

			var certificates = Factory.Load<GlbExternalPassword>(query);
			if (certificates.Any())
			{
				if (!certificates.Any(x => x.GP_PasswordStatus == PasswordStatusList.Codes.Valid))
				{
					var passwordStatusDescription = PasswordStatusList.GetFullList(Factory).GetDescriptionFromCode(certificates.FirstOrDefault().GP_PasswordStatus);
					errorMessages.Add(Res.GetString("9B2C8ECA-77D6-4765-9A9E-F73F3A24BEDB", "Unable to load Digital Certificate due to Status: {0}. Please check the Login Company > Accounting Configuration > e-Invoice Credentials tab.", passwordStatusDescription));
				}
			}
			else
			{
				errorMessages.Add(Res.GetString("9F0498C7-01B2-47F4-84EF-2D17D0D581BC", "Unable to load Digital Certificate. Please check the Login Company > Accounting Configuration > e-Invoice Credentials tab."));
			}
		}

		void CheckRegistryNumberForProxy(InvoicingBase transaction, List<ZString> errorMessages)
		{
			var branchOrgProxy = transaction.Branch.OrgProxy;
			if (branchOrgProxy != null)
			{
				CheckProxyRegistryNumber(branchOrgProxy, (NoResString)"Branch");
			}
			else
			{
				CheckProxyRegistryNumber(transaction.Company.OrgProxy, (NoResString)"Company");
			}

			void CheckProxyRegistryNumber(OrgHeader proxy, string proxyLevel)
			{
				if (proxy == null)
				{
					return;
				}

				if (GetRegistrationNumberByType(proxy, MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber).IsEmpty)
				{
					errorMessages.Add(Res.GetString("40F8F8E5-D708-4A72-8943-07E140268503", "Please record MY TIN - Tax Identification Number against the Login {0} Organization Proxy.", proxyLevel));
				}
				if (GetRegistrationNumberByType(proxy, MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany).IsEmpty)
				{
					errorMessages.Add(Res.GetString("3F73C7BA-DA41-46AC-9768-C9B14CD3DB12", "Please record MY ROC - Registrar of Company No against the Login {0} Organization Proxy.", proxyLevel));
				}
				if (GetRegistrationNumberByType(proxy, MalaysiaOrgCusCodeInfo.OrgCusCodes.SER).IsEmpty)
				{
					errorMessages.Add(Res.GetString("26423E96-989C-4487-91FD-DD35CEB0A6ED", "Please record MY SER - Government Service Tax Registration Number against the Login {0} Organization Proxy.", proxyLevel));
				}
				if (transaction.AH_Ledger != LedgerTypes.AccountsPayable && !IsValidSICRegistrationNumber(proxy))
				{
					errorMessages.Add(Res.GetString("DADBF30E-E028-4E9A-8C0B-576DE71A7C35", "Missing or invalid MY SIC - Standard Industrial Classification recorded against the Login {0} Organization Proxy.", proxyLevel));
				}
			}
		}

		void CheckRegistryNumberForOrganisation(InvoicingBase transaction, List<ZString> errorMessages)
		{
			var orgHeader = transaction.Header;
			var (participant, party, _, _) = GetMultipleInfo(transaction);

			if (GetRegistrationNumberByType(orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber).IsEmpty)
			{
				errorMessages.Add(Res.GetString("06FA8254-4E80-49F4-A05B-FA77C5F47A45", "{0}'s TIN - Tax Identification Number is mandatory for E-Invoicing. Please record a TIN Number against the {1} Organization.", party, participant));
			}

			if (transaction.AH_Ledger == LedgerTypes.AccountsPayable && !IsValidSICRegistrationNumber(orgHeader))
			{
				errorMessages.Add(Res.GetString("986E4E54-41B3-4756-B533-DB5ACAEC838F", "Missing or invalid MY SIC - Standard Industrial Classification recorded against the Creditor Organization."));
			}

			if (orgHeader.UNLOCO == null || orgHeader.UNLOCO.RL_RN_NKCountryCode.IsEmpty)
			{
				errorMessages.Add(Res.GetString("34266814-05AA-411D-858A-D86C043A700F", "{0}'s UNLOCO is mandatory for {0} address. Please record correct UNLOCO against the {1} Organization.", party, participant));
				return;
			}

			var unlocoCoutry = orgHeader.UNLOCO.RL_RN_NKCountryCode;
			var category = orgHeader.OH_Category;
			if (unlocoCoutry == Constants.CountryCodes.Malaysia)
			{
				if (category == OrgConstants.Category.Business && GetRegistrationNumberByType(orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany).IsEmpty)
				{
					errorMessages.Add(Res.GetString("731669F0-3146-4929-A0FE-91A03B2CB968", "Business located in Malaysia must have ROC - Registrar of Company No for E-Invoicing. Please record a ROC Number against the {0} Organization.", participant));
				}
				if (category == OrgConstants.Category.NaturalPersonIndividual)
				{
					if (GetRegistrationNumberByType(orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber).IsEmpty)
					{
						errorMessages.Add(Res.GetString("B99FDD19-D4F0-4AC9-A13C-FF3F517D4314", "Individual located in Malaysia must have PIC - Personal Identification Card Number for E-Invoicing. Please record a PIC Number against the {0} Organization.", participant));
					}
					if (GetRegistrationNumberByType(orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber) == OrgCusCodeSpecialRegistrationNumber.Number1
						&& GetRegistrationNumberByType(orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber) == OrgCusCodeSpecialRegistrationNumber.Number0)
					{
						errorMessages.Add(Res.GetString("179B4D12-D2E7-4772-8348-D6FDA07302A3", "Individual located in Malaysia cannot use TIN - Tax Identification Number 'EI00000000010' and PIC - Personal Identification Card Number '000000000000' at the same time for E-Invoicing. Please record an authentic TIN or PIC against the {0} Organization.", participant));
					}
				}
			}
			else
			{
				if (category == OrgConstants.Category.NaturalPersonIndividual)
				{
					if (OrgCusCodeByType(orgHeader, OrgCusCode.CodeTypes.PassportID) == null)
					{
						errorMessages.Add(Res.GetString("9E657560-A6F7-4DBB-A019-135A1D287D5E", "Foreigner must have PAS - Passport Number for E-Invoicing. Please record a PAS Number against the {0} Organization.", participant));
					}
					if (GetRegistrationNumberByType(orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber) == OrgCusCodeSpecialRegistrationNumber.Number2
						&& GetRegistrationNumberByType(orgHeader, OrgCusCode.CodeTypes.PassportID) == OrgCusCodeSpecialRegistrationNumber.Number0)
					{
						errorMessages.Add(Res.GetString("15ABC52C-3016-4072-BBDE-E3F1BAA87099", "Foreigner cannot use TIN - Tax Identification Number 'EI00000000020' and PAS - Passport Number '000000000000' at the same time for E-Invoicing. Please record an authentic TIN or PAS against the {0} Organization.", participant));
					}
				}
			}
		}

		void CheckTaxExemptionMessage(InvoicingBase transaction, List<ZString> errorMessages)
		{
			if (transaction.Lines.Cast<AccTransactionLines>().Any(line => (line.TaxRate?.AT_Type ?? string.Empty) == AccTaxRate.Types.Exempt && line.AL_A9_VATClass.IsEmpty))
			{
				errorMessages.Add(Res.GetString("F24AF0E6-966D-460C-84BA-B828EDA4C00F", "Tax Exemption message is mandatory for Tax Exemption lines for E-Invoicing. Please record Tax Message for all charge lines subject to Tax Exemption before posting."));
			}
		}

		void CheckCreatorContactNumber(InvoicingBase transaction, List<ZString> errorMessages)
		{
			var staff = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_Code, transaction.AH_SystemCreateUser));
			if ((staff?.GS_WorkPhone ?? ZString.Empty).IsEmpty)
			{
				var contactType = transaction.AH_Ledger == LedgerTypes.AccountsReceivable ? ContactType.Receivables : ContactType.Payables;
				var proxy = transaction.Branch.OrgProxy;
				var proxyLevel = proxy != null ? (NoResString)"Branch" : (NoResString)"Company";
				proxy ??= transaction.Company.OrgProxy;

				var isExistPhone = proxy.Contacts.Cast<OrgContact>().Any(x =>
						x.OC_IsActive &&
						!x.OC_Phone.IsEmpty &&
						x.Documents.Cast<OrgDocument>().Any(y =>
								y.OD_DefaultContact &&
								(y.OD_DocumentGroup == contactType || y.OD_DocumentGroup == ContactType.All)));
				if (!isExistPhone)
				{
					errorMessages.Add(Res.GetString("54D81AD4-D70C-49CB-8EE6-31F298C0A1DD", "Phone number is mandatory for E-Invoicing. Please add a Phone for the Staff that creates the transaction, or add a Phone for the official {0} (or ALL) group contact against the Login {1} Organization Proxy.", contactType, proxyLevel));
				}
			}
		}

		void CheckPartyContactNumber(InvoicingBase transaction, List<ZString> errorMessages)
		{
			var (participant, party, group, contactType) = GetMultipleInfo(transaction);
			if (transaction.AH_OC_InvoiceContactOverride.IsEmpty)
			{
				var contact = transaction.Header?.Contacts.Cast<OrgContact>().FirstOrDefault(x => x.OC_IsActive && x.Documents.Cast<OrgDocument>().Any(y => y.OD_DefaultContact && y.OD_DocumentGroup == contactType.Code));
				if ((contact?.OC_Phone ?? ZString.Empty).IsEmpty)
				{
					errorMessages.Add(Res.GetString("71EFF091-5A6E-4AA7-BC13-5408952D6BED", "{0}'s Phone number is mandatory for E-Invoicing. Please add a Phone against official {1} group contact of the {2} Organization.", party, group, participant));
				}
			}
			else
			{
				if ((transaction.InvoiceContactOverride?.OC_Phone ?? ZString.Empty).IsEmpty)
				{
					errorMessages.Add(Res.GetString("A4B38211-1626-492D-BC3F-22DE3F5974B0", "{0}'s Phone number is mandatory for E-Invoicing. Please add a Phone against the invoice Override Contact.", party));
				}
			}
		}

		ZString GetRegistrationNumberByType(OrgHeader orgHeader, string codeType) => OrgCusCodeByType(orgHeader, codeType)?.OK_CustomsRegNo ?? ZString.Empty;

		OrgCusCode OrgCusCodeByType(OrgHeader orgHeader, string codeType) => orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(codeType, Constants.CountryCodes.Malaysia);

		static class OrgCusCodeSpecialRegistrationNumber
		{
			public const string Number0 = "000000000000";
			public const string Number1 = "EI00000000010";
			public const string Number2 = "EI00000000020";
		}

		(string participant, string party, string group, ContactType contactType) GetMultipleInfo(InvoicingBase transaction)
		{
			if (transaction.AH_Ledger == LedgerTypes.AccountsReceivable)
			{
				return ((NoResString)"Debtor", (NoResString)"Buyer", "A/R", ContactType.Receivables);
			}
			else
			{
				return ((NoResString)"Creditor", (NoResString)"Supplier", "A/P", ContactType.Payables);
			}
		}

		bool IsValidSICRegistrationNumber(OrgHeader orgHeader)
		{
			var customsRegNo = GetRegistrationNumberByType(orgHeader, OrgCusCode.CodeTypes.StandardIndustrialClassification);
			return AccountingMasterFilesUtils.IsValidSIC(CountryCodes.Malaysia, customsRegNo);
		}
	}
}
