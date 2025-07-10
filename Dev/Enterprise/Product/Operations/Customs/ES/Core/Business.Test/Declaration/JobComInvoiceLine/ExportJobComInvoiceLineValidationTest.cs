using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest
	{
		public void TestCheckJI_CountryOfOriginInfo_Empty_ForInstructionEXS()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoNotifications("JI_CountryOfOriginInfo for instruction with EXS", invoiceLine.JI_CountryOfOriginInfo);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invoiceLine.JI_CountryOfOrigin = ZString.Empty;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertHasMessageErrorContaining("JI_CountryOfOriginInfo for instruction with EXS A", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_StateOrRegionOfOriginInfo_Empty_ForInstructionEXS()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invoiceLine.JI_StateOrRegionOfOrigin = ZString.Empty;
				invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
				AssertNoNotifications("JI_StateOrRegionOfOriginInfo for instruction with EXS", invoiceLine.JI_StateOrRegionOfOriginInfo);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				declaration.JE_GoodsOrigin = CountryCodes.Spain;
				invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
				invoiceLine.JI_StateOrRegionOfOrigin = ZString.Empty;
				invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
				AssertHasMessageErrorContaining("JI_StateOrRegionOfOriginInfo for instruction A", invoiceLine.JI_StateOrRegionOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_FormattedTariffInfo_Empty_ForInstructionEXS()
		{
			const string message = "Tariff may not be empty";
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invoiceLine.JI_FormattedTariff = ZString.Empty;
				invoiceLine.JI_Tariff = ZString.Empty;
				invoiceLine.JI_Description = "Description";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoNotifications("JI_FormattedTariffInfo for instruction with EXS", invoiceLine.JI_FormattedTariffInfo);
				AssertNoNotifications("JI_TariffInfo for instruction with EXS", invoiceLine.JI_TariffInfo);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invoiceLine.JI_FormattedTariff = ZString.Empty;
				invoiceLine.JI_Tariff = ZString.Empty;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining("JI_FormattedTariffInfo for instruction A", invoiceLine.JI_FormattedTariffInfo, message);
				AssertHasMessageErrorContaining("JI_TariffInfo for instruction A", invoiceLine.JI_TariffInfo, message);
			});
		}

		public void TestCheckJI_FormattedTariffInfo_MandatoryWithoutDescription_ForInstructionEXS()
		{
			const string message = "Tariff may not be empty";
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invoiceLine.JI_FormattedTariff = ZString.Empty;
				invoiceLine.JI_Tariff = ZString.Empty;
				invoiceLine.JI_Description = "Description";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoNotifications("JI_FormattedTariffInfo for instruction with EXS with description", invoiceLine.JI_FormattedTariffInfo);
				AssertNoNotifications("JI_TariffInfo for instruction with EXS with description", invoiceLine.JI_TariffInfo);

				invoiceLine.JI_FormattedTariff = ZString.Empty;
				invoiceLine.JI_Tariff = ZString.Empty;
				invoiceLine.JI_Description = ZString.Empty;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining("JI_FormattedTariffInfo for instruction EXS without description", invoiceLine.JI_FormattedTariffInfo, message);
				AssertHasMessageErrorContaining("JI_TariffInfo for instruction EXS without description", invoiceLine.JI_TariffInfo, message);
			});
		}

		public void TestCheckJI_DescriptionInfo_Empty_ForInstructionEXS()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invoiceLine.JI_Description = ZString.Empty;
				invoiceLine.Validation.ValidateJI_Description();
				AssertNoNotifications("JI_DescriptionInfo for instruction with EXS", invoiceLine.JI_DescriptionInfo);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invoiceLine.JI_Description = ZString.Empty;
				invoiceLine.Validation.ValidateJI_Description();
				AssertHasMessageErrorContaining("JI_DescriptionInfo for instruction A", invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_FormattedProcedureInfo_Empty_ForInstructionEXS()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.Spain, "", "12", "34", "001", "AB DESC 1", "EXP", group: "AAA");
			Factory.Save();

			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				entryInstruction.CEI_Style = "AAA";
				invoiceLine.JI_FormattedProcedure = ZString.Empty;
				invoiceLine.JI_Procedure = ZString.Empty;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoNotifications("JI_ProcedureInfo for instruction with EXS", invoiceLine.JI_ProcedureInfo);
				AssertNoNotifications("JI_FormattedProcedureInfo for instruction with EXS", invoiceLine.JI_FormattedProcedureInfo);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				entryInstruction.CEI_Style = "AAA";
				invoiceLine.JI_FormattedProcedure = ZString.Empty;
				invoiceLine.JI_Procedure = ZString.Empty;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageErrorContaining("JI_ProcedureInfo for instruction A", invoiceLine.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("JI_FormattedProcedureInfo for instruction A", invoiceLine.JI_FormattedProcedureInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckMessagePackagesAndCircumstanceEmpyOrE_ForInstructionEXS()
		{
			const string message = "This may contribute to a potential overall lack of packaging details";
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
				invoiceLine.Validation.ValidateAll();
				AssertNotEquals("MessagePackages for circumstance E with entry EXS aaaa", 0, invoiceLine.Notifications.Count(x => x.Message.Contains(message)));

				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments;
				invoiceLine.Validation.ValidateAll();
				AssertEquals("MessagePackages for circumstance A with entry EXS", 0, invoiceLine.Notifications.Count(x => x.Message.Contains(message)));

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.AuthorizedEconomicOperators;
				invoiceLine.Validation.ValidateAll();
				AssertNotEquals("MessagePackages for circumstance E with entry A", 0, invoiceLine.Notifications.Count(x => x.Message.Contains(message)));

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				declaration.ZG_SpecificCircumstanceIndicator = SpecificCircumstanceIndicator.Codes.PostalAndExpressConsignments;
				invoiceLine.Validation.ValidateAll();
				AssertNotEquals("MessagePackages for circumstance A with entry A", 0, invoiceLine.Notifications.Count(x => x.Message.Contains(message)));
			});
		}

		public void TestCheckMessageTariffNotBeEmptyWithGoodsDescriptionHasValue_ForInstructionEXS()
		{
			const string message = "Tariff may not be empty";
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invoiceLine.JI_FormattedTariff = ZString.Empty;
				invoiceLine.JI_Tariff = ZString.Empty;
				invoiceLine.JI_Description = "Description";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertNoNotifications("JI_TariffInfo without ji_tarif and with description for instruction EXS", invoiceLine.JI_TariffInfo);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invoiceLine.JI_FormattedTariff = ZString.Empty;
				invoiceLine.JI_Tariff = ZString.Empty;
				invoiceLine.JI_Description = ZString.Empty;
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining("JI_TariffInfo without ji_tarif and without description for instruction A", invoiceLine.JI_TariffInfo, message);

				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invoiceLine.JI_FormattedTariff = ZString.Empty;
				invoiceLine.JI_Tariff = ZString.Empty;
				invoiceLine.JI_Description = "Description";
				invoiceLine.Validation.ValidateJI_Tariff();
				AssertHasMessageErrorContaining("JI_TariffInfo without ji_tarif and with description for instruction A", invoiceLine.JI_TariffInfo, message);
			});
		}

		public void TestMaxAdditionalSupplyChainActor()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			for (int i = 0; i < 100; i++)
			{
				var cusSupplyChainActor = invoiceLine.CusSupplyChainActorReferences.AddNew();
				cusSupplyChainActor.CFR_Code = $"{i}";
			}

			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 99 Supply Chain Actor per line.");

				invoiceLine.CusSupplyChainActorReferences.Delete(invoiceLine.CusSupplyChainActorReferences.Last());
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 99 Supply Chain Actor per line.");
			});
		}

		public void TestMaxAdditionalDocumentsTRA()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			for (int i = 0; i < 100; i++)
			{
				var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo.CSI_Code = $"{i}";
				additionalInfo.CSI_SubType = "TRA";
			}

			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 99 TRA Additional Documents per line.");

				invoiceLine.AdditionalInfos.Remove(invoiceLine.AdditionalInfos.Last());
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 99 TRA Additional Documents per line.");
			});
		}

		public void TestOneAdditionalDocumentsTRA_ForEntryEXS()
		{
			const string message = "You have not entered a transport document for this line";
			GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateAll();
				AssertEquals("One Additional Documents TRA for entry Export, not exist", 0, invoiceLine.Notifications.Count(x => x.Message.Contains(message)));

				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);

				invoiceLine.Validation.ValidateAll();
				AssertEquals("One Additional Documents TRA for entry EXS, not exist", 1, invoiceLine.Notifications.Count(x => x.Message.Contains(message)));

				var additionalInfo = invoiceHeader.AdditionalInfos.AddNew();
				additionalInfo.CSI_Code = "XX";
				additionalInfo.CSI_SubType = "TRA";

				invoiceLine.Validation.ValidateAll();
				AssertEquals("One Additional Documents TRA for entry EXS, exist", 0, invoiceLine.Notifications.Count(x => x.Message.Contains(message)));
			});
		}

		public void TestMaxAdditionalDocumentsINF()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();

			for (int i = 0; i < 100; i++)
			{
				var additionalInfo = invoiceLine.AdditionalInfos.AddNew();
				additionalInfo.CSI_Code = $"{i}";
				additionalInfo.CSI_SubType = "INF";
			}

			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 99 INF Additional Documents per line.");

				invoiceLine.AdditionalInfos.Remove(invoiceLine.AdditionalInfos.Last());
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(invoiceLine, "Customs will not accept a declaration with more than 99 INF Additional Documents per line.");
			});
		}

		public void TestCheckJI_CEITypeB()
		{
			CombineAssertions(() =>
			{
				var expectedMessageError = "Simplified declarations (type B) require a national 9VA or 9PV concession in Box 37";

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				AssertNoMessageErrorContaining("Assert not mandatory national regime for empty JI_CEI", invoiceLine.JI_CEIInfo, expectedMessageError);

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_SubStyle = "B";
				invoiceLine.JI_CEI = entryInstruction.PK;

				invoiceLine.Validation.ValidateJI_CEI();
				AssertHasMessageErrorContaining("Assert mandatory national regime for type B JI_CEI", invoiceLine.JI_CEIInfo, expectedMessageError);

				invoiceLine.JI_FormattedProcedure = "23029VA";
				invoiceLine.Validation.ValidateJI_CEI();
				AssertNoMessageErrorContaining("Assert mandatory national regime for type B JI_CEI", invoiceLine.JI_CEIInfo, expectedMessageError);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				invoiceLine.JI_FormattedProcedure = ZString.Empty;

				invoiceLine.Validation.ValidateJI_CEI();
				AssertNoMessageErrorContaining("Assert not mandatory national regime for type B JI_CEI and no export declaration", invoiceLine.JI_CEIInfo, expectedMessageError);
			});
		}

		public void TestCheckJI_DescriptionExport()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
				invoiceLine.JI_Description = "Meat";
				AssertNoMessageErrorContaining("Assert mandatory JI_Description has value for Export", invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
				invoiceLine.JI_Description = ZString.Empty;
				AssertHasMessageErrorContaining("Assert mandatory JI_Description has no value for Export", invoiceLine.JI_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_DescriptionExport_Length()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.EXS);
				invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaafbbbb";
				invoiceLine.Validation.ValidateAll();
				AssertHasWarningContaining("Warns if JI_Description length is longer than 520 characters for EXS", invoiceLine.JI_DescriptionInfo, "When sending EXS declarations, the maximum length accepted for the description is 520, so it will be trimmed");

				invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaf";
				invoiceLine.Validation.ValidateAll();
				AssertNoWarningContaining("No warning if JI_Description length is not longer than 520 characters for EXS", invoiceLine.JI_DescriptionInfo, "When sending EXS declarations, the maximum length accepted for the description is 520, so it will be trimmed");

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes))
				{
					GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
					invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaf";
					invoiceLine.Validation.ValidateAll();
					AssertHasWarningContaining("Warns if JI_Description length is longer than 512 characters for Export AES", invoiceLine.JI_DescriptionInfo, "When sending export declarations, the maximum length accepted for the description is 512, so it will be trimmed");

					invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaf";
					invoiceLine.Validation.ValidateAll();
					AssertNoWarningContaining("No warning if JI_Description length is not longer than 512 characters for Export AES", invoiceLine.JI_DescriptionInfo, "When sending export declarations, the maximum length accepted for the description is 512, so it will be trimmed");
				}

				using (RegistryTemporarySetterHelper.SetESExportMessageVersion(EXPORTVersionNumberList.Codes.Aes11))
				{
					GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
					invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaf";
					invoiceLine.Validation.ValidateAll();
					AssertHasWarningContaining("Warns if JI_Description length is longer than 512 characters for Export AES 1.1", invoiceLine.JI_DescriptionInfo, "When sending export declarations, the maximum length accepted for the description is 512, so it will be trimmed");

					invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaf";
					invoiceLine.Validation.ValidateAll();
					AssertNoWarningContaining("No warning if JI_Description length is not longer than 512 characters for Export AES 1.1", invoiceLine.JI_DescriptionInfo, "When sending export declarations, the maximum length accepted for the description is 512, so it will be trimmed");
				}

				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaafbbb";
				invoiceLine.Validation.ValidateAll();
				AssertHasWarningContaining("Warns if JI_Description length is longer than 250 characters for T2LExpedition", invoiceLine.JI_DescriptionInfo, "When sending T2L Expedition declarations, the maximum length accepted for the description is 250, so it will be trimmed");

				invoiceLine.JI_Description = "goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaf";
				invoiceLine.Validation.ValidateAll();
				AssertNoWarningContaining("No warning if JI_Description length is not longer than 250 characters for T2LExpedition", invoiceLine.JI_DescriptionInfo, "When sending T2L Expedition declarations, the maximum length accepted for the description is 250, so it will be trimmed");
			});
		}

		public void TestCheckJI_StateOrRegionOfOrigin_JE_GoodsOrigin_ListValidation()
		{
			var helper = new ESUniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeListTerritory(CountryCodes.Spain, "01", "Test 1");

			GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			ValidationTestHelper.AssertInvalidCodeMessageError(invoiceLine.JI_StateOrRegionOfOriginInfo, "04", "01");
		}

		public void TestCheckJI_StateOrRegionOfOrigin_IsExportEntry_NotMandatory()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LClearanceExport);
			declaration.JE_GoodsOrigin = CountryCodes.Spain;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
				AssertNoMessageErrorContaining("T2L Entry", invoiceLine.JI_StateOrRegionOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
				AssertHasMessageErrorContaining("Normal Declaration", invoiceLine.JI_StateOrRegionOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				invoiceLine.Validation.ValidateJI_StateOrRegionOfOrigin();
				AssertNoMessageErrorContaining("T2C Entry", invoiceLine.JI_StateOrRegionOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_StateOrRegionOfOrigin_JE_GoodsOrigin_Mandatory()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			declaration.JE_GoodsOrigin = CountryCodes.Spain;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Germany;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_StateOrRegionOfOriginInfo);
		}

		public void TestCheckJI_StateOrRegionOfOrigin_JI_CountryOfOrigin_Mandatory()
		{
			GetDeclarationForTesting(GetDeclarationTypeForTesting.Export);
			declaration.JE_GoodsOrigin = CountryCodes.UnitedKingdom;
			invoiceLine.JI_CountryOfOrigin = CountryCodes.Spain;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.JI_StateOrRegionOfOriginInfo);
		}

		public void TestCheckAtLeastOneSupportingDocument_T2LExpedition()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageError("Entry SubStyle T2L", invoiceLine, OneSupportingDocumentMessageError);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageError("Entry SubStyle A", invoiceLine, OneSupportingDocumentMessageError);
			});
		}

		public void TestCheckAtLeastOneSupportingDocument_InvoiceLineDocument()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invoiceLine.SupportingDocuments.RemoveAndDeleteAll();
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageError("No Supporting Document", invoiceLine, OneSupportingDocumentMessageError);

				var docInvoiceLine = invoiceLine.SupportingDocuments.AddNew();
				docInvoiceLine.CSI_Code = "CC";
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageError("Has Supporting Document", invoiceLine, OneSupportingDocumentMessageError);
			});
		}

		public void TestCheckAtLeastOneSupportingDocument_InvoiceHeaderDocument()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invoiceHeader.SupportingDocuments.RemoveAndDeleteAll();
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageError("No Supporting Document", invoiceLine, OneSupportingDocumentMessageError);

				var docInvoiceHeader = invoiceHeader.SupportingDocuments.AddNew();
				docInvoiceHeader.CSI_Code = "BB";
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageError("Has Supporting Document", invoiceLine, OneSupportingDocumentMessageError);
			});
		}

		public void TestCheckAtLeastOneSupportingDocument_DeclarationDocument()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				declaration.SupportingDocuments.RemoveAndDeleteAll();
				invoiceLine.Validation.ValidateAll();
				AssertHasRowMessageError(invoiceLine, OneSupportingDocumentMessageError);

				var docDeclaration = declaration.SupportingDocuments.AddNew();
				docDeclaration.CSI_Code = "AA";
				invoiceLine.Validation.ValidateAll();
				AssertNoRowMessageError(invoiceLine, OneSupportingDocumentMessageError);
			});
		}

		public void TestCheckJI_Procedure_Mandatory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure(CountryCodes.Spain, "", "12", "34", "001", "AB DESC 1", "EXP", group: "AAA");
			Factory.Save();

			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				entryInstruction.CEI_Style = "AAA";
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("No mandatory message error for Export T2L", invoiceLine.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertHasMessageErrorContaining("Mandatory message error for Export non T2L/T2C", invoiceLine.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				invoiceLine.Validation.ValidateJI_Procedure();
				AssertNoMessageErrorContaining("No mandatory message error for Export T2C", invoiceLine.JI_ProcedureInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckJI_CountryOfOrigin()
		{
			CombineAssertions(() =>
			{
				GetDeclarationForTesting(GetDeclarationTypeForTesting.T2LExpedition);
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageErrorContaining("No mandatory message error for Export T2L", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertHasMessageErrorContaining("Mandatory message error for Export non T2L/T2C", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
				invoiceLine.Validation.ValidateJI_CountryOfOrigin();
				AssertNoMessageErrorContaining("No mandatory message error for Export T2C", invoiceLine.JI_CountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		enum GetDeclarationTypeForTesting { Export, T2LExpedition, T2LClearanceExport, EXS }

		void GetDeclarationForTesting(GetDeclarationTypeForTesting declarationType, ValidationModes validationMode = ValidationModes.None)
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			switch (declarationType)
			{
				case GetDeclarationTypeForTesting.Export:
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
					invoiceLine.JI_CEI = entryInstruction.PK;
					break;
				case GetDeclarationTypeForTesting.T2LExpedition:
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2L;
					invoiceLine.JI_CEI = entryInstruction.PK;
					break;
				case GetDeclarationTypeForTesting.T2LClearanceExport:
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.T2C;
					invoiceLine.JI_CEI = entryInstruction.PK;
					break;
				case GetDeclarationTypeForTesting.EXS:
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					entryInstruction.CEI_SubStyle = ExsEntrySubStyleList.Codes.EXS;
					invoiceLine.JI_CEI = entryInstruction.PK;
					break;
				default:
					break;
			}

			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.ValidationMode = validationMode;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		JobComInvoiceLine invoiceLine;

		const string OneSupportingDocumentMessageError = "You have not entered any supporting documents for this line";
	}
}
