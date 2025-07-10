using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PrintChargesBilledToLocalClientAtDestAsCollect))]
	sealed class PrintChargesBilledToLocalClientAtDestAsCollectTest : RegistryBusinessObjectTemplateTestCase<PrintChargesBilledToLocalClientAtDestAsCollect>
	{
		public void TestValidateTransportMode()
		{
			Row1.TransportMode = string.Empty;
			AssertHasError("Empty TransportMode", Row1.TransportModeInfo, "Please enter a Transport Mode.");
			Row1.TransportMode = "XXX";
			AssertHasError("Invalid TransportMode", Row1.TransportModeInfo, "Enter a valid Transport Mode.");
			Row1.TransportMode = Core.Constants.TransportModes.Sea;
			AssertNoErrors("Valid TransportMode", Row1.TransportModeInfo);
		}

		public void TestValidateExportCountry()
		{
			Row1.ExportCountry = string.Empty;
			AssertNoErrors("Valid ExportCountry", Row1.ExportCountryInfo);
			Row1.ExportCountry = "XX";
			AssertHasError("Invalid ExportCountry", Row1.ExportCountryInfo, "Enter a valid Export Country.");
			Row1.ExportCountry = Core.Constants.CountryCodes.Belgium;
			AssertNoErrors("Valid ExportCountry", Row1.ExportCountryInfo);
		}

		public void TestValidateImportCountry()
		{
			Row1.ImportCountry = string.Empty;
			AssertNoErrors("Valid ImportCountry", Row1.ImportCountryInfo);
			Row1.ImportCountry = "XX";
			AssertHasError("Invalid ImportCountry", Row1.ImportCountryInfo, "Enter a valid Import Country.");
			Row1.ImportCountry = Core.Constants.CountryCodes.Belgium;
			AssertNoErrors("Valid ImportCountry", Row1.ImportCountryInfo);
		}

		public void TestUniqueRow()
		{
			var errorMessage = "The combination of Transport Mode, Export Country and Import Country may only appear once in this list.";

			var belgium = Core.Constants.CountryCodes.Belgium;
			var netherlands = Core.Constants.CountryCodes.Netherlands;
			var germany = Core.Constants.CountryCodes.Germany;
			var france = Core.Constants.CountryCodes.France;

			Row1.TransportMode = Core.Constants.TransportModes.Sea;
			Row2.TransportMode = Core.Constants.TransportModes.Sea;

			Row1.ExportCountry = belgium;
			Row1.ImportCountry = netherlands;
			Row2.ExportCountry = germany;
			Row2.ImportCountry = france;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid", Row1.TransportModeInfo);
			AssertNoErrors("Valid", Row1.ExportCountryInfo);
			AssertNoErrors("Valid", Row1.ImportCountryInfo);
			AssertNoErrors("Valid", Row2.TransportModeInfo);
			AssertNoErrors("Valid", Row2.ExportCountryInfo);
			AssertNoErrors("Valid", Row2.ImportCountryInfo);

			Row1.ExportCountry = belgium;
			Row1.ImportCountry = netherlands;
			Row2.ExportCountry = germany;
			Row2.ImportCountry = belgium;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid", Row1.TransportModeInfo);
			AssertNoErrors("Valid", Row1.ExportCountryInfo);
			AssertNoErrors("Valid", Row1.ImportCountryInfo);
			AssertNoErrors("Valid", Row2.TransportModeInfo);
			AssertNoErrors("Valid", Row2.ExportCountryInfo);
			AssertNoErrors("Valid", Row2.ImportCountryInfo);

			Row1.ExportCountry = belgium;
			Row1.ImportCountry = france;
			Row2.ExportCountry = germany;
			Row2.ImportCountry = france;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid", Row1.TransportModeInfo);
			AssertNoErrors("Valid", Row1.ExportCountryInfo);
			AssertNoErrors("Valid", Row1.ImportCountryInfo);
			AssertNoErrors("Valid", Row2.TransportModeInfo);
			AssertNoErrors("Valid", Row2.ExportCountryInfo);
			AssertNoErrors("Valid", Row2.ImportCountryInfo);

			Row1.ExportCountry = belgium;
			Row1.ImportCountry = ZString.Empty;
			Row2.ExportCountry = germany;
			Row2.ImportCountry = ZString.Empty;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid", Row1.TransportModeInfo);
			AssertNoErrors("Valid", Row1.ExportCountryInfo);
			AssertNoErrors("Valid", Row1.ImportCountryInfo);
			AssertNoErrors("Valid", Row2.TransportModeInfo);
			AssertNoErrors("Valid", Row2.ExportCountryInfo);
			AssertNoErrors("Valid", Row2.ImportCountryInfo);

			Row1.ExportCountry = ZString.Empty;
			Row1.ImportCountry = netherlands;
			Row2.ExportCountry = ZString.Empty;
			Row2.ImportCountry = france;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid", Row1.TransportModeInfo);
			AssertNoErrors("Valid", Row1.ExportCountryInfo);
			AssertNoErrors("Valid", Row1.ImportCountryInfo);
			AssertNoErrors("Valid", Row2.TransportModeInfo);
			AssertNoErrors("Valid", Row2.ExportCountryInfo);
			AssertNoErrors("Valid", Row2.ImportCountryInfo);

			Row1.ExportCountry = belgium;
			Row1.ImportCountry = ZString.Empty;
			Row2.ExportCountry = ZString.Empty;
			Row2.ImportCountry = france;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid", Row1.TransportModeInfo);
			AssertNoErrors("Valid", Row1.ExportCountryInfo);
			AssertNoErrors("Valid", Row1.ImportCountryInfo);
			AssertNoErrors("Valid", Row2.TransportModeInfo);
			AssertNoErrors("Valid", Row2.ExportCountryInfo);
			AssertNoErrors("Valid", Row2.ImportCountryInfo);

			Row1.ExportCountry = ZString.Empty;
			Row1.ImportCountry = netherlands;
			Row2.ExportCountry = germany;
			Row2.ImportCountry = ZString.Empty;
			Collection.RunPreSaveValidation();
			AssertNoErrors("Valid", Row1.TransportModeInfo);
			AssertNoErrors("Valid", Row1.ExportCountryInfo);
			AssertNoErrors("Valid", Row1.ImportCountryInfo);
			AssertNoErrors("Valid", Row2.TransportModeInfo);
			AssertNoErrors("Valid", Row2.ExportCountryInfo);
			AssertNoErrors("Valid", Row2.ImportCountryInfo);

			Row1.ExportCountry = belgium;
			Row1.ImportCountry = netherlands;
			Row2.ExportCountry = belgium;
			Row2.ImportCountry = netherlands;
			Collection.RunPreSaveValidation();
			AssertHasError("Duplicate", Row1.TransportModeInfo, errorMessage);
			AssertHasError("Duplicate", Row1.ExportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row1.ImportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row2.TransportModeInfo, errorMessage);
			AssertHasError("Duplicate", Row2.ExportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row2.ImportCountryInfo, errorMessage);

			Row1.ExportCountry = belgium;
			Row1.ImportCountry = ZString.Empty;
			Row2.ExportCountry = belgium;
			Row2.ImportCountry = ZString.Empty;
			Collection.RunPreSaveValidation();
			AssertHasError("Duplicate", Row1.TransportModeInfo, errorMessage);
			AssertHasError("Duplicate", Row1.ExportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row1.ImportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row2.TransportModeInfo, errorMessage);
			AssertHasError("Duplicate", Row2.ExportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row2.ImportCountryInfo, errorMessage);

			Row1.ExportCountry = ZString.Empty;
			Row1.ImportCountry = netherlands;
			Row2.ExportCountry = ZString.Empty;
			Row2.ImportCountry = netherlands;
			Collection.RunPreSaveValidation();
			AssertHasError("Duplicate", Row1.TransportModeInfo, errorMessage);
			AssertHasError("Duplicate", Row1.ExportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row1.ImportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row2.TransportModeInfo, errorMessage);
			AssertHasError("Duplicate", Row2.ExportCountryInfo, errorMessage);
			AssertHasError("Duplicate", Row2.ImportCountryInfo, errorMessage);
		}

		#region Implementation
		PrintChargesBilledToLocalClientAtDestAsCollectCollection Collection
		{
			get
			{
				return collection ?? (collection = new PrintChargesBilledToLocalClientAtDestAsCollectCollection());
			}
		}
		PrintChargesBilledToLocalClientAtDestAsCollectCollection collection;

		PrintChargesBilledToLocalClientAtDestAsCollect Row1
		{
			get
			{
				if (row1 == null)
				{
					row1 = Collection.AddNew();
					row1.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
				}
				return row1;
			}
		}
		PrintChargesBilledToLocalClientAtDestAsCollect row1;

		PrintChargesBilledToLocalClientAtDestAsCollect Row2
		{
			get
			{
				if (row2 == null)
				{
					row2 = Collection.AddNew();
					row2.CurrentFallbackLevel = new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty);
				}
				return row2;
			}
		}
		PrintChargesBilledToLocalClientAtDestAsCollect row2;

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return true;
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PrintChargesBilledToLocalClientAtDestAsCollect GetBusinessObjectToClone()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override PrintChargesBilledToLocalClientAtDestAsCollect GetBusinessObjectToSerialise()
		{
			return GetNewPopulatedBusinessObject();
		}

		protected override void CheckAllPropertiesAreEqual(PrintChargesBilledToLocalClientAtDestAsCollect originalBusinessObject, PrintChargesBilledToLocalClientAtDestAsCollect newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);
			AssertEquals("TransportMode", originalBusinessObject.TransportMode, newBusinessObject.TransportMode);
			AssertEquals("ExportCountry", originalBusinessObject.ExportCountry, newBusinessObject.ExportCountry);
			AssertEquals("ImportCountry", originalBusinessObject.ImportCountry, newBusinessObject.ImportCountry);
		}

		PrintChargesBilledToLocalClientAtDestAsCollect GetNewPopulatedBusinessObject()
		{
			PrintChargesBilledToLocalClientAtDestAsCollect transportMode = new PrintChargesBilledToLocalClientAtDestAsCollect();
			transportMode.TransportMode = Core.Constants.TransportModes.Sea;
			transportMode.ExportCountry = Core.Constants.CountryCodes.Belgium;
			transportMode.ImportCountry = Core.Constants.CountryCodes.Australia;
			return transportMode;
		}

		#endregion
	}
}
