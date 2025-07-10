using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	class AdditionalInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRuleC0612_EntryInstruction()
		{
			AssertRuleC0612Validation(docLinkedToEntryInstruction: true);
		}

		public void TestCheckRuleC0612_InvoiceHeader()
		{
			AssertRuleC0612Validation(docLinkedToEntryInstruction: false);
		}

		class TestImportAdditionalInfoValidationDecider : IAdditionalInfoValidationDecider
		{
			public bool IsBR2038Rule => true;

			public bool IsC0612Rule => true;
		}

		void AssertRuleC0612Validation(bool docLinkedToEntryInstruction)
		{
			var validationDecider = new TestImportAdditionalInfoValidationDecider();

			var instructionConfigurationMock = new Mock<InstructionConfiguration>();
			instructionConfigurationMock
				.Protected()
				.Setup<IAdditionalInfoValidationDecider>("GetAdditionalInfoValidationDeciderCore", ItExpr.IsAny<CusEntryInstruction>())
				.Returns(validationDecider);

			var invoiceHeaderConfigurationMock = new Mock<InvoiceHeaderConfiguration>();
			invoiceHeaderConfigurationMock
				.Protected()
				.Setup<IAdditionalInfoValidationDecider>("GetAdditionalInfoValidationDeciderCore", ItExpr.IsAny<JobComInvoiceHeader>())
				.Returns(validationDecider);

			var declarationConfigurationMock = new Mock<DeclarationConfiguration>();
			declarationConfigurationMock.CallBase = true;
			declarationConfigurationMock
				.Protected()
				.Setup<InstructionConfiguration>("GetNewInstructionConfiguration")
				.Returns(instructionConfigurationMock.Object);
			declarationConfigurationMock
				.Protected()
				.Setup<InvoiceHeaderConfiguration>("GetNewInvoiceHeaderConfiguration")
				.Returns(invoiceHeaderConfigurationMock.Object);

			var countryOrGrouping = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.ClearCachedValue<DeclarationConfiguration>($"DeclarationConfiguration_{countryOrGrouping}");

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject()).Returns(declarationConfigurationMock.Object);

			var declarationConfiguration = new KeyObjectHandleDictionaryObject
			{
				{ countryOrGrouping, objectHandleMock.Object }
			};

			using (ObjectFactory.Substitute(nameof(DeclarationConfiguration), declarationConfiguration))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AdditionalInfo additionalDocument = null;

				if (docLinkedToEntryInstruction)
				{
					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					additionalDocument = entryInstruction.AdditionalInfos.AddNew();
				}
				else
				{
					var invoiceHeader = declaration.Invoices.AddNew();
					additionalDocument = invoiceHeader.AdditionalInfos.AddNew();
				}

				var invalidCodeTypes = new string[] {
					CusSupportingInfoTypes.C057,
					CusSupportingInfoTypes.C079,
					CusSupportingInfoTypes.C082,
					CusSupportingInfoTypes.C085,
					CusSupportingInfoTypes.C640,
					CusSupportingInfoTypes.C644,
					CusSupportingInfoTypes.C678,
					CusSupportingInfoTypes.E013,
					CusSupportingInfoTypes.L100,
					CusSupportingInfoTypes.N853,
					CusSupportingInfoTypes.Y120,
					CusSupportingInfoTypes.Y121,
					CusSupportingInfoTypes.Y123,
					CusSupportingInfoTypes.Y124,
					CusSupportingInfoTypes.Y125,
					CusSupportingInfoTypes.Y951,
					CusSupportingInfoTypes.Y986
				};

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
				foreach (var invalidCode in invalidCodeTypes)
				{
					additionalDocument.CSI_Code = invalidCode;
					AssertHasMessageError(
						$"When CSI_Code = {additionalDocument.CSI_Code} and CSI_SubType = {additionalDocument.CSI_SubType}",
						additionalDocument.CSI_CodeInfo,
						$"[C0612] A CERTEX certificate of type {additionalDocument.CSI_Code} can only be declared on the invoice line level.");
				}

				additionalDocument.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalInformation;
				foreach (var invalidCode in invalidCodeTypes)
				{
					additionalDocument.CSI_Code = invalidCode;
					AssertNoMessageError(
						$"When CSI_Code = {additionalDocument.CSI_Code} and CSI_SubType = {additionalDocument.CSI_SubType}",
						additionalDocument.CSI_CodeInfo,
						$"[C0612] A CERTEX certificate of type {additionalDocument.CSI_Code} can only be declared on the invoice line level.");
				}
			}
		}

		public void TestCheckRuleBR2038_EntryInstruction()
		{
			var additionalInfo = declaration
				.CustomsEntryInstructions.AddNew()
				.AdditionalInfos.AddNew();
			AssertRuleBR2038Validation(additionalInfo);
		}

		public void TestCheckRuleBR2038_InvoiceHeader()
		{
			var additionalInfo = declaration
				.Invoices.AddNew()
				.AdditionalInfos.AddNew();
			AssertRuleBR2038Validation(additionalInfo);
		}

		public void TestCheckRuleBR2038_InvoiceLine()
		{
			var additionalInfo = declaration
				.Invoices.AddNew()
				.InvoiceLines.AddNew()
				.AdditionalInfos.AddNew();
			AssertRuleBR2038Validation(additionalInfo);
		}

		void AssertRuleBR2038Validation(AdditionalInfo additionalInfo) => AssertRuleBR2038Validation(additionalInfo, additionalInfo.CSI_ReferenceNumberInfo);
		void AssertRuleBR2038Validation(AdditionalInfo additionalInfo, ZPropertyInfo propertyInfo)
		{
			const string message = "Invalid check digit.";
			AssertNotNull("[PRE-CONDITION] additionalInfo should not be null", additionalInfo);
			AssertNotNull("[PRE-CONDITION] Parent should implement IAdditionalInfosProviderWithValidationDecider", additionalInfo.GetValidationProvider());
			additionalInfo.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CombineAssertions($"When asserting validation rule BR2038 of {additionalInfo.Parent.GetType().Name}", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(additionalInfo.Declaration, configurationValue: true))
				{
					AssertNotNull("[PRE-CONDITION] ValidationDecider should not be null", additionalInfo.GetValidationDecider());
					additionalInfo.CSI_Type = Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo;
					additionalInfo.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
					additionalInfo.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N740;
					additionalInfo.CSI_ReferenceNumber = "73412345674";
					AssertNoWarningContaining("IsUCC6 and CSI_Code is 'N470' and invalid check digit", propertyInfo, message);

					additionalInfo.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N741;
					additionalInfo.CSI_ReferenceNumber = "73412345674";
					AssertHasWarningContaining("IsUCC6 and CSI_Code is 'N471' and invalid check digit", propertyInfo, message);

					additionalInfo.CSI_ReferenceNumber = "73412345675";
					AssertNoWarningContaining("IsUCC6 and CSI_Code is 'N471' and valid check digit", propertyInfo, message);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(additionalInfo.Declaration, configurationValue: false))
				{
					AssertNull("[PRE-CONDITION] ValidationDecider should be null", additionalInfo.GetValidationDecider());
					additionalInfo.CSI_Code = UniversalReferenceConstants.SupportingDocumentTypes.N741;
					additionalInfo.CSI_ReferenceNumber = "73412345674";
					AssertNoWarningContaining("!IsUCC6 and CSI_Code is 'N471' and invalid check digit", propertyInfo, message);
				}
			});
		}

		public void TestCode_Mandatory()
		{
			var (additionalInfo, additionalInfoValidationMock) = SetUpMock();
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(additionalInfo.CSI_CodeInfo);
				additionalInfoValidationMock.Protected().Setup<bool>("IsCodeMandatory").Returns(false);
				additionalInfo.CSI_Code = ZString.Empty;
				additionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageErrorContaining("Code is not mandatory", additionalInfo.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCode_List()
		{
			var (additionalInfo, additionalInfoValidationMock) = SetUpMock();
			CusEntryHeaderTestHelper.CreateAdditionalInfos(Factory);
			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertInvalidCodeMessageError(additionalInfo.CSI_CodeInfo, "^_^", "9002");

				additionalInfoValidationMock.Protected().Setup<bool>("ShouldCodeBeInTheList").Returns(false);
				additionalInfo.CSI_Code = "^_^";
				additionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageError("Code is not expected to be in the list", additionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);

				additionalInfoValidationMock.Protected().Setup<bool>("IsCodeEnabled").Returns(false);
				additionalInfo.CSI_Code = "^_^";
				additionalInfo.Validation.ValidateCSI_Code();
				AssertNoMessageError("Code is not enabled", additionalInfo.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestIsOtherFieldsEnabled()
		{
			var (additionalInfo, additionalInfoValidationMock) = SetUpMock();
			CombineAssertions(() =>
			{
				additionalInfo.CSI_Status = ZString.Empty;
				additionalInfo.Validation.ValidateCSI_Status();
				AssertHasMessageErrorContaining("Other fields is enabled, but not entered.", additionalInfo.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
				additionalInfoValidationMock.Protected().Setup<bool>("IsOtherFieldsEnabled").Returns(false);
				additionalInfo.Validation.ValidateCSI_Status();
				AssertNoMessageErrorContaining("Other fields is not enabled, hence no message error even not entered.", additionalInfo.CSI_StatusInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		(AdditionalInfo additionalInfo, Mock<AdditionalInfoValidation> additionalInfoValidationMock) SetUpMock()
		{
			var additionalInfoMock = Factory.NewMoq<AdditionalInfo>();
			var additionalInfo = additionalInfoMock.Object;
			additionalInfo.CSI_ParentID = declaration.PK;
			additionalInfo.CSI_ParentTableCode = declaration.TablePrefix;
			var additionalInfoValidationMock = new Mock<AdditionalInfoValidation>(additionalInfo);
			additionalInfoValidationMock.CallBase = true;
			additionalInfoMock.Protected().Setup<CusSupportingInfoValidation>("GetNewValidation")
				.Returns(additionalInfoValidationMock.Object);
			return (additionalInfo, additionalInfoValidationMock);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
		}

		JobDeclaration declaration;
	}
}
