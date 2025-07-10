using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocPackLines))]
	sealed class DocPackLinesTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
				{
					DocPackLines.New(APackLine, Factory),
					DocPackLines.New(Factory, APackLine.PK)
				};
		}

		#region LoadList Document

		public void TestContainerCode()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			ForwardingConsol consol2 = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol1.Containers.AddNew();
			var container2 = (CommonContainer)consol2.Containers.AddNew();
			container1.JC_ContainerCount = 2;
			container2.JC_ContainerCount = 3;
			var containerType1 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP");
			var containerType2 = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			container1.JC_RC = containerType1.PK;
			container2.JC_RC = containerType2.PK;
			PackLine aPackLine = shipment.OuterPackLines.AddNew();
			aPackLine.SetContainer(consol1, container1);
			aPackLine.SetContainer(consol2, container2);
			aPackLine.CurrentConsol = consol1;
			DocPackLines packLineWrapper = DocPackLines.New(aPackLine, Factory);
			DocContainer container1Wrapper = DocContainer.New(container1, Factory);
			AssertEquals("ContainerCode should have container 1 details: ", "20GP (2)", packLineWrapper.ContainerCode);
			container1.JC_ContainerNum = "111";
			AssertEquals("ContainerCode should have container 1 number: ", "111", packLineWrapper.ContainerCode);

			DocContainer container2Wrapper = DocContainer.New(container2, Factory);
			packLineWrapper.CurrentConsol = DocForwardingConsol.New(consol2, Factory);
			AssertEquals("ContainerCode should have container 2 details: ", "40GP (3)", packLineWrapper.ContainerCode);
			container2.JC_ContainerNum = "222";
			AssertEquals("ContainerCode should have container 2 number: ", "222", packLineWrapper.ContainerCode);
		}

		public void TestConsignorAndConsigneeForLoadList()
		{
			PackLineWrapper.ConsignorAndConsigneeForLoadList = "Consignor\nConsignee";
			AssertEquals("ConsignorAndConsignee should be ", "Consignor\nConsignee", PackLineWrapper.ConsignorAndConsigneeForLoadList);
		}

		public void TestCustomsBrokerForLoadList()
		{
			PackLineWrapper.CustomsBrokerForLoadList = "Customs Broker";
			AssertEquals("Customs broker should be ", "Customs Broker", PackLineWrapper.CustomsBrokerForLoadList);
		}

		#endregion

		#region Calc_Properties

		public void TestChargeableWeightLB()
		{
			APackLine.JL_ActualVolume = 100;
			APackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicInches;
			APackLine.JL_ActualWeight = 10;
			APackLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals("Precondition: volume weight should be less than actual weight", true, PackLineWrapper.ActualVolumeWeightLB < PackLineWrapper.ActualWeightLB);
			AssertNotEquals(0, PackLineWrapper.ActualVolumeWeightLB);
			AssertNotEquals(0, PackLineWrapper.ActualWeightLB);
			AssertEquals(PackLineWrapper.ActualWeightLB, PackLineWrapper.ChargeableWeightLB);

			APackLine.JL_ActualVolume = 10000;
			APackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicInches;
			APackLine.JL_ActualWeight = 10;
			APackLine.JL_ActualWeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals("Precondition: volume weight should be greater than actual weight", true, PackLineWrapper.ActualVolumeWeightLB > PackLineWrapper.ActualWeightLB);
			AssertNotEquals(0, PackLineWrapper.ActualVolumeWeightLB);
			AssertNotEquals(0, PackLineWrapper.ActualWeightLB);
			AssertEquals(PackLineWrapper.ActualVolumeWeightLB, PackLineWrapper.ChargeableWeightLB);
		}

		public void TestActualVolumeWeightLB()
		{
			APackLine.JL_ActualVolume = 194 * 2;
			APackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicInches;
			AssertEquals(new ZDecimal(2), PackLineWrapper.ActualVolumeWeightLB);

			APackLine.JL_ActualVolume = 50;
			APackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertNotEquals(0, PackLineWrapper.ActualVolumeWeightLB);
			AssertEquals(ZArchitecture.Core.Utilities.Round(Core.Constants.Volume.ConvertSafe(50, Core.Constants.Volume.CubicMetres, Core.Constants.Volume.CubicInches) / 194, 2), PackLineWrapper.ActualVolumeWeightLB);
		}

		ZDecimal GetExpectedMeasureInIN(ZDecimal measure, ZString uQ)
		{
			ZDecimal result = ZDecimal.Zero;
			return Core.Constants.Length.ConvertSafe(measure, uQ, Core.Constants.Length.Inches);
		}

		public void TestLengthInIN()
		{
			APackLine.JL_Length = 0;
			AssertEquals(0m, PackLineWrapper.LengthInIN);

			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals(0m, PackLineWrapper.LengthInIN);

			APackLine.JL_Length = 10;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Inches;
			AssertEquals(10m, PackLineWrapper.LengthInIN);

			APackLine.JL_Length = 40;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals(GetExpectedMeasureInIN(40, Core.Constants.Length.Metres), PackLineWrapper.LengthInIN);

			APackLine.JL_Length = 14;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Feet;
			AssertEquals(GetExpectedMeasureInIN(14, Core.Constants.Length.Feet), PackLineWrapper.LengthInIN);
		}

		public void TestWidthInIN()
		{
			APackLine.JL_Width = 0;
			AssertEquals(0m, PackLineWrapper.WidthInIN);

			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals(0m, PackLineWrapper.WidthInIN);

			APackLine.JL_Width = 10;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Inches;
			AssertEquals(10m, PackLineWrapper.WidthInIN);

			APackLine.JL_Width = 40;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals(GetExpectedMeasureInIN(40, Core.Constants.Length.Metres), PackLineWrapper.WidthInIN);

			APackLine.JL_Width = 14;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Feet;
			AssertEquals(GetExpectedMeasureInIN(14, Core.Constants.Length.Feet), PackLineWrapper.WidthInIN);
		}

		public void TestHeightInIN()
		{
			APackLine.JL_Height = 0;
			AssertEquals(0m, PackLineWrapper.HeightInIN);

			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals(0m, PackLineWrapper.HeightInIN);

			APackLine.JL_Height = 10;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Inches;
			AssertEquals(10m, PackLineWrapper.HeightInIN);

			APackLine.JL_Height = 40;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals(GetExpectedMeasureInIN(40, Core.Constants.Length.Metres), PackLineWrapper.HeightInIN);

			APackLine.JL_Height = 14;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Feet;
			AssertEquals(GetExpectedMeasureInIN(14, Core.Constants.Length.Feet), PackLineWrapper.HeightInIN);
		}

		public void TestDimensionsInIN()
		{
			APackLine.JL_Length = 0;
			APackLine.JL_Width = 0;
			APackLine.JL_Height = 0;
			AssertEquals("0x0x0", PackLineWrapper.DimensionsInIN);

			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals("0x0x0", PackLineWrapper.DimensionsInIN);

			APackLine.JL_Length = 10;
			APackLine.JL_Width = 20;
			APackLine.JL_Height = 30;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Inches;
			AssertEquals("10x20x30", PackLineWrapper.DimensionsInIN);

			APackLine.JL_Length = 40;
			APackLine.JL_Width = 10;
			APackLine.JL_Height = 5;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals(GetExpectedDimensionsInIN(40, 10, 5, Core.Constants.Length.Metres), PackLineWrapper.DimensionsInIN);

			APackLine.JL_Length = 14;
			APackLine.JL_Width = 22;
			APackLine.JL_Height = 31;
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Feet;
			AssertEquals(GetExpectedDimensionsInIN(14, 22, 31, Core.Constants.Length.Feet), PackLineWrapper.DimensionsInIN);
		}

		ZString GetExpectedDimensionsInIN(ZDecimal length, ZDecimal width, ZDecimal height, ZString uQ)
		{
			ZString result = ZString.Empty;

			result += ZArchitecture.Core.Utilities.Round(Core.Constants.Length.ConvertSafe(length, uQ, Core.Constants.Length.Inches), 0);
			result += "x" + ZArchitecture.Core.Utilities.Round(Core.Constants.Length.ConvertSafe(width, uQ, Core.Constants.Length.Inches), 0);
			result += "x" + ZArchitecture.Core.Utilities.Round(Core.Constants.Length.ConvertSafe(height, uQ, Core.Constants.Length.Inches), 0);
			return result;
		}

		public void TestContainerNumber()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			ForwardingConsol consol2 = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol1.Containers.AddNew();
			var container2 = (CommonContainer)consol2.Containers.AddNew();
			container1.JC_ContainerNum = "111";
			container2.JC_ContainerNum = "222";
			PackLine aPackLine = shipment.OuterPackLines.AddNew();
			aPackLine.SetContainer(consol1, container1);
			aPackLine.SetContainer(consol2, container2);
			aPackLine.CurrentConsol = consol1;
			DocPackLines packLineWrapper = DocPackLines.New(aPackLine, Factory);
			AssertEquals("ContainerNumber should have container 1 details: ", container1.JC_ContainerNum, packLineWrapper.ContainerNumber);

			packLineWrapper.CurrentConsol = DocForwardingConsol.New(consol2, Factory);
			AssertEquals("ContainerNumber should have container 2 details: ", container2.JC_ContainerNum, packLineWrapper.ContainerNumber);
		}

		public void TestSealNumber()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			ForwardingConsol consol2 = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol1.Containers.AddNew();
			var container2 = (CommonContainer)consol2.Containers.AddNew();
			container1.JC_SealNum = "111";
			container2.JC_SealNum = "222";
			PackLine aPackLine = shipment.OuterPackLines.AddNew();
			aPackLine.SetContainer(consol1, container1);
			aPackLine.SetContainer(consol2, container2);
			aPackLine.CurrentConsol = consol1;
			DocPackLines packLineWrapper = DocPackLines.New(aPackLine, Factory);
			AssertEquals("ContainerNumber should have container 1 details: ", container1.JC_SealNum, packLineWrapper.SealNumber);

			packLineWrapper.CurrentConsol = DocForwardingConsol.New(consol2, Factory);
			AssertEquals("ContainerNumber should have container 2 details: ", container2.JC_SealNum, packLineWrapper.SealNumber);
		}

		public void TestContainerType()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol1 = shipment.Consols.AddNew();
			ForwardingConsol consol2 = shipment.Consols.AddNew();
			var container1 = (CommonContainer)consol1.Containers.AddNew();
			var container2 = (CommonContainer)consol2.Containers.AddNew();
			var containerType1 = Factory.LoadTop1<RefContainer>(new ZQuery());
			var containerType2 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.PK, SQLComparisonOperator.NotEqual, containerType1.PK));
			container1.JC_RC = containerType1.PK;
			container2.JC_RC = containerType2.PK;
			PackLine aPackLine = shipment.OuterPackLines.AddNew();
			aPackLine.SetContainer(consol1, container1);
			aPackLine.SetContainer(consol2, container2);
			aPackLine.CurrentConsol = consol1;
			DocPackLines packLineWrapper = DocPackLines.New(aPackLine, Factory);
			AssertEquals("ContainerNumber should have container 1 details: ", container1.Container.RC_Code, packLineWrapper.ContainerType);

			packLineWrapper.CurrentConsol = DocForwardingConsol.New(consol2, Factory);
			AssertEquals("ContainerNumber should have container 2 details: ", container2.Container.RC_Code, packLineWrapper.ContainerType);
		}

		#endregion

		public void TestIsHazardous()
		{
			PackLine line = Factory.New<PackLine>();
			DocPackLines lineWrapped = DocPackLines.New(line, Factory);

			AssertEquals(false, lineWrapped.IsHazardous);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals(true, lineWrapped.IsHazardous);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals(false, lineWrapped.IsHazardous);

			line.UNDGs.AddNew().DI_DG = Factory.NewWithValidTestData<UNDGSubstance>().PK;
			AssertEquals(true, lineWrapped.IsHazardous);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals(true, lineWrapped.IsHazardous);
		}

		public void TestHazardousDescription()
		{
			var line = Factory.New<PackLine>();
			var lineWrapped = DocPackLines.New(line, Factory);

			AssertEquals("HazardousDescription should be empty", " - 0 KG", lineWrapped.HazardousDescription);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;

			AssertEquals("HazardousDescription with com.code", "HAZ - 0 KG", lineWrapped.HazardousDescription);

			var undg = Factory.New<UNDGSubstance>();
			undg.DG_UNNO = "2345";
			undg.DG_PSN = "Shipping Name";
			undg.DG_Class = "6.2";
			undg.DG_PG = "II";
			undg.DG_Variant = "b";
			undg.DG_MP = "Y";
			undg.DG_SubLabel1 = "5";
			undg.DG_FlashPoint = string.Empty;

			line.UNDGs.AddNew().DI_DG = undg.PK;
			line.JL_ActualWeight = 123.00m;
			line.JL_ActualWeightUQ = Constants.Weight.Kilograms;
			line.JL_DetailedDescription = "Methyl";

			AssertEquals("HazardousDescription with com.code+UNDG", "HAZ - UN2345, Shipping Name, class 6.2 (5), PG II, MARINE POLLUTANT - 123 KG", lineWrapped.HazardousDescription);
		}

		public void TestHazardousDescriptionWithoutWeight()
		{
			var line = Factory.New<PackLine>();
			var lineWrapped = DocPackLines.New(line, Factory);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;

			var undg = Factory.New<UNDGSubstance>();
			undg.DG_UNNO = "2345";
			undg.DG_PSN = "Shipping Name";
			undg.DG_Class = "6.2";
			undg.DG_PG = "II";
			undg.DG_Variant = "b";
			undg.DG_MP = "Y";
			undg.DG_SubLabel1 = "5";
			undg.DG_FlashPoint = string.Empty;

			var undg2 = Factory.New<UNDGSubstance>();
			undg2.DG_UNNO = "2478";
			undg2.DG_PSN = "Zayden";
			undg2.DG_Class = "3.1";
			undg2.DG_PG = "I";
			undg2.DG_Variant = "c";
			undg2.DG_MP = "N";
			undg2.DG_SubLabel1 = "A";
			undg2.DG_FlashPoint = "27cc";

			line.UNDGs.AddNew().DI_DG = undg.PK;
			line.UNDGs.AddNew().DI_DG = undg2.PK;
			line.JL_ActualWeight = 123.00m;
			line.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			line.JL_DetailedDescription = "Methyl";

			AssertEquals(@"HAZ - UN2345, Shipping Name, class 6.2 (5), PG II, MARINE POLLUTANT
HAZ - UN2478, Zayden, class 3.1 (A), PG I, (27.0C c.c.), MARINE POLLUTANT", lineWrapped.HazardousDescriptionWithoutWeight);
		}

		public void TestContainerManifestMarksAndNumbers()
		{
			AssertEquals("", PackLineWrapper.ContainerManifestMarksAndNumbers);

			PackLineWrapper.ContainerManifestMarksAndNumbers = "Marks1";
			AssertEquals("Marks1", PackLineWrapper.ContainerManifestMarksAndNumbers);

			PackLineWrapper.ContainerManifestMarksAndNumbers += System.Environment.NewLine + "Marks2";
			AssertEquals("Marks1\r\nMarks2", PackLineWrapper.ContainerManifestMarksAndNumbers);
		}

		public void TestWeightVolumeAndPacks()
		{
			APackLine.JL_PackageCount = 9;
			APackLine.JL_F3_NKPackType = "PLT";
			APackLine.JL_ActualVolume = 10M;
			APackLine.JL_ActualVolumeUQ = "L";
			APackLine.JL_ActualWeight = 30M;
			APackLine.JL_ActualWeightUQ = "T";

			PackLineWrapper = DocPackLines.New(APackLine, Factory);
			AssertEquals("Using actual weight and volume", "30 T\n10 L\n9 Pallet", PackLineWrapper.WeightVolumeAndPacks);
		}

		public void TestContainerManifestWeightVolumeAndPacks()
		{
			PackLineWrapper = DocPackLines.New(APackLine, Factory);
			PackLineWrapper.ContainerManifestWeight = 2m;
			PackLineWrapper.ContainerManifestWeightUnit = Constants.Weight.Tonnes;
			PackLineWrapper.ContainerManifestVolume = 500m;
			PackLineWrapper.ContainerManifestVolumeUnit = Constants.Volume.Litre;
			PackLineWrapper.ContainerManifestPackage = 9;
			PackLineWrapper.ContainerManifestPackType = "PLT";
			AssertEquals("Using actual weight and volume", "2 T\n500 L\n9 PLT", PackLineWrapper.ContainerManifestWeightVolumeAndPacks);
		}

		public void TestEnsureGetContainerOnConsolDoesNotBlowUp()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			var cont1 = (CommonContainer)consol1.Containers.AddNew();
			cont1.JC_ContainerNum = "CONT1";
			var cont2 = (CommonContainer)consol1.Containers.AddNew();
			cont2.JC_ContainerNum = "CONT2";

			var shipment1 = (CommonShipment)consol1.Shipments.AddNew();
			var shipment2 = (CommonShipment)consol1.Shipments.AddNew();

			shipment1.JS_PackingMode = "BCN";
			shipment2.JS_PackingMode = "FCL";

			shipment2.JS_JS_ColoadMasterShipment = shipment1.PK;
			shipment1.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;

			var line1 = shipment1.OuterPackLines.AddNew();
			line1.JL_Description = "LINE1";
			line1.JL_ActualVolume = 10m;
			line1.JL_ActualWeight = 5m;
			line1.JL_PackageCount = 15;
			line1.JL_F3_NKPackType = "PLT";
			line1.JL_ActualVolumeUQ = "M3";
			line1.JL_ActualWeightUQ = "KG";
			line1.SetContainer(consol1, cont1);

			var line2 = shipment2.OuterPackLines.AddNew();
			line2.JL_Description = "LINE2";
			line2.JL_ActualVolume = 110m;
			line2.JL_ActualWeight = 15m;
			line2.JL_PackageCount = 115;
			line2.JL_F3_NKPackType = "PLT";
			line2.JL_ActualVolumeUQ = "M3";
			line2.JL_ActualWeightUQ = "KG";
			line2.SetContainer(consol1, cont1);

			Factory.Save();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(consol1, Factory);
			ZString expected = "CONTAINER/ WGT/ VOL/ PKG" + System.Environment.NewLine + "CONT1/ 5KG/ 10M3/ 15PLT";
			ZString expected2 = "CONTAINER/ WGT/ VOL/ PKG" + System.Environment.NewLine + "CONT1/ 15KG/ 110M3/ 115PLT";
			DocTranshipmentCollection transshipments = consolWrapper.Transhipments;
			transshipments.Sort("ShipmentPackLineDetails", ListSortDirection.Descending);
			AssertEquals("Should have packline details with container", expected, transshipments[0].ShipmentPackLineDetails);
			AssertEquals("Should have packline details with container", expected2, transshipments[1].ShipmentPackLineDetails);

			DocumentsDataRegistry.Instance.DisplayContainerDetailsOnConsol.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			consolWrapper.SetReportNameForTesting("Forwarding Instruction");
			transshipments = consolWrapper.Transhipments;
			transshipments.Sort("ShipmentPackLineDetails", ListSortDirection.Descending);
			AssertEquals("Should have packline details with container", expected, transshipments[0].ShipmentPackLineDetails);
			AssertEquals("Should have packline details with container", expected2, transshipments[1].ShipmentPackLineDetails);

			DocumentsDataRegistry.Instance.DisplayContainerDetailsOnConsol.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			transshipments = consolWrapper.Transhipments;
			transshipments.Sort("ShipmentPackLineDetails", ListSortDirection.Descending);
			AssertEquals("Should print nothing", "", transshipments[0].ShipmentPackLineDetails);
			AssertEquals("Should print nothing", "", transshipments[1].ShipmentPackLineDetails);
		}

		public void TestLengthUnit()
		{
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Feet;
			AssertEquals("FT", PackLineWrapper.LengthUnit);
		}
		public void TestHeightUnit()
		{
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Metres;
			AssertEquals("M", PackLineWrapper.HeightUnit);
		}
		public void TestWidthUnit()
		{
			APackLine.JL_UnitOfDimension = Core.Constants.Length.Yards;
			AssertEquals("YD", PackLineWrapper.WidthUnit);
		}

		public void TestActualVolumeUQ()
		{
			AssertEquals(Env.Registry.FreightVolumeUnit, PackLineWrapper.ActualVolumeUQ);
			APackLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			AssertEquals("L", PackLineWrapper.ActualVolumeUQ);
		}

		public void TestActualVolume()
		{
			AssertEquals(0M, PackLineWrapper.ActualVolume);
			APackLine.JL_ActualVolume = 1000M;
			AssertEquals(1000M, PackLineWrapper.ActualVolume);
		}

		public void TestActualWeight()
		{
			AssertEquals(0M, PackLineWrapper.ActualWeight);
			APackLine.JL_ActualWeight = 1000M;
			AssertEquals(1000M, PackLineWrapper.ActualWeight);
		}

		public void TestActualWeightUQ()
		{
			AssertEquals(Env.Registry.FreightWeightUnit, PackLineWrapper.ActualWeightUQ);
			APackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("G", PackLineWrapper.ActualWeightUQ);
		}

		public void TestDamaged()
		{
			AssertEquals(0, PackLineWrapper.Damaged);
			APackLine.JL_Damaged = 10;
			AssertEquals(10, PackLineWrapper.Damaged);
		}

		public void TestDescription()
		{
			AssertEquals("", PackLineWrapper.Description);
			APackLine.JL_Description = "TEST packline desc";
			AssertEquals("TEST packline desc", PackLineWrapper.Description);
		}

		public void TestEndItemNo()
		{
			AssertEquals(ZShort.Zero, PackLineWrapper.EndItemNo);
			APackLine.JL_EndItemNo = 10;
			AssertEquals(ZShort.Parse("10"), PackLineWrapper.EndItemNo);
		}

		public void TestFreightMode()
		{
			APackLine.JL_FreightMode = "OUT";
			AssertEquals("OUT", PackLineWrapper.FreightMode);
		}

		public void TestPackType()
		{
			APackLine.JL_F3_NKPackType = "BOX";
			AssertEquals("BOX", PackLineWrapper.PackType);
		}

		public void TestPackTypeDescription()
		{
			APackLine.JL_F3_NKPackType = "PLT";
			AssertEquals("Pallet", PackLineWrapper.PackTypeDescription);
			APackLine.JL_F3_NKPackType = "CTN";
			AssertEquals("Carton", PackLineWrapper.PackTypeDescription);
		}

		public void TestPackTypeDescriptionPlural()
		{
			APackLine.JL_F3_NKPackType = "PLT";
			AssertEquals("Pallet(s)", PackLineWrapper.PackTypeDescriptionPlural);
			APackLine.JL_F3_NKPackType = "CTN";
			AssertEquals("Carton(s)", PackLineWrapper.PackTypeDescriptionPlural);
		}

		public void TestHeight()
		{
			AssertEquals(0M, PackLineWrapper.Height);
			APackLine.JL_Height = 0.2M;
			AssertEquals(0.2M, PackLineWrapper.Height);
		}

		public void TestShipment()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingPackLine packLine = shipment.OuterPackLines.AddNew();
			DocPackLines packLineWrapper = DocPackLines.New(packLine, Factory);

			AssertEquals(shipment, packLineWrapper.Shipment.CommonShipment);

			packLine = Factory.New<ForwardingPackLine>();
			packLineWrapper = DocPackLines.New(packLine, Factory);

			AssertNull(packLineWrapper.Shipment);
		}

		public void TestShipmentOverride()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingPackLine packLine = shipment.OuterPackLines.AddNew();

			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			DocPackLines packLineWrapper = DocPackLines.New(packLine, shipment2, Factory);

			AssertEquals(shipment2, packLineWrapper.Shipment.CommonShipment);
		}

		public void TestConsignee()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AAAA";
			org1.OH_FullName = "A Company";
			Shipment.ConsigneePK = org1.PK;

			AssertEquals("Consignee should be calculated from Packline", org1.OH_FullName, PackLineWrapper.Consignee);
		}

		public void TestConsignor()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AAAA";
			org1.OH_FullName = "A Company";
			Shipment.ConsignorPK = org1.PK;
			AssertEquals("Consignor should be calculated from Packline", org1.OH_FullName, PackLineWrapper.Consignor);
		}

		public void TestMarksAndNumbers()
		{
			ZString test = "Mark";
			ZString test2 = "Numbers";

			Shipment.JS_MarksAndNumbers = test;
			AssertEquals("Use the marks and numbers on the shipment if not set on the packline", test, PackLineWrapper.MarksAndNumbers);

			((PackLine)PackLineWrapper.WrappedObject).JL_MarksAndNumbers = test2;
			AssertEquals("Use the marks and numbers on the packline when available.", test2, PackLineWrapper.MarksAndNumbers);
		}

		public void TestHouseBill()
		{
			ZString test = "Bill";
			Shipment.JS_HouseBill = test;
			AssertEquals("Consignee should be calculated from Packline", test.ToUpper(), PackLineWrapper.HouseBill);
		}

		public void TestGoodsDescription()
		{
			ZString test = "Bill";
			Shipment.JS_GoodsDescription = test;
			AssertEquals("Consignee should be calculated from Packline", test, PackLineWrapper.GoodsDescription);
		}

		public void TestDestination()
		{
			ZString test = "ARGH";
			Shipment.JS_RL_NKDestination = test;
			AssertEquals("Consignee should be calculated from Packline", test, PackLineWrapper.Destination);
		}

		public void TestCustomAttribute()
		{
			APackLine.JL_CustomAttrib1 = "CUSTOMATTRB1";
			APackLine.JL_CustomAttrib2 = "CUSTOMATTRB2";
			APackLine.JL_CustomAttrib3 = "CUSTOMATTRB3";
			APackLine.JL_CustomAttrib4 = "CUSTOMATTRB4";
			AssertEquals("Custom Attribute1", "CUSTOMATTRB1", PackLineWrapper.CustomAttribute1);
			AssertEquals("Custom Attribute2", "CUSTOMATTRB2", PackLineWrapper.CustomAttribute2);
			AssertEquals("Custom Attribute3", "CUSTOMATTRB3", PackLineWrapper.CustomAttribute3);
			AssertEquals("Custom Attribute4", "CUSTOMATTRB4", PackLineWrapper.CustomAttribute4);
		}

		public void TestLength()
		{
			AssertEquals(0M, PackLineWrapper.Length);
			APackLine.JL_Length = 12.5M;
			AssertEquals(12.5M, PackLineWrapper.Length);
		}

		public void TestOutturn()
		{
			AssertEquals(0, PackLineWrapper.Outturn);
			APackLine.JL_Outturn = 10;
			AssertEquals(10, PackLineWrapper.Outturn);
		}
		public void TestOutturnedWeight()
		{
			AssertEquals(0.00m, PackLineWrapper.OutturnedWeight);
			APackLine.JL_OutturnedWeight = 1000.99m;
			AssertEquals(1000.99m, PackLineWrapper.OutturnedWeight);
		}
		public void TestOutturnedVolume()
		{
			AssertEquals(0.00m, PackLineWrapper.OutturnedVolume);
			APackLine.JL_OutturnedVolume = 1000.99m;
			AssertEquals(1000.99m, PackLineWrapper.OutturnedVolume);
		}

		public void TestPillaged()
		{
			AssertEquals(0, PackLineWrapper.Pillaged);
			APackLine.JL_Pillaged = 10;
			AssertEquals(10, PackLineWrapper.Pillaged);
		}

		public void TestPackagesDelivered()
		{
			AssertEquals(0, PackLineWrapper.PackagesDelivered);
			APackLine.JL_FreightMode = FreightConstants.OuterPackType;
			CommonPickupDeliveryConfirm leg = Shipment.DeliveryConfirms.AddNew();
			CommonConfirmDivot divot = leg.GetDivot(APackLine);
			divot.J8_PackagesDelivered = 20;
			AssertEquals(20, PackLineWrapper.PackagesDelivered);
		}

		public void TestContainerNum()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			JobSailing sailing = CreateNewSailing(true);
			consol.Transports[0].JW_JX = sailing.PK;
			consol.JK_RL_NKLoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "AAAA1111111";

			CreateContainerPackLinePivot(container1.PK, APackLine.PK);
			AssertEquals("AAAA1111111", PackLineWrapper.FirstImportContainer.ContainerNumber);
		}

		public void TestFirstImportContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			JobSailing sailing = CreateNewSailing(true);
			consol.JK_RL_NKLoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
			consol.Transports[0].JW_JX = sailing.PK;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "BLAA1111111";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_SealNum = "Seal 123";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			JobSailing sailing2 = CreateNewSailing(false);
			consol2.JK_RL_NKLoadPort = sailing2.JX_JA_RL_NKPortOfLoading;
			consol2.JK_RL_NKDischargePort = sailing2.JX_JB_RL_NKPortOfDischarge;
			consol2.Transports[0].JW_JX = sailing2.PK;

			var container2 = (CommonContainer)consol2.Containers.AddNew();
			container2.JC_ContainerNum = "YAAA1111111";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container2.JC_SealNum = "Seal 333";

			CreateContainerPackLinePivot(container1.PK, APackLine.PK);
			CreateContainerPackLinePivot(container2.PK, APackLine.PK);
			AssertNotNull(PackLineWrapper.FirstImportContainer);

			AssertEquals("BLAA1111111", PackLineWrapper.FirstImportContainer.ContainerNumber);
			AssertEquals("FCL", PackLineWrapper.FirstImportContainer.ContainerMode);
			AssertEquals("Seal 123", PackLineWrapper.FirstImportContainer.SealNumber);
		}

		public void TestFirstExportContainer()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			JobSailing sailing = CreateNewSailing(false);
			consol.JK_RL_NKLoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
			consol.Transports[0].JW_JX = sailing.PK;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "BLAA22222";
			container1.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container1.JC_SealNum = "Seal 123";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			JobSailing sailing2 = CreateNewSailing(true);
			consol2.JK_RL_NKLoadPort = sailing2.JX_JA_RL_NKPortOfLoading;
			consol2.JK_RL_NKDischargePort = sailing2.JX_JB_RL_NKPortOfDischarge;
			consol2.Transports[0].JW_JX = sailing2.PK;

			var container2 = (CommonContainer)consol2.Containers.AddNew();
			container2.JC_ContainerNum = "NOOO1111111";
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			container2.JC_SealNum = "Seal 333";

			CreateContainerPackLinePivot(container1.PK, APackLine.PK);
			CreateContainerPackLinePivot(container2.PK, APackLine.PK);
			AssertNotNull(PackLineWrapper.FirstExportContainer);

			AssertEquals("BLAA22222", PackLineWrapper.FirstExportContainer.ContainerNumber);
			AssertEquals("FCL", PackLineWrapper.FirstExportContainer.ContainerMode);
			AssertEquals("Seal 123", PackLineWrapper.FirstExportContainer.SealNumber);
		}
		public void TestConvertedWeight()
		{
			AssertEquals(0M, PackLineWrapper.ConvertedWeight);
			APackLine.JL_ActualWeight = 1000M;
			AssertEquals(1000M, PackLineWrapper.ConvertedWeight);

			APackLine.JL_ActualWeightUQ = Env.Registry.FreightWeightUnit;
			AssertEquals(1000M, PackLineWrapper.ConvertedWeight);

			APackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals(1M, PackLineWrapper.ConvertedWeight);
		}

		public void TestConvertedVolume()
		{
			AssertEquals(0M, PackLineWrapper.ConvertedVolume);
			APackLine.JL_ActualVolume = 1000M;
			AssertEquals(1000M, PackLineWrapper.ConvertedVolume);

			APackLine.JL_ActualVolumeUQ = Env.Registry.FreightVolumeUnit;
			AssertEquals(1000M, PackLineWrapper.ConvertedVolume);

			APackLine.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			AssertEquals(1000M * 0.001M, PackLineWrapper.ConvertedVolume);
		}

		public void TestWeightUQ()
		{
			AssertEquals(Env.Registry.FreightWeightUnit, PackLineWrapper.WeightUQ);
			APackLine.JL_ActualWeightUQ = Core.Constants.Weight.Grams;
			AssertEquals("G", PackLineWrapper.WeightUQ);
		}

		public void TestVolumeUQ()
		{
			AssertEquals(Env.Registry.FreightVolumeUnit, PackLineWrapper.VolumeUQ);
			APackLine.JL_ActualVolumeUQ = Core.Constants.Volume.CubicMetres;
			AssertEquals("M3", PackLineWrapper.VolumeUQ);
		}

		public void TestOutturnComments()
		{
			AssertEquals("No comments", "", PackLineWrapper.OutturnComments);

			APackLine.JL_OutturnComment = "\r\nNo comments\r\nNo comments line two";
			AssertEquals("Comments", "No comments\r\nNo comments line two", PackLineWrapper.OutturnComments);
		}

		public void TestRemarks()
		{
			AssertEquals("No Dimensions, No comments", "", PackLineWrapper.Remarks);

			APackLine.JL_OutturnComment = "\r\nNo comments\r\nNo comments line two";
			AssertEquals("No Dimensions, Comments", "No comments\r\nNo comments line two", PackLineWrapper.Remarks);

			APackLine.JL_OutturnComment = "";
			APackLine.JL_Length = 10M;
			APackLine.JL_PackageCount = 2;
			APackLine.JL_UnitOfDimension = "M";
			APackLine.JL_Outturn = 0;
			AssertEquals("Dimensions, No Comments", PackLineWrapper.Dimensions, PackLineWrapper.Remarks);

			APackLine.JL_Outturn = 2;
			APackLine.JL_OutturnedLength = 2M;
			AssertEquals("Outturned Dimensions, No Comments", PackLineWrapper.OutturnedDimensions, PackLineWrapper.Remarks);

			APackLine.JL_OutturnComment = "\r\nNo comments\r\nNo comments line two";
			AssertEquals("Outturned Dimensions, Comments", PackLineWrapper.OutturnedDimensions + System.Environment.NewLine + "No comments\r\nNo comments line two", PackLineWrapper.Remarks);
		}

		public void TestCargoLocationAndPacksWithShipmentLocationOfGoods()
		{
			APackLine.JL_F3_NKPackType = "CNT";

			var location1 = APackLine.PackLocations.AddNew();
			location1.JQ_NoPackages = 3;
			location1.JQ_WarehouseLocation = "WAREHOUSE1";

			var location2 = APackLine.PackLocations.AddNew();
			location2.JQ_NoPackages = 20;
			location2.JQ_WarehouseLocation = "BLOCK BC";

			AssertEquals("WAREHOUSE1,  Packs: 3 CNT   BLOCK BC,  Packs: 20 CNT", PackLineWrapper.CargoLocationAndPacksLine);

			var testConsignor = Factory.New<OrgHeader>();
			testConsignor.OH_FullName = "Test Consignor";

			Shipment.ConsignorPK = testConsignor.PK;

			var address = Factory.New<OrgAddress>();
			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Pickup);
			address.OA_Address1 = "Address 1";
			address.OA_Address2 = "Address 2";
			testConsignor.Addresses.Add(address);

			APackLine.JL_PackageCount = 30;

			AssertEquals("CargoLocationAndPacksWithShipmentLocationOfGoods", "WAREHOUSE1,  Packs: 3 CNT   BLOCK BC,  Packs: 20 CNT   ADDRESS 1\nADDRESS 2\n" + Env.CurrentCompany.Country.Description.ToUpper() + ",  Packs: 7 CNT", PackLineWrapper.CargoLocationAndPacksWithShipmentLocationOfGoods);
		}

		public void TestCargoLocationAndPacks()
		{
			AssertEquals("", PackLineWrapper.CargoLocationAndPacks);
			AssertEquals("", PackLineWrapper.CargoLocationAndPacksLine);
			APackLine.JL_F3_NKPackType = "CNT";

			WhsRow row = Factory.New<WhsRow>();
			row.WR_Name = "BLOCK AA";

			WhsLocation whsLoc = Factory.New<WhsLocation>();
			whsLoc.WLV_WR = row.PK;

			var location1 = APackLine.PackLocations.AddNew();
			location1.JQ_NoPackages = 3;
			location1.JQ_WarehouseLocation = "WAREHOUSE1";
			location1.JQ_WL = whsLoc.PK;

			var location2 = APackLine.PackLocations.AddNew();
			location2.JQ_WarehouseLocation = "BLOCK BC";
			location2.JQ_NoPackages = 20;
			location2.JQ_WL = whsLoc.PK;

			ZBool oldRegVal = new ZBool();

			try
			{
				oldRegVal = Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.Value;

				Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

				AssertEquals("WAREHOUSE1,  Packs: 3 CNT\nBLOCK BC,  Packs: 20 CNT", PackLineWrapper.CargoLocationAndPacks);
				AssertEquals("WAREHOUSE1,  Packs: 3 CNT   BLOCK BC,  Packs: 20 CNT", PackLineWrapper.CargoLocationAndPacksLine);

				Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

				AssertEquals("BLOCK AA,  Packs: 3 CNT\nBLOCK AA,  Packs: 20 CNT", PackLineWrapper.CargoLocationAndPacks);
				AssertEquals("BLOCK AA,  Packs: 3 CNT   BLOCK AA,  Packs: 20 CNT", PackLineWrapper.CargoLocationAndPacksLine);
			}
			finally
			{
				Registry.Business.Warehouse.WarehouseDataRegistry.Instance.FreightLocationIsTrueWHSLocation.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, oldRegVal);
			}
		}

		public void TestPacksAndLocationLine()
		{
			AssertEquals("", PackLineWrapper.PacksAndLocationLine);
			APackLine.JL_F3_NKPackType = "CNT";

			var location1 = APackLine.PackLocations.AddNew();
			location1.JQ_NoPackages = 3;
			location1.JQ_WarehouseLocation = "WAREHOUSE1";

			var location2 = APackLine.PackLocations.AddNew();
			location2.JQ_NoPackages = 20;
			location2.JQ_WarehouseLocation = "BLOCK BC";

			AssertEquals("3 CNT WAREHOUSE1,   20 CNT BLOCK BC", PackLineWrapper.PacksAndLocationLine);
		}
		public void TestContainerManifestWeight()
		{
			//not a packline field so only testing initial value
			AssertEquals(0M, PackLineWrapper.ContainerManifestWeight);
		}

		public void TestContainerManifestVolume()
		{
			//not a packline field so only testing initial value
			AssertEquals(0M, PackLineWrapper.ContainerManifestVolume);
		}

		public void TestContainerManifestPackage()
		{
			//not a packline field so only testing initial value
			AssertEquals(0, PackLineWrapper.ContainerManifestPackage);
		}

		public void TestPackageDetailsAndHandlingInstructionNote()
		{
			AssertEquals("", PackLineWrapper.PackageDetailsAndHandlingInstructionNote);

			var note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_ParentID = Shipment.PK;
			note.ST_Table = Shipment.TableName;
			note.ST_NoteDataAsText = "Do not break\nHandle with care.";

			AssertEquals("Do not break\nHandle with care.", PackLineWrapper.PackageDetailsAndHandlingInstructionNote);

			PackLineWrapper.PackageDetails = "10 PALLETS 12 KG 5 M3\n20 CARTONS 1 KG 5 M3";
			AssertEquals("Do not break\nHandle with care.", PackLineWrapper.PackageDetailsAndHandlingInstructionNote);

			PackLineWrapper.IsGroupPackLine = ZBool.True;
			AssertEquals("10 PALLETS 12 KG 5 M3\n20 CARTONS 1 KG 5 M3\nDo not break\nHandle with care.", PackLineWrapper.PackageDetailsAndHandlingInstructionNote);
		}

		public void TestDimensions()
		{
			AssertEquals("", PackLineWrapper.Dimensions);

			APackLine.JL_Length = 10M;
			APackLine.JL_UnitOfDimension = "M";
			AssertEquals("(L): 10  (W): 0  (H): 0 M", PackLineWrapper.Dimensions);

			APackLine.JL_Width = 2M;
			AssertEquals("(L): 10  (W): 2  (H): 0 M", PackLineWrapper.Dimensions);

			APackLine.JL_Height = 3M;
			AssertEquals("(L): 10  (W): 2  (H): 3 M", PackLineWrapper.Dimensions);
		}

		public void TestOutturnedDimensions()
		{
			AssertEquals("", PackLineWrapper.OutturnedDimensions);

			APackLine.JL_OutturnedLength = 10M;
			APackLine.JL_UnitOfDimension = "M";
			AssertEquals("(L): 10  (W): 0  (H): 0 M", PackLineWrapper.OutturnedDimensions);

			APackLine.JL_OutturnedWidth = 2M;
			AssertEquals("(L): 10  (W): 2  (H): 0 M", PackLineWrapper.OutturnedDimensions);

			APackLine.JL_OutturnedHeight = 3M;
			AssertEquals("(L): 10  (W): 2  (H): 3 M", PackLineWrapper.OutturnedDimensions);
		}

		public void TestOutturnedOrActualDimensions()
		{
			APackLine.JL_Length = 10M;
			APackLine.JL_PackageCount = 2;
			APackLine.JL_UnitOfDimension = "M";
			APackLine.JL_Outturn = 0;
			AssertEquals(PackLineWrapper.Dimensions, PackLineWrapper.OutturnedOrActualDimensions);

			APackLine.JL_Outturn = 2;
			APackLine.JL_OutturnedLength = 2M;
			AssertEquals(PackLineWrapper.OutturnedDimensions, PackLineWrapper.OutturnedOrActualDimensions);
		}

		public void TestPackageDetailsWithDimensions()
		{
			AssertEquals("", PackLineWrapper.PackageDetailsAndHandlingInstructionNote);

			PackLineWrapper.IsGroupPackLine = ZBool.False;
			AssertEquals("", PackLineWrapper.PackageDetailsAndHandlingInstructionNote);

			APackLine.JL_Length = 10M;
			APackLine.JL_Width = 2M;
			APackLine.JL_Height = 3M;
			APackLine.JL_UnitOfDimension = "M";
			AssertEquals("(L): 10  (W): 2  (H): 3 M", PackLineWrapper.PackageDetailsAndHandlingInstructionNote);

			var note = Shipment.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.HandlingInstructions.Description;
			note.ST_ParentID = Shipment.PK;
			note.ST_Table = Shipment.TableName;
			note.ST_NoteDataAsText = "Do not break\nHandle with care.";

			AssertEquals("(L): 10  (W): 2  (H): 3 M\n" + "Do not break\nHandle with care.", PackLineWrapper.PackageDetailsAndHandlingInstructionNote);
		}

		public void TestFirstExportContainerNumber()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			JobSailing sailing = CreateNewSailing(false);
			consol.JK_RL_NKLoadPort = sailing.JX_JA_RL_NKPortOfLoading;
			consol.JK_RL_NKDischargePort = sailing.JX_JB_RL_NKPortOfDischarge;
			consol.Transports[0].JW_JX = sailing.PK;

			var container1 = (CommonContainer)consol.Containers.AddNew();
			container1.JC_ContainerNum = "BLAA22222";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Transport transport2 = consol2.Transports[0];
			transport2.JW_JX = CreateNewSailing(true).PK;

			var container2 = (CommonContainer)consol2.Containers.AddNew();
			container2.JC_ContainerNum = "NOOO1111111";

			CreateContainerPackLinePivot(container1.PK, APackLine.PK);
			CreateContainerPackLinePivot(container2.PK, APackLine.PK);
			AssertNotNull(PackLineWrapper.FirstExportContainer);

			AssertEquals("BLAA22222", PackLineWrapper.FirstExportContainer.ContainerNumber);
		}
		public void TestFormattedWeightAndUnit()
		{
			AssertEquals("", PackLineWrapper.FormattedWeightAndUnit);
			APackLine.JL_ActualWeight = 125.999M;
			AssertEquals("125.999 KG", PackLineWrapper.FormattedWeightAndUnit);
			APackLine.JL_ActualWeightUQ = "G";
			AssertEquals("125.999 G", PackLineWrapper.FormattedWeightAndUnit);
		}

		public void TestFormattedVolumeAndUnit()
		{
			AssertEquals("", PackLineWrapper.FormattedVolumeAndUnit);
			APackLine.JL_ActualVolume = 4.0M;
			AssertEquals("4 M3", PackLineWrapper.FormattedVolumeAndUnit);
			APackLine.JL_ActualVolumeUQ = "L";
			AssertEquals("4 L", PackLineWrapper.FormattedVolumeAndUnit);
		}

		public void TestFormattedLength()
		{
			AssertEquals("", PackLineWrapper.FormattedLength);
			APackLine.JL_Length = 0.100M;
			AssertEquals("0.1", PackLineWrapper.FormattedLength);
		}

		public void TestFormattedWidth()
		{
			AssertEquals("", PackLineWrapper.FormattedWidth);
			APackLine.JL_Width = 0.010M;
			AssertEquals("0.01", PackLineWrapper.FormattedWidth);
		}

		public void TestFormattedHeight()
		{
			AssertEquals("", PackLineWrapper.FormattedHeight);
			APackLine.JL_Height = 0.0010M;
			AssertEquals("0.001", PackLineWrapper.FormattedHeight);
		}

		public void TestDimensionUnit()
		{
			AssertEquals("", PackLineWrapper.DimensionUnit);
			APackLine.JL_UnitOfDimension = "M";
			AssertEquals("", PackLineWrapper.DimensionUnit);
			APackLine.JL_Length = 4.5M;
			AssertEquals("M", PackLineWrapper.DimensionUnit);
			APackLine.JL_Length = 0M;
			AssertEquals("", PackLineWrapper.DimensionUnit);
			APackLine.JL_Width = 2.99M;
			AssertEquals("M", PackLineWrapper.DimensionUnit);
			APackLine.JL_Width = 0M;
			AssertEquals("", PackLineWrapper.DimensionUnit);
			APackLine.JL_Height = 1.09M;
			AssertEquals("M", PackLineWrapper.DimensionUnit);
			APackLine.JL_Height = 0M;
			AssertEquals("", PackLineWrapper.DimensionUnit);

			APackLine.JL_Height = 10M;
			APackLine.JL_Width = 20M;
			APackLine.JL_Length = 30M;
			AssertEquals("M", PackLineWrapper.DimensionUnit);
		}

		public void TestUNDG()
		{
			AssertNull(PackLineWrapper.UNDG);
			var uNDG = Factory.LoadTop1<UNDGSubstance>(new ZQuery());
			APackLine.UNDGs.AddNew().DI_DG = uNDG.PK;
			AssertEquals(uNDG.DG_UNNO, PackLineWrapper.UNDG.UNNumber);
		}

		public void TestCommodity()
		{
			APackLine.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			AssertEquals("GEN", PackLineWrapper.Commodity.Code);

			APackLine.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			AssertEquals("HAZ", PackLineWrapper.Commodity.Code);
		}

		public void TestShipmentConsolContainer()
		{
			AssertNull("No Container for this packline", PackLineWrapper.ShipmentConsolContainer);

			var consol1 = (CommonConsol)Shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "MYKEL";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			AssertNull("Consol exist but no containers yet. Should not blow up here", PackLineWrapper.ShipmentConsolContainer);

			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";

			APackLine.SetContainer(consol1, container1);
			PackLineWrapper = DocPackLines.New(APackLine, Factory);
			AssertEquals("Packline attached to Container1", "CONT1", PackLineWrapper.ShipmentConsolContainer.ContainerNumber);

			var consol2 = (CommonConsol)Shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "USLAX";
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";

			APackLine.SetContainer(consol2, container2);
			PackLineWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("Should be showing Import Container", "CONT2", PackLineWrapper.ShipmentConsolContainer.ContainerNumber);

			PackLineWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("Should be showing Export Container", "CONT1", PackLineWrapper.ShipmentConsolContainer.ContainerNumber);
		}

		//DocWrappers should not be setting CurrentConsol on Business Layer, so it has its own
		public void TestCurrentConsol()
		{
			AssertNull("DocWrapper CurrentConsol should be null", PackLineWrapper.CurrentConsol);

			var consol = Factory.New<ForwardingConsol>();
			DocForwardingConsol docForwardingConsol = DocForwardingConsol.New(consol, Factory);
			PackLineWrapper.CurrentConsol = docForwardingConsol;
			AssertEquals("DocWrapper CurrentConsol should be DocForwardingConsol", docForwardingConsol, PackLineWrapper.CurrentConsol);
		}

		public void TestContainer()
		{
			AssertNull("No Container for this packline", PackLineWrapper.Container);
			ForwardingConsol consol1 = Shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "MYKEL";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			AssertNull("Consol exist but no containers yet. Should not blow up here", PackLineWrapper.Container);

			var container1 = (CommonContainer)consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1";

			APackLine.SetContainer(consol1, container1);
			APackLine.CurrentConsol = consol1;
			PackLineWrapper = DocPackLines.New(APackLine, Factory);
			AssertEquals("Packline attached to Container1", "CONT1", PackLineWrapper.Container.ContainerNumber);

			ForwardingConsol consol2 = Shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "USLAX";
			var container2 = (CommonContainer)consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT2";

			APackLine.SetContainer(consol2, container2);
			PackLineWrapper.CurrentConsol = DocForwardingConsol.New(consol2, Factory);
			AssertEquals("Should be showing Container 2", "CONT2", PackLineWrapper.Container.ContainerNumber);

			PackLineWrapper.CurrentConsol = DocForwardingConsol.New(consol1, Factory);
			AssertEquals("Should be showing Container 1 again", "CONT1", PackLineWrapper.Container.ContainerNumber);
		}

		public void TestGetContainerOnConsol()
		{
			ForwardingConsol consol1 = Shipment.Consols.AddNew();
			ForwardingConsol consol2 = Shipment.Consols.AddNew();
			var cont1 = (CommonContainer)consol1.Containers.AddNew();
			cont1.JC_ContainerNum = "C1";
			var cont2 = (CommonContainer)consol2.Containers.AddNew();
			cont2.JC_ContainerNum = "C2";
			APackLine.SetContainer(consol1, cont1);
			APackLine.SetContainer(consol2, cont2);
			Factory.Save();

			PackLineWrapper = DocPackLines.New(APackLine, Factory);
			AssertEquals("Should get container 1", "C1", PackLineWrapper.GetContainerOnConsol(DocForwardingConsol.New(consol1, Factory)).ContainerNumber);
			AssertEquals("Should get container 2", "C2", PackLineWrapper.GetContainerOnConsol(DocForwardingConsol.New(consol2, Factory)).ContainerNumber);
		}

		public void TestContainerPackingOrder()
		{
			APackLine.JL_ContainerPackingOrder = 3;
			PackLineWrapper = DocPackLines.New(APackLine, Factory);
			AssertEquals("Should get 3", 3, PackLineWrapper.ContainerPackingOrder);

			APackLine.JL_ContainerPackingOrder = 5;
			PackLineWrapper = DocPackLines.New(APackLine, Factory);
			AssertEquals("Should get 5", 5, PackLineWrapper.ContainerPackingOrder);
		}

		#region Collections

		public void TestDocPackLocationCollection()
		{
			PackLine line = Factory.New<PackLine>();
			line.PackLocations.AddNew();
			line.PackLocations.AddNew();
			line.PackLocations.AddNew();

			PackLineWrapper = DocPackLines.New(line, Factory);
			AssertEquals(3, PackLineWrapper.PackLocationCollection.Count);
		}

		public void TestPackLocations()
		{
			APackLine.PackLocations.RemoveAll();
			AssertEquals("PackLocations.Count", 0, PackLineWrapper.PackLocations.Count);

			APackLine.PackLocations.AddNew();
			APackLine.PackLocations.AddNew();
			AssertEquals("PackLocations.Count", 2, PackLineWrapper.PackLocations.Count);
		}

		#endregion

		#region Implementation

		JobSailing CreateNewSailing(bool import)
		{
			ZString domesticPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			ZString foreignPort;

			if (GlbBranch.CurrentBranch.HomePort.RL_RN_NKCountryCode == Enterprise.Core.Constants.CountryCodes.Singapore)
			{
				foreignPort = "AUBNE";
			}
			else
			{
				foreignPort = "SGSIN";
			}

			if (import)
			{
				return CreateNewSailing(foreignPort, domesticPort);
			}
			else
			{
				return CreateNewSailing(domesticPort, foreignPort);
			}
		}

		JobSailing CreateNewSailing(ZString portOfLoading, ZString portOfDischarge)
		{
			var voyage = Factory.New<JobVoyage>();
			var vessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "123";

			var origin = Factory.New<VoyageOrigin>();
			origin.JA_RL_NKPortOfLoading = portOfLoading;
			origin.JA_E_DEP = ZDateTime.Today.AddDays(-10);
			voyage.Origins.Add(origin);

			var destination = Factory.New<VoyageDestination>();
			destination.JB_RL_NKPortOfDischarge = portOfDischarge;
			destination.JB_E_ARV = ZDateTime.Today;
			voyage.Destinations.Add(destination);

			voyage.GenerateSailings();
			return voyage.Sailings[0];
		}

		protected override void SetUp()
		{
			Shipment = Factory.New<ForwardingShipment>();
			APackLine = Shipment.OuterPackLines.Count > 0 ? Shipment.OuterPackLines[0] : Shipment.OuterPackLines.AddNew();
			PackLineWrapper = DocPackLines.New(APackLine, Factory);
			AssertNotNull("Packline wrapper should not be null", PackLineWrapper);
			base.SetUp();
		}

		void CreateContainerPackLinePivot(ZGuid containerPK, ZGuid packLinePK)
		{
			var line = Factory.Load<PackLine>(packLinePK);
			var container = Factory.Load<ForwardingContainer>(containerPK);

			if (line != null && container != null && line.Shipment != null && container.Consol != null && !line.Shipment.Consols.Contains(container.Consol))
			{
				line.Shipment.Consols.Add(container.Consol);
			}

			var pivot = Factory.New<JobContainerPackPivot>();
			pivot.J6_JL = packLinePK;
			pivot.J6_JC = containerPK;
		}

		DocPackLines PackLineWrapper;
		PackLine APackLine;
		ForwardingShipment Shipment;

		#endregion
	}
}
