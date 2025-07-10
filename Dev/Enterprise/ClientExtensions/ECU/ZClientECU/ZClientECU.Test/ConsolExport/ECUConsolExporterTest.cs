using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.ECU.ConsolExport.Testing
{
	[TestedType(typeof(ECUConsolExporter))]
	public class ECUConsolExporterTest : FlatFileDataExporterTestCase
	{
		[TestDate(2006, 6, 6, 6, 30, 0)]
		public void TestPopulateExportInstructions()
		{
			NotificationBuffer notification = new NotificationBuffer();
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_RL_NKLoadPort = "AUSYD";
			Transport transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USLAX";
			ReceivingAgent.EDICommunicationsModes.ClientSpecificDestination = "Test@Test.com";
			ReceivingAgent.EDICommunicationsModes.ClientSpecificCommunicationTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
			consol.JK_OA_ReceivingForwarderAddress = ReceivingAgent.MainAddress.PK;
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = "AUSYD";
			Factory.Save();
			Env.OutgoingMailManager.EmailsCreated.Clear();
			ECUConsolExporter exporter = new ECUConsolExporter(Factory);
			ForwardingConsolCollection collection = new ForwardingConsolCollection(Factory);
			collection.Add(consol);
			CollectionWrapperBusinessObjectReader reader = new CollectionWrapperBusinessObjectReader(collection);
			exporter.Export(reader, notification);
			AssertEquals("1 email should be sent", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			EmailDef email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Subject of Email should be 'ECU Manifest Export'", "ECU Manifest Export", email.Subject);
			AssertEquals("Email should have 1 attachment", 1, email.Attachments.Count);
			AssertEquals("1 recipient", 1, email.Recipients.Count);
			AssertEquals("Recipient Address", "Test@Test.com", email.Recipients[0]);
		}

#region Setup
		OrgHeader Consignee, Consignor, LoadingAgent, NotifyParty, ReceivingAgent;
		public override FlatFileDataExporter GetDataExporter()
		{
			return new ECUConsolExporter(Factory);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ECUConsolExporter(Factory);
		}

#region GetPopulatedCollectionToSaveAndExport
		public override IBusinessObjectCollection GetPopulatedCollectionToSaveAndExport()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = "SEA";
			voyage.JV_RV_NKVessel = RefVessel.LookupVesselByName("AMERICA STAR", Factory).First().RV_FK;
			voyage.JV_VoyageFlight = "Voyage123";
			VoyageOrigin origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = new ZDateTime(2005, 04, 01);
			VoyageDestination destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "USLAX";
			destination.JB_E_ARV = new ZDateTime(2005, 05, 01);
			voyage.GenerateSailings();
			JobSailing sailing = voyage.Sailings[0];
			ForwardingConsol populatedConsol = Factory.New<ForwardingConsol>();
			populatedConsol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			populatedConsol.JK_MasterBillNum = "MASTERBILL";
			populatedConsol.JK_UniqueConsignRef = "C00001002";
			Transport populatedTransport = populatedConsol.Transports[0];
			populatedTransport.JW_JX = sailing.PK;
			populatedConsol.AutomaticallyUpdatePackLineContainers = false;
			populatedConsol.JK_OA_ReceivingForwarderAddress = LoadingAgent.MainAddress.PK;
			populatedConsol.JK_PrepaidCollect = "PPD";
			populatedConsol.JK_OA_ReceivingForwarderAddress = ReceivingAgent.MainAddress.PK;
			CommonContainer container = populatedConsol.Containers.AddNew();
			container.JC_ContainerNum = "ABCD1111";
			container.JC_SealNum = "SEAL1";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;
			container.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			CommonContainer container2 = populatedConsol.Containers.AddNew();
			container2.JC_ContainerNum = "ABCD2222";
			container2.JC_SealNum = "SEAL24";
			container2.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "40RE")).PK;
			container2.JC_ContainerMode = Core.Constants.ContainerModes.LCL;
			//Shipment 1
			CommonShipment shipment1 = populatedConsol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "S00001110";
			shipment1.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			shipment1.ConsignorDocumentaryAddress.OrganisationPK = Consignor.PK;
			shipment1.JS_RL_NKOrigin = "AUSYD";
			shipment1.JS_RL_NKDestination = "USLAX";
			shipment1.JS_E_DEP = new ZDateTime(2005, 04, 01);
			shipment1.JS_E_ARV = new ZDateTime(2005, 05, 01);
			shipment1.JS_HouseBill = "11111111";
			shipment1.JS_UnitFreightRate = new ZDecimal(0.74);
			shipment1.JS_RX_NKFrtRateCurrency = "USD";
			shipment1.NotifyPartyDocumentaryAddress.OrganisationPK = NotifyParty.PK;
			shipment1.JS_GoodsDescription = "PENCIL";
			shipment1.JS_MarksAndNumbers = "MARKSANDNUMBERS";
			shipment1.JS_OuterPacks = 10;
			shipment1.JS_F3_NKPackType = Core.Constants.PkgUnit.Bottle;
			shipment1.JS_ActualVolume = 10.5m;
			shipment1.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment1.JS_ActualWeight = 2.5m;
			shipment1.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			SetupPackLine(shipment1.OuterPackLines, 10.5m, 2.5m, 10, "PENCIL", "MARKSANDNUMBERS", Core.Constants.PkgUnit.Bottle, container.PK);
			//Shipment 2
			CommonShipment shipment2 = populatedConsol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "S00002220";
			shipment2.ConsigneeDocumentaryAddress.OrganisationPK = Consignee.PK;
			shipment2.ConsignorDocumentaryAddress.OrganisationPK = Consignor.PK;
			shipment2.JS_RL_NKOrigin = "AUSYD";
			shipment2.JS_RL_NKDestination = "USLAX";
			shipment2.JS_E_DEP = new ZDateTime(2005, 04, 01);
			shipment2.JS_E_ARV = new ZDateTime(2005, 05, 01);
			shipment2.JS_HouseBill = "22222222";
			shipment2.JS_UnitFreightRate = new ZDecimal(0.74);
			shipment2.JS_RX_NKFrtRateCurrency = "USD";
			shipment2.NotifyPartyDocumentaryAddress.OrganisationPK = NotifyParty.PK;
			shipment2.JS_GoodsDescription = "LOLLIES";
			shipment2.JS_MarksAndNumbers = "MARKSANDNUMBERS3";
			shipment2.JS_OuterPacks = 10;
			shipment2.JS_F3_NKPackType = Core.Constants.PkgUnit.Piece;
			shipment2.JS_ActualVolume = 10.5m;
			shipment2.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment2.JS_ActualWeight = 2.5m;
			shipment2.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			SetupPackLine(shipment1.OuterPackLines, 10.5m, 2.5m, 10, "LOLLIES", "MARKSANDNUMBERS3", Core.Constants.PkgUnit.Piece, container.PK);
			ForwardingConsolCollection consols = new ForwardingConsolCollection(Factory);
			consols.Add(populatedConsol);
			Factory.Save();
			return consols;
		}

