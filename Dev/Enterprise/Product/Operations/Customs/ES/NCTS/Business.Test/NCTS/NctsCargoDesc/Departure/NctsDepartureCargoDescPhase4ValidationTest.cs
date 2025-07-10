using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureCargoDescPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestTransportDocumentWithTheSecurityCheckbox()
		{
			const string message = "You have not entered a transport document for this item.";
			CombineAssertions(() =>
			{
				header.BH_FTZMove = false;
				detail.Validation.ValidateAll();
				AssertNoRowWarningContaining("Safety and segurity not chequed and no supporting documents", detail, message);

				var doc1 = detail.SupportingDocuments.AddNew();
				doc1.CSI_Code = "N271";
				doc1.CSI_ReferenceNumber = "ABC123";
				doc1.CSI_Description = "Manifest";
				detail.Validation.ValidateAll();
				AssertNoRowWarningContaining("Safety and segurity not chequed and yes supporting documents", detail, message);

				header.BH_FTZMove = true;
				detail.Validation.ValidateAll();
				AssertNoRowWarningContaining("Safety and segurity chequed and yes supporting documents", detail, message);

				detail.SupportingDocuments.RemoveAndDeleteAll();
				detail.Validation.ValidateAll();
				AssertHasRowWarningContaining(detail, message);
			});
		}

		public void TestCheckHasPackages()
		{
			const string message = "This line has no packaging details.";
			CombineAssertions(() =>
			{
				detail.IsVehicles = true;
				detail.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(detail, message);

				var pac1 = detail.Packages.AddNew();
				pac1.B5_UnitType = "4H";
				pac1.B5_MarksAndNumbers = "MARKS";
				pac1.B5_UnitCount = 2;
				detail.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(detail, message);

				detail.IsVehicles = false;
				var pac2 = detail.Packages.AddNew();
				pac2.B5_UnitType = "4H";
				pac2.B5_MarksAndNumbers = "MARKS";
				pac2.B5_UnitCount = 2;
				detail.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(detail, message);

				detail.Packages.RemoveAndDeleteAll();
				detail.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(detail, message);
			});
		}

		public void TestCheckHasVehicles()
		{
			const string message = "This line has no vehicle details.";
			CombineAssertions(() =>
			{
				detail.IsVehicles = false;
				detail.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(detail, message);

				var veh1 = detail.Packages.AddNew();
				veh1.B5_PackageID = "5289JJJ";
				veh1.B5_Brand = "BRAND";
				veh1.B5_Model = "MODEL";
				detail.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(detail, message);

				detail.IsVehicles = true;
				var veh2 = detail.Packages.AddNew();
				veh2.B5_PackageID = "5289JJJ1";
				veh2.B5_Brand = "BRAND1";
				veh2.B5_Model = "MODEL1";
				detail.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(detail, message);

				detail.Packages.RemoveAndDeleteAll();
				detail.Validation.ValidateAll();
				AssertHasRowMessageErrorContaining(detail, message);
			});
		}

		public void TestCheckBY_CustomsSecondQuantityIsValidZDecimal()
		{
			const string message = "The number 100,000,000,000.000 is too large, the maximum value allowed for Supplementary Quantity is 99,999,999,999.999.";
			detail.Validation.ValidateBY_CustomsSecondQuantity();
			AssertNoError(detail.BY_CustomsSecondQuantityInfo, message);
			detail.BY_CustomsSecondQuantity = 100000000000.000m;
			AssertHasError(detail.BY_CustomsSecondQuantityInfo, message);
			detail.BY_CustomsSecondQuantity = 20.303m;
			AssertNoError(detail.BY_CustomsSecondQuantityInfo, message);
		}

		public void TestCheckBY_CustomsSecondQuantity()
		{
			CombineAssertions(() =>
			{
				detail.BY_CustomsSecondUnitQty = "NAR";
				detail.BY_CustomsSecondQuantity = 0;
				AssertHasMessageErrorContaining("Has UOM and No Qty", detail.BY_CustomsSecondQuantityInfo, MandatoryValidation.ValueCannotBeZero);

				detail.BY_CustomsSecondQuantity = 12.3m;
				AssertNoMessageErrorContaining("Has Qty", detail.BY_CustomsSecondQuantityInfo, MandatoryValidation.ValueCannotBeZero);
			});
		}

		public void TestCheckBY_MonetaryValueIsValidMoney()
		{
			detail.Validation.ValidateBY_MonetaryValue();
			AssertNoError(detail.BY_MonetaryValueInfo, $"The number 100,000,000,000.00 is too large, the maximum value allowed for {detail.BY_MonetaryValueInfo.Description} is 99,999,999,999.99.");
			detail.BY_MonetaryValue = 100000000000.00m;
			AssertHasError(detail.BY_MonetaryValueInfo, $"The number 100,000,000,000.00 is too large, the maximum value allowed for {detail.BY_MonetaryValueInfo.Description} is 99,999,999,999.99.");
			detail.BY_MonetaryValue = 20.33m;
			AssertNoError(detail.BY_MonetaryValueInfo, $"The number 100,000,000,000.00 is too large, the maximum value allowed for {detail.BY_MonetaryValueInfo.Description} is 99,999,999,999.99.");
		}

		public void TestValidateMaxVehicles()
		{
			detail.IsVehicles = true;

			for (int i = 0; i < 99; i++)
			{
				var vehicle = detail.Packages.AddNew();
				vehicle.B5_PackageID = $"vehicle{i}";
			}
			CombineAssertions(() =>
			{
				detail.Validation.ValidateAll();
				AssertEquals("Prereq: 99 vehicles", 99, detail.Packages.Count);
				AssertNoRowMessageErrorContaining(detail, "Customs will not accept a declaration with more than 99 vehicles.");

				var nctsVehicle = detail.Packages.AddNew();
				nctsVehicle.B5_PackageID = "vehicleNew";
				detail.Validation.ValidateAll();
				AssertEquals("Prereq: 100 vehicles", 100, detail.Packages.Count);
				AssertHasRowMessageErrorContaining(detail, "Customs will not accept a declaration with more than 99 vehicles.");

				detail.IsVehicles = false;
				detail.Validation.ValidateAll();
				AssertNoRowMessageErrorContaining(detail, "Customs will not accept a declaration with more than 99 vehicles.");
			});
		}

		public void TestCheckIsVehicles()
		{
			var expectedWarning = "The VIN may not be required for tariff:";
			CombineAssertions(() =>
			{
				detail.IsVehicles = false;
				AssertNoWarningContaining(detail.IsVehiclesInfo, expectedWarning);

				detail.IsVehicles = true;
				AssertHasWarningContaining(detail.IsVehiclesInfo, expectedWarning);

				detail.BY_HarmonisedTariff = "8727282";
				detail.Validation.ValidateAll();
				AssertNoWarningContaining(detail.IsVehiclesInfo, expectedWarning);

				detail.BY_HarmonisedTariff = "8435627";
				detail.Validation.ValidateAll();
				AssertNoWarningContaining(detail.IsVehiclesInfo, expectedWarning);
			});
		}

		public void TestCheckBY_GrossWeightMandatoryValidation()
		{
			CombineAssertions(() =>
			{
				detail.Validation.ValidateBY_GrossWeight();
				AssertHasMessageErrorContaining("Gross weight is required", detail.BY_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
				detail.BY_GrossWeight = 10;
				AssertNoMessageErrorContaining("Gross weight is valid", detail.BY_GrossWeightInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckBY_GrossWeightNegativeValidation()
		{
			CombineAssertions(() =>
			{
				detail.BY_GrossWeight = -5;
				detail.Validation.ValidateBY_GrossWeight();
				AssertHasMessageErrorContaining("Gross weight cannot be negative", detail.BY_GrossWeightInfo, MandatoryValidation.ValueCannotBeNegative);
				detail.BY_GrossWeight = 5;
				AssertNoMessageErrorContaining("Gross weight is valid", detail.BY_GrossWeightInfo, MandatoryValidation.ValueCannotBeNegative);
			});
		}

		public void TestCheckBY_GrossWeightUnitMandatoryValidation()
		{
			CombineAssertions(() =>
			{
				detail.BY_GrossWeightUnit = ZString.Empty;
				detail.Validation.ValidateBY_GrossWeightUnit();
				AssertHasMessageErrorContaining("Gross weight unit is required", detail.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
				detail.BY_GrossWeightUnit = "XX";
				AssertNoMessageErrorContaining("Gross weight unit is valid", detail.BY_GrossWeightUnitInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckBY_NetWeightMandatoryValidation()
		{
			CombineAssertions(() =>
			{
				detail.Validation.ValidateBY_NetWeight();
				AssertHasMessageErrorContaining("Net weight is required", detail.BY_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
				detail.BY_NetWeight = 10;
				AssertNoMessageErrorContaining("Net weight is valid", detail.BY_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckBY_MonetaryValue()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var message = "If Stat. Value is declared with value = 0, Customs Authorities will lock an amount of 10.000€ in the declared guarantee.";

				CombineAssertions("When current company is Spain", () =>
				{
					detail.Validation.ValidateBY_MonetaryValue();
					AssertHasWarningContaining("When Stat Value is empty there is a message error", detail.BY_MonetaryValueInfo, message);
					detail.BY_MonetaryValue = 10;
					AssertNoWarningContaining("When Stat Value is not empty there is no message error", detail.BY_MonetaryValueInfo, message);
				});
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var message = "If Stat. Value is declared with value = 0, Customs Authorities will lock an amount of 10,000€ in the declared guarantee.";

				CombineAssertions("When current company is Australia", () =>
				{
					detail.BY_MonetaryValue = ZDecimal.Zero;
					detail.Validation.ValidateBY_MonetaryValue();
					AssertHasWarningContaining("When Stat Value is empty there is a message error", detail.BY_MonetaryValueInfo, message);
					detail.BY_MonetaryValue = 10;
					AssertNoWarningContaining("When Stat Value is not empty there is no message error", detail.BY_MonetaryValueInfo, message);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			detail = header.MovementHeader.GoodsItems.AddNew();
		}
		NctsDepartureCargoDesc detail;
		NctsHeader header;
	}
}
