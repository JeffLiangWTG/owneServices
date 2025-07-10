using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;
using Enterprise.Customs.JP.Common.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using NUnit.Framework;
using static Enterprise.Customs.JP.Business.DeclarationCargoTypeList;
using static Enterprise.Customs.JP.Business.JPImportDeclarationTypeList;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(JobDeclarationValidation))]
	sealed class JobDeclarationValidationTest : BaseJobDeclarationValidationTest<JobDeclaration>
	{
		public void TestCheckJE_OA_Representative()
		{
			var representative = Factory.NewWithValidTestData<OrgAddress>();
			JobDeclaration.JE_OA_Representative = representative.PK;

			AssertHasMessageError(JobDeclaration.JE_OA_RepresentativeInfo, "The selected Attorney for Customs Procedures does not have a Customs Importer/Exporter Code. To add one, click on the organization field and press F3 to visit the organization, go to Details > Config > Registration Numbers/Codes, and add a new registration with the entered address as the Premises Address, JP as the Country/Region of Issue, and CIE/LPC/JAS as the Type.");

			representative.CustomsCodes.AddNew(OrgCusCode.JapanCodeTypes.CIE, "C0000123456789012");
			JobDeclaration.Validation.ValidateJE_OA_Representative();
			AssertNoMessageError(JobDeclaration.JE_OA_RepresentativeInfo, "The selected Attorney for Customs Procedures does not have a Customs Importer/Exporter Code. To add one, click on the organization field and press F3 to visit the organization, go to Details > Config > Registration Numbers/Codes, and add a new registration with the entered address as the Premises Address, JP as the Country/Region of Issue, and CIE/LPC/JAS as the Type.");
		}

		public void TestCheckJE_ACP_POA()
		{
			var representative = Factory.NewWithValidTestData<OrgAddress>();
			JobDeclaration.JE_OA_Representative = representative.PK;

			AssertHasMessageError(JobDeclaration.JE_ACP_POAInfo, "You have not entered an ACP Power of Attorney.");

			JobDeclaration.JE_ACP_POA = "1234567890";
			AssertNoMessageError(JobDeclaration.JE_ACP_POAInfo, "You have not entered an ACP Power of Attorney.");

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var eDoc = supplier.RequiredDocuments.AddNew();
			eDoc.EQ_ValidToDate = ZDateTime.Today.AddDays(1);
			eDoc.EQ_DateReceived = ZDateTimeOffset.Today.AddDays(-1);
			eDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.ClientSupplierRelationship;
			eDoc.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorney;
			eDoc.EQ_DocUsage = JobRequiredDocument.DocUsage.AttorneyForCustomsProcedures;
			eDoc.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Japan;
			eDoc.EQ_DocNumber = "0987654321";
			eDoc.EQ_OH_DocumentOwner = representative.OA_OH;

			Factory.Save();

			JobDeclaration.JE_OH_Supplier = supplier.PK;
			AssertHasWarning(JobDeclaration.JE_ACP_POAInfo, "The entered ACP Power of Attorney is different from the one configured in the Supplier organization: 0987654321");

			JobDeclaration.JE_ACP_POA = "0987654321";
			AssertNoWarning(JobDeclaration.JE_ACP_POAInfo, "The entered ACP Power of Attorney is different from the one configured in the Supplier organization: 0987654321");
		}

		[TestDate(2024, 12, 25)]
		public void TestCheckJE_ExportDate()
		{
			var targetInfo = JobDeclaration.JE_ExportDateInfo;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(targetInfo);

			JobDeclaration.JE_ExportDate = new ZDateTime(2024, 12, 24);
			AssertHasMessageError(targetInfo, "Please enter Date of Departure after the system date.");

			JobDeclaration.JE_ExportDate = new ZDateTime(2024, 12, 25);
			AssertNoMessageError(targetInfo, "Please enter Date of Departure after the system date.");
		}

		public void TestCheckJE_ExportDate_IsForMarineProductsExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingDataGrouping("JP");
			helper.CreateNewOrGetExistingCusCodeType("JPBLC", "JPBLC", Core.Constants.CountryCodes.Japan);
			var refCusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPBLC", "2HDN8", startDate, endDate);
			refCusCodeList.Attributes.DeleteAll();
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.Type, "洋上");

			Factory.Save();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_CustomsRegNo = "2HDN8";
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			customsCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			JobDeclaration.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;

			var expectedMessageError = "Date of Departure must be empty when Bonded Location Code is 洋上.";

			JobDeclaration.JE_ExportDate = ZDateTime.Empty;
			AssertNoMessageError(JobDeclaration.JE_ExportDateInfo, expectedMessageError);

			JobDeclaration.JE_ExportDate = new ZDateTime(2025, 4, 9);
			AssertHasMessageError(JobDeclaration.JE_ExportDateInfo, expectedMessageError);
		}

		public void TestCheckJE_ExportDateFMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var targetInfo = declaration.JE_ExportDateInfo;
			var warningMessage = $"You have not entered {targetInfo.HumanReadableName}. You can leave it empty if you are sure it is known to customs. Or the message may be rejected.";
			void AssertExportDate()
			{
				declaration.JE_ExportDate = ZDateTime.Empty;
				foreach (var mailDeclarationCargoType in new[]
				{
					MailedCargoList.Codes.E,
					MailedCargoList.Codes.M,
					MailedCargoList.Codes.U,
					MailedCargoList.Codes.H
				})
				{
					entryInstruction.CEI_DeclarationCargoType = mailDeclarationCargoType;
					declaration.Validation.ValidateJE_ExportDate();
					if (!entryInstruction.IsExportOrReturnedGoodsDeclarationType)
					{
						AssertNoWarning(targetInfo, warningMessage);
					}
				}

				foreach (var otherDeclarationCargoType in new[]
				{
					DeclarationCargoTypeList.Codes.S,
					DeclarationCargoTypeList.Codes.B,
					DeclarationCargoTypeList.Codes.L,
					DeclarationCargoTypeList.Codes.X,
					DeclarationCargoTypeList.Codes.G,
					DeclarationCargoTypeList.Codes.P,
					DeclarationCargoTypeList.Codes.K,
				})
				{
					entryInstruction.CEI_DeclarationCargoType = otherDeclarationCargoType;
					ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);
				}
			}

			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			foreach (var declarationType in new[]
			{
				JPExportDeclarationTypeList.Codes.N,
				JPExportDeclarationTypeList.Codes.M,
				JPExportDeclarationTypeList.Codes.T,
				JPExportDeclarationTypeList.Codes.G,
			})
			{
				entryInstruction.CEI_Style = declarationType;

				AssertExportDate();
			}
		}

		public void TestCheckJE_NACCSCredential()
		{
			CombineAssertions(() =>
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_Code = "st1";

				JobDeclaration.JE_TransportMode = UserCodeSpecificTransportModeList.Codes.SEA;
				JobDeclaration.JE_GS_NKCusAgent = staff.GS_Code;
				JobDeclaration.Validation.ValidateJE_NACCSCredential();
				AssertHasMessageErrorContaining(JobDeclaration.JE_NACCSCredentialInfo, "The Broker you have selected does not have a valid credential");

				var staff2 = Factory.NewWithValidTestData<GlbStaff>();
				staff2.GS_Code = "st2";

				var bthPassword2 = Factory.NewWithValidTestData<GlbExternalPasswordCUS>();
				bthPassword2.GP_GS = staff2.PK;
				bthPassword2.GP_PasswordType = JPPasswordType.Codes.CUS;
				bthPassword2.GP_Transport = UserCodeSpecificTransportModeList.Codes.BTH;
				bthPassword2.GP_GC = GlbCompany.CurrentCompany.PK;
				JobDeclaration.JE_GS_NKCusAgent = staff2.GS_Code;
				JobDeclaration.JE_NACCSCredential = ZGuid.Empty;
				AssertHasMessageErrorContaining(JobDeclaration.JE_NACCSCredentialInfo, "You have not entered");
				JobDeclaration.JE_NACCSCredential = bthPassword2.PK;
				AssertNoNotifications(JobDeclaration.JE_NACCSCredentialInfo);
			});
		}

		public void TestCheckJE_PaymentDeadlineExtensionCode()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var instruction = JobDeclaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.C;
			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.Comprehensive;
			AssertNoMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are H, K, M, A, B, C.");

			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndIndividual;
			AssertNoMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are H, K, M, A, B, C.");

			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewSpecial;
			AssertHasMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are H, K, M, A, B, C.");

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.J;
			JobDeclaration.Validation.ValidateJE_PaymentDeadlineExtension();
			AssertNoMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are T, E, F.");

			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndIndividual;
			AssertHasMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are T, E, F.");

			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.Comprehensive;
			AssertHasMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are T, E, F.");

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			JobDeclaration.Validation.ValidateJE_PaymentDeadlineExtension();
			AssertNoMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are H, K.");

			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndIndividual;
			AssertHasMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are H, K.");

			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewSpecial;
			AssertHasMessageError(JobDeclaration.JE_PaymentDeadlineExtensionInfo, $"The entered value is invalid for Declaration Type {instruction.CEI_Style}. The valid values are H, K.");

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.D;
			JobDeclaration.Validation.ValidateJE_PaymentDeadlineExtension();
			AssertHasMessageErrorContaining(JobDeclaration.JE_PaymentDeadlineExtensionInfo, MandatoryValidation.DoNotEntered);

			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndIndividual;
			AssertHasMessageErrorContaining(JobDeclaration.JE_PaymentDeadlineExtensionInfo, MandatoryValidation.DoNotEntered);

			JobDeclaration.JE_PaymentDeadlineExtension = PaymentDeadlineExtensionCodeList.Codes.Comprehensive;
			AssertHasMessageErrorContaining(JobDeclaration.JE_PaymentDeadlineExtensionInfo, MandatoryValidation.DoNotEntered);

			JobDeclaration.JE_PaymentDeadlineExtension = ZString.Empty;
			AssertNoMessageErrorContaining(JobDeclaration.JE_PaymentDeadlineExtensionInfo, MandatoryValidation.DoNotEntered);
		}

		public void TestCheckJE_OH_Importer()
		{
			JobDeclaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var instruction = JobDeclaration.CustomsEntryInstructions.AddNew();
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			JobDeclaration.JE_OH_Importer = importer.PK;
			JobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = Guid.Empty;
			Factory.Save();

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.H;
			JobDeclaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(JobDeclaration.JE_OH_ImporterInfo, "The selected importer must have a legal person code or importer/exporter code.");

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.J;
			JobDeclaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(JobDeclaration.JE_OH_ImporterInfo, "The selected importer must have a legal person code or importer/exporter code.");

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.R;
			JobDeclaration.Validation.ValidateJE_OH_Importer();
			AssertHasMessageError(JobDeclaration.JE_OH_ImporterInfo, "The selected importer must have a legal person code or importer/exporter code.");

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
			JobDeclaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(JobDeclaration.JE_OH_ImporterInfo, "The selected importer must have a legal person code or importer/exporter code.");

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.R;
			var customsCode = importer.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Japan;
			customsCode.OK_CodeType = OrgCusCode.JapanCodeTypes.CIE;
			customsCode.OK_CustomsRegNo = "1234567890333";
			JobDeclaration.Validation.ValidateJE_OH_Importer();
			AssertNoMessageError(JobDeclaration.JE_OH_ImporterInfo, "The selected importer must have a legal person code or importer/exporter code.");
		}

		public void TestCheckJE_OH_Supplier()
		{
			JobDeclaration.JE_MessageType = JPJobMessageTypeList.Codes.Import;
			var supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();
			JobDeclaration.JE_OH_Supplier = supplier.PK;
			Factory.Save();

			AssertHasMessageError(JobDeclaration.JE_OH_SupplierInfo, "The selected supplier should have POC in Japan.");

			var requiredDocument = supplier.RequiredDocuments.AddNew();
			requiredDocument.EQ_DocType = Core.Constants.RefDocTypes.PowerOfAttorneyCustoms;
			requiredDocument.EQ_RN_NKRelatedCountry = Core.Constants.CountryCodes.Japan;
			JobDeclaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError(JobDeclaration.JE_OH_SupplierInfo, "The selected supplier should have POC in Japan.");

			supplier.RequiredDocuments.RemoveAll();
			var importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			JobDeclaration.JE_OH_Importer = importer.PK;
			JobDeclaration.Validation.ValidateJE_OH_Supplier();
			AssertNoMessageError(JobDeclaration.JE_OH_SupplierInfo, "The selected supplier should have POC in Japan.");
		}

		public void TestCheckJE_ValuationDate()
		{
			var expectedErrorMessage = "You have not entered Valuation Date.";

			JobDeclaration.JE_ValuationDate = ZDate.Empty;
			JobDeclaration.Validation.ValidateJE_ValuationDate();
			AssertHasMessageErrorContaining(JobDeclaration.JE_ValuationDateInfo, expectedErrorMessage);

			var header = JobDeclaration.Invoices.AddNew();
			JobDeclaration.Validation.ValidateJE_ValuationDate();
			Assert(header.JZ_ValuationDateOverride.IsEmpty);
			AssertHasMessageErrorContaining(JobDeclaration.JE_ValuationDateInfo, expectedErrorMessage);

			JobDeclaration.JE_ValuationDate = ZDate.Today;
			JobDeclaration.Validation.ValidateJE_ValuationDate();
			AssertNoMessageErrorContaining(JobDeclaration.JE_ValuationDateInfo, expectedErrorMessage);

			JobDeclaration.JE_ValuationDate = ZDate.Empty;
			header.JZ_ValuationDateOverride = ZDate.Today;
			JobDeclaration.Validation.ValidateJE_ValuationDate();
			AssertNoMessageErrorContaining(JobDeclaration.JE_ValuationDateInfo, expectedErrorMessage);
		}

		public void TestCheckJE_GS_NKCusAgent()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "XXX";
			var certificate = staff1.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Japan;
			certificate.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(-1);
			certificate.XZ_Type = CertificateTypePairList.Codes.BR1;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "YYY";
			certificate = staff2.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Japan;
			certificate.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(25);
			certificate.XZ_Type = CertificateTypePairList.Codes.BR1;

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "ZZZ";
			certificate = staff3.Certificates.AddNew();
			certificate.XZ_RN_NKCountryOfIssuance = Core.Constants.CountryCodes.Japan;
			certificate.XZ_ExpiryOrDueDate = ZDateTime.Today.AddDays(36);
			certificate.XZ_Type = CertificateTypePairList.Codes.BR1;

			JobDeclaration.JE_GS_NKCusAgent = staff1.GS_Code;
			AssertHasMessageErrorContaining(JobDeclaration.JE_GS_NKCusAgentInfo, "The selected Broker's certificate has expired. Press F3 to visit the Staff and go to Human Resources > Certificate, ID and Training to update the JP-BRK certificate.");

			JobDeclaration.JE_GS_NKCusAgent = staff2.GS_Code;
			AssertHasMessageErrorContaining(JobDeclaration.JE_GS_NKCusAgentInfo, "The selected Broker's certificate will expire in 30 days. Please remember to renew the certificate.");

			JobDeclaration.JE_GS_NKCusAgent = staff3.GS_Code;
			AssertNoMessageErrorContaining(JobDeclaration.JE_GS_NKCusAgentInfo, "The selected Broker's certificate has expired. Press F3 to visit the Staff and go to Human Resources > Certificate, ID and Training to update the JP-BRK certificate.");
			AssertNoMessageErrorContaining(JobDeclaration.JE_GS_NKCusAgentInfo, "The selected Broker's certificate will expire in 30 days. Please remember to renew the certificate.");
		}

		public void TestCheckJE_HouseBill()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			JobDeclaration.JE_HouseBill = "1234";
			AssertHasMessageError(JobDeclaration.JE_HouseBillInfo, "House Bills should have at least 5 characters.");

			JobDeclaration.JE_TransportMode = Enterprise.Core.Constants.TransportModes.Air;
			JobDeclaration.JE_HouseBill = "123456789012345678901";
			AssertHasMessageError(JobDeclaration.JE_HouseBillInfo, "HAWB should have less than or equal to 20 characters.");

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_HouseBill = ZString.Empty;
			AssertHasMessageErrorContaining(JobDeclaration.JE_HouseBillInfo, MandatoryValidation.YouHaveNotEntered);

			JobDeclaration.JE_HouseBill = "00001";
			AssertNoMessageErrorContaining(JobDeclaration.JE_HouseBillInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJE_CustomsOffice()
		{
			var message = "The entered customs office does not exist.";

			var validCode = "1A";
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, validCode);

			JobDeclaration.JE_CustomsOffice = "LL";
			AssertHasMessageErrorContaining(JobDeclaration.JE_CustomsOfficeInfo, message);

			JobDeclaration.JE_CustomsOffice = validCode;
			AssertNoMessageErrorContaining(JobDeclaration.JE_CustomsOfficeInfo, message);
		}

		public void TestCheckJE_CustomsOfficeDepartment()
		{
			var message = "The entered customs office department does not exist.";

			var validCustomsOfficeCode = "1A";
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, validCustomsOfficeCode);

			var validCusOffDeptCode = "1A06";
			Factory.CreateRefDataForTest(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.JapanCustomsOfficeDepartment, validCusOffDeptCode);

			JobDeclaration.JE_CustomsOffice = validCustomsOfficeCode;
			JobDeclaration.JE_CustomsOfficeDepartment = "XX";
			AssertHasMessageErrorContaining(JobDeclaration.JE_CustomsOfficeDepartmentInfo, message);

			JobDeclaration.JE_CustomsOfficeDepartment = validCusOffDeptCode.Substring(2, 2);
			AssertNoMessageErrorContaining(JobDeclaration.JE_CustomsOfficeDepartmentInfo, message);
		}

		public void TestCheckJE_RL_NKPortOfLoading()
		{
			var message = "Port of Loading cannot be located in JP or ZY.";
			var targetInfo = JobDeclaration.JE_RL_NKPortOfLoadingInfo;
			var cusEntryInstruction = JobDeclaration.CustomsEntryInstructions.AddNew();

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_RL_NKPortOfLoading = "JPTYO";
			AssertHasMessageError(targetInfo, message);

			JobDeclaration.JE_RL_NKPortOfLoading = "ZYXXX";
			AssertHasMessageError(targetInfo, message);

			JobDeclaration.JE_RL_NKPortOfLoading = "AUBNE";
			AssertNoMessageError(targetInfo, message);

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			JobDeclaration.JE_RL_NKPortOfLoading = "JPTYO";
			AssertNoMessageError(targetInfo, message);

			JobDeclaration.JE_RL_NKPortOfLoading = "ZYXXX";
			AssertNoMessageError(targetInfo, message);

			JobDeclaration.JE_RL_NKPortOfLoading = "ZZZ";
			AssertHasMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);

			JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			JobDeclaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertNoMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);

			JobDeclaration.JE_TransportMode = ZString.Empty;
			cusEntryInstruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.MailedCargoList.Codes.U;
			JobDeclaration.Validation.ValidateJE_RL_NKPortOfLoading();
			AssertNoMessageError(targetInfo, JobDeclarationValidation.MessageErrorPortCodeInvalid);

			var warningMessage = $"You have not entered {targetInfo.HumanReadableName}. You can leave it empty if you are sure it is known to customs. Or the message may be rejected.";

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.JE_RL_NKPortOfLoading = ZString.Empty;
			AssertNoWarning(targetInfo, warningMessage);

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.ExportDeclarationByExternalBroker;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);
		}

		public void TestCheckJE_RL_NKPortOfLoading_IsForMarineProductsExport()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var startDate = ZDateTime.Today.AddDays(-1);
			var endDate = ZDateTime.Today.AddDays(1);
			helper.CreateNewOrGetExistingDataGrouping("JP");
			helper.CreateNewOrGetExistingCusCodeType("JPBLC", "JPBLC", Core.Constants.CountryCodes.Japan);
			var refCusCodeList = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Japan, "JPBLC", "2HDN8", startDate, endDate);
			refCusCodeList.Attributes.DeleteAll();
			helper.CreateNewOrGetExistingCusCodeListAttribute(refCusCodeList.PK, RefCusCodeListAttributeTypes.Codes.Type, "洋上");

			Factory.Save();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var customsCode = org.CustomsCodes.AddNew();
			customsCode.OK_RN_NKCodeCountry = "JP";
			customsCode.OK_CustomsRegNo = "2HDN8";
			customsCode.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			customsCode.OK_OA_PremisesAddress = org.MainAddress.PK;
			JobDeclaration.DepotDocAddress.E2_OA_Address = org.MainAddress.PK;

			var expectedMessageError = "Port of Loading must be ZZZ when Customs Depot is 洋上.";

			JobDeclaration.JE_RL_NKPortOfLoading = "";
			AssertHasMessageError(JobDeclaration.JE_RL_NKPortOfLoadingInfo, expectedMessageError);

			JobDeclaration.JE_RL_NKPortOfLoading = "ZZZ";
			AssertNoMessageError(JobDeclaration.JE_RL_NKPortOfLoadingInfo, expectedMessageError);
		}

		bool IsValidCombination(string paymentDeadlineExtension, string paymentMethod, bool isDefermentAccountEmpty)
		{
			(string[] paymentDeadlineExtensions, string[] paymentMethods, bool isDefermentAccountEmpty)[] validCombinations =
			{
				(paymentDeadlineExtensions: new[] { "" }, paymentMethods: new[] { PaymentMethodCodeList.Codes.MPN, PaymentMethodCodeList.Codes.MPNNotForLumpSumPayment }, isDefermentAccountEmpty: true),
				(paymentDeadlineExtensions: new[] { "" }, paymentMethods: new[] { PaymentMethodCodeList.Codes.DP, PaymentMethodCodeList.Codes.DirectPaymentNotforLumpSumPayment }, isDefermentAccountEmpty: true),
				(paymentDeadlineExtensions: new[] { "" }, paymentMethods: new[] { PaymentMethodCodeList.Codes.RealtimeAccount, PaymentMethodCodeList.Codes.RealtimeAccountNotForLumpSumPayment, PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeduction, PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeduction }, isDefermentAccountEmpty: false),
				(paymentDeadlineExtensions: new[] { PaymentDeadlineExtensionCodeList.Codes.Comprehensive, PaymentDeadlineExtensionCodeList.Codes.Individual, PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndIndividual, PaymentDeadlineExtensionCodeList.Codes.Special }, paymentMethods: new[] { PaymentMethodCodeList.Codes.MPN }, isDefermentAccountEmpty: true),
				(paymentDeadlineExtensions: new[] { PaymentDeadlineExtensionCodeList.Codes.Comprehensive, PaymentDeadlineExtensionCodeList.Codes.Individual, PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndIndividual, PaymentDeadlineExtensionCodeList.Codes.Special  }, paymentMethods: new[] { PaymentMethodCodeList.Codes.DP }, isDefermentAccountEmpty: true),
				(paymentDeadlineExtensions: new[] { PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.IndividualAndFewInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewIndividual, PaymentDeadlineExtensionCodeList.Codes.SpecialAndFewInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewSpecial }, paymentMethods: new[] { PaymentMethodCodeList.Codes.MPN, PaymentMethodCodeList.Codes.MPNNotForLumpSumPayment }, isDefermentAccountEmpty: true),
				(paymentDeadlineExtensions: new[] { PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.IndividualAndFewInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewIndividual, PaymentDeadlineExtensionCodeList.Codes.SpecialAndFewInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewSpecial }, paymentMethods: new[] { PaymentMethodCodeList.Codes.DP, PaymentMethodCodeList.Codes.DirectPaymentNotforLumpSumPayment }, isDefermentAccountEmpty: true),
				(paymentDeadlineExtensions: new[] { PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.IndividualAndFewInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewIndividual, PaymentDeadlineExtensionCodeList.Codes.SpecialAndFewInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewSpecial }, paymentMethods: new[] { PaymentMethodCodeList.Codes.RealtimeAccount, PaymentMethodCodeList.Codes.RealtimeAccountNotForLumpSumPayment, PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeduction, PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeduction }, isDefermentAccountEmpty: false),
				(paymentDeadlineExtensions: new[] { PaymentDeadlineExtensionCodeList.Codes.ComprehensiveAndInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.IndividualAndFewInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewIndividual, PaymentDeadlineExtensionCodeList.Codes.SpecialAndFewInstantConsumption, PaymentDeadlineExtensionCodeList.Codes.InstantConsumptionAndFewSpecial }, paymentMethods: new[] { PaymentMethodCodeList.Codes.TaxDeadlineExtension, PaymentMethodCodeList.Codes.RealtimeAccountAll, PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeductionOrTaxDeadlineExtension, PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeductionOrTaxDeadlineExtension }, isDefermentAccountEmpty: false),
			};
			foreach (var (validExtensions, validMethods, validDeferment) in validCombinations)
			{
				if (validExtensions.Contains(paymentDeadlineExtension) && validMethods.Contains(paymentMethod) && validDeferment == isDefermentAccountEmpty)
				{
					return true;
				}
			}
			return false;
		}
		public void TestCheckJE_DefermentAccountNumber()
		{
			var instruction = JobDeclaration.CustomsEntryInstructions.AddNew();
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			instruction.CEI_BeforePermitApplicationReason = BeforePermitApplicationReasonList.Codes.X9;
			JobDeclaration.Validation.ValidateJE_DefermentAccountNumber();
			AssertHasMessageError(JobDeclaration.JE_DefermentAccountNumberInfo, "Bank account number cannot be empty when Before Permit Application Reason is 9X.");

			instruction.CEI_BeforePermitApplicationReason = BeforePermitApplicationReasonList.Codes.G2;
			JobDeclaration.JE_DefermentAccountNumber = "12345";
			AssertNoMessageError(JobDeclaration.JE_DefermentAccountNumberInfo, "Bank account number cannot be empty when Before Permit Application Reason is 9X.");
		}

		public void TestCheckCombinationOfPaymentMethod()
		{
			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var paymentMethodCodes = new PaymentMethodCodeList().GetAllCodes();
			var paymentDeadlineExtensionCodes = new PaymentDeadlineExtensionCodeList();
			paymentDeadlineExtensionCodes.AddPair("", "");
			foreach (var paymentMethodCode in paymentMethodCodes)
			{
				JobDeclaration.JE_PaymentMethod = paymentMethodCode;
				foreach (var paymentDeadlineExtensionCode in paymentDeadlineExtensionCodes.GetAllCodes())
				{
					JobDeclaration.JE_PaymentDeadlineExtension = paymentDeadlineExtensionCode;
					foreach (var defermentAccountNumber in new[] { "", "123" })
					{
						JobDeclaration.JE_DefermentAccountNumber = defermentAccountNumber;
						JobDeclaration.Validation.ValidateJE_DefermentAccountNumber();
						JobDeclaration.Validation.ValidateJE_PaymentMethod();
						if (IsValidCombination(
							JobDeclaration.JE_PaymentDeadlineExtension.ToString(),
							JobDeclaration.JE_PaymentMethod.ToString(),
							string.IsNullOrEmpty(defermentAccountNumber)))
						{
							AssertEquals("JE_DefermentAccountNumber shoud not have any message errors", false, JobDeclaration.JE_DefermentAccountNumberInfo.HasMessageErrors());
							AssertEquals("JE_PaymentMethod shoud not have any message errors", false, JobDeclaration.JE_PaymentMethodInfo.HasMessageErrors());
						}
						else
						{
							if (JobDeclaration.JE_DefermentAccountNumber.IsEmpty && (JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.RealtimeAccount || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.RealtimeAccountNotForLumpSumPayment || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeduction || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeduction || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.TaxDeadlineExtension || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.RealtimeAccountAll || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeductionOrTaxDeadlineExtension || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeductionOrTaxDeadlineExtension))
							{
								AssertHasMessageError(JobDeclaration.JE_DefermentAccountNumberInfo, string.Format("Bank account number is required when Payment Method is {0}.", JobDeclaration.JE_PaymentMethod));
							}
							else if (JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.DP && !JobDeclaration.JE_DefermentAccountNumber.IsEmpty)
							{
								AssertHasMessageError(JobDeclaration.JE_DefermentAccountNumberInfo, "Bank Account Number is not required when Payment Method is empty.");
							}
							else if (JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.DirectPaymentNotforLumpSumPayment || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.MPN || JobDeclaration.JE_PaymentMethod == PaymentMethodCodeList.Codes.MPNNotForLumpSumPayment)
							{
								if (!JobDeclaration.JE_DefermentAccountNumber.IsEmpty)
								{
									AssertHasMessageError(JobDeclaration.JE_DefermentAccountNumberInfo, string.Format("Bank Account Number is not required when Payment Method is {0}.", JobDeclaration.JE_PaymentMethod));
								}
								else
								{
									AssertHasMessageError(JobDeclaration.JE_PaymentMethodInfo, string.Format("Payment Method must be empty or M when Payment Deadline Extension is {0}.", JobDeclaration.JE_PaymentDeadlineExtension));
								}
							}
							else if (JobDeclaration.JE_PaymentDeadlineExtension.IsEmpty && !JobDeclaration.JE_PaymentMethod.IsEmpty && JobDeclaration.JE_PaymentMethod != PaymentMethodCodeList.Codes.MPN && JobDeclaration.JE_PaymentMethod != PaymentMethodCodeList.Codes.MPNNotForLumpSumPayment && JobDeclaration.JE_PaymentMethod != PaymentMethodCodeList.Codes.DirectPaymentNotforLumpSumPayment && JobDeclaration.JE_PaymentMethod != PaymentMethodCodeList.Codes.RealtimeAccount && JobDeclaration.JE_PaymentMethod != PaymentMethodCodeList.Codes.RealtimeAccountNotForLumpSumPayment && JobDeclaration.JE_PaymentMethod != PaymentMethodCodeList.Codes.NotifyTaxpayerOfDeduction && JobDeclaration.JE_PaymentMethod != PaymentMethodCodeList.Codes.NotifyTaxpayerAndImporterOfDeduction)
							{
								AssertHasMessageError(JobDeclaration.JE_PaymentMethodInfo, "Payment Method must be one of empty, M, W, X, R, Y, E, S when Payment Deadline Extension is empty.");
							}
							else
							{
								AssertHasMessageError(JobDeclaration.JE_PaymentMethodInfo, string.Format("Payment Method must be empty or M when Payment Deadline Extension is {0}.", JobDeclaration.JE_PaymentDeadlineExtension));
							}
						}
					}
				}
			}
		}

		public void TestCheckJE_MasterBill()
		{
			var message = @"There is not any bill added under ""Packing - Bills"".";

			JobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobDeclaration.Validation.ValidateJE_MasterBill();
			AssertHasMessageError(JobDeclaration.JE_MasterBillInfo, message);

			JobDeclaration.Bills.AddNew();
			JobDeclaration.Validation.ValidateJE_MasterBill();
			AssertNoMessageError(JobDeclaration.JE_MasterBillInfo, message);
		}

		public void TestCheckJE_CarrierCode()
		{
			var carrier = Factory.NewWithValidTestData<ZZRefCarrierCombined>();
			carrier.ZZ4_CountryOrGrouping = Core.Constants.CountryCodes.Japan;
			carrier.ZZ4_Code = "2B23";
			carrier.ZZ4_IsSea = true;
			Factory.Save();

			JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.JE_CarrierCodeInfo, "1AA", "1A");

			JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.JE_CarrierCodeInfo, "1A", "2B23");
		}

		public void TestCheckInspectionWitnessCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.InspectionWitnessCodeInfo, "1234", "12345", "NACCS User Code should be exactly 5 characters long.");
		}

		public void TestCheckExternalBrokerCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.ExternalBrokerCodeInfo, "1234", "12345", "NACCS User Code should be exactly 5 characters long.");
		}

		public void TestCheckAirCargoAgentNACCSCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.AirCargoAgentNACCSCodeInfo, "1234", "12345", "NACCS User Code should be exactly 5 characters long.");
		}

		public void TestCheckAirCargoAgentLocationCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.AirCargoAgentLocationCodeInfo, "a12", "ABC", "Please enter no longer than 3 characters consisting solely of digits and capital letters [A-Z].");
		}

		public void TestCheckForwarderCode()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.ForwarderCodeInfo, "1234", "12345", "NACCS User Code should be exactly 5 characters long.");
		}

		public void TestCheckPortOfLoadingIATACode()
		{
			Factory.CreateUNLOCOData();
			JobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var targetInfo = JobDeclaration.PortOfLoadingIATACodeInfo;
			var expectedErrorMessage = "You have not entered";
			JobDeclaration.Validation.ValidatePortOfLoadingIATACode();
			AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

			JobDeclaration.JE_RL_NKPortOfLoading = "JP001";
			JobDeclaration.Validation.ValidatePortOfLoadingIATACode();
			AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

			JobDeclaration.JE_RL_NKPortOfLoading = "JP002";
			JobDeclaration.PortOfLoadingIATACode = "TKU";
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);
			expectedErrorMessage = string.Format("The entered {0} IATA Code is not recognized and will be cleared upon reloading the screen. To save this value, please either create a new {0} with the specified IATA Code or add it to an existing {0}.", JobDeclaration.JE_RL_NKPortOfLoadingInfo.HumanReadableName);
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			JobDeclaration.JE_RL_NKPortOfLoading = ZString.Empty;
			JobDeclaration.PortOfLoadingIATACode = "123";
			AssertHasMessageError(targetInfo, expectedErrorMessage);
		}

		public void TestCheckFinalDestinationIATACode()
		{
			Factory.CreateUNLOCOData();
			JobDeclaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var targetInfo = JobDeclaration.FinalDestinationIATACodeInfo;
			var expectedErrorMessage = "You have not entered";
			JobDeclaration.Validation.ValidateFinalDestinationIATACode();
			AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

			JobDeclaration.JE_RL_NKFinalDestination = "JP001";
			JobDeclaration.Validation.ValidateFinalDestinationIATACode();
			AssertHasMessageErrorContaining(targetInfo, expectedErrorMessage);

			JobDeclaration.JE_RL_NKFinalDestination = "JP002";
			JobDeclaration.FinalDestinationIATACode = "TKU";
			AssertNoMessageErrorContaining(targetInfo, expectedErrorMessage);
			expectedErrorMessage = string.Format("The entered {0} IATA Code is not recognized and will be cleared upon reloading the screen. To save this value, please either create a new {0} with the specified IATA Code or add it to an existing {0}.", JobDeclaration.JE_RL_NKFinalDestinationInfo.HumanReadableName);
			AssertNoMessageError(targetInfo, expectedErrorMessage);

			JobDeclaration.JE_RL_NKFinalDestination = ZString.Empty;
			JobDeclaration.FinalDestinationIATACode = "123";
			AssertHasMessageError(targetInfo, expectedErrorMessage);
		}

		public void TestCheckJE_ReceiptMode()
		{
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_MessageType = "EXP";
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.JE_ReceiptModeInfo, "XX", "51");

			JobDeclaration.JE_ReceiptMode = ZString.Empty;
			JobDeclaration.Validation.ValidateJE_ReceiptMode();
			AssertNoMessageErrorContaining(JobDeclaration.JE_ReceiptModeInfo, MandatoryValidation.YouHaveNotEntered);

			JobDeclaration.CustomsEntryInstructions.AddNew();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(JobDeclaration.JE_ReceiptModeInfo);
		}

		public void TestCheckJE_DeliveryMode()
		{
			JobDeclaration.JE_TransportMode = "SEA";
			JobDeclaration.JE_MessageType = "EXP";
			ValidationTestHelper.AssertInvalidCodeMessageError(JobDeclaration.JE_DeliveryModeInfo, "XX", "51");
		}

		public void TestCheckJE_RL_NKFinalDestination()
		{
			var declaration = Factory.New<JobDeclaration>();
			var targetInfo = declaration.JE_RL_NKFinalDestinationInfo;
			var warningMessage = $"You have not entered {targetInfo.HumanReadableName}. You can leave it empty if you are sure it is known to customs. Or the message may be rejected.";
			declaration.JE_MessageType = "EXP";
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);
		}

		public void TestCheckJE_VesselName()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var targetInfo = declaration.JE_VesselNameInfo;
			var warningMessage = $"You have not entered {targetInfo.HumanReadableName}. You can leave it empty if you are sure it is known to customs. Or the message may be rejected.";
			void AssertVesselName()
			{
				declaration.JE_VesselName = ZString.Empty;
				foreach (var mailDeclarationCargoType in new[]
				{
					MailedCargoList.Codes.E,
					MailedCargoList.Codes.M,
					MailedCargoList.Codes.U,
					MailedCargoList.Codes.H
				})
				{
					entryInstruction.CEI_DeclarationCargoType = mailDeclarationCargoType;
					declaration.Validation.ValidateJE_VesselName();
					if (declaration.IsExport && !entryInstruction.IsExportOrReturnedGoodsDeclarationType)
					{
						AssertNoWarning(targetInfo, warningMessage);
					}
				}

				foreach (var otherDeclarationCargoType in new[]
				{
					DeclarationCargoTypeList.Codes.S,
					DeclarationCargoTypeList.Codes.B,
					DeclarationCargoTypeList.Codes.L,
					DeclarationCargoTypeList.Codes.X,
					DeclarationCargoTypeList.Codes.G,
					DeclarationCargoTypeList.Codes.P,
					DeclarationCargoTypeList.Codes.K,
				})
				{
					entryInstruction.CEI_DeclarationCargoType = otherDeclarationCargoType;
					ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);
				}
			}

			foreach (var (messageType, declarationType) in new[]
			{
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, TaxationDeclarationTypeList.Codes.C),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, TaxationDeclarationTypeList.Codes.F),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, InbondDeclarationTypeList.Codes.S),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, InbondDeclarationTypeList.Codes.M),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, InbondDeclarationTypeList.Codes.A),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import, InbondDeclarationTypeList.Codes.G),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.N),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.M),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.T),
				(Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export, JPExportDeclarationTypeList.Codes.G),
			})
			{
				declaration.JE_MessageType = messageType;
				entryInstruction.CEI_Style = declarationType;

				AssertVesselName();
			}
		}

		public void TestCheckJE_RL_NKOriginMMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var targetInfo = declaration.JE_RL_NKOriginInfo;
			void AssertOrigin()
			{
				invoiceLine.JI_BondedDate = ZDateTime.Empty;
				entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.E;
				declaration.JE_RL_NKOrigin = ZString.Empty;
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.M;
				declaration.Validation.ValidateJE_RL_NKOrigin();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.H;
				declaration.Validation.ValidateJE_RL_NKOrigin();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.U;
				declaration.Validation.ValidateJE_RL_NKOrigin();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.S;
				declaration.Validation.ValidateJE_RL_NKOrigin();
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_BondedDate = ZDateTime.Now;
				declaration.Validation.ValidateJE_RL_NKOrigin();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_RL_NKOrigin = "TPE";
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			}

			foreach (var declarationType in new[]
			{
				JPImportDeclarationTypeList.Codes.C,
				JPImportDeclarationTypeList.Codes.F,
				JPImportDeclarationTypeList.Codes.Y,
				JPImportDeclarationTypeList.Codes.H,
				JPImportDeclarationTypeList.Codes.N,
				JPImportDeclarationTypeList.Codes.J,
				JPImportDeclarationTypeList.Codes.P,
				JPImportDeclarationTypeList.Codes.T,
				JPImportDeclarationTypeList.Codes.V,
				JPImportDeclarationTypeList.Codes.S,
				JPImportDeclarationTypeList.Codes.M,
				JPImportDeclarationTypeList.Codes.A,
				JPImportDeclarationTypeList.Codes.G,
			})
			{
				entryInstruction.CEI_Style = declarationType;
				AssertOrigin();
			}
		}

		public void TestCheckJE_RL_NKOriginFMandatory()
		{
			var declaration = JobDeclaration;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var targetInfo = declaration.JE_RL_NKOriginInfo;
			var warningMessage = $"You have not entered {targetInfo.HumanReadableName}. You can leave it empty if you are sure it is known to customs. Or the message may be rejected.";
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.K;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.D;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.U;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.L;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.E;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.R;
			declaration.Validation.ValidateJE_RL_NKOrigin();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.C;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.F;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);
		}

		public void TestCheckJE_RL_NKPortOfArrivalMMandatory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var targetInfo = declaration.JE_RL_NKPortOfArrivalInfo;
			void AssertOrigin()
			{
				invoiceLine.JI_BondedDate = ZDateTime.Empty;
				entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.E;
				declaration.JE_RL_NKPortOfArrival = ZString.Empty;
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.M;
				declaration.Validation.ValidateJE_RL_NKPortOfArrival();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.H;
				declaration.Validation.ValidateJE_RL_NKPortOfArrival();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_DeclarationCargoType = MailedCargoList.Codes.U;
				declaration.Validation.ValidateJE_RL_NKPortOfArrival();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_DeclarationCargoType = DeclarationCargoTypeList.Codes.S;
				declaration.Validation.ValidateJE_RL_NKPortOfArrival();
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine.JI_BondedDate = ZDateTime.Now;
				declaration.Validation.ValidateJE_RL_NKPortOfArrival();
				AssertHasMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);

				declaration.JE_RL_NKPortOfArrival = "TPE";
				AssertNoMessageErrorContaining(targetInfo, MandatoryValidation.YouHaveNotEntered);
			}

			foreach (var declarationType in new[]
			{
				JPImportDeclarationTypeList.Codes.C,
				JPImportDeclarationTypeList.Codes.F,
				JPImportDeclarationTypeList.Codes.Y,
				JPImportDeclarationTypeList.Codes.H,
				JPImportDeclarationTypeList.Codes.N,
				JPImportDeclarationTypeList.Codes.J,
				JPImportDeclarationTypeList.Codes.P,
				JPImportDeclarationTypeList.Codes.T,
				JPImportDeclarationTypeList.Codes.V,
				JPImportDeclarationTypeList.Codes.S,
				JPImportDeclarationTypeList.Codes.M,
				JPImportDeclarationTypeList.Codes.A,
				JPImportDeclarationTypeList.Codes.G,
			})
			{
				entryInstruction.CEI_Style = declarationType;
				AssertOrigin();
			}
		}

		public void TestCheckJE_RL_NKPortOfArrivalFMandatory()
		{
			var declaration = JobDeclaration;
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var targetInfo = declaration.JE_RL_NKPortOfArrivalInfo;
			var warningMessage = $"You have not entered {targetInfo.HumanReadableName}. You can leave it empty if you are sure it is known to customs. Or the message may be rejected.";
			declaration.JE_MessageType = "IMP";
			declaration.JE_TransportMode = "SEA";
			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.K;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.D;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.U;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.L;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.B;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.E;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.R;
			declaration.Validation.ValidateJE_RL_NKPortOfArrival();
			AssertNoWarning(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.C;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.F;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);

			instruction.CEI_Style = JPImportDeclarationTypeList.Codes.Y;
			ValidationTestHelper.AssertWarningIfNotEntered(targetInfo, warningMessage);
		}

		public void TestCheckJE_RadioCallSign()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateVesselZZ("LA BOUDEUSE", "LXBH", "CV", "JP");
			Factory.Save();
			var targetInfo = JobDeclaration.JE_RadioCallSignInfo;
			JobDeclaration.JE_TransportMode = "AIR";
			JobDeclaration.Validation.ValidateJE_RadioCallSign();
			AssertNoMessageErrors(targetInfo);

			JobDeclaration.JE_TransportMode = "SEA";
			ValidationTestHelper.AssertInvalidCodeMessageError(targetInfo, "XX", "LXBH");
		}

		public void TestCheckJE_DateAtOrigin()
		{
			var expectedMessage = "Date of Departure must be today or a future date.";

			AssertEntityValidation(JobDeclaration)
				.WhenProperty(x => x.JE_MessageType, Is.EqualTo(JPJobMessageTypeList.Codes.Export))
				.WhenProperty(x => x.JE_TransportMode, Is.EqualTo(TransportTypeList.Codes.Sea))
				.WhenProperty(x => x.JE_EntryStatus, Is.EqualTo(ZString.Empty))
				.WhenProperty(x => x.JE_DateAtOrigin, Is.EqualTo(ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.JE_DateAtOriginInfo, Has.MessageErrorContaining(expectedMessage));

			AssertEntityValidation(JobDeclaration)
				.WhenProperty(x => x.JE_DateAtOrigin, Is.EqualTo(ZDateTime.Today.AddDays(1)))
				.ShouldCheckThat(x => x.JE_DateAtOriginInfo, Has.NoMessageErrorContaining(expectedMessage));

			AssertEntityValidation(JobDeclaration)
				.WhenProperty(x => x.JE_EntryStatus, Is.EqualTo(EntryStatusList.Codes.CLR))
				.WhenProperty(x => x.JE_DateAtOrigin, Is.EqualTo(ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.JE_DateAtOriginInfo, Has.NoMessageErrorContaining(expectedMessage));

			AssertJE_DateAtOriginOnSendingMessage(JobDeclaration, JPProcedureCodeList.Codes.EDA, expectedMessage);
			AssertJE_DateAtOriginOnSendingMessage(JobDeclaration, JPProcedureCodeList.Codes.EDA01, expectedMessage);
			AssertJE_DateAtOriginOnSendingMessage(JobDeclaration, JPProcedureCodeList.Codes.ECR, expectedMessage);
			AssertJE_DateAtOriginOnSendingMessage(JobDeclaration, JPProcedureCodeList.Codes.ECR11, expectedMessage, false);
		}

		void AssertJE_DateAtOriginOnSendingMessage(JobDeclaration declaration, string procedureCode, string expectedMessage, bool hasErrorMessage = true)
		{
			var messageSendingContext = new MessageSendingContext() { ProcedureCode = procedureCode };
			using (declaration.SetCurrentMessageSendingContext(messageSendingContext))
			{
				AssertEntityValidation(declaration)
				.WhenProperty(x => x.JE_EntryStatus, Is.EqualTo(EntryStatusList.Codes.CLR))
				.WhenProperty(x => x.JE_DateAtOrigin, Is.EqualTo(ZDateTime.Today.AddDays(-1)))
				.ShouldCheckThat(x => x.JE_DateAtOriginInfo, hasErrorMessage ? Has.MessageErrorContaining(expectedMessage) : Has.NoMessageErrorContaining(expectedMessage));
			}
		}

		public void TestValidateAdditionalEntryNumber()
		{
			var expectedMessage = "The booking number's maximum length is 16.";

			var bookingReference = JobDeclaration.AdditionalReferenceNumbers.AddNew();
			bookingReference.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.COC;
			bookingReference.CE_EntryNum = "12345678901234567";
			AssertNoMessageError(bookingReference.CE_EntryNumInfo, expectedMessage);

			bookingReference.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.BKG;
			AssertHasMessageError(bookingReference.CE_EntryNumInfo, expectedMessage);

			bookingReference.CE_EntryNum = "1234567890123456";
			AssertNoMessageError(bookingReference.CE_EntryNumInfo, expectedMessage);
		}

		public void TestCheckJE_VoyageFlightNo()
		{
			var expectedMessage = "The flight number is incorrect. It should consist of two uppercase letters, followed by three digits, and optionally, an additional uppercase letter or digit.";

			JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var targetInfo = JobDeclaration.JE_VoyageFlightNoInfo;

			JobDeclaration.JE_VoyageFlightNo = "11ABCD";
			AssertNoMessageError(targetInfo, expectedMessage);

			JobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertHasMessageError(targetInfo, expectedMessage);

			JobDeclaration.JE_VoyageFlightNo = "ABA000";
			AssertHasMessageError(targetInfo, expectedMessage);

			JobDeclaration.JE_VoyageFlightNo = "A000";
			AssertHasMessageError(targetInfo, expectedMessage);

			JobDeclaration.JE_VoyageFlightNo = "AB00";
			AssertHasMessageError(targetInfo, expectedMessage);

			JobDeclaration.JE_VoyageFlightNo = "BA00011";
			AssertHasMessageError(targetInfo, expectedMessage);

			JobDeclaration.JE_VoyageFlightNo = "BA0001";
			AssertNoMessageError(targetInfo, expectedMessage);

			JobDeclaration.JE_VoyageFlightNo = "BA000A";
			AssertNoMessageError(targetInfo, expectedMessage);

			JobDeclaration.JE_VoyageFlightNo = "BA000";
			AssertNoMessageError(targetInfo, expectedMessage);
		}

		JobDeclaration JobDeclaration => jobDeclaration ??= Factory.New<JobDeclaration>();
		JobDeclaration jobDeclaration;
	}
}