#endregion
		void SetupPackLine(OuterPackLineCollection packLineCollection, ZDecimal vol, ZDecimal weight, int packCount, ZString desc, ZString marksAndNumbers, ZString packType, ZGuid containerPK)
		{
			PackLine pack1 = packLineCollection.AddNew();
			pack1.JL_JC = containerPK;
			pack1.JL_PackageCount = packCount;
			pack1.JL_F3_NKPackType = packType;
			pack1.JL_MarksAndNumbers = marksAndNumbers;
			pack1.JL_ActualVolume = vol;
			pack1.JL_ActualWeight = weight;
			pack1.JL_Description = desc;
		}

		void SetupOrganisation()
		{
			Consignee = Factory.New<OrgHeader>();
			Consignee.OH_Code = "BIGBIRD";
			Consignee.OH_FullName = "BigBird Incorporated Consignee";
			OrgAddress consigneeAddress = Consignee.Addresses[0];
			consigneeAddress.OA_Address1 = "Level 3";
			consigneeAddress.OA_Address2 = "1 Sesame St";
			consigneeAddress.OA_City = "Sydney";
			consigneeAddress.OA_State = "NSW";
			consigneeAddress.OA_Phone = "33333333";
			consigneeAddress.OA_Fax = "44444444";
			Consignor = Factory.New<OrgHeader>();
			Consignor.OH_Code = "COOKIEMAN";
			Consignor.OH_FullName = "CookieMan Incorporated Consignor";
			OrgAddress consignorAddress = Consignor.Addresses[0];
			consignorAddress.OA_Address1 = "2 Sesame St";
			consignorAddress.OA_Address2 = "";
			consignorAddress.OA_City = "Los Angeles";
			consignorAddress.OA_State = "CA";
			consignorAddress.OA_Phone = "55555555";
			consignorAddress.OA_Fax = "66666666";
			NotifyParty = Factory.New<OrgHeader>();
			NotifyParty.OH_Code = "BERT";
			NotifyParty.OH_FullName = "Bert Incorporated Notify Party";
			OrgAddress notifyPartyAddress = NotifyParty.Addresses[0];
			notifyPartyAddress.OA_Address1 = "3 Sesame St";
			notifyPartyAddress.OA_Address2 = "";
			notifyPartyAddress.OA_City = "Melbourne";
			notifyPartyAddress.OA_State = "VIC";
			notifyPartyAddress.OA_Phone = "77777777";
			notifyPartyAddress.OA_Fax = "88888888";
			LoadingAgent = Factory.New<OrgHeader>();
			LoadingAgent.OH_Code = "ERNIE";
			LoadingAgent.OH_FullName = "Ernie Incorporated Delivery Agent";
			OrgAddress loadingAgentAddress = LoadingAgent.Addresses[0];
			loadingAgentAddress.OA_Address1 = "4 Sesame St";
			loadingAgentAddress.OA_Address2 = "";
			loadingAgentAddress.OA_City = "Sydney";
			loadingAgentAddress.OA_State = "NSW";
			loadingAgentAddress.OA_Phone = "99999999";
			loadingAgentAddress.OA_Fax = "00000000";
			ReceivingAgent = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			ReceivingAgent.OH_Code = "RECAGT";
			OrgAddress address = ReceivingAgent.Addresses.AddNew();
			address.OA_Address1 = "blah";
			OrgCusCode gTN = ReceivingAgent.CustomsCodes.AddNew();
			gTN.OK_RN_NKCodeCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			gTN.OK_CodeType = OrgCusCode.CodeTypes.GlobalTrackingName;
			gTN.OK_CustomsRegNo = "ECUAGENTCODE";
			Factory.Save();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupOrganisation();
		}
#endregion
	}
}
