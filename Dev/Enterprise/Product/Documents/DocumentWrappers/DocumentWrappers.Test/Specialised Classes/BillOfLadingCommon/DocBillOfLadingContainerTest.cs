using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocFormedPagesContainer))]
	sealed class DocBillOfLadingContainerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateFromIDocContainer()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			CommonContainer container = consol.Containers.AddNew();

			container.JC_ContainerNum = "ABCD1";
			container.JC_SealNum = "SEAL1";
			container.JC_AdditionalSealNum = "SEAL2";
			container.JC_Additional2SealNum = "SEAL3";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20RE")).PK;
			container.JC_DeliveryMode = "CYE";
			container.JC_TareWeight = 2200;
			container.JC_IsControlledAtmosphere = true;
			container.JC_SetPointTemp = -4;
			container.JC_SetPointTempUnit = "C";
			container.JC_HumidityPercent = 20;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 20;
			packLine.JL_F3_NKPackType = "BAG";
			packLine.JL_ActualVolume = 2;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine.JL_ActualWeight = 150;
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 14;
			packLine.JL_F3_NKPackType = "PLT";
			packLine.JL_ActualVolume = 1.5;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine.JL_ActualWeight = 50;
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			DocShipment docShipment = DocShipment.New(shipment, Factory);
			DocFormedPagesContainer formedPagesContainer = new DocFormedPagesContainer(docShipment.Containers[0]);

			AssertEquals("Container Number", "ABCD1", formedPagesContainer.ContainerNumber);
			AssertEquals("Seal", "SEAL1, SEAL2, SEAL3", formedPagesContainer.ContainerSeal);
			AssertEquals("Type", "20RE", formedPagesContainer.ContainerType);
			AssertEquals("Delivery Mode", "CYE*", formedPagesContainer.DeliveryMode);
			AssertEquals("Gross", (ZDecimal)2400, formedPagesContainer.ContainerGross);
			AssertEquals("Tare", (ZDecimal)2200, formedPagesContainer.ContainerTare);
			AssertEquals("Net", (ZDecimal)200, formedPagesContainer.ContainerNet);
			AssertEquals("WeightUQ", "KG", formedPagesContainer.ContainerWeightUQ);
			AssertEquals("Temp", "-4C", formedPagesContainer.Temperature);
			AssertEquals("Humidity", "20%", formedPagesContainer.Humidity);
			AssertEquals("Packs", 34, formedPagesContainer.Packs);
			AssertEquals("PackType", "PCE", formedPagesContainer.PackType);
			AssertEquals("Volume", (ZDecimal)3.5, formedPagesContainer.Volume);
			AssertEquals("VolumeUQ", "M3", formedPagesContainer.VolumeUQ);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		public void TestCreateFromIDocContainerWithImperialUnits()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			CommonContainer container = consol.Containers.AddNew();

			container.JC_ContainerNum = "ABCD1";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20RE")).PK;
			container.JC_GrossWeightUQ = "LB";
			container.JC_TareWeight = 2200;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 20;
			packLine.JL_F3_NKPackType = "BAG";
			packLine.JL_ActualVolume = 2;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			packLine.JL_ActualWeight = 150;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 14;
			packLine.JL_F3_NKPackType = "PLT";
			packLine.JL_ActualVolume = 1.5;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			packLine.JL_ActualWeight = 50;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;

			DocShipment docShipment = DocShipment.New(shipment, Factory);
			DocFormedPagesContainer formedPagesContainer = new DocFormedPagesContainer(docShipment.Containers[0]);

			AssertEquals("Container Number", "ABCD1", formedPagesContainer.ContainerNumber);
			AssertEquals("Gross", (ZDecimal)2400, formedPagesContainer.ContainerGross);
			AssertEquals("Tare", (ZDecimal)2200, formedPagesContainer.ContainerTare);
			AssertEquals("Net", (ZDecimal)200, formedPagesContainer.ContainerNet);
			AssertEquals("WeightUQ", "LB", formedPagesContainer.ContainerWeightUQ);
			AssertEquals("Volume", (ZDecimal)3.5, formedPagesContainer.Volume);
			AssertEquals("VolumeUQ", "CF", formedPagesContainer.VolumeUQ);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		public void TestCreateFromIDocContainerWithMixedUnits()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonShipment shipment = consol.Shipments.AddNew();
			CommonContainer container = consol.Containers.AddNew();

			container.JC_ContainerNum = "ABCD1";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20RE")).PK;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 2200;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 20;
			packLine.JL_F3_NKPackType = "BAG";
			packLine.JL_ActualVolume = 2;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			packLine.JL_ActualWeight = 150;
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 14;
			packLine.JL_F3_NKPackType = "PLT";
			packLine.JL_ActualVolume = 1.5;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine.JL_ActualWeight = 50;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;

			DocShipment docShipment = DocShipment.New(shipment, Factory);
			DocFormedPagesContainer formedPagesContainer = new DocFormedPagesContainer(docShipment.Containers[0]);

			AssertEquals("Container Number", "ABCD1", formedPagesContainer.ContainerNumber);
			AssertEquals("Gross", (ZDecimal)2372.68, formedPagesContainer.ContainerGross);
			AssertEquals("Tare", (ZDecimal)2200, formedPagesContainer.ContainerTare);
			AssertEquals("Net", (ZDecimal)172.68, formedPagesContainer.ContainerNet);
			AssertEquals("WeightUQ", "KG", formedPagesContainer.ContainerWeightUQ);
			AssertEquals("Volume", (ZDecimal)1.557, formedPagesContainer.Volume);
			AssertEquals("VolumeUQ", "M3", formedPagesContainer.VolumeUQ);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		public void TestCreateFromBillOfLadingContainer()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			ForwardingContainer container = consol.Containers.AddNew();

			container.JC_ContainerNum = "EFGH1";
			container.JC_SealNum = "SEAL1";
			container.JC_AdditionalSealNum = ZString.Empty;
			container.JC_Additional2SealNum = "SEAL3";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40RE")).PK;
			container.JC_DeliveryMode = "DM";
			container.JC_TareWeight = 4200;
			container.JC_IsControlledAtmosphere = true;
			container.JC_SetPointTemp = -12;
			container.JC_SetPointTempUnit = "C";
			container.JC_HumidityPercent = 40;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 10;
			packLine.JL_F3_NKPackType = "KEG";
			packLine.JL_ActualVolume = 1.5;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine.JL_ActualWeight = 120;
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 19;
			packLine.JL_F3_NKPackType = "BAG";
			packLine.JL_ActualVolume = 1.2;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine.JL_ActualWeight = 55;
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			DocForwardingShipment docShipment = DocForwardingShipment.New(shipment, Factory);
			DocBillOfLading docBOL = new DocBillOfLading(docShipment);
			var supporter = (IFormedPagesSupporter)docBOL;
			DocFormedPagesContainer formedPagesContainer = supporter.Containers[0];

			AssertEquals("Container Number", "EFGH1", formedPagesContainer.ContainerNumber);
			AssertEquals("Seal", "SEAL1, SEAL3", formedPagesContainer.ContainerSeal);
			AssertEquals("Type", "40RE", formedPagesContainer.ContainerType);
			AssertEquals("Delivery Mode", "DM", formedPagesContainer.DeliveryMode);
			AssertEquals("Gross", (ZDecimal)4375, formedPagesContainer.ContainerGross);
			AssertEquals("Tare", (ZDecimal)4200, formedPagesContainer.ContainerTare);
			AssertEquals("Net", (ZDecimal)175, formedPagesContainer.ContainerNet);
			AssertEquals("WeightUQ", "KG", formedPagesContainer.ContainerWeightUQ);
			AssertEquals("Temp", "-12C", formedPagesContainer.Temperature);
			AssertEquals("Humidity", "40%", formedPagesContainer.Humidity);
			AssertEquals("Packs", 29, formedPagesContainer.Packs);
			AssertEquals("PackType", "PCE", formedPagesContainer.PackType);
			AssertEquals("Volume", (ZDecimal)2.7, formedPagesContainer.Volume);
			AssertEquals("VolumeUQ", "M3", formedPagesContainer.VolumeUQ);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		public void TestCreateFromBillOfLadingContainerWithImperialUnits()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			ForwardingContainer container = consol.Containers.AddNew();

			container.JC_ContainerNum = "EFGH1";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40RE")).PK;
			container.JC_GrossWeightUQ = Constants.Weight.Pounds;
			container.JC_TareWeight = 4200;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 10;
			packLine.JL_F3_NKPackType = "KEG";
			packLine.JL_ActualVolume = 1.5;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			packLine.JL_ActualWeight = 120;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 19;
			packLine.JL_F3_NKPackType = "BAG";
			packLine.JL_ActualVolume = 1.2;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			packLine.JL_ActualWeight = 55;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;

			DocForwardingShipment docShipment = DocForwardingShipment.New(shipment, Factory);
			DocBillOfLading docBOL = new DocBillOfLading(docShipment);
			var supporter = (IFormedPagesSupporter)docBOL;
			DocFormedPagesContainer formedPagesContainer = supporter.Containers[0];

			AssertEquals("Container Number", "EFGH1", formedPagesContainer.ContainerNumber);
			AssertEquals("Gross", (ZDecimal)4375, formedPagesContainer.ContainerGross);
			AssertEquals("Tare", (ZDecimal)4200, formedPagesContainer.ContainerTare);
			AssertEquals("Net", (ZDecimal)175, formedPagesContainer.ContainerNet);
			AssertEquals("WeightUQ", "LB", formedPagesContainer.ContainerWeightUQ);
			AssertEquals("VolumeUQ", "CF", formedPagesContainer.VolumeUQ);
			AssertEquals("Volume", (ZDecimal)2.7, formedPagesContainer.Volume);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		public void TestCreateFromBillOfLadingContainerWithMixedUnits()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			ForwardingContainer container = consol.Containers.AddNew();

			container.JC_ContainerNum = "EFGH1";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40RE")).PK;
			container.JC_GrossWeightUQ = "KG";
			container.JC_TareWeight = 4200;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			PackLine packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 10;
			packLine.JL_F3_NKPackType = "KEG";
			packLine.JL_ActualVolume = 1.5;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			packLine.JL_ActualWeight = 120;
			packLine.JL_ActualWeightUQ = Constants.Weight.Pounds;

			packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(container.PK);
			packLine.JL_PackageCount = 19;
			packLine.JL_F3_NKPackType = "BAG";
			packLine.JL_ActualVolume = 1.2;
			packLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine.JL_ActualWeight = 55;
			packLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			DocForwardingShipment docShipment = DocForwardingShipment.New(shipment, Factory);
			DocBillOfLading docBOL = new DocBillOfLading(docShipment);
			var supporter = (IFormedPagesSupporter)docBOL;
			DocFormedPagesContainer formedPagesContainer = supporter.Containers[0];

			AssertEquals("Container Number", "EFGH1", formedPagesContainer.ContainerNumber);
			AssertEquals("Gross", (ZDecimal)4309.431, formedPagesContainer.ContainerGross);
			AssertEquals("Tare", (ZDecimal)4200, formedPagesContainer.ContainerTare);
			AssertEquals("Net", (ZDecimal)109.431, formedPagesContainer.ContainerNet);
			AssertEquals("WeightUQ", "KG", formedPagesContainer.ContainerWeightUQ);
			AssertEquals("Volume", (ZDecimal)1.242, formedPagesContainer.Volume);
			AssertEquals("VolumeUQ", "M3", formedPagesContainer.VolumeUQ);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		public void TestTotalWeightAndVolumeOnCoLoadShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			var masterShipment = consol.Shipments.AddNew();
			masterShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.CoLoadMaster;

			var subShipment1 = masterShipment.CoLoadShipments.AddNew();
			subShipment1.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var subShipment2 = masterShipment.CoLoadShipments.AddNew();
			subShipment2.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "ASDF1233212";
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			var packLine1 = subShipment1.OuterPackLines.AddNew();
			packLine1.SetContainer(container.PK);
			packLine1.JL_ActualVolume = 5;
			packLine1.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine1.JL_ActualWeight = 400;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var packLine2 = subShipment2.OuterPackLines.AddNew();
			packLine2.SetContainer(container.PK);
			packLine2.JL_ActualVolume = 3;
			packLine2.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine2.JL_ActualWeight = 200;
			packLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var docShipment = DocForwardingShipment.New(masterShipment, Factory);
			var docBOL = new DocBillOfLading(docShipment);
			var supporter = (IFormedPagesSupporter)docBOL;
			var formedPagesContainer = supporter.Containers[0];

			AssertEquals("Total weight from sub shipments", (ZDecimal)600, formedPagesContainer.ContainerNet);
			AssertEquals("WeightUQ", "KG", formedPagesContainer.ContainerWeightUQ);
			AssertEquals("Total volume from subshipments", (ZDecimal)8, formedPagesContainer.Volume);
			AssertEquals("VolumeUQ", "M3", formedPagesContainer.VolumeUQ);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		#region Testing Container Net

		public void TestContainerNet_FCLShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20RE")).PK;
			container.JC_TareWeight = 1200;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.FCL;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(container.PK);
			packLine1.JL_ActualWeight = 1000;
			packLine1.JL_ActualVolume = 10;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(container.PK);
			packLine2.JL_ActualWeight = 3000;
			packLine2.JL_ActualVolume = 30;
			packLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var anotherShipment = consol.Shipments.AddNew();
			anotherShipment.JS_PackingMode = Constants.ContainerModes.FCL;
			var anotherPackLine = anotherShipment.OuterPackLines.AddNew();
			anotherPackLine.SetContainer(container.PK);
			anotherPackLine.JL_ActualWeight = 2000;
			anotherPackLine.JL_ActualVolume = 20;
			anotherPackLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var docShipment = DocForwardingShipment.New(shipment, Factory);
			var docBOL = new DocBillOfLading(docShipment) as IFormedPagesSupporter;
			var formedPagesContainer = docBOL.Containers[0];

			AssertEquals("Net", (ZDecimal)4000, formedPagesContainer.ContainerNet);
			AssertEquals("Volume", (ZDecimal)40, formedPagesContainer.Volume);
			AssertEquals("Volume Units", Constants.Volume.CubicMetres, formedPagesContainer.VolumeUQ);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		public void TestContainerNet_LCLShipments()
		{
			var consol = Factory.New<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20RE")).PK;
			container.JC_TareWeight = 1200;
			container.JC_ContainerMode = Constants.ContainerModes.FCL;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_PackingMode = Constants.ContainerModes.LCL;

			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.SetContainer(container.PK);
			packLine1.JL_ActualWeight = 1000;
			packLine1.JL_ActualVolume = 10;
			packLine1.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var packLine2 = shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(container.PK);
			packLine2.JL_ActualWeight = 3000;
			packLine2.JL_ActualVolume = 30;
			packLine2.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var anotherShipment = consol.Shipments.AddNew();
			anotherShipment.JS_PackingMode = Constants.ContainerModes.LCL;
			var anotherPackLine = anotherShipment.OuterPackLines.AddNew();
			anotherPackLine.SetContainer(container.PK);
			anotherPackLine.JL_ActualWeight = 2000;
			anotherPackLine.JL_ActualVolume = 20;
			anotherPackLine.JL_ActualWeightUQ = Constants.Weight.Kilograms;

			var docShipment = DocForwardingShipment.New(shipment, Factory);
			var docBOL = new DocBillOfLading(docShipment) as IFormedPagesSupporter;
			var formedPagesContainer = docBOL.Containers[0];

			AssertEquals("Net", (ZDecimal)4000, formedPagesContainer.ContainerNet);
			AssertEquals("Volume", (ZDecimal)40, formedPagesContainer.Volume);
			AssertEquals("Volume Units", Constants.Volume.CubicMetres, formedPagesContainer.VolumeUQ);
			AssertEquals("ContainerMode", Constants.ContainerModes.FCL, formedPagesContainer.ContainerMode);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			CommonContainer container = Factory.New<CommonContainer>();
			DocContainer docContainer = DocContainer.New(container, Factory);
			return new DocFormedPagesContainer(docContainer);
		}

		#endregion
	}
}
