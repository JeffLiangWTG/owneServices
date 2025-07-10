using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Freight.Testing
{
	[TestedType(typeof(DocTranshipment))]
	sealed class DocTranshipmentTestCase : NonPersistentBusinessObjectTestCase
	{
		#region Overrides
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DocTranshipment(DocForwardingConsol.New(Consol, Factory), null);
		}
		#endregion

		public void TestNextConsol()
		{
			AssertNull(TranshipmentWrapper.NextConsol);
			AssertEquals(ZBool.False, TranshipmentWrapper.PrintTranshipmentDetails);

			Consol.JK_UniqueConsignRef = "CONSOL1";
			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKDiscPort = "AUBNE";

			var nextConsol = Shipment.Consols.AddNew();
			Transport nextTransport = nextConsol.Transports[0];
			nextConsol.JK_UniqueConsignRef = "CONSOL2";
			var consolAfterTheNext = Shipment.Consols.AddNew();
			consolAfterTheNext.JK_UniqueConsignRef = "CONSOL3";

			nextTransport.JW_RL_NKLoadPort = "AUBNE";
			nextTransport.JW_RL_NKDiscPort = "NZAKL";

			Transport transportAfterTheNext = consolAfterTheNext.Transports[0];
			transportAfterTheNext.JW_RL_NKLoadPort = "NZAKL";
			transportAfterTheNext.JW_RL_NKDiscPort = "NZWWW";

			TranshipmentWrapper = new DocTranshipment(DocForwardingConsol.New(Consol, Factory), DocForwardingShipment.New(Shipment, Factory));
			AssertEquals("CONSOL2", TranshipmentWrapper.NextConsol.ConsolNumber);
			AssertEquals(ZBool.True, TranshipmentWrapper.PrintTranshipmentDetails);
		}

		public void TestContainerNumbers()
		{
			Consol.JK_UniqueConsignRef = "CONSOL1";
			Transport.JW_RL_NKDiscPort = "AUBNE";

			var nextConsol = Shipment.Consols.AddNew();
			nextConsol.JK_UniqueConsignRef = "CONSOL2";

			Transport nextTransport = nextConsol.Transports[0];
			nextTransport.JW_RL_NKLoadPort = "AUBNE";
			nextTransport.JW_RL_NKDiscPort = "NZAKL";

			var cont1 = (CommonContainer)Consol.Containers.AddNew();
			cont1.JC_ContainerNum = "CONT1";
			var cont2 = (CommonContainer)Consol.Containers.AddNew();
			cont2.JC_ContainerNum = "CONT2";

			var cont3 = (CommonContainer)nextConsol.Containers.AddNew();
			cont3.JC_ContainerNum = "CONT3";
			var cont4 = (CommonContainer)nextConsol.Containers.AddNew();
			cont4.JC_ContainerNum = "CONT4";
			var cont5 = (CommonContainer)nextConsol.Containers.AddNew();
			cont5.JC_ContainerNum = "CONT5";

			var line1 = (PackLine)Shipment.OuterPackLines.AddNew();
			line1.JL_Description = "LINE1";
			var line2 = (PackLine)Shipment.OuterPackLines.AddNew();
			line2.JL_Description = "LINE2";
			var line3 = (PackLine)Shipment.OuterPackLines.AddNew();
			line3.JL_Description = "LINE3";
			var line4 = (PackLine)Shipment.OuterPackLines.AddNew();
			line4.JL_Description = "LINE4";
			var line5 = (PackLine)Shipment.OuterPackLines.AddNew();
			line5.JL_Description = "LINE5";

			line1.SetContainer(Consol, cont1);
			line2.SetContainer(Consol, cont1);
			line3.SetContainer(Consol, cont1);
			line4.SetContainer(Consol, cont1);
			line5.SetContainer(Consol, cont1);

			line1.SetContainer(nextConsol, cont5);
			line2.SetContainer(nextConsol, cont4);
			line3.SetContainer(nextConsol, cont3);
			line4.SetContainer(nextConsol, cont3);
			line5.SetContainer(nextConsol, cont4);
			Factory.Save();

			TranshipmentWrapper = new DocTranshipment(DocForwardingConsol.New(Consol, Factory), DocForwardingShipment.New(Shipment, Factory));
			AssertEquals("Should have CONT1 only", "CONT1", TranshipmentWrapper.ContainerNumbersOnCurrentConsol);
			AssertEquals("Should have CONT3, CONT4 and CONT5", "CONT3, CONT4, CONT5", TranshipmentWrapper.ContainerNumbersOnNextConsol);

			line1.SetContainer(Consol, cont2);
			line4.SetContainer(Consol, cont2);
			Factory.Save();
			TranshipmentWrapper = new DocTranshipment(DocForwardingConsol.New(Consol, Factory), DocForwardingShipment.New(Shipment, Factory));
			AssertEquals("Should have CONT1 and CONT2", "CONT1, CONT2", TranshipmentWrapper.ContainerNumbersOnCurrentConsol);
			AssertEquals("Should have CONT3, CONT4 and CONT5", "CONT3, CONT4, CONT5", TranshipmentWrapper.ContainerNumbersOnNextConsol);

			line1.SetContainer(nextConsol, null);
			line2.SetContainer(nextConsol, null);
			Factory.Save();
			TranshipmentWrapper = new DocTranshipment(DocForwardingConsol.New(Consol, Factory), DocForwardingShipment.New(Shipment, Factory));
			AssertEquals("Should have CONT1 and CONT2", "CONT1, CONT2", TranshipmentWrapper.ContainerNumbersOnCurrentConsol);
			AssertEquals("Should have CONT3 and CONT4 only ", "CONT3, CONT4", TranshipmentWrapper.ContainerNumbersOnNextConsol);
		}

		public void TestShipmentPackLineDetails()
		{
			var cont1 = (CommonContainer)Consol.Containers.AddNew();
			cont1.JC_ContainerNum = "CONT1";
			var cont2 = (CommonContainer)Consol.Containers.AddNew();
			cont2.JC_ContainerNum = "CONT2";

			var line1 = (PackLine)Shipment.OuterPackLines.AddNew();
			line1.JL_Description = "LINE1";
			line1.JL_ActualVolume = 10m;
			line1.JL_ActualWeight = 5m;
			line1.JL_PackageCount = 15;
			line1.JL_F3_NKPackType = "PLT";
			line1.JL_ActualVolumeUQ = "M3";
			line1.JL_ActualWeightUQ = "KG";
			line1.SetContainer(Consol, cont1);

			Factory.Save();
			DocForwardingConsol consolWrapper = DocForwardingConsol.New(Consol, Factory);
			TranshipmentWrapper = new DocTranshipment(consolWrapper, DocForwardingShipment.New(Shipment, Factory));

			ZString expectedPlt = "CONTAINER/ WGT/ VOL/ PKG" + System.Environment.NewLine + "CONT1/ 5KG/ 10M3/ 15PLT";
			AssertEquals("Should have packline details with container", expectedPlt, TranshipmentWrapper.ShipmentPackLineDetails);

			line1.JL_F3_NKPackType = "KEG";
			ZString expectedKeg = "CONTAINER/ WGT/ VOL/ PKG" + System.Environment.NewLine + "CONT1/ 5KG/ 10M3/ 15KEG";
			AssertEquals("Should have packline details with container", expectedKeg, TranshipmentWrapper.ShipmentPackLineDetails);

			DocumentsDataRegistry.Instance.DisplayContainerDetailsOnConsol.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			consolWrapper.SetReportNameForTesting("Forwarding Instruction");
			AssertEquals("Should have packline details with container", expectedKeg, TranshipmentWrapper.ShipmentPackLineDetails);

			DocumentsDataRegistry.Instance.DisplayContainerDetailsOnConsol.SetValue(
				GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals("Should print nothing", "", TranshipmentWrapper.ShipmentPackLineDetails);
		}

		#region Setup

		ForwardingConsol Consol;
		Transport Transport;
		ForwardingShipment Shipment;
		DocTranshipment TranshipmentWrapper;

		protected override void SetUp()
		{
			Factory.GetDocWrapperContextManager().UpdateDocWrapperContextFromReportConstants(null);

			Consol = Factory.New<ForwardingConsol>();
			Transport = Consol.Transports[0];
			Shipment = Consol.Shipments.AddNew();

			DocForwardingConsol consolWrapper = DocForwardingConsol.New(Consol, Factory);
			DocForwardingShipment shipmentWrapper = DocForwardingShipment.New(Shipment, Factory);
			TranshipmentWrapper = new DocTranshipment(consolWrapper, shipmentWrapper);
			AssertNotNull(TranshipmentWrapper);

			base.SetUp();
		}

		#endregion
	}
}
