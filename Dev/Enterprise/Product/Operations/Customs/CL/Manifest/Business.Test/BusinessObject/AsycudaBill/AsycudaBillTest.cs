using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	sealed class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		#region Consignee

		public void TestConsigneeRegNo()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME 1";
			var orgAddress1 = org1.MainAddress;
			org1.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals("12345678", bill.ABL_ConsigneeRegNo);
			bill.ABL_OA_Consignee = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
		}

		public void TestConsigneeWithTwoRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "J074878112");
			org.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Consignee = orgAddress.PK;
			AssertEquals("12345678", bill.ABL_ConsigneeRegNo);
			AssertEquals("RUT", bill.ABL_ConsigneeRegNoType);
		}

		public void TestConsigneeRegNoType()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME 1";
			var orgAddress1 = org1.MainAddress;
			org1.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals("RUT", bill.ABL_ConsigneeRegNoType);
			bill.ABL_OA_Consignee = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);
		}

		#endregion

		#region Shipper

		public void TestShipperRegNo()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME";
			var orgAddress1 = org1.MainAddress;
			org1.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Shipper = orgAddress1.PK;
			AssertEquals("12345678", bill.ABL_ShipperRegNo);
			bill.ABL_OA_Shipper = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNo);
		}

		public void TestShipperWithTwoRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "J074878112");
			org.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_Shipper = orgAddress.PK;
			AssertEquals("12345678", bill.ABL_ShipperRegNo);
			AssertEquals("RUT", bill.ABL_ShipperRegNoType);
		}

		public void TestShipperRegNoType()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME";
			var orgAddress1 = org1.MainAddress;
			org1.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Shipper = orgAddress1.PK;
			AssertEquals("RUT", bill.ABL_ShipperRegNoType);
			bill.ABL_OA_Shipper = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_ShipperRegNoType);
		}

		#endregion

		#region Notify Party

		public void TestNotifyPartyRegNo()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME";
			var orgAddress1 = org1.MainAddress;
			org1.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_NotifyParty = orgAddress1.PK;
			AssertEquals("12345678", bill.ABL_NotifyPartyRegNo);
			bill.ABL_OA_NotifyParty = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNo);
		}

		public void TestNotifyPartyWithTwoRegNo()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_FullName = "FULL NAME";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "J074878112");
			org.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_OA_NotifyParty = orgAddress.PK;
			AssertEquals("12345678", bill.ABL_NotifyPartyRegNo);
			AssertEquals("RUT", bill.ABL_NotifyPartyRegNoType);
		}

		public void TestNotifyPartyRegNoType()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME";
			var orgAddress1 = org1.MainAddress;
			org1.CustomsCodes.AddNew(ChileOrgCusCodeInfo.OrgCusCodes.RUT, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			bill.ABL_OA_NotifyParty = orgAddress1.PK;
			AssertEquals("RUT", bill.ABL_NotifyPartyRegNoType);
			bill.ABL_OA_NotifyParty = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNoType);
		}

		#endregion

		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.CLManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.Packs);
		}

		public void TestGetCountryCode()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.CountryCodes.Chile, bill.GetCountryCode());
		}

		public void TestCreateNewAsycudaPackCollection()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertType<AsycudaPackCollection<AsycudaPack, AsycudaBill>>(bill.CreateNewAsycudaPackCollection());
		}

		public void TestGetPackTypeCore()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(typeof(AsycudaPack), bill.GetPackTypeCore());
		}

		public void TestABL_BillNumber_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals(false, bill.ABL_BillNumber_ReadOnly);

			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;
			AssertEquals(true, bill.ABL_BillNumber_ReadOnly);

			bill.ABL_BillStatus = CustomsStatusList.Codes.ERR;
			AssertEquals(false, bill.ABL_BillNumber_ReadOnly);
		}

		public void TestDelete()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			var arrivalHeader = header.ArrivalHeaders.AddNew();
			var arrivalLine = arrivalHeader.ArrivalDetails.AddNew();

			arrivalLine.ATL_ABL_AsycudaBill = bill.PK;
			arrivalLine.ATL_APA_AsycudaPack = pack.PK;

			bill.Delete();
			Assert(arrivalLine.IsDeleted);
		}

		public void TestBillCanBeSent()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			var bill = header.Bills.AddNew();

			AssertEquals("If the Bill has not status, it must be true", true, bill.CanSendOriginalMessage);

			bill.ABL_BillStatus = "";
			bill.Messages.Add(CreateMessage());

			AssertEquals("If the Bill has not status, but Correlativo_Parcial is = or > Leg Order", false, bill.CanSendOriginalMessage);

			var arrivalHeader2 = header.ArrivalHeaders.AddNew();
			AssertEquals("If the Bill has not status, but Correlativo_Parcial < Leg Order", true, bill.CanSendOriginalMessage);
		}

		public void TestBillCanBeDeleted()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			var bill = header.Bills.AddNew();

			AssertEquals("If the Bill has not status, it must be true", true, bill.CanDelete);

			bill.ABL_BillStatus = "ACP";
			AssertEquals("If the Bill has a status of Accepted, it must be false", false, bill.CanDelete);

			bill.ABL_BillStatus = "CAN";
			AssertEquals("If the Bill has a status of CAN, it must be false", false, bill.CanDelete);

			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";
			AssertEquals("If the Bill has a status of SNT and MessageStatus AWA, it must be false", false, bill.CanDelete);
		}

		public void TestBillCantBeDeleteWhenStatusIsAccepted()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACP";
			AssertEquals(false, bill.CanDelete);

			AssertEquals("This Bill is already sent and accepted.\r\nIf you need to cancel it from Customs, you must use the Cancel option from Manifest menu.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestBillCantBeDeleteWhenStatusIsWaiting()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";
			AssertEquals(false, bill.CanDelete);

			AssertEquals("This Bill is already sent and it is awaiting for a response.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			bill.ABL_MessageStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestBillCantBeDeleteWhenStatusIsCancelled()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "CAN";
			AssertEquals(false, bill.CanDelete);

			AssertEquals("This bill is canceled. Must be kept for history purposes.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestPacksReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var bill = header.Bills.AddNew();
			AssertEquals(false, bill.Packs.ReadOnly);

			bill.ABL_BillStatus = "ACP";
			AssertEquals(true, bill.Packs.ReadOnly);

			bill.ABL_BillStatus = ZString.Empty;
			AssertEquals(false, bill.Packs.ReadOnly);

			bill.ABL_BillStatus = "CAN";
			AssertEquals(true, bill.Packs.ReadOnly);

			bill.ABL_BillStatus = ZString.Empty;
			AssertEquals(false, bill.Packs.ReadOnly);

			bill.ABL_BillStatus = "SNT";
			bill.ABL_MessageStatus = "AWA";
			AssertEquals(true, bill.Packs.ReadOnly);
		}

		public void TestManifestUQIsNotConverted()
		{
			var pack = Factory.New<CusRefPacks>();
			pack.RP_CustomsPack = "BT";
			pack.RP_Type = "GMB";
			pack.RP_CustomsCountry = "CL";
			pack.RP_ConversionFactor = 2;
			pack.RP_CommercialPack = "PCS";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_OuterPacks = 10;
			shipment.JS_F3_NKPackType = "PCS";
			var consol = shipment.Consols.AddNew();

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			var bill = manifestHeader.Bills[0];
			CombineAssertions(() =>
			{
				AssertEquals("ABL_ManifestQty is not converted", 10, bill.ABL_ManifestQty);
				AssertEquals("ABL_ManifestUQ is not converted", "PCS", bill.ABL_ManifestUQ);
			});
		}

		EDIMessage CreateMessage()
		{
			var message = Factory.New<CLMessage>();
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.CLCustoms;
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			message.EM_IsTestMessage = true;
			message.EM_MessageNum = "01";
			message.EM_MessageText = airRequest;
			message.EM_MessageType = MessageTypes.Codes.CHE;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;

			Factory.Save();
			return message;
		}

		readonly string airRequest = "<?xml version=\"1.0\" encoding=\"iso-8859-1\"?>\r\n<Documento xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" tipo=\"GA\" version=\"2.0\">\r\n  <tipo-accion>M</tipo-accion>\r\n  <numero-referencia>(H)QRHW20050055C</numero-referencia>\r\n  <total-bultos>1</total-bultos>\r\n  <total-peso>170.000</total-peso>\r\n  <unidad-peso>KGM</unidad-peso>\r\n  <total-volumen>0.76</total-volumen>\r\n  <unidad-volumen>MTQ</unidad-volumen>\r\n  <total-item>20</total-item>\r\n  <parcial>S</parcial>\r\n  <correlativo-parcial>00001</correlativo-parcial>\r\n  <OpTransporte>\r\n    <optransporte>\r\n      <viaje>Voyage</viaje>\r\n      <sentido-operacion>I</sentido-operacion>\r\n      <tipo-operacion>TR</tipo-operacion>\r\n    </optransporte>\r\n  </OpTransporte>\r\n  <Fechas>\r\n    <fecha>\r\n      <nombre>FEM</nombre>\r\n      <valor>21-06-2020</valor>\r\n    </fecha>\r\n  </Fechas>\r\n  <Locaciones>\r\n    <locacion>\r\n      <nombre>PD</nombre>\r\n      <codigo>CLSCL</codigo>\r\n    </locacion>\r\n    <locacion>\r\n      <nombre>PE</nombre>\r\n      <codigo>CLSCL</codigo>\r\n    </locacion>\r\n  </Locaciones>\r\n  <Participaciones>\r\n    <participacion>\r\n      <nombre>ALM</nombre>\r\n      <tipo-id>RUT</tipo-id>\r\n      <valor-id>96888200-7</valor-id>\r\n      <nombres>DEPOCARGO LTDA.</nombres>\r\n    </participacion>\r\n    <participacion>\r\n      <nombre>CAER</nombre>\r\n      <tipo-id>RUT</tipo-id>\r\n      <valor-id>77491900-7</valor-id>\r\n      <nombres>TRANS AMERICAN AIR LINES S.A.</nombres>\r\n    </participacion>\r\n    <participacion>\r\n      <nombre>EMI</nombre>\r\n      <tipo-id>RUT</tipo-id>\r\n      <valor-id>76006380-9</valor-id>\r\n      <nombres>SACO SHIPPING S.A.</nombres>\r\n    </participacion>\r\n    <participacion>\r\n      <nombre>EMIDO</nombre>\r\n      <valor-id>76006281-9</valor-id>\r\n      <nombres>NEW CHARTER SRL</nombres>\r\n    </participacion>\r\n    <participacion>\r\n      <nombre>CONS</nombre>\r\n      <tipo-id>RUT</tipo-id>\r\n      <valor-id>77144766-K</valor-id>\r\n      <nombres>COMERCIAL Y SERVICIOS HYDROVAK S.P.A.</nombres>\r\n    </participacion>\r\n    <participacion>\r\n      <nombre>NOTI</nombre>\r\n      <tipo-id>RUT</tipo-id>\r\n      <valor-id>77144744-3</valor-id>\r\n      <nombres>ACRICIEL S.A.</nombres>\r\n    </participacion>\r\n    <participacion>\r\n      <nombre>CNTE</nombre>\r\n      <valor-id>99999999-9</valor-id>\r\n      <nombres>JUROP S.P.A.</nombres>\r\n    </participacion>\r\n  </Participaciones>\r\n  <Items>\r\n    <item>\r\n      <numero-item>1</numero-item>\r\n      <carga-peligrosa>N</carga-peligrosa>\r\n      <cantidad>1</cantidad>\r\n      <peso-bruto>170.000</peso-bruto>\r\n      <unidad-peso>KGM</unidad-peso>\r\n      <volumen>0.76</volumen>\r\n      <unidad-volumen>MTQ</unidad-volumen>\r\n      <ProdItem>\r\n        <proditem>\r\n          <descripcion>Product name</descripcion>\r\n          <cantidad>1</cantidad>\r\n          <unidad-medida>KGM</unidad-medida>\r\n        </proditem>\r\n      </ProdItem>\r\n    </item>\r\n  </Items>\r\n  <Cargos>\r\n    <cargo>\r\n      <tipo-cargo>FLET</tipo-cargo>\r\n      <monto>399.65</monto>\r\n      <moneda>USD</moneda>\r\n      <cond-pago>P</cond-pago>\r\n    </cargo>\r\n  </Cargos>\r\n  <Referencias>\r\n    <referencia>\r\n      <tipo-referencia>REF</tipo-referencia>\r\n      <tipo-documento>GA</tipo-documento>\r\n      <numero>879835</numero>\r\n      <fecha>07-12-2020</fecha>\r\n    </referencia>\r\n  </Referencias>\r\n  <Observaciones>\r\n    <observacion>\r\n      <nombre>GRAL</nombre>\r\n      <contenido>Reason</contenido>\r\n    </observacion>\r\n  </Observaciones>\r\n</Documento>\r\n";

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZString GetCountryCode() => base.GetCountryCode();
			public new ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => base.CreateNewAsycudaPackCollection();
			public new Type GetPackTypeCore() => base.GetPackTypeCore();
		}

		#endregion
	}
}
