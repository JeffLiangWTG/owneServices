using System.Collections;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	class CusAuthorizationUsageValidationTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestValidateRuleR0010()
		{
			var duplicateAuthorizationMessage = "[R0010] Value can’t be entered in both Entry Instruction and Invoice lines.";
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			var testInstruction1 = testDeclaration.CustomsEntryInstructions.AddNew();
			testInstruction1.CEI_JE = testDeclaration.PK;
			var testInstructionAuthorization1 = testInstruction1.CusAuthorizationUsages.AddNew();
			var testInstructionAuthorization2 = testInstruction1.CusAuthorizationUsages.AddNew();
			var testInvoice = testDeclaration.Invoices.AddNew();
			var testInvoiceLine1 = testInvoice.JobComInvoiceLines.AddNew();
			var testInvoiceLine2 = testInvoice.JobComInvoiceLines.AddNew();
			testInvoiceLine1.JI_CEI = testInstruction1.PK;
			testInvoiceLine2.JI_CEI = testInstruction1.PK;
			var testInvoiceLineAuthorization1 = testInvoiceLine1.CusAuthorizationUsages.AddNew();
			var testInvoiceLineAuthorization2 = testInvoiceLine1.CusAuthorizationUsages.AddNew();
			var testInvoiceLineAuthorization3 = testInvoiceLine2.CusAuthorizationUsages.AddNew();

			using var context = new CusAuthorizationUsageValidationDeciderTestContext(testDeclaration, isUCC6: true);
			context.EnableRule(x => x.IsRuleR0010Active);

			CombineAssertions("R0010 : declaration is UCC6 and rule is activated", () =>
			{
				testInstructionAuthorization1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				testInstructionAuthorization1.EffectiveReferenceNumber = "001";

				testInvoiceLineAuthorization1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				testInvoiceLineAuthorization1.EffectiveReferenceNumber = "001";
				testInvoiceLineAuthorization1.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(testInvoiceLineAuthorization1, duplicateAuthorizationMessage);

				testInvoiceLineAuthorization1.EffectiveReferenceNumber = "002";
				testInvoiceLineAuthorization1.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(testInvoiceLineAuthorization1, duplicateAuthorizationMessage);

				testInstructionAuthorization1.EffectiveReferenceNumber = "002";
				testInstructionAuthorization1.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(testInstructionAuthorization1, duplicateAuthorizationMessage);

				testInstructionAuthorization1.RemoveRowMessageError(duplicateAuthorizationMessage);

				testInvoiceLineAuthorization3.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				testInvoiceLineAuthorization3.EffectiveReferenceNumber = "002";
				testInstructionAuthorization1.Validation.ValidateAll();

				NUnit.Framework.Assert.That(testInstructionAuthorization1.GetMessageErrors().Count(), NUnit.Framework.Is.EqualTo(1));

				testInvoiceLineAuthorization2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				testInvoiceLineAuthorization2.EffectiveReferenceNumber = "001";
				testInvoiceLineAuthorization2.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(testInvoiceLineAuthorization2, duplicateAuthorizationMessage);

				testInstructionAuthorization2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				testInstructionAuthorization2.EffectiveReferenceNumber = "001";
				testInstructionAuthorization2.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(testInstructionAuthorization2, duplicateAuthorizationMessage);

				testInstructionAuthorization2.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer;
				testInstructionAuthorization2.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(testInstructionAuthorization2, duplicateAuthorizationMessage);

				testInvoiceLineAuthorization3.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer;
				testInvoiceLineAuthorization3.EffectiveReferenceNumber = "001";
				testInvoiceLineAuthorization3.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(testInvoiceLineAuthorization3, duplicateAuthorizationMessage);

				testInvoiceLineAuthorization3.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				testInvoiceLineAuthorization3.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(testInvoiceLineAuthorization3, duplicateAuthorizationMessage);
			});

			context.DisableRule(x => x.IsRuleR0010Active);

			CombineAssertions("R0010 : declaration is UCC6 and rule is deactivated", () =>
			{
				testInstructionAuthorization1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				testInstructionAuthorization1.EffectiveReferenceNumber = "001";

				testInvoiceLineAuthorization1.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				testInvoiceLineAuthorization1.EffectiveReferenceNumber = "001";
				testInvoiceLineAuthorization1.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(testInvoiceLineAuthorization1, duplicateAuthorizationMessage);

				testInvoiceLineAuthorization1.EffectiveReferenceNumber = "002";
				testInvoiceLineAuthorization1.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(testInvoiceLineAuthorization1, duplicateAuthorizationMessage);
			});
		}

		public void TestCheckAGC_Code()
		{
			var propertyInfo = cusAuthorizationUsage.AGC_CodeInfo;

			CombineAssertions(() =>
			{
				ValidationTestHelper.AssertErrorIfNotEntered(propertyInfo);
				ValidationTestHelper.AssertInvalidCodeMessageError(propertyInfo, "XXX", CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir);
			});
		}

		public void TestValidateRuleR0675() => CombineAssertions(() =>
		{
			const string expectedErrorMessage = "[R0675] Customs office of Presentation is required for centralized clearance (denoted via Authorization Type CCL).";
			var propertyInfo = cusAuthorizationUsage.AGC_CodeInfo;

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var testContext = new CusAuthorizationUsageValidationDeciderTestContext(declaration, isUCC6: true))
			{
				testContext.EnableRule(x => x.IsRuleR0675Active);

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
				declaration.CustomsOffices.RemoveAndDeleteAll();
				AssertRule(mustHaveErrorMessage: true);

				var office = declaration.CustomsOffices.AddNew();
				office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
				AssertRule(mustHaveErrorMessage: false);

				office.CY_Code = EuOfficeCodesTypes.Codes.CustomsOfficeForTemporaryStorage;
				AssertRule(mustHaveErrorMessage: true);
				office.CY_Code = EuOfficeCodesTypes.Codes.OfficeOfPresentation;
				declaration.CustomsOffices.RemoveAndDeleteAll();

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				AssertRule(mustHaveErrorMessage: false);
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;

				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertRule(mustHaveErrorMessage: false);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

				testContext.DisableRule(x => x.IsRuleR0675Active);
				AssertRule(mustHaveErrorMessage: false);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var testContext = new CusAuthorizationUsageValidationDeciderTestContext(declaration, isUCC6: false))
			{
				testContext.EnableRule(x => x.IsRuleR0675Active);
				AssertRule(mustHaveErrorMessage: false);
			}

			void AssertRule(bool mustHaveErrorMessage)
			{
				var ruleIsActive = cusAuthorizationUsage.Parent is ICusAuthorizationUsageProviderWithValidationDecider { ValidationDecider.IsRuleR0675Active: true };
				var hasOfficeOfPresentation = declaration.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfPresentation);
				var description =
					$"IsUCC6: {declaration.IsUCC6}\r\n" +
					$"IsExport: {declaration.IsExport}\r\n" +
					$"R0675 is active: {ruleIsActive}\r\n" +
					$"AGC_Code: {cusAuthorizationUsage.AGC_Code}\r\n" +
					$"Has office of presentation: {hasOfficeOfPresentation}";
				cusAuthorizationUsage.Validation.ValidateAGC_Code();
				if (mustHaveErrorMessage)
				{
					AssertHasMessageError(description, propertyInfo, expectedErrorMessage);
				}
				else
				{
					AssertNoMessageError(description, propertyInfo, expectedErrorMessage);
				}
			}
		});

		[ExpectNoExceptions]
		public void TestCheckAGC_Number_Mandatory_NotEnableAdHoc()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			var propertyInfo = cusAuthorizationUsage.AGC_NumberInfo;
			cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = false;

			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EnableAdHoc, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Prerequisite: ");
				ValidationTestHelper.AssertErrorIfNotEntered(cusAuthorizationUsage.AGC_NumberInfo);

				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = true;
				cusAuthorizationUsage.AGC_Number = ZString.Empty;
				AssertNoNotifications("UseEffectiveReferenceNumber is enabled", propertyInfo);
			});
		}

		[ExpectNoExceptions]
		public void TestCheckAGC_Number_Mandatory_EnableAdHoc()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected()
				.Setup<bool>("EnableAdHocCore")
				.Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EnableAdHoc, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Prerequisite: ");
				var message = "Either Authorization or Number must have a value.";
				var propertyInfo = cusAuthorizationUsage.AGC_NumberInfo;
				var authorizationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();

				cusAuthorizationUsage.AGC_Number = "001";
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_CPH_Authorization.IsEmpty, NUnit.Framework.Is.True, "Prerequis : ");
				AssertNoErrorContaining("AGC_Number have a value => no error.", propertyInfo, message);

				cusAuthorizationUsage.AGC_Number = ZString.Empty;
				AssertHasErrorContaining("AGC_Number and AGC_CPH_Authorization are empty => error.", propertyInfo, message);

				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = true;
				cusAuthorizationUsage.Validation.ValidateAGC_Number();
				AssertNoErrorContaining("UseEffectiveReferenceNumber is enabled", propertyInfo, message);

				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = false;
				cusAuthorizationUsage.AGC_CPH_Authorization = authorizationHeader.PK;
				AssertNoErrorContaining("AGC_CPH_Authorization have a value => no error.", propertyInfo, message);
			}
		}

		[ExpectNoExceptions]
		public void TestCheckAGC_CPH_Authorization_NotEnableAdHoc()
		{
			var propertyInfo = cusAuthorizationUsage.AGC_CPH_AuthorizationInfo;

			var authorizationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EnableAdHoc, NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "Prerequisite: ");
				cusAuthorizationUsage.AGC_CPH_Authorization = authorizationHeader.PK;
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number.IsEmpty, NUnit.Framework.Is.True, "Prerequis : ");
				AssertNoNotifications("AGC_CPH_Authorization have a value => no error.", propertyInfo);

				cusAuthorizationUsage.AGC_CPH_Authorization = ZGuid.Empty;
				AssertNoNotifications("AGC_CPH_Authorization is Empty => no error.", propertyInfo);

				cusAuthorizationUsage.AGC_Number = "001";
				AssertNoNotifications("AGC_Number have a value => no error.", propertyInfo);
			});
		}

		[ExpectNoExceptions]
		public void TestCheckAGC_CPH_Authorization_EnableAdHoc()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			var message = "Either Authorization or Number must have a value.";
			var propertyInfo = cusAuthorizationUsage.AGC_CPH_AuthorizationInfo;

			var cusAuthorisationHeaderProviderMock = new Mock<CusAuthorisationHeaderProvider>(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			cusAuthorisationHeaderProviderMock.CallBase = true;

			cusAuthorisationHeaderProviderMock.Protected()
				.Setup<bool>("EnableAdHocCore")
				.Returns(true);

			var objectHandleMock = new Mock<ObjectHandle>();
			objectHandleMock.Setup(m => m.GetObject(It.IsAny<object>())).Returns(cusAuthorisationHeaderProviderMock.Object);

			var config = new Hashtable();
			config.Add(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, objectHandleMock.Object);

			using (ObjectFactory.Substitute("CusAuthorisationHeaderProviders", config))
			{
				var authorizationHeader = Factory.NewWithValidTestData<CusAuthorisationHeader>();
				NUnit.Framework.Assert.That(cusAuthorizationUsage.EnableAdHoc, NUnit.Framework.Is.EqualTo(true).Using(CustomComparers.TypeComparison), "Prerequisite: ");

				cusAuthorizationUsage.AGC_CPH_Authorization = authorizationHeader.PK;
				NUnit.Framework.Assert.That(cusAuthorizationUsage.AGC_Number.IsEmpty, NUnit.Framework.Is.True, "Prerequis : ");
				AssertNoErrorContaining("AGC_CPH_Authorization have a value => no error.", propertyInfo, message);

				cusAuthorizationUsage.AGC_CPH_Authorization = ZGuid.Empty;
				AssertHasErrorContaining("AGC_Number and AGC_CPH_Authorization are empty => error.", propertyInfo, message);

				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = true;
				cusAuthorizationUsage.Validation.ValidateAGC_CPH_Authorization();
				AssertNoErrorContaining("UseEffectiveReferenceNumber is enabled", propertyInfo, message);

				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = false;
				cusAuthorizationUsage.AGC_Number = "001";
				AssertNoErrorContaining("AGC_Number have a value => no error.", propertyInfo, message);
			}
		}

		public void TestCheckAGC_Number_Valid()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "OH2";
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var cusAuthorizationUsageForTest = Factory.New<CusAuthorizationUsageForTest>();
			var propertyInfo = cusAuthorizationUsageForTest.AGC_NumberInfo;

			CombineAssertions(() =>
			{
				cusAuthorizationUsageForTest.UseEffectiveReferenceNumberOverriden = true;
				cusAuthorizationUsageForTest.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				cusAuthorizationUsageForTest.AGC_OH_Owner = orgHeader2.PK;
				cusAuthorizationUsageForTest.AGC_Number = "001";
				AssertNoWarnings("UseEffectiveReferenceNumber is enbaled", propertyInfo);

				cusAuthorizationUsageForTest.UseEffectiveReferenceNumberOverriden = false;
				cusAuthorizationUsageForTest.Validation.ValidateAGC_Number();
				AssertHasWarningContaining("ACT + 001 + OH2", propertyInfo, "Authorization number: 001 doesn't exist for Code: ACT, Owner: OH2");

				cusAuthorizationUsageForTest.AGC_OH_Owner = orgHeader.PK;
				cusAuthorizationUsageForTest.AGC_Number = "XXX";
				AssertHasWarningContaining("ACT + XXX + OH1", propertyInfo, "Authorization number: XXX doesn't exist for Code: ACT, Owner: OH1");

				cusAuthorizationUsageForTest.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				cusAuthorizationUsageForTest.AGC_Number = "001";
				AssertHasWarningContaining("ACE + 001 + OH1", propertyInfo, "Authorization number: 001 doesn't exist for Code: ACE, Owner: OH1");

				cusAuthorizationUsageForTest.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				cusAuthorizationUsageForTest.Validation.ValidateAGC_Number();
				AssertNoWarnings("ACT + 001 + OH1", propertyInfo);
			});

			var invoiceLineConfigurationMock = new Mock<InstructionConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("UseEoriForAuthorisationReferenceCore").Returns(ZBool.True);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInstructionConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				var declaration = Factory.New<JobDeclaration>();
				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

				cusAuthorizationUsage.AGC_Code = "1";
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
				cusAuthorizationUsage.AGC_Number = "Any EORI";
				AssertNoWarnings("No warning for 'unknown' authorisation when set to default EORIs", cusAuthorizationUsage.AGC_NumberInfo);
			}
		}

		[ExpectNoExceptions]
		public void TestParentType()
		{
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			NUnit.Framework.Assert.That(authorizationHeader.Validation.Parent, NUnit.Framework.Is.TypeOf(typeof(CusAuthorisationHeader)));
		}

		public void TestCheckAGC_OH_Owner()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_OH_Owner = OrgHeader.UnmatchedOrganisationPK;
				AssertHasWarningContaining("Unmatched", cusAuthorizationUsage.AGC_OH_OwnerInfo, "Unmatched Owner has been selected");

				cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
				AssertNoWarnings("Valid Owner", cusAuthorizationUsage.AGC_OH_OwnerInfo);
			});
		}

		public void TestMaxNumberDiffCusAuthorizationUsage()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

			CheckMaxNumberDiffCusAuthorizationUsage(entryInstruction.CusAuthorizationUsages);
		}

		public void TestMaxNumberDiffCusAuthorizationUsageItem()
		{
			var testDec = Factory.New<JobDeclaration>();
			var invoiceLine = testDec.Invoices.AddNew().InvoiceLines.AddNew();

			CheckMaxNumberDiffCusAuthorizationUsage(invoiceLine.CusAuthorizationUsages);
		}

		public void TestCheckEffectiveReferenceNumber_Entered()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			var propertyInfo = cusAuthorizationUsage.EffectiveReferenceNumberInfo;
			cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = false;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.Validation.ValidateEffectiveReferenceNumber();
				AssertNoErrorContaining("UseEffectiveReferenceNumber disabled - no value", propertyInfo, MandatoryValidation.MustBeEntered);

				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = true;
				cusAuthorizationUsage.Validation.ValidateEffectiveReferenceNumber();
				AssertHasErrorContaining("UseEffectiveReferenceNumber enabled - no value", propertyInfo, MandatoryValidation.MustBeEntered);

				cusAuthorizationUsage.EffectiveReferenceNumber = "123";
				AssertNoErrorContaining("have value", propertyInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckEffectiveReferenceNumber_DifferentOwner()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "OH1";
			var orgHeader2 = Factory.New<OrgHeader>();
			orgHeader2.OH_Code = "OH2";
			var authorizationHeader = Factory.New<CusAuthorisationHeader>();
			authorizationHeader.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
			authorizationHeader.CPH_Number = "001";
			authorizationHeader.CPH_OH_PermitHolder = orgHeader.PK;

			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			var propertyInfo = cusAuthorizationUsage.EffectiveReferenceNumberInfo;
			cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = false;

			CombineAssertions(() =>
			{
				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader2.PK;
				cusAuthorizationUsage.EffectiveReferenceNumber = "001";
				AssertNoWarnings("UseEffectiveReferenceNumber is disabled", propertyInfo);

				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = true;
				cusAuthorizationUsage.Validation.ValidateEffectiveReferenceNumber();
				AssertHasWarningContaining("ACT + 001 + OH2", propertyInfo, "Authorization number: 001 doesn't exist for Code: ACT, Owner: OH2");

				cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
				cusAuthorizationUsage.EffectiveReferenceNumber = "XXX";
				AssertHasWarningContaining("ACT + XXX + OH1", propertyInfo, "Authorization number: XXX doesn't exist for Code: ACT, Owner: OH1");

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
				cusAuthorizationUsage.EffectiveReferenceNumber = "001";
				AssertHasWarningContaining("ACE + 001 + OH1", propertyInfo, "Authorization number: 001 doesn't exist for Code: ACE, Owner: OH1");

				cusAuthorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir;
				cusAuthorizationUsage.Validation.ValidateEffectiveReferenceNumber();
				AssertNoWarnings("ACT + 001 + OH1", propertyInfo);
			});
		}

		[ExpectNoExceptions]
		public void TestUseEffectiveReferenceNumberValidation()
		{
			var cusAuthorizationUsage = Factory.New<CusAuthorizationUsageForTest>();
			var validation = new CusAuthorizationUsageValidationForTest(cusAuthorizationUsage);
			CombineAssertions(() =>
			{
				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = false;
				NUnit.Framework.Assert.That(validation.UseEffectiveReferenceNumberValidationExposed, NUnit.Framework.Is.EqualTo(false), "In EU when UseEffectiveReferenceNumber is disabled UseEffectiveReferenceNumberValidation should also be disabled");

				cusAuthorizationUsage.UseEffectiveReferenceNumberOverriden = true;
				NUnit.Framework.Assert.That(validation.UseEffectiveReferenceNumberValidationExposed, NUnit.Framework.Is.EqualTo(true), "In EU when UseEffectiveReferenceNumber is enabled UseEffectiveReferenceNumberValidation should also be enabled");
			});
		}

		protected void CheckMaxNumberDiffCusAuthorizationUsage<T>(T authorisations) where T : IBusinessObjectCollection
		{
			var validationMessage = "Only 9 authorizations are allowed.";

			CusAuthorizationUsage authorisation = null;

			for (int i = 1; i <= 9; i++)
			{
				authorisation = (CusAuthorizationUsage)authorisations.AddNew();
				authorisation.AGC_Code = CusAuthorizationHeaderTypeList.Codes.TemporaryAdmission;
			}
			AssertNoRowError(authorisation, validationMessage);

			authorisation = (CusAuthorizationUsage)authorisations.AddNew();
			authorisation.AGC_Code = CusAuthorizationHeaderTypeList.Codes.AuthorizedIssuer;
			AssertHasRowError(authorisation, validationMessage);
		}

		protected override void SetUp()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, null, grouping);
			helper.CreateCusCodeType("AUTH", "Authorisation");
			helper.CreateCusCodeList("EUN", "AUTH", "ACT", "ACT", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			Factory.Save();

			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
		}

		JobDeclaration declaration;
		Business.Declaration.CusEntryInstruction entryInstruction;
		CusAuthorizationUsage cusAuthorizationUsage;

		class CusAuthorizationUsageValidationForTest : CusAuthorizationUsageValidation
		{
			public CusAuthorizationUsageValidationForTest(AutoCusAuthorizationUsage parent) : base(parent)
			{
			}

			public bool UseEffectiveReferenceNumberValidationExposed => UseEffectiveReferenceNumberValidation;
		}
	}
}
