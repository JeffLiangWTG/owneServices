using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class CFRPSNComponentTest : TestCaseWithFactory
	{
		public void TestCFRPSNComponent_SolidSubstance_Hot()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_State = "S";
			substance.CFR_PSN = "Steven's new iPad";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 300;
			packline.JL_RequiredTemperatureUnit = "C";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var moltenPSNComponent = new CFRPSNComponent() as IUNDGSummaryWriterComponent;
			var result = moltenPSNComponent.Write(wrapper);

			AssertEquals("Solid substance at elevated temp should have \"HOT\"", "HOT - Steven's new iPad", result);
		}

		public void TestCFRPSNComponent_SolidSubstance_NotHot()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_State = "S";
			substance.CFR_PSN = "Steven's new keyboard";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 160;
			packline.JL_RequiredTemperatureUnit = "C";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var moltenPSNComponent = new CFRPSNComponent() as IUNDGSummaryWriterComponent;
			var result = moltenPSNComponent.Write(wrapper);

			AssertEquals("Solid substance at non elevated temp should not have \"HOT\"", "Steven's new keyboard", result);
		}

		public void TestCFRPSNComponent_LiquidSubstance_Hot()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_State = "L";
			substance.CFR_PSN = "Gutter Grime";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 105;
			packline.JL_RequiredTemperatureUnit = "C";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var moltenPSNComponent = new CFRPSNComponent() as IUNDGSummaryWriterComponent;
			var result = moltenPSNComponent.Write(wrapper);

			AssertEquals("Liquid substance at elevated temp should have \"HOT\"", "HOT - Gutter Grime", result);
		}

		public void TestCFRPSNComponent_LiquidSubstance_NotHot()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_State = "L";
			substance.CFR_PSN = "Gutter Ooze";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 40;
			packline.JL_RequiredTemperatureUnit = "C";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var moltenPSNComponent = new CFRPSNComponent() as IUNDGSummaryWriterComponent;
			var result = moltenPSNComponent.Write(wrapper);

			AssertEquals("Liquid substance at non elevated temp should not have \"HOT\"", "Gutter Ooze", result);
		}

		public void TestCFRPSNComponent_LiquidSubstance_Flashpoint_Hot()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_State = "L";
			substance.CFR_PSN = "Felix Felicis";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);
			undgDataItem.DI_IsCombustible = true;
			undgDataItem.DI_DGFlashPoint = 39;

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 40;
			packline.JL_RequiredTemperatureUnit = "C";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var moltenPSNComponent = new CFRPSNComponent() as IUNDGSummaryWriterComponent;
			var result = moltenPSNComponent.Write(wrapper);

			AssertEquals("Liquid substance with flash point above 38 and max temp above flash point should have \"HOT\"", "HOT - Felix Felicis", result);
		}

		public void TestCFRPSNComponent_LiquidSubstance_Flashpoint_NotHot()
		{
			var substance = Factory.New<UNDGSubstanceCFR>();
			substance.CFR_UNNO = "1234";
			substance.CFR_State = "L";
			substance.CFR_PSN = "Rat Tonic";

			Factory.Save();

			var cfrSubstance = UNDGSubstanceLoader.LoadSubstances(Factory, "1234", standard: UNDGSubstanceLookups.UNDGSubstanceStandardTypes.CFR).First();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();

			var undgDataItem = packline.UNDGs.AddNew();
			undgDataItem.DI_DG = cfrSubstance.PK;
			undgDataItem.LinkDefault(cfrSubstance);
			undgDataItem.DI_IsCombustible = true;
			undgDataItem.DI_DGFlashPoint = 35;

			packline.JL_RequiresTemperatureControl = true;
			packline.JL_RequiredTemperatureMaximum = 37;
			packline.JL_RequiredTemperatureUnit = "C";

			var wrapper = new UNDGSubstanceWrapper(undgDataItem, Factory);
			var moltenPSNComponent = new CFRPSNComponent() as IUNDGSummaryWriterComponent;
			var result = moltenPSNComponent.Write(wrapper);

			AssertEquals("Liquid substance with flash point below 38 and max temp above flash point should not have \"HOT\"", "Rat Tonic", result);
		}
	}
}
