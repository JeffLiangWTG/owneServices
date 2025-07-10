using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocBillofLadingContainer))]
	sealed class DocBillofLadingContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeliveryModeFromConstructor()
		{
			var registryDeliveryList = FreightDataRegistry.Instance.ContainerDeliveryModeList.Value;
			registryDeliveryList.Add(new DeliveryMode { Code = "ABC", UserDefinedCode = "ABC" });
			registryDeliveryList.Add(new DeliveryMode { Code = "EFG", Description = (NoResString)"Blah blah blah", UserDefinedCode = "EFG", UserDefinedDescription = (NoResString)"Blah blah blah" });
			FreightDataRegistry.Instance.ContainerDeliveryModeList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryDeliveryList);

			CommonConsol consol = Factory.New<CommonConsol>();
			FreightContainer.JC_JK = consol.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;

			CommonShipment shipment = Factory.New<CommonShipment>();
			FreightContainer.JC_DeliveryMode = "";
			DocContainer docContainer = DocContainer.New(FreightContainer, shipment, Factory);
			DocBillofLadingContainer hBLContainer = DocBillofLadingContainer.New(docContainer);
			AssertEquals("Delivery Mode", "-", hBLContainer.DeliveryMode);

			FreightContainer.JC_DeliveryMode = Core.Constants.DeliveryModes.Codes.CY_CY;
			docContainer = DocContainer.New(FreightContainer, shipment, Factory);
			hBLContainer = DocBillofLadingContainer.New(docContainer);
			AssertEquals("Delivery Mode", Core.Constants.DeliveryModes.Descriptions.CY_CY + "*", hBLContainer.DeliveryMode);

			FreightContainer.JC_DeliveryMode = "ABC";
			docContainer = DocContainer.New(FreightContainer, shipment, Factory);
			hBLContainer = DocBillofLadingContainer.New(docContainer);
			AssertEquals("Delivery Mode", "ABC", hBLContainer.DeliveryMode);

			FreightContainer.JC_DeliveryMode = "EFG";
			docContainer = DocContainer.New(FreightContainer, shipment, Factory);
			hBLContainer = DocBillofLadingContainer.New(docContainer);
			AssertEquals("Delivery Mode", "Blah blah blah", hBLContainer.DeliveryMode);
		}

		public void TestPackingDetailsBreakdown()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_FreightMode = FreightConstants.OuterPackType;
			packLine1.JL_PackageCount = 2;
			packLine1.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine1.JL_Description = "Goods\nDescription";
			packLine1.JL_RH_NKCommodityCode = "HAZ";
			packLine1.JL_ActualWeight = 22.3m;
			packLine1.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine1.JL_ActualVolume = 21m;
			packLine1.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			packLine1.JL_MarksAndNumbers = "Line1 Marks\nBlaticus";

			PackLine packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.JL_FreightMode = FreightConstants.OuterPackType;
			packLine2.JL_PackageCount = 3;
			packLine2.JL_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			packLine2.JL_Description = "Description";
			packLine2.JL_RH_NKCommodityCode = "GEN";
			packLine2.JL_ActualWeight = 22.4m;
			packLine2.JL_ActualWeightUQ = Core.Constants.Weight.Kilograms;
			packLine2.JL_ActualVolume = 20.5m;
			packLine2.JL_ActualVolumeUQ = Core.Constants.Volume.Litre;
			packLine2.JL_MarksAndNumbers = "Line2 Marks";

			UNDGSubstance undgSubstance = Factory.New<UNDGSubstance>();
			undgSubstance.DG_UNNO = "2";
			undgSubstance.DG_Variant = "b";
			undgSubstance.DG_PSN = "PSN";
			undgSubstance.DG_Class = "1.1";
			undgSubstance.DG_PG = "PG";
			packLine1.UNDGs.AddNew().DI_DG = undgSubstance.PK;

			packLine1.Containers.Add(FreightContainer);
			packLine2.Containers.Add(FreightContainer);

			DocContainer docContainer = DocContainer.New(FreightContainer, shipment, Factory);
			DocBillofLadingContainer hblContainer = DocBillofLadingContainer.New(docContainer);

			Dictionary<string, object> constants = new Dictionary<string, object>();

			((IBODocDataProvider)docContainer.Shipment).SetDocWrapperContext(constants);
			AssertEquals("default max line length", 112, docContainer.Shipment.MaxLineLength);

			TextSection packingDetailsBreakdown = hblContainer.GetPackingDetailsBreakdown(docContainer.Shipment.MaxLineLength);

			AssertMultilineASCIIEquals("",
				"     2 PLT - 22.3 KG - HAZ - UN2, PSN, class 1.1, PG PG - Goods Description",
				packingDetailsBreakdown[0].ToString());
			AssertEquals(1, packingDetailsBreakdown[0].Height);
			AssertEquals("     3 PLT - 22.4 KG - GEN - Description", packingDetailsBreakdown[1].ToString());
			AssertEquals(1, packingDetailsBreakdown[1].Height);
			AssertEquals(2, packingDetailsBreakdown.Height);

			UNDGDataItem undgDataItem = packLine1.UNDGs.AddNew();
			undgDataItem.DI_DG = undgSubstance.PK;
			undgDataItem.DI_TechnicalName = "Ranked from most to least dangerous, the most dangerous substances is deemed to be heroin aka smack.";

			packingDetailsBreakdown = hblContainer.GetPackingDetailsBreakdown(docContainer.Shipment.MaxLineLength);

			AssertMultilineASCIIEquals("",
				"     2 PLT - 22.3 KG - HAZ - UN2, PSN, class 1.1, PG PG\r\n" +
				"UN2, PSN (Ranked from most to least dangerous, the most dangerous substances is deemed to be heroin aka smack.), class 1.1, PG PG - Goods Description",
				packingDetailsBreakdown[0].ToString());
			AssertEquals(2, packingDetailsBreakdown[0].Height);
			AssertEquals("     3 PLT - 22.4 KG - GEN - Description", packingDetailsBreakdown[1].ToString());
			AssertEquals(1, packingDetailsBreakdown[1].Height);
			AssertEquals(3, packingDetailsBreakdown.Height);

			constants[DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineCommodityCode] = false;
			constants[DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineCommodityDescription] = true;
			((IBODocDataProvider)docContainer.Shipment).SetDocWrapperContext(constants);
			packingDetailsBreakdown = hblContainer.GetPackingDetailsBreakdown(docContainer.Shipment.MaxLineLength);

			AssertMultilineASCIIEquals("",
				"     2 PLT - 22.3 KG - HAZARDOUS GOODS - UN2, PSN, class 1.1, PG PG\r\n" +
				"UN2, PSN (Ranked from most to least dangerous, the most dangerous substances is deemed to be heroin aka smack.), class 1.1, PG PG - Goods Description",
				packingDetailsBreakdown[0].ToString());

			constants[DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineVolume] = true;
			constants[DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineCommodityDescription] = false;
			((IBODocDataProvider)docContainer.Shipment).SetDocWrapperContext(constants);
			packingDetailsBreakdown = hblContainer.GetPackingDetailsBreakdown(docContainer.Shipment.MaxLineLength);

			AssertMultilineASCIIEquals("",
				"     2 PLT - 22.3 KG - 21 L - UN2, PSN, class 1.1, PG PG\r\n" +
				"UN2, PSN (Ranked from most to least dangerous, the most dangerous substances is deemed to be heroin aka smack.), class 1.1, PG PG - Goods Description",
				packingDetailsBreakdown[0].ToString());

			constants[DocumentEngineIntegration.Constants.TemplateDefined.ShowPacklineMarksAndNumbers] = true;
			((IBODocDataProvider)docContainer.Shipment).SetDocWrapperContext(constants);
			packingDetailsBreakdown = hblContainer.GetPackingDetailsBreakdown(docContainer.Shipment.MaxLineLength);

			AssertMultilineASCIIEquals("",
				"     2 PLT - 22.3 KG - 21 L - UN2, PSN, class 1.1, PG PG\r\n" +
				"UN2, PSN (Ranked from most to least dangerous, the most dangerous substances is deemed to be heroin aka smack.), class 1.1, PG PG - Goods Description - Line1 Marks Blaticus",
				packingDetailsBreakdown[0].ToString());
		}

		public void PackType()
		{
			AssertEquals("Packtype empty", ZString.Empty, HBLContainer.PackType);

			HBLContainer.PackType = "blah";
			AssertEquals("Packtype", "blah", HBLContainer.PackType);

			HBLContainer.PackType = "Blahblah";
			AssertEquals("Packtype", "Blahblah", HBLContainer.PackType);
		}

		public void TestDeliveryMode()
		{
			AssertEquals("Delivery Mode", "-", HBLContainer.DeliveryMode);

			HBLContainer.DeliveryMode = "blah";
			AssertEquals("Delivery Mode", "blah", HBLContainer.DeliveryMode);

			HBLContainer.DeliveryMode = "CY";
			AssertEquals("Delivery Mode", "CY*", HBLContainer.DeliveryMode);
		}

		public void TestContainerMode()
		{
			AssertEquals("Container Mode", ZString.Empty, HBLContainer.ContainerMode);

			HBLContainer.ContainerMode = "blah";
			AssertEquals("Container Mode", "blah", HBLContainer.ContainerMode);

			HBLContainer.ContainerMode = "FCL";
			AssertEquals("Container Mode", "FCL", HBLContainer.ContainerMode);
		}

		public void TestTemperature()
		{
			AssertEquals("", HBLContainer.Temperature);

			FreightContainer.JC_SetPointTemp = 5m;
			FreightContainer.JC_SetPointTempUnit = "C";
			AssertEquals("5C", HBLContainer.Temperature);

			FreightContainer.JC_SetPointTemp = 5.5m;
			FreightContainer.JC_SetPointTempUnit = "F";
			AssertEquals("5.5F", HBLContainer.Temperature);

			FreightContainer.JC_SetPointTemp = 0;
			FreightContainer.JC_IsControlledAtmosphere = true;
			AssertEquals("0F", HBLContainer.Temperature);
		}

		public void TestHumidity()
		{
			AssertEquals("", HBLContainer.Humidity);

			FreightContainer.JC_HumidityPercent = 15;
			AssertEquals("15%", HBLContainer.Humidity);
		}

		public void TestContainerNumber()
		{
			FreightContainer.JC_ContainerNum = "blah";
			HBLContainer = DocBillofLadingContainer.New(DocContainer);
			AssertEquals("Container Number", "BLAH", HBLContainer.ContainerNumber);
		}

		public void TestContainerSeal()
		{
			AssertEquals("Seal Number", "-", HBLContainer.ContainerSeal);

			FreightContainer.JC_SealNum = "blah";
			AssertEquals("Seal Number", "blah", HBLContainer.ContainerSeal);

			FreightContainer.JC_AdditionalSealNum = "moo";
			AssertEquals("Seal Number", "blah, moo", HBLContainer.ContainerSeal);

			FreightContainer.JC_Additional2SealNum = "oink";
			AssertEquals("Seal Number", "blah, moo, oink", HBLContainer.ContainerSeal);

			FreightContainer.JC_SealNum = "";
			AssertEquals("Seal Number", "moo, oink", HBLContainer.ContainerSeal);
		}

		public void TestContainerType()
		{
			AssertEquals("Type", "-", HBLContainer.ContainerType);

			var refContainer = Factory.LoadTop1<RefContainer>(new ZQuery());
			FreightContainer.JC_RC = refContainer.PK;
			AssertEquals("Type", refContainer.RC_Code, HBLContainer.ContainerType);
		}

		public void TestContainerGross()
		{
			AssertEquals("Gross", 0M, HBLContainer.ContainerGross);

			FreightContainer.JC_GrossWeight = 1M;
			FreightContainer.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Gross Weight is calculated from Tare + Weight of goods on this shipment - not set, so should be 0", 0M, HBLContainer.ContainerGross);

			FreightContainer.JC_TareWeight = 1M;
			HBLContainer.Weight = 2000M;
			AssertEquals("Gross Weight is calculated from Tare + Weight of goods on this shipment", 3000M, HBLContainer.ContainerGross);
		}

		public void TestContainerTare()
		{
			AssertEquals("Tare", 0M, HBLContainer.ContainerTare);

			FreightContainer.JC_TareWeight = 1M;
			FreightContainer.JC_GrossWeightUQ = Core.Constants.Weight.Tonnes;
			AssertEquals("Tare", 1000M, HBLContainer.ContainerTare);
		}

		public void TestWeightsAsEntered()
		{
			FreightContainer.JC_GrossWeightUQ = "LB";
			FreightContainer.JC_TareWeight = 1234;
			FreightContainer.JC_GrossWeight = 1254;

			AssertEquals("Gross", 1254M, HBLContainer.ContainerGrossAsEntered);
			AssertEquals("Tare", 1234M, HBLContainer.ContainerTareAsEntered);
			AssertEquals("UQ", "LB", HBLContainer.ContainerWeightUQAsEntered);

			FreightContainer.JC_GrossWeightUQ = "T";
			FreightContainer.JC_TareWeight = 3.4;
			FreightContainer.JC_GrossWeight = 4;

			AssertEquals("Gross", 4M, HBLContainer.ContainerGrossAsEntered);
			AssertEquals("Tare", 3.4M, HBLContainer.ContainerTareAsEntered);
			AssertEquals("UQ", "T", HBLContainer.ContainerWeightUQAsEntered);
		}

		public void TestVolumeAsEntered()
		{
			var consol = Factory.New<CommonConsol>();
			var shipment = consol.Shipments.AddNew();
			var pack1 = shipment.OuterPackLines.AddNew();
			var pack2 = shipment.OuterPackLines.AddNew();

			consol.Containers.Add(FreightContainer);

			pack1.SetContainer(FreightContainer.PK);
			pack2.SetContainer(FreightContainer.PK);

			pack1.JL_ActualVolume = 50;
			pack1.JL_ActualVolumeUQ = "CF";

			pack2.JL_ActualVolume = 38;
			pack2.JL_ActualVolumeUQ = "CF";

			AssertEquals("Volume - CF", 88M, HBLContainer.ContainerVolumeAsEntered);
			AssertEquals("UQ - CF", "CF", HBLContainer.ContainerVolumeUQAsEntered);

			pack1.JL_ActualVolumeUQ = "L";

			AssertEquals("Volume - M3", 1.126M, HBLContainer.ContainerVolumeAsEntered);
			AssertEquals("UQ - M3", "M3", HBLContainer.ContainerVolumeUQAsEntered);

			pack2.JL_ActualVolumeUQ = "L";

			AssertEquals("Volume - L", 88M, HBLContainer.ContainerVolumeAsEntered);
			AssertEquals("UQ - L", "L", HBLContainer.ContainerVolumeUQAsEntered);
		}

		#region Implementation

		CommonContainer FreightContainer;
		DocContainer DocContainer;
		DocBillofLadingContainer HBLContainer;

		protected override void SetUp()
		{
			FreightContainer = Factory.NewWithValidTestData<CommonContainer>();
			DocContainer = DocContainer.New(FreightContainer, Factory);
			HBLContainer = DocBillofLadingContainer.New(DocContainer);

			base.SetUp();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			FreightContainer = Factory.NewWithValidTestData<CommonContainer>();
			DocContainer = DocContainer.New(FreightContainer, Factory);
			return DocBillofLadingContainer.New(DocContainer);
		}
		#endregion
	}
}
