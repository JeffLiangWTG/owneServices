using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class FreightExtensionsTest : TestCaseWithFactory
	{
		public void TestGetContainerNumForConsole()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			shipment.JS_OuterPacks = 5;
			var packLine = shipment.OuterPackLines[0];
			AssertEquals("No exception", ZString.Empty, packLine.GetContainerNumForConsol(null));

			var console1 = Factory.New<ForwardingConsol>();
			console1.Shipments.Add(shipment);
			AssertEquals("No container", ZString.Empty, packLine.GetContainerNumForConsol(console1));

			var c1 = console1.Containers.AddNew();
			c1.JC_ContainerNum = "C1";
			AssertEquals("C1", "C1", packLine.GetContainerNumForConsol(console1));

			var console2 = Factory.New<ForwardingConsol>();
			console2.Shipments.Add(shipment);
			var c2 = console2.Containers.AddNew();
			c2.JC_ContainerNum = "C2";
			AssertEquals("C1", "C1", packLine.GetContainerNumForConsol(console1));
			AssertEquals("C2", "C2", packLine.GetContainerNumForConsol(console2));
		}

		public void TestGetAUDeclaration()
		{
			var shipment = Factory.New<CommonShipment>();

			var auCompany = Factory.NewWithValidTestData<GlbCompany>();
			auCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			var auBranch = Factory.NewWithValidTestData<GlbBranch>();
			auBranch.GB_GC = auCompany.PK;
			var auDeclaration = Factory.New<BaseJobDeclaration>();
			auDeclaration.JE_GB = auBranch.PK;
			auDeclaration.JE_JS = shipment.PK;

			var usCompany = Factory.NewWithValidTestData<GlbCompany>();
			usCompany.SetCountry(Core.Constants.CountryCodes.UnitedStates);
			var usBranch = Factory.NewWithValidTestData<GlbBranch>();
			usBranch.GB_GC = usCompany.PK;
			var usDeclaration = Factory.New<BaseJobDeclaration>();
			usDeclaration.JE_GB = usBranch.PK;
			usDeclaration.JE_JS = shipment.PK;

			var auDeclarations = new List<JobDeclaration>(shipment.GetAUDeclarations());
			AssertEquals("have 1 au company", 1, auDeclarations.Count);
			AssertEquals(auDeclaration, auDeclarations[0]);
		}

		public void TestGetHVLVConsignmentLines()
		{
			var hvlShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var otherShipment = Factory.NewWithValidTestData<ForwardingShipment>();

			hvlShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
			otherShipment.JS_ShipmentType = Core.Constants.ShipmentTypes.StandardHouse;

			var hvlConsignment1 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());
			var hvlConsignment2 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType<IHVLVConsignment>());

			hvlConsignment1.HVC_JS_ManifestedOnShipment = hvlShipment.PK;
			hvlConsignment2.HVC_JS_ManifestedOnShipment = hvlShipment.PK;

			Factory.Save();

			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder("HVL Shipment - Uses HVLVConsignments", new[] { hvlConsignment1.PK, hvlConsignment2.PK }, hvlShipment.GetHVLVConsignmentLines().Select(line => line.PK));
				AssertEquals("Other Shipment Type - Empty Collection", 0, otherShipment.GetHVLVConsignmentLines().Count());
			});
		}

		public void TestGetHVLVConsignmentLines_ShouldNotIncludeStandAloneDeclarations()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;

			var consignment1 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
			var consignment2 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
			var consignment3 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;

			var declaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var declaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			consignment1.HVC_JE_ImportDeclaration = declaration1.PK;
			consignment2.HVC_JE_ExportDeclaration = declaration2.PK;

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("Does not contain consignments with standalone declaration", new[] { consignment3.PK }, shipment.GetHVLVConsignmentLines().Select(line => line.PK));
		}
	}
}
