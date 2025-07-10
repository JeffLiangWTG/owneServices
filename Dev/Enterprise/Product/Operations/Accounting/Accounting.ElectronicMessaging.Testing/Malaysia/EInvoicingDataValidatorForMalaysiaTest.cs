using System;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.FeatureControl.Abstractions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation;
using Enterprise.Accounting.ElectronicMessaging.Common.DataValidation.Testing;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.GlobalEInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Malaysia;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Testing.Malaysia
{
	public class EInvoicingDataValidatorForMalaysiaTest : BaseEInvoicingDataValidatorTest
	{
		public void TestCheckClientIdAndSecret()
		{
			var expectedError = "Malaysia E-Invoicing Client ID and Secret must be set against the registry 'E-Invoicing Credentials'.";
			var (arInvoice, _, apInvoice, _, _, _) = Prepare();

			AssertValidationResult(arInvoice, expectedError);
			AssertValidationResult(apInvoice, expectedError);

			var credential = AccountingElectronicMessagingRegistry.Instance.MalaysiaEInvoicingCredentials.Value;
			credential.ClientId = "username";
			credential.ClientSecret = "password";
			AccountingElectronicMessagingRegistry.Instance.MalaysiaEInvoicingCredentials.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credential);

			AssertValidationResult(arInvoice, expectedError, hasError: false);
			AssertValidationResult(apInvoice, expectedError, hasError: false);
		}

		public void TestCheckDigitalSignature()
		{
			var (arInvoice, _, apInvoice, _, _, _) = Prepare();
			var missingCertificateExpectedError = "Unable to load Digital Certificate. Please check the Login Company > Accounting Configuration > e-Invoice Credentials tab.";

			AssertValidationResult(arInvoice, missingCertificateExpectedError);
			AssertValidationResult(apInvoice, missingCertificateExpectedError);

			using (var root = SampleCertificateHelper.CreateSampleRootCertificate(new X500DistinguishedName(CertificateRootName, X500DistinguishedNameFlags.UseSemicolons)))
			using (var cert = SampleCertificateHelper.CreateSampleChildCertificate(root, new X500DistinguishedName(CertificateChildName, X500DistinguishedNameFlags.UseSemicolons), issueDate: ZDateTime.Today.AddDays(-30), expiryDate: ZDateTime.Today.AddDays(30)))
			{
				var credential = CreateCompanyCredential(cert);

				credential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
				Factory.Save();

				var invalidStatusExpectedError = "Unable to load Digital Certificate due to Status: Invalid. Please check the Login Company > Accounting Configuration > e-Invoice Credentials tab.";
				AssertValidationResult(arInvoice, invalidStatusExpectedError);
				AssertValidationResult(apInvoice, invalidStatusExpectedError);

				credential.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
				Factory.Save();

				AssertValidationResult(arInvoice, missingCertificateExpectedError, hasError: false);
				AssertValidationResult(arInvoice, invalidStatusExpectedError, hasError: false);
				AssertValidationResult(apInvoice, missingCertificateExpectedError, hasError: false);
				AssertValidationResult(apInvoice, invalidStatusExpectedError, hasError: false);
			}

			GlbCompanyEInvoicingCertificateCredential CreateCompanyCredential(X509Certificate2 cert)
			{
				var credential = Factory.New<GlbCompanyEInvoicingCertificateCredential>();
				credential.CredentialSettings = CountryEInvoicingObjectFactory.Credentials;
				credential.GP_GB = ZGuid.Empty;
				credential.GP_GC = GlbCompany.CurrentCompany.PK;
				if (cert != null)
				{
					credential.GP_Certificate = cert.Export(X509ContentType.Pfx, CertificatePassword.SecurePassword);
					credential.CurrentDecryptedCertificatePassphrase = CertificatePassword.Password;
				}

				Factory.Save();
				return credential;
			}
		}

		public void TestCheckRegistrationNumberForProxy_WhenHasBranchProxy()
		{
			var proxyLevel = "Branch";
			var (arInvoice, _, apInvoice, _, _, _) = Prepare();

			TestCheckRegistrationNumberForProxy(arInvoice, arInvoice.Branch.OrgProxy, proxyLevel);
			TestCheckRegistrationNumberForProxy(apInvoice, apInvoice.Branch.OrgProxy, proxyLevel);
		}

		public void TestCheckRegistrationNumberForProxy_WhenHasCompanyProxy()
		{
			var proxyLevel = "Company";
			var (arInvoice, _, apInvoice, _, _, _) = Prepare();
			arInvoice.Branch.GB_OH_OrgProxy = ZGuid.Empty;
			apInvoice.Branch.GB_OH_OrgProxy = ZGuid.Empty;

			TestCheckRegistrationNumberForProxy(arInvoice, arInvoice.Company.OrgProxy, proxyLevel);
			TestCheckRegistrationNumberForProxy(apInvoice, apInvoice.Company.OrgProxy, proxyLevel);
		}

		void TestCheckRegistrationNumberForProxy(InvoicingBase invoice, OrgHeader orgHeader, string proxyLevel)
		{
			AssertRegistrationNumber(invoice, orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "1", CountryCodes.Malaysia, expectedError: $"Please record MY TIN - Tax Identification Number against the Login {proxyLevel} Organization Proxy.");
			AssertRegistrationNumber(invoice, orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany, "1", CountryCodes.Malaysia, expectedError: $"Please record MY ROC - Registrar of Company No against the Login {proxyLevel} Organization Proxy.");
			AssertRegistrationNumber(invoice, orgHeader, MalaysiaOrgCusCodeInfo.OrgCusCodes.SER, "1", CountryCodes.Malaysia, expectedError: $"Please record MY SER - Government Service Tax Registration Number against the Login {proxyLevel} Organization Proxy.");

			var expectedSCIError = $"Missing or invalid MY SIC - Standard Industrial Classification recorded against the Login {proxyLevel} Organization Proxy.";
			AssertSICRegistrationNumber();
			void AssertSICRegistrationNumber()
			{
				if (invoice.AH_Ledger == LedgerTypes.AccountsPayable)
				{
					return;
				}

				var mockIFeatureData = new Mock<IFeatureData>();
				var reportingBookFeatureControlData = new AccRegistrationNumberFeatureControlData
				{
					RegistrationNumbers =
					[
						new RegistrationNumberEntry
						{
							Country = "MY",
							Type = "SIC",
							Numbers = [new NumberTuple { Number = "12345" }]
						},

						new RegistrationNumberEntry
						{
							Country = "US",
							Type = "SIC",
							Numbers = [new NumberTuple { Number = "67890" }]
						}
					]
				};
				mockIFeatureData.Setup(x => x.TryDeserializeParameterAsJson(out reportingBookFeatureControlData)).Returns(true);
				var mockIFeatureControlManager = new Mock<IFeatureControlManager>();
				mockIFeatureControlManager.Setup(x => x.GetFeatureDataAsync(LicenceFeatureCodeList.Codes.AccountingRegistrationNumberFeature, CancellationToken.None)).Returns(Task.FromResult(mockIFeatureData.Object));

				using (ObjectFactory.Substitute(mockIFeatureControlManager.Object))
				{
					AssertRegistrationNumber(invoice, orgHeader, OrgCusCode.CodeTypes.StandardIndustrialClassification, MYMSICCodeDescriptionPairList.Codes.AirportAndAirTrafficControl, CountryCodes.Malaysia, expectedError: expectedSCIError);
					AssertRegistrationNumber(invoice, orgHeader, OrgCusCode.CodeTypes.StandardIndustrialClassification, "12345", CountryCodes.Malaysia, expectedError: expectedSCIError);
					AssertRegistrationNumber(invoice, orgHeader, OrgCusCode.CodeTypes.StandardIndustrialClassification, "67890", CountryCodes.Malaysia, expectedError: expectedSCIError, hasError: true);
				}
			}
		}

		void AssertRegistrationNumber(InvoicingBase arInvoice, OrgHeader orgHeader, ZString codeType, ZString number, ZString countryCode, string expectedError, bool hasError = false)
		{
			orgHeader.CustomsCodes.RemoveAll();
			AssertValidationResult(arInvoice, expectedError);

			orgHeader.CustomsCodes.AddNew(codeType, number, countryCode);
			AssertValidationResult(arInvoice, expectedError, hasError: hasError);
		}

		void AssertValidationResult(InvoicingBase transaction, string expectedError, bool hasError = true)
		{
			var errorMessages = EInvoicingDataValidator.ValidateTransaction(transaction, TestPivot);
			var containError = errorMessages.Contains(expectedError);
			Assert(hasError ? containError : !containError);
		}

		AccEInvoicingTransactionPivot TestPivot
		{
			get
			{
				if (testPivot == null)
				{
					testPivot = Factory.New<AccEInvoicingTransactionPivot>();
					testPivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
				}

				return testPivot;
			}
		}

		AccEInvoicingTransactionPivot testPivot;

		public void TestCheckRegistrationNumberForOrganisationForAR()
		{
			var (arInvoice, _, _, _, debtor, _) = Prepare();

			Factory.Save();

			var closestPort = debtor.OH_RL_NKClosestPort;
			debtor.OH_RL_NKClosestPort = ZString.Empty;
			AssertValidationResult(arInvoice, "Buyer's UNLOCO is mandatory for Buyer address. Please record correct UNLOCO against the Debtor Organization.");

			debtor.OH_RL_NKClosestPort = closestPort;
			debtor.OH_Category = OrgConstants.Category.Business;
			debtor.UNLOCO.RL_RN_NKCountryCode = "";
			AssertValidationResult(arInvoice, "Buyer's UNLOCO is mandatory for Buyer address. Please record correct UNLOCO against the Debtor Organization.");

			debtor.UNLOCO.RL_RN_NKCountryCode = CountryCodes.Malaysia;
			AssertRegistrationNumber(arInvoice, debtor, MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "1", CountryCodes.Malaysia, expectedError: "Buyer's TIN - Tax Identification Number is mandatory for E-Invoicing. Please record a TIN Number against the Debtor Organization.");
			AssertRegistrationNumber(arInvoice, debtor, MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany, "2", CountryCodes.Malaysia, expectedError: "Business located in Malaysia must have ROC - Registrar of Company No for E-Invoicing. Please record a ROC Number against the Debtor Organization.");

			debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			AssertRegistrationNumber(arInvoice, debtor, MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber, "1", CountryCodes.Malaysia, expectedError: "Individual located in Malaysia must have PIC - Personal Identification Card Number for E-Invoicing. Please record a PIC Number against the Debtor Organization.");

			var expectedError = "Individual located in Malaysia cannot use TIN - Tax Identification Number 'EI00000000010' and PIC - Personal Identification Card Number '000000000000' at the same time for E-Invoicing. Please record an authentic TIN or PIC against the Debtor Organization.";
			debtor.CustomsCodes.RemoveAll();
			debtor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "EI00000000010", CountryCodes.Malaysia);
			debtor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber, "000000000000", CountryCodes.Malaysia);
			AssertValidationResult(arInvoice, expectedError);

			debtor.CustomsCodes.RemoveAll();
			debtor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "EI1234", CountryCodes.Malaysia);
			debtor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber, "000000000000", CountryCodes.Malaysia);
			AssertValidationResult(arInvoice, expectedError, hasError: false);

			debtor.UNLOCO.RL_RN_NKCountryCode = CountryCodes.Australia;
			debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			AssertRegistrationNumber(arInvoice, debtor, OrgCusCode.CodeTypes.PassportID, "1", CountryCodes.Malaysia, expectedError: "Foreigner must have PAS - Passport Number for E-Invoicing. Please record a PAS Number against the Debtor Organization.");

			expectedError = "Foreigner cannot use TIN - Tax Identification Number 'EI00000000020' and PAS - Passport Number '000000000000' at the same time for E-Invoicing. Please record an authentic TIN or PAS against the Debtor Organization.";
			debtor.CustomsCodes.RemoveAll();
			debtor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "EI00000000020", CountryCodes.Malaysia);
			debtor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "000000000000", CountryCodes.Malaysia);
			AssertValidationResult(arInvoice, expectedError);

			debtor.CustomsCodes.RemoveAll();
			debtor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "EI1234", CountryCodes.Malaysia);
			debtor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "000000000000", CountryCodes.Malaysia);
			AssertValidationResult(arInvoice, expectedError, hasError: false);
		}

		public void TestCheckRegistrationNumberForOrganisationForAP()
		{
			var (_, _, apInvoice, _, _, creditor) = Prepare();

			Factory.Save();

			var closestPort = creditor.OH_RL_NKClosestPort;
			creditor.OH_RL_NKClosestPort = ZString.Empty;
			AssertValidationResult(apInvoice, "Supplier's UNLOCO is mandatory for Supplier address. Please record correct UNLOCO against the Creditor Organization.");

			creditor.OH_RL_NKClosestPort = closestPort;
			creditor.OH_Category = OrgConstants.Category.Business;
			creditor.UNLOCO.RL_RN_NKCountryCode = "";
			AssertValidationResult(apInvoice, "Supplier's UNLOCO is mandatory for Supplier address. Please record correct UNLOCO against the Creditor Organization.");

			creditor.UNLOCO.RL_RN_NKCountryCode = CountryCodes.Malaysia;
			AssertRegistrationNumber(apInvoice, creditor, MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "1", CountryCodes.Malaysia, expectedError: "Supplier's TIN - Tax Identification Number is mandatory for E-Invoicing. Please record a TIN Number against the Creditor Organization.");
			AssertRegistrationNumber(apInvoice, creditor, MalaysiaOrgCusCodeInfo.OrgCusCodes.RegistrarOfCompany, "2", CountryCodes.Malaysia, expectedError: "Business located in Malaysia must have ROC - Registrar of Company No for E-Invoicing. Please record a ROC Number against the Creditor Organization.");

			var expectedError = "Missing or invalid MY SIC - Standard Industrial Classification recorded against the Creditor Organization.";
			AssertRegistrationNumber(apInvoice, creditor, OrgCusCode.CodeTypes.StandardIndustrialClassification, MYMSICCodeDescriptionPairList.Codes.AirportAndAirTrafficControl, CountryCodes.Malaysia, expectedError: expectedError);

			creditor.CustomsCodes.RemoveAll();
			creditor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.StandardIndustrialClassification, "123", CountryCodes.Malaysia);
			AssertValidationResult(apInvoice, expectedError);

			creditor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			AssertRegistrationNumber(apInvoice, creditor, MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber, "1", CountryCodes.Malaysia, expectedError: "Individual located in Malaysia must have PIC - Personal Identification Card Number for E-Invoicing. Please record a PIC Number against the Creditor Organization.");

			expectedError = "Individual located in Malaysia cannot use TIN - Tax Identification Number 'EI00000000010' and PIC - Personal Identification Card Number '000000000000' at the same time for E-Invoicing. Please record an authentic TIN or PIC against the Creditor Organization.";
			creditor.CustomsCodes.RemoveAll();
			creditor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "EI00000000010", CountryCodes.Malaysia);
			creditor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber, "000000000000", CountryCodes.Malaysia);
			AssertValidationResult(apInvoice, expectedError);

			creditor.CustomsCodes.RemoveAll();
			creditor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "EI1234", CountryCodes.Malaysia);
			creditor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.PersonalIdentificationCardNumber, "000000000000", CountryCodes.Malaysia);
			AssertValidationResult(apInvoice, expectedError, hasError: false);

			creditor.UNLOCO.RL_RN_NKCountryCode = CountryCodes.Australia;
			creditor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			AssertRegistrationNumber(apInvoice, creditor, OrgCusCode.CodeTypes.PassportID, "1", CountryCodes.Malaysia, expectedError: "Foreigner must have PAS - Passport Number for E-Invoicing. Please record a PAS Number against the Creditor Organization.");

			expectedError = "Foreigner cannot use TIN - Tax Identification Number 'EI00000000020' and PAS - Passport Number '000000000000' at the same time for E-Invoicing. Please record an authentic TIN or PAS against the Creditor Organization.";
			creditor.CustomsCodes.RemoveAll();
			creditor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "EI00000000020", CountryCodes.Malaysia);
			creditor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "000000000000", CountryCodes.Malaysia);
			AssertValidationResult(apInvoice, expectedError);

			creditor.CustomsCodes.RemoveAll();
			creditor.CustomsCodes.AddNew(MalaysiaOrgCusCodeInfo.OrgCusCodes.TaxIdentificationNumber, "EI1234", CountryCodes.Malaysia);
			creditor.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "000000000000", CountryCodes.Malaysia);
			AssertValidationResult(apInvoice, expectedError, hasError: false);
		}

		public void TestInvoiceLinesDetail()
		{
			var expectedError = "Tax Exemption message is mandatory for Tax Exemption lines for E-Invoicing. Please record Tax Message for all charge lines subject to Tax Exemption before posting.";

			var extTaxRate = TestObjectCreator.CreateTaxRate("EXT1", "Exempt Rate 1", AccTaxRate.Types.Exempt, 10, string.Empty, 0, 1);
			var (arInvoice, arLine, apInvoice, apLine, _, _) = Prepare();
			arLine.AL_AT = extTaxRate.PK;
			apLine.AL_AT = extTaxRate.PK;

			Factory.Save();

			AssertValidationResult(arInvoice, expectedError);
			AssertValidationResult(apInvoice, expectedError);

			var taxMessage = TestObjectCreator.CreateTaxMsg("MSG1", "MSG1 Description", "English Msg 1", "Local Msg 1");
			arLine.AL_A9_VATClass = taxMessage.PK;
			apLine.AL_A9_VATClass = taxMessage.PK;

			AssertValidationResult(arInvoice, expectedError, hasError: false);
			AssertValidationResult(apInvoice, expectedError, hasError: false);
		}

		public void TestCreatorContactNumber()
		{
			var expectedError = "Phone number is mandatory for E-Invoicing. Please add a Phone for the Staff that creates the transaction, or add a Phone for the official {0} (or ALL) group contact against the Login {1} Organization Proxy.";
			var (arInvoice, _, apInvoice, _, _, _) = Prepare();
			var proxy = TestObjectCreator.Debtor1;
			proxy.Contacts.RemoveAll();
			var contact = TestObjectCreator.CreateContact(proxy, "first contact");
			contact.Documents.RemoveAll();
			var orgDocument = contact.Documents.AddNew();
			Factory.Save();

			AssertCreatorContactNumber(arInvoice, true, string.Format(expectedError, "A/R", "Branch"));
			AssertCreatorContactNumber(arInvoice, false, string.Format(expectedError, "A/R", "Company"));
			AssertCreatorContactNumber(apInvoice, true, string.Format(expectedError, "A/P", "Branch"));
			AssertCreatorContactNumber(apInvoice, false, string.Format(expectedError, "A/P", "Company"));

			void AssertCreatorContactNumber(InvoicingBase invoice, bool hasBranchProxy, string expectedError)
			{
				var query = new ZQuery(GlbStaffSchema.GS_Code, invoice.AH_SystemCreateUser);
				var staff = Factory.LoadTop1<GlbStaff>(query);
				staff.GS_WorkPhone = "123";
				Factory.Save();

				AssertValidationResult(invoice, expectedError, hasError: false);

				if (hasBranchProxy)
				{
					invoice.Branch.GB_OH_OrgProxy = proxy.PK;
				}
				else
				{
					invoice.Branch.GB_OH_OrgProxy = ZGuid.Empty;
					invoice.Company.GC_OH_OrgProxy = proxy.PK;
				}

				staff.GS_WorkPhone = ZString.Empty;
				contact.OC_Phone = "123";
				orgDocument.OD_DocumentGroup = invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? ContactType.Receivables.Code : ContactType.Payables.Code;
				orgDocument.OD_DefaultContact = true;
				Factory.Save();

				AssertValidationResult(invoice, expectedError, hasError: false);

				orgDocument.OD_DefaultContact = false;

				AssertValidationResult(invoice, expectedError);

				orgDocument.OD_DefaultContact = true;
				contact.OC_Phone = string.Empty;

				AssertValidationResult(invoice, expectedError);
			}
		}

		public void TestCreatorContactNumber_WhenHasMultipleDocumentGroup()
		{
			var expectedError = "Phone number is mandatory for E-Invoicing. Please add a Phone for the Staff that creates the transaction, or add a Phone for the official A/R (or ALL) group contact against the Login Branch Organization Proxy.";
			var (arInvoice, _, apInvoice, _, _, _) = Prepare();
			var query = new ZQuery(GlbStaffSchema.GS_Code, arInvoice.AH_SystemCreateUser);
			var staff = Factory.LoadTop1<GlbStaff>(query);
			staff.GS_WorkPhone = ZString.Empty;

			var proxy = TestObjectCreator.Debtor1;
			proxy.Contacts.RemoveAll();
			var contact1 = CreateContact("first contact", ContactType.Receivables.Code, string.Empty);
			var contact2 = CreateContact("second contact", ContactType.Receivables.Code, string.Empty);
			Factory.Save();

			AssertValidationResult(arInvoice, expectedError, hasError: true);

			contact1.OC_Phone = "123";
			Factory.Save();

			AssertValidationResult(arInvoice, expectedError, hasError: false);

			contact1.OC_Phone = string.Empty;
			contact2.OC_Phone = "123";
			Factory.Save();

			AssertValidationResult(arInvoice, expectedError, hasError: false);

			OrgContact CreateContact(string name, string group, string phone, bool isDefault = true)
			{
				var contact = TestObjectCreator.CreateContact(proxy, name);
				contact.OC_Phone = phone;
				var orgDocument = contact.Documents.AddNew();
				orgDocument.OD_DocumentGroup = group;
				orgDocument.OD_DefaultContact = isDefault;

				return contact;
			}
		}

		public void TestPartyContactNumber()
		{
			var (arInvoice, _, apInvoice, _, _, _) = Prepare();
			AssertPartyContactNumber(arInvoice, "Buyer's Phone number is mandatory for E-Invoicing. Please add a Phone against official A/R group contact of the Debtor Organization.", "Buyer's Phone number is mandatory for E-Invoicing. Please add a Phone against the invoice Override Contact.");
			AssertPartyContactNumber(apInvoice, "Supplier's Phone number is mandatory for E-Invoicing. Please add a Phone against official A/P group contact of the Creditor Organization.", "Supplier's Phone number is mandatory for E-Invoicing. Please add a Phone against the invoice Override Contact.");
		}

		public void AssertPartyContactNumber(InvoicingBase invoice, string expectedError1, string expectedError2)
		{
			var contact1 = TestObjectCreator.CreateContact(invoice.Header, "first contact for " + invoice.AH_TransactionNum);
			contact1.OC_Phone = "123";
			var contact2 = TestObjectCreator.CreateContact(invoice.Header, "second contact for " +  invoice.AH_TransactionNum);
			Factory.Save();

			Assert(invoice.AH_OC_InvoiceContactOverride.IsEmpty);
			AssertValidationResult(invoice, expectedError1);

			var orgDocument = contact1.Documents.AddNew();
			orgDocument.OD_DocumentGroup = invoice.AH_Ledger == LedgerTypes.AccountsReceivable ? ContactType.Receivables.Code : ContactType.Payables.Code;
			Factory.Save();

			AssertValidationResult(invoice, expectedError1, hasError: false);

			contact1.OC_IsActive = false;

			AssertValidationResult(invoice, expectedError1);

			invoice.AH_OC_InvoiceContactOverride = contact2.PK;

			AssertValidationResult(invoice, expectedError2);

			contact2.OC_Phone = "456";
			Factory.Save();

			AssertValidationResult(invoice, expectedError2, hasError: false);
		}

		public void TestValidateTransaction_IfActionTypeIsNotSubmitShouldReturnEmptyErrorMessages()
		{
			var pivot = Factory.New<AccEInvoicingTransactionPivot>();

			pivot.AIP_ActionType = EInvoicingPivotActionType.Submit;

			AssertExceptionThrown<NullReferenceException>(() => EInvoicingDataValidator.ValidateTransaction(null, pivot));

			foreach (var type in EInvoicingPivotActionType.CommandActionTypes.Where(x => x != EInvoicingPivotActionType.Submit).Concat(EInvoicingPivotActionType.QueryActionTypes))
			{
				pivot.AIP_ActionType = type;
				AssertEquals(0, EInvoicingDataValidator.ValidateTransaction(null, pivot).Count);
			}
		}

		(InvoicingBase ARInvoice, InvoicingLineBase ARLine, InvoicingBase APInvoice, InvoicingLineBase APLine, OrgHeader Debtor, OrgHeader Creditor) Prepare()
		{
			TestObjectCreator.SetCurrentCompanyCountryCode(CountryCodes.Malaysia);
			var testBranch = TestObjectCreator.CreateBranch("TBN", GlbCompany.CurrentCompany, BranchProxy);

			var debtor = TestObjectCreator.Debtor;
			debtor.OH_RL_NKClosestPort = "AUSYD";
			GlbBranch.CurrentBranch.GB_OH_OrgProxy = BranchProxy.PK;

			var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.AUD, 1m, debtor);
			arInvoice.AH_GB = testBranch.PK;
			var arLine = TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);

			var creditor = TestObjectCreator.AALSHI;
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("ARINV001", TestObjectCreator.AUD, 1M, 100M, 0M, 0M, 100M, 0M, 0M, creditor);
			apInvoice.AH_GB = testBranch.PK;
			var apLine = TestObjectCreator.CreateInvoiceLine(apInvoice, apInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);
			Factory.Save();

			return (arInvoice, arLine, apInvoice, apLine, debtor, creditor);
		}

		public override BaseEInvoicingDataValidator GetEInvoicingDataValidatorForTest()
		{
			return new EInvoicingDataValidatorForMalaysia(GlbCompany.CurrentCompany, CountryEInvoicingObjectFactory);
		}

		public void TestErrorSendsEmailNotification()
		{
			var (arInvoice, _, _, _, _, _) = Prepare();
			var batch = TestObjectCreator.CreateEInvoicingBatch(100, EInvoicingBatchState.Ready, GlbCompany.CurrentCompany);
			var pivot = TestObjectCreator.CreateEInvoicingTransactionPivot(batch, arInvoice, EInvoicingPivotState.Batched);
			Factory.Save();

			var notificationGroupPK = new EInvoicingTestHelper(TestObjectCreator).CreateNotificationGroup("Test User", "company.user@abc.com");
			var logger = new DetailedLoggerForTest();

			using (AccountingConfigurationRegistry.Instance.EInvoicingErrorNotificationGroup.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, notificationGroupPK.ToGuid()))
			{
				AssertEquals("Malaysia should not send email before validation.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
				GetEInvoicingDataValidatorForTest().Run(logger);
				AssertEquals("Malaysia should send email notifications on any errors", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			}

			AssertNotEquals("Errors should be recorded when validation fails.", string.Empty, pivot.AIP_ErrorDescription);
			AssertEquals("Pivot status should be Batched With Error when validation fails", EInvoicingPivotState.BatchedWithError, pivot.AIP_Status);
			AssertEquals("Pivot should not be of the batch when validation fails", ZGuid.Empty, pivot.AIP_AIB);
			AssertEquals("One Warning level log messages should be logged for transaction with validation error", 1, logger.Logs.Count(x => x.Item1 == LogType.Warning));
		}

		BaseEInvoicingDataValidator EInvoicingDataValidator => eInvoicingDataValidator ?? (eInvoicingDataValidator = GetEInvoicingDataValidatorForTest());
		BaseEInvoicingDataValidator eInvoicingDataValidator;

		ICountryEInvoicingObjectFactory CountryEInvoicingObjectFactory => countryEInvoicingObjectFactory ?? (countryEInvoicingObjectFactory = new MalaysiaEInvoicingObjectFactory());
		ICountryEInvoicingObjectFactory countryEInvoicingObjectFactory;

		const string CertificateRootName = "CN=Test Root Certificate;O=Some Organization Pty Ltd;C=PL";
		const string CertificateChildName = "CN=Test Certificate;O=Wisetech Global Limited;OU=Accounting Team;L=Alexendria;S=NSW;C=AU";

		OrgHeader BranchProxy => TestObjectCreator.Debtor1;

		static readonly NetworkCredential CertificatePassword = new NetworkCredential("user", "password12345678");
	}
}
