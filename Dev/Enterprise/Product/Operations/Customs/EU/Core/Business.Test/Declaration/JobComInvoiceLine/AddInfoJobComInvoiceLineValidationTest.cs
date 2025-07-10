using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs.EU;
using Moq;
using Moq.Protected;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	class AddInfoJobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckZG_TransNature_RuleC0627()
		{
			const string C0627ErrorMessageWhenSubStyleIsCOrF = "[C0627] This field must be empty in case of Declaration Sub Type C or F.";

			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.InvoiceLines.AddNew();
			invoiceLine1.JI_CEI = entryInstruction1.PK;

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.EnableRule(r => r.IsRuleC0627Active);
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				invoiceLine1.ZG_TransNature = "12";
				AssertHasMessageError("IsRuleC0627Active is true, Sub Style in C/F, ZG_TransNature of invoice header should be empty", invoiceLine1.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

				context.DisableRule(r => r.IsRuleC0627Active);
				invoiceLine1.AddInfoValidation.ValidateZG_TransNature();
				AssertNoMessageError("IsRuleC0627Active is false, Rule C0627 does not apply", invoiceLine1.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);

				context.EnableRule(r => r.IsRuleC0627Active);
				invoiceLine1.ZG_TransNature = ZString.Empty;
				AssertNoMessageError(invoiceLine1.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SupplementaryDeclarationForCodeBOrCodeE;
				invoiceLine1.ZG_TransNature = "12";
				AssertNoMessageError("Sub Style is not C/F", invoiceLine1.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);
			}

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.DisableRule(r => r.IsRuleC0627Active);
				entryInstruction1.CEI_SubStyle = EntrySubStyleList.Codes.SimplifiedDeclaration;
				invoiceLine1.ZG_TransNature = "12";
				AssertNoMessageError("Rule C0627 does not apply if disabled.", invoiceLine1.ZG_TransNatureInfo, C0627ErrorMessageWhenSubStyleIsCOrF);
			}
		}

		public void TestCheckZG_CountryOfDispatch()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_CountryOfDispatchInfo, "X1", Core.Constants.CountryCodes.Australia);
		}

		public void TestMessageErrorOnInvalidPrincipalsRepresentativeCity()
		{
			CombineAssertions(() =>
			{
				invoiceLine.ZG_RL_NKPrincipalsRepresentativeCity = "XXXXX";
				AssertHasMessageError("Invalid Code", invoiceLine.ZG_RL_NKPrincipalsRepresentativeCityInfo, ListValidation.InvalidCodeMessageError);

				invoiceLine.ZG_RL_NKPrincipalsRepresentativeCity = "GBLHR";
				AssertNoMessageError("Valid Code", invoiceLine.ZG_RL_NKPrincipalsRepresentativeCityInfo, ListValidation.InvalidCodeMessageError);
			});
		}

		public void TestCheckZG_ValuationMethod()
		{
			CombineAssertions(() =>
			{
				invoiceLine.JI_ValuationCode = ValuationMethodList.Codes._1;
				AssertNoMessageErrorContaining("Valid", invoiceLine.JI_ValuationCodeInfo, "list");

				invoiceLine.JI_ValuationCode = "x";
				AssertHasMessageErrorContaining("Invalid", invoiceLine.JI_ValuationCodeInfo, "list");

				invoiceLine.JI_ValuationCode = "";
				AssertNoMessageErrorContaining("Empty", invoiceLine.JI_ValuationCodeInfo, "list");
			});
		}

		public void TestCheckZG_SecondQuota_Import()
		{
			const string errorMessage = "Quota number should be 6 characters long.";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			CombineAssertions(() =>
			{
				AssertNoMessageError("ZG_SecondQuota empty", invoiceLine.ZG_SecondQuotaInfo, errorMessage);

				invoiceLine.ZG_SecondQuota = "123";
				AssertHasMessageError("ZG_SecondQuota < 6 chars", invoiceLine.ZG_SecondQuotaInfo, errorMessage);

				invoiceLine.ZG_SecondQuota = "123456";
				AssertNoMessageError("ZG_SecondQuota 6 chars", invoiceLine.ZG_SecondQuotaInfo, errorMessage);

				invoiceLine.ZG_SecondQuota = "1234567";
				AssertHasMessageError("ZG_SecondQuota > 6 chars", invoiceLine.ZG_SecondQuotaInfo, errorMessage);
			});
		}

		public void TestCheckZG_SecondQuota_Export()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.ZG_SecondQuota = "123";
			AssertNoNotifications("ZG_SecondQuota < 6 chars, No notification", invoiceLine.ZG_SecondQuotaInfo);
		}

		public void TestCheckZG_CountryOfDestination()
		{
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

				ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);
				var invoiceLineConfigurationMock1 = new Mock<InvoiceLineConfiguration>();
				invoiceLineConfigurationMock1.Protected()
					.Setup<ZBool>("CountryOfDestinationVisibleOnImportControlCore", ItExpr.IsAny<BusinessObject>())
					.Returns(true);

				using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock1.Object, declaration.GetDefaultDataGroupingCode()))
				{
					invoiceLine.ZG_CountryOfDestination = "Y~";
					AssertHasMessageError("Import (config set to true)", invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
				}

				ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);
				var invoiceLineConfigurationMock2 = new Mock<InvoiceLineConfiguration>();
				invoiceLineConfigurationMock2.Protected()
					.Setup<ZBool>("CountryOfDestinationVisibleOnImportControlCore", ItExpr.IsAny<BusinessObject>())
					.Returns(false);

				using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock2.Object, declaration.GetDefaultDataGroupingCode()))
				{
					invoiceLine.ZG_CountryOfDestination = "Y~";
					AssertNoMessageError("Import (config set to false)", invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
				}

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);
				var invoiceLineConfigurationMock3 = new Mock<InvoiceLineConfiguration>();
				invoiceLineConfigurationMock3.Protected()
					.Setup<ZBool>("CountryOfDestinationVisibleOnExportControlCore", ItExpr.IsAny<BusinessObject>())
					.Returns(true);

				using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock3.Object, declaration.GetDefaultDataGroupingCode()))
				{
					invoiceLine.ZG_CountryOfDestination = "X~";
					AssertHasMessageError("Export (config set to true)", invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
				}

				ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);
				var invoiceLineConfigurationMock4 = new Mock<InvoiceLineConfiguration>();
				invoiceLineConfigurationMock4.Protected()
					.Setup<ZBool>("CountryOfDestinationVisibleOnExportControlCore", ItExpr.IsAny<BusinessObject>())
					.Returns(false);

				using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(declaration.Factory, "GetNewInvoiceLineConfiguration", invoiceLineConfigurationMock4.Object, declaration.GetDefaultDataGroupingCode()))
				{
					invoiceLine.ZG_CountryOfDestination = "X~";
					AssertNoMessageError("Export (config set to false)", invoiceLine.ZG_CountryOfDestinationInfo, ListValidation.InvalidCodeMessageError);
				}
			});
		}

		public void TestZG_StatisticalValueReadOnly()
		{
			invoiceLine.ZG_StatisticalValueManualOverride = true;
			AssertEquals(false, invoiceLine.ZG_StatisticalValueInfo.ReadOnly);

			invoiceLine.ZG_StatisticalValueManualOverride = false;
			AssertEquals(true, invoiceLine.ZG_StatisticalValueInfo.ReadOnly);
		}

		public void TestZG_CommercialReference_Validation()
		{
			invoiceLine.ZG_CommercialReferenceInfo.ClearAllNotifications();
			invoiceLine.ZG_CommercialReference = "Commercial Ref";
			declaration.JE_OwnerRef = null;
			AssertNoMessageError(invoiceLine.ZG_CommercialReferenceInfo, "Commercial Reference Number at Invoice Line level should not be declared if [7] Declarant’s Ref is declared");

			invoiceLine.ZG_CommercialReferenceInfo.ClearAllNotifications();
			invoiceLine.ZG_CommercialReference = null;
			declaration.JE_OwnerRef = "Declarant Ref";
			AssertNoMessageError(invoiceLine.ZG_CommercialReferenceInfo, "Commercial Reference Number at Invoice Line level should not be declared if [7] Declarant’s Ref is declared");

			declaration.JE_OwnerRef = "Declarant Ref";
			invoiceLine.ZG_CommercialReference = "Commercial Ref";
			AssertHasMessageError(invoiceLine.ZG_CommercialReferenceInfo, "Commercial Reference Number at Invoice Line level should not be declared if [7] Declarant’s Ref is declared");
		}

		public void TestCheckZG_CountryOfSupply()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_CountryOfSupplyInfo, "X1", Core.Constants.CountryCodes.Australia);
		}

		public void TestCountryOfSupplyForCD5151()
		{
			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.EnableRule(r => r.IsRuleCD5151ActiveForZG_CountryOfSupply);
				declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();
				invoiceLine = invoice.JobComInvoiceLines.AddNew();
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				invoiceLine.JI_CEI = instruction.PK;
				instruction.CEI_Style = "I1";
				invoiceLine.JI_PrimaryPreference = "475";
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();

				var countryOfSupplyRequiredForPreferenceMsgError = $"[CD5151] {invoiceLine.ZG_CountryOfSupplyInfo.Description} is required when the first digit of {invoiceLine.JI_PrimaryPreferenceInfo.Description} is '1', '4' or '5' and is not equal to Pref. Orig.";
				AssertHasMessageError("CEI_Style = 'I1' and PrimaryPreference starts with 4", invoiceLine.ZG_CountryOfSupplyInfo, countryOfSupplyRequiredForPreferenceMsgError);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Netherlands;
				AssertNoMessageError(invoiceLine.ZG_CountryOfSupplyInfo, countryOfSupplyRequiredForPreferenceMsgError);

				var originNotEqualSupplyMessageError = $"[CD5151] {invoiceLine.ZG_CountryOfSupplyInfo.Description} must be equal to {invoiceLine.JI_CountryOfOriginInfo.Description} for Import and {instruction.CEI_StyleInfo.Description} of 'I1'.";
				invoiceLine.JI_PrimaryPreference = "2B";
				invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Sweden;
				invoiceLine.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertHasMessageError("CEI_Style = 'I1' and Country of Supply does not equal Country of Origin", invoiceLine.ZG_CountryOfSupplyInfo, originNotEqualSupplyMessageError);

				invoiceLine.ZG_CountryOfSupply = Core.Constants.CountryCodes.Sweden;
				AssertNoMessageError(invoiceLine.ZG_CountryOfSupplyInfo, originNotEqualSupplyMessageError);
			}
		}

		public void TestCountryOfSupply()
		{
			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.EnableRule(r => r.IsRuleCD5151ActiveForZG_CountryOfSupply);
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				instruction.CEI_Style = "H1";
				var invoiceHeader = declaration.Invoices.AddNew();
				var invoiceLine1 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine1.ZG_CountryOfSupply = CountryCodes.Finland;
				AssertNoMessageError(invoiceLine1.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);

				var invoiceLine2 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine2.ZG_CountryOfSupply = "";
				AssertHasMessageErrorContaining("Country of Supply is empty on this line", invoiceLine2.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);

				var invoiceLine3 = invoiceHeader.InvoiceLines.AddNew();
				invoiceLine3.ZG_CountryOfSupply = CountryCodes.Norway;
				AssertNoMessageError(invoiceLine3.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);

				instruction.CEI_Style = "IFD";
				invoiceLine2.AddInfoValidation.ValidateZG_CountryOfSupply();
				AssertNoMessageErrorContaining("Country of Supply is empty but is now not required on this line", invoiceLine2.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);

				context.DisableRule(r => r.IsRuleCD5151ActiveForZG_CountryOfSupply);
				invoiceLine1.ZG_CountryOfSupply = CountryCodes.Finland;
				AssertNoMessageError(invoiceLine1.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);

				invoiceLine2.ZG_CountryOfSupply = "";
				AssertNoMessageErrorContaining("Country of Supply is empty - not IAS validation", invoiceLine2.ZG_CountryOfSupplyInfo, MandatoryValidation.YouHaveNotEntered);
			}
		}

		public void TestCheckZG_CusNumber_ListValidation()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			helper.CreateNewOrGetExistingDataGrouping(currentCountry, parent: grouping);
			helper.CreateNewOrGetExistingCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "European Customs Inventory of Chemical Substance");
			helper.CreateCusCodeList(currentCountry, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0053963-1", "DESC1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(currentCountry, UniversalReferenceConstants.RefCusCodeListType.Code.Code_ECICS, "0053963-2", "DESC2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var invoiceLine = Factory.New<JobDeclaration>().Invoices.AddNew().InvoiceLines.AddNew();
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.ZG_CusNumberInfo, "0078304-2", "0053963-1");
		}

		public void TestCheckZG_CusNumber_Length()
		{
			const string errorMessage = "The number should be 9 characters.";
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			CombineAssertions(() =>
			{
				invoiceLine.AddInfoValidation.ValidateZG_CusNumber();
				AssertNoMessageError("Empty CUS Number should not have error message.", invoiceLine.ZG_CusNumberInfo, errorMessage);

				invoiceLine.ZG_CusNumber = "AA";
				AssertHasMessageError($"CUS Number with invalid length ({invoiceLine.ZG_CusNumber.Length}) should have error message.", invoiceLine.ZG_CusNumberInfo, errorMessage);

				invoiceLine.ZG_CusNumber = "AAAAAAAAA";
				AssertNoMessageError($"CUS Number with valid length ({invoiceLine.ZG_CusNumber.Length}) should not have error message.", invoiceLine.ZG_CusNumberInfo, errorMessage);
			});
		}

		public void TestCheckRuleC0002_Destination()
		{
			var messageForInvoiceLine = "[C0002] In case data entered in Invoice line level, all related lines must have a value in this field.";

			var declaration = Factory.New<JobDeclaration>();

			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			var instruction2 = declaration.CustomsEntryInstructions.AddNew();

			var invoiceheader1 = declaration.Invoices.AddNew();
			var invoiceheader2 = declaration.Invoices.AddNew();

			var invoiceLine1_1 = invoiceheader1.InvoiceLines.AddNew();
			var invoiceLine1_2 = invoiceheader1.InvoiceLines.AddNew();

			var invoiceLine2_1 = invoiceheader2.InvoiceLines.AddNew();
			var invoiceLine2_2 = invoiceheader2.InvoiceLines.AddNew();

			invoiceLine1_1.JI_CEI = instruction1.PK;
			invoiceLine1_2.JI_CEI = instruction2.PK;
			invoiceLine2_1.JI_CEI = instruction2.PK;
			invoiceLine2_2.JI_CEI = instruction1.PK;

			using (var context = new InvoiceLineValidationDeciderTestContext<IImportInvoiceLineValidationDecider>(declaration))
			{
				context.DisableRule(r => r.IsRuleC0002Active);

				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				invoiceLine1_1.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Disabled : invoiceLine1_1 has value for destination", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 1", invoiceLine1_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);

					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 2", invoiceLine1_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
				});

				context.EnableRule(r => r.IsRuleC0002Active);
				declaration.JE_GoodsDestination = Core.Constants.CountryCodes.France;
				invoiceLine1_1.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Active : invoiceLine1_1 has value for destination", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 1", invoiceLine1_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
					AssertHasMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);

					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 2", invoiceLine1_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
				});

				context.DisableRule(r => r.IsRuleC0002Active);
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Disable : invoiceLine1_1 has value for destination", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
				});

				context.EnableRule(r => r.IsRuleC0002Active);
				invoiceLine1_2.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
				invoiceLine2_2.ZG_CountryOfDestination = Core.Constants.CountryCodes.France;
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Active : invoiceLine1_1  and invoiceLine1_2  and invoiceLine2_2 have value for destination", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 1", invoiceLine1_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);

					AssertHasMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 2", invoiceLine1_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
				});

				context.DisableRule(r => r.IsRuleC0002Active);
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Disable : invoiceLine1_1  and invoiceLine1_2  and invoiceLine2_2 have value for destination", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
				});

				context.EnableRule(r => r.IsRuleC0002Active);
				invoiceLine2_1.ZG_CountryOfDestination = Core.Constants.CountryCodes.Spain;
				ValidateAllRuleC0002(invoiceheader1, invoiceheader2);
				CombineAssertions("Rule C0002 is Active : all invoice lines have value for destination", () =>
				{
					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 1", invoiceLine1_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 1", invoiceLine2_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);

					AssertNoMessageErrorContaining("InvoiceLine 1 of Instruction 2", invoiceLine2_1.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
					AssertNoMessageErrorContaining("InvoiceLine 2 of Instruction 2", invoiceLine1_2.ZG_CountryOfDestinationInfo, messageForInvoiceLine);
				});
			}
		}

		void ValidateAllRuleC0002(JobComInvoiceHeader invoice1, JobComInvoiceHeader invoice2)
		{
			invoice1.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.AddInfoValidation.ValidateAll());
			invoice2.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.AddInfoValidation.ValidateAll());
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}
		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
	}
}
