using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocShipmentConsol))]
	sealed class DocShipmentConsolTestCase : DocBaseConsolAbstractTestClass
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { ConsolWrapper };
		}

		public override void TestETA()
		{
			ZDateTime now = ZDateTime.Now;

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUMEL";
			Consol.JK_RL_NKDischargePort = "CNSHA";

			Transport transport1 = Consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ETD = now.AddDays(2);
			transport1.JW_ETA = now.AddDays(4);

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "HKHKG";
			transport2.JW_ETD = now.AddDays(6);
			transport2.JW_ETA = now.AddDays(8);

			AssertEquals(now.AddDays(8), ConsolWrapper.ETA);
			AssertEquals(now.AddDays(8).ToShortDateString(), ConsolWrapper.ETAString);
		}

		public override void TestETD()
		{
			ZDateTime now = ZDateTime.Now;

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUMEL";
			Consol.JK_RL_NKDischargePort = "CNSHA";

			Transport transport1 = Consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ETD = now.AddDays(2);
			transport1.JW_ETA = now.AddDays(4);

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "HKHKG";
			transport2.JW_ETD = now.AddDays(6);
			transport2.JW_ETA = now.AddDays(8);

			AssertEquals(now.AddDays(2), ConsolWrapper.ETD);
			AssertEquals(now.AddDays(2).ToShortDateString(), ConsolWrapper.ETDString);
		}

		public void TestATA()
		{
			ZDateTime now = ZDateTime.Now;

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUMEL";
			Consol.JK_RL_NKDischargePort = "CNSHA";

			Transport transport1 = Consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ATD = now.AddDays(2);
			transport1.JW_ATA = now.AddDays(4);

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "HKHKG";
			transport2.JW_ATD = now.AddDays(6);
			transport2.JW_ATA = now.AddDays(8);

			AssertEquals(now.AddDays(8), ConsolWrapper.ATA);
		}

		public void TestATD()
		{
			ZDateTime now = ZDateTime.Now;

			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUMEL";
			Consol.JK_RL_NKDischargePort = "CNSHA";

			Transport transport1 = Consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ATD = now.AddDays(2);
			transport1.JW_ATA = now.AddDays(4);

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "HKHKG";
			transport2.JW_ATD = now.AddDays(6);
			transport2.JW_ATA = now.AddDays(8);

			AssertEquals(now.AddDays(2), ConsolWrapper.ATD);
			AssertEquals(now.AddDays(2).ToShortDateString(), ConsolWrapper.ATDString);
		}

		public override void TestPortOfLoading()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUMEL";
			Consol.JK_RL_NKDischargePort = "CNSHA";

			Transport transport1 = Consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "HKHKG";

			AssertNotNull("PortOfLoading", ConsolWrapper.PortOfLoading);
			AssertEquals("Correct port", "AUMEL", ConsolWrapper.PortOfLoading.Code);
		}

		public override void TestPortOfDischarge()
		{
			Consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			Consol.JK_RL_NKLoadPort = "AUMEL";
			Consol.JK_RL_NKDischargePort = "CNSHA";

			Transport transport1 = Consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "SGSIN";

			Transport transport2 = Consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "SGSIN";
			transport2.JW_RL_NKDiscPort = "HKHKG";

			AssertNotNull("PortOfDischarge", ConsolWrapper.PortOfDischarge);
			AssertEquals("Correct port", "CNSHA", ConsolWrapper.PortOfDischarge.Code);
		}

		public void TestAseanPointOfOriginForBOL()
		{
			AssertEquals("AseanPointOfOriginForBOL", "", ConsolWrapper.AseanPointOfOriginForBOL);

			Consol.JK_RL_NKLoadPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery()).Code;
			AssertEquals("AseanPointOfOriginForBOL", "", ConsolWrapper.AseanPointOfOriginForBOL);

			ZQuery filter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, "HK");
			RefUNLOCO loco = Factory.LoadTop1<RefUNLOCO>(filter);
			Consol.JK_RL_NKLoadPort = loco.Code;
			AssertEquals("AseanPointOfOriginForBOL", loco.RL_PortName + "," + loco.Country.RN_Desc, ConsolWrapper.AseanPointOfOriginForBOL);
		}

		public void TestShipments()
		{
			AssertEquals(0, ConsolWrapper.Shipments.Count);

			Consol.Shipments.AddNew();
			Consol.Shipments.AddNew();
			Consol.Shipments.AddNew();
			AssertEquals(3, ConsolWrapper.Shipments.Count);
		}

		#region Forwarding Instruction

		public void TestCarrier()
		{
			AssertEquals("Carrier", ZString.Empty, ConsolWrapper.Carrier);

			var testShippingLine = Factory.New<OrgHeader>();
			testShippingLine.OH_FullName = "Test Shipping Line";
			testShippingLine.MainAddress.OA_Address1 = "Shipping Line Address 1";
			testShippingLine.MainAddress.OA_Address2 = "Shipping Line Address 2";
			testShippingLine.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Consol.SetDefaultShippingLineAddress(testShippingLine);
			AssertEquals("Carrier", "TEST SHIPPING LINE\nSHIPPING LINE ADDRESS 1\nSHIPPING LINE ADDRESS 2\nNSW\nAUSTRALIA", ConsolWrapper.Carrier);

			Consol.JK_AgentType = Core.Constants.AgentType.CoLoad;
			AssertEquals("Carrier", "TEST SHIPPING LINE\nSHIPPING LINE ADDRESS 1\nSHIPPING LINE ADDRESS 2\nNSW\nAUSTRALIA", ConsolWrapper.Carrier);

			var testCreditor = Factory.New<OrgHeader>();
			testCreditor.OH_FullName = "Test Creditor";
			testCreditor.MainAddress.OA_Address1 = "Creditor Address 1";
			testCreditor.MainAddress.OA_Address2 = "Creditor Address 2";
			testCreditor.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			Consol.SetDefaultShippingLineAddress(testCreditor);
			AssertEquals("Carrier", "TEST CREDITOR\nCREDITOR ADDRESS 1\nCREDITOR ADDRESS 2\nNSW\nAUSTRALIA", ConsolWrapper.Carrier);
		}

		#endregion

		#region Implementation

		ForwardingConsol Consol;
		DocShipmentConsol ConsolWrapper;

		protected override void SetUp()
		{
			Consol = Factory.New<ForwardingConsol>();
			ConsolWrapper = DocShipmentConsol.New(Consol, Factory);
			base.SetUp();
		}

		#endregion
	}
}
