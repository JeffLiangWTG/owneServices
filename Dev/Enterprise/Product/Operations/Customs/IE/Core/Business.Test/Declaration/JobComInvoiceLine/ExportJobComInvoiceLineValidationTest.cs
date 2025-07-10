using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IE.Business.Declaration.Testing
{
	class ExportJobComInvoiceLineValidationTest : JobComInvoiceLineValidationTest<ExportJobComInvoiceLineValidation>
	{
		public void TestZG_CountryOfDestination()
		{
			(_, var invoiceLine, _, _) = SetupData();
			invoiceLine.ZG_CountryOfDestination = string.Empty;
			AssertHasMessageErrorContaining("Country of Destination is mandatory", invoiceLine.ZG_CountryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.ZG_CountryOfDestination = "IE";
			AssertNoMessageErrorContaining("Country of Destination is populated", invoiceLine.ZG_CountryOfDestinationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckJI_Weight()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			invoiceLine.JI_Weight = 0m;
			AssertHasMessageErrorContaining(invoiceLine.JI_WeightInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.JI_Weight = 200m;
			AssertNoMessageErrorContaining(invoiceLine.JI_WeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestJI_TariffInfo()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var entry = declaration.CustomsEntryInstructions.AddNew();
			entry.CEI_Style = "A1";
			invoiceLine.JI_Tariff = ZString.Empty;
			AssertHasMessageError("Ensure validation present where JE_MessageType is EXP", invoiceLine.JI_TariffInfo, "Tariff may not be empty");
		}

		public void TestJI_CountryOfOrigin()
		{
			(var validation, var invoiceLine, _, var declaration) = SetupData();
			var errorMessage = $"You have not entered a {invoiceLine.JI_CountryOfOriginInfo.HumanReadableName} Origin is mandatory when Additional Procedure begins with 'E'.";

			invoiceLine.JI_Procedure = "1000E00";
			invoiceLine.JI_CountryOfOrigin = "";
			validation.ValidateJI_CountryOfOrigin();
			AssertHasMessageError("Has error when JI_Procedure starts with E", invoiceLine.JI_CountryOfOriginInfo, errorMessage);

			invoiceLine.JI_Procedure = "1000F00";
			validation.ValidateJI_CountryOfOrigin();
			AssertNoMessageError("No error when JI_Procedure does not start with E", invoiceLine.JI_CountryOfOriginInfo, errorMessage);
		}

		public void TestCheckJI_Procedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedure = helper.CreateOrFindExistingRefCusProcedure(Core.Constants.CountryCodes.Ireland, "", "76", "00", "E71", "", MessageType, "B3");
			procedure.ZZ6_IntoWarehouse = "N";
			procedure.ZZ6_OutOfWarehouse = "Y";
			(_, var invoiceLine, _, var declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
			invoiceLine.JI_CEI = instruction.PK;

			invoiceLine.JI_Procedure = "7600E71";
			var message = "Procedure must be into warehouse when Instruction Declaration Type is B3.";
			AssertHasMessageErrorContaining(invoiceLine.JI_ProcedureInfo, message);

			procedure.ZZ6_IntoWarehouse = "Y";
			procedure.ZZ6_OutOfWarehouse = "N";
			invoiceLine.Validation.ValidateJI_Procedure();
			AssertNoMessageErrorContaining(instruction.CEI_OA_WarehouseInfo, message);
		}

		public void TestCheckJI_CustomsSecondUnitQty()
		{
			var factory = new BusinessObjectFactory();

			var startDate = ZDateTime.MinSmallDateTimeValue;
			var endDate = ZDateTime.MaxSmallDateTimeValue;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ireland, Universal.Constants.TariffTypes.Export);
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Ireland, tariffType.PK, "9999001000", startDate, endDate);
			helper.CreateTariffUOM(tariff, Universal.Constants.UnitOfMeasureTypes.AdditionalUOMType, Customs.Business.UniversalReferenceConstants.RefCusCodeList.CustomsUq.Weight.Kilogram);
			factory.Save();

			(_, var invoiceLine, _, _) = SetupData();
			invoiceLine.JI_Tariff = tariff.ZZ1_TariffCode;
			invoiceLine.JI_CustomsSecondUnitQty = ZString.Empty;
			invoiceLine.Validation.ValidateJI_CustomsSecondUnitQty();
			AssertHasMessageErrorContaining(invoiceLine.JI_CustomsSecondUnitQtyInfo, $"{invoiceLine.JI_CustomsSecondUnitQtyInfo.HumanReadableName} for this {invoiceLine.JI_TariffInfo.HumanReadableName} should be KGM");
		}

		public void TestCheckJI_LinePrice()
		{
			var message = "You have not entered a Price. Price is needed to execute correct calculation for the required statistical value.";

			(_, var invoiceLine, _, var declaration) = SetupData();
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = ExportDeclarationTypeList.Codes.B1;
			invoiceLine.JI_CEI = instruction.PK;
			invoiceLine.JI_LinePrice = ZDecimal.Zero;

			CombineAssertions(() =>
			{
				invoiceLine.Validation.ValidateJI_LinePrice();
				AssertHasMessageErrorContaining("Line price zero, declaration type B1 or B2 - should have error", invoiceLine.JI_LinePriceInfo, message);
				invoiceLine.JI_LinePrice = 33.33m;
				invoiceLine.Validation.ValidateJI_LinePrice();
				AssertNoMessageErrorContaining("Line price not zero, declaration type B1 or B2 - should not have error", invoiceLine.JI_LinePriceInfo, message);
				invoiceLine.JI_LinePrice = ZDecimal.Zero;
				instruction.CEI_Style = ExportDeclarationTypeList.Codes.B3;
				invoiceLine.Validation.ValidateJI_LinePrice();
				AssertNoMessageErrorContaining("Line price zero, declaration type not B1 or B2 - should not have error", invoiceLine.JI_LinePriceInfo, message);
			});
		}

		public void Test_PackagesAreSelected()
		{
			(_, var invoiceLine, _, var declaration) = SetupData();
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = "A3";
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(invoiceLine, "This line has no packaging details");
			AssertEquals("Message should not contain text ['31']", false, invoiceLine.RowMessageErrors.First(m => m.Message.Contains("This line has no packaging details")).Message.Contains("[31]"));
		}

		public void TestCheckJI_ContainerMode_ContainerNotLinked()
		{
			var warning = "Invoice Line is in containerized mode, but is not linked to a container, a container can be associated to invoice lines from the menu on the Packaging tab -> sub tab Packing Details";
			(_, var invoiceLine, _, var declaration) = SetupData();
			invoiceLine.JI_ContainerMode = Core.Constants.ContainerModes.ULD;
			AssertHasWarning("JI_ContainerMode has correct warning", invoiceLine.JI_ContainerModeInfo, warning);

			var container = declaration.CusContainers.AddNew();
			var pivot = invoiceLine.ContainersPivot.AddNew();
			pivot.C2_CO = container.PK;
			invoiceLine.Validation.ValidateJI_ContainerMode();
			AssertNoWarning("JI_ContainerMode - no warning on linked invoice lines", invoiceLine.JI_ContainerModeInfo, warning);
		}

		public void TestCheckTransportDocumentCount()
		{
			(_, var invoiceLine, _, _) = SetupData();
			var additionalInfoInInvoiceLine = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoInInvoiceLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;

			var additionalInfos = invoiceLine.AdditionalInfos;
			for (int i = 0; i < 99; i++)
			{
				var transportDoc = additionalInfos.AddNew();
				transportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.TransportDocument;
			}

			var transportDocs = additionalInfos
				.Where(addInfo => addInfo.CSI_SubType == AdditionalInfoSubTypeList.Codes.TransportDocument);
			AssertEquals("Precondition", 100, transportDocs.Count());

			var rowMessageError = "The maximum number of transport documents allowed is 99.";
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageError("When count > 99", invoiceLine, rowMessageError);

			additionalInfos.RemoveAndDelete(invoiceLine.AdditionalInfos[99]);
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageError("When count <= 99", invoiceLine, rowMessageError);
		}

		public void TestCheckAdditionalReferenceCount()
		{
			(_, var invoiceLine, _, _) = SetupData();
			var additionalInfoInInvoiceLine = invoiceLine.AdditionalInfos.AddNew();
			additionalInfoInInvoiceLine.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;

			var additionalInfos = invoiceLine.AdditionalInfos;
			for (int i = 0; i < 99; i++)
			{
				var transportDoc = additionalInfos.AddNew();
				transportDoc.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			}

			var additionalRefs = additionalInfos
				.Where(addInfo => addInfo.CSI_SubType == AdditionalInfoSubTypeList.Codes.AdditionalReference);
			AssertEquals("Precondition", 100, additionalRefs.Count());

			var rowMessageError = "The maximum number of additional references allowed is 99.";
			invoiceLine.Validation.ValidateAll();
			AssertHasRowMessageError("When count > 99", invoiceLine, rowMessageError);

			additionalInfos.RemoveAndDelete(invoiceLine.AdditionalInfos[99]);
			invoiceLine.Validation.ValidateAll();
			AssertNoRowMessageError("When count <= 99", invoiceLine, rowMessageError);
		}

		protected override string MessageType => IEJobMessageTypeList.Codes.Export;
	}
}
