using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.TGE.Business;
using Enterprise.ClientSharedComponents;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.TGE
{
	internal class TGETestHelper : SharedTestHelper
	{
		internal TGETestHelper() : base() { }

		internal TGETestHelper(BusinessObjectFactory factory) : base(factory) { }

		public override void SetValidRegistryAll()
		{
			SetValidCustomCodes();
			SetValidRegistryCSSDataExportEnabled();
		}

		void SetValidCustomCodes()
		{
			TGEEventRegistryBusinessObjectCollection importCodes = new TGEEventRegistryBusinessObjectCollection();
			TGEEventRegistryBusinessObject cusEvent = importCodes.AddNew();
			cusEvent.Code = "ADD";
			TGEDataRegistry.Instance.CSSImportCustomsStatusCodes = importCodes;

			TGEEventRegistryBusinessObjectCollection exportCodes = new TGEEventRegistryBusinessObjectCollection();
			cusEvent = exportCodes.AddNew();
			cusEvent.Code = "ADD";
			TGEDataRegistry.Instance.CSSExportCustomsStatusCodes = exportCodes;
		}

		public ZString GetExportDirectory
		{
			get { return Path.Combine(Env.TempPath, "TGE"); }
		}

		public void SetValidRegistryForCSSExporterTest()
		{
			DataTransferSwitchRegistryBusinessObject newValue = new DataTransferSwitchRegistryBusinessObject(Factory);
			newValue.EnableInterface = true;
			newValue.Directory = GetExportDirectory;
			newValue.Interval = 1;
			newValue.UpdateRuns(ZDateTime.Now);
			newValue.GroupPK = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK;
			DataTransferSwitchRegistryItemTest.UpdateValue(rego.CSSDataTransferSwitchRegistryItem, newValue);
		}

		public void SetValidRegistryCSSDataExportEnabled()
		{
			DataTransferSwitchRegistryBusinessObject newValue = new DataTransferSwitchRegistryBusinessObject(Factory);
			newValue.EnableInterface = true;
			newValue.Directory = Env.TempPath;
			newValue.Interval = 1;
			newValue.UpdateRuns(ZDateTime.Now);
			newValue.GroupPK = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK;
			DataTransferSwitchRegistryItemTest.UpdateValue(rego.CSSDataTransferSwitchRegistryItem, newValue);
		}

		public DataTransferSwitchRegistryBusinessObject GetValidDataTransferSwitchRegistryBusinessObject(ZDateTime nextRunTime)
		{
			DataTransferSwitchRegistryBusinessObject newValue = GetValidDataTransferSwitchRegistryBusinessObject();
			newValue.NextRunDateTime = nextRunTime;
			return newValue;
		}

		public DataTransferSwitchRegistryBusinessObject GetValidDataTransferSwitchRegistryBusinessObject(ZDateTime lastRunTime, ZDateTime nextRunTime)
		{
			DataTransferSwitchRegistryBusinessObject newValue = GetValidDataTransferSwitchRegistryBusinessObject();
			newValue.NextRunDateTime = nextRunTime;
			newValue.LastRunDateTime = lastRunTime;
			return newValue;
		}

		public DataTransferSwitchRegistryBusinessObject GetValidDataTransferSwitchRegistryBusinessObject()
		{
			DataTransferSwitchRegistryBusinessObject newValue = new DataTransferSwitchRegistryBusinessObject(Factory);
			newValue.EnableInterface = true;
			newValue.Directory = Env.TempPath;
			newValue.Interval = 1;
			newValue.UpdateRuns(ZDateTime.Now);
			newValue.GroupPK = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL")).PK;
			return newValue;
		}

		internal IList<BusinessObject> GetPopulatedCollection()
		{
			List<BusinessObject> result = new List<BusinessObject>(2);
			result.Add(GetCusHawb());
			result.Add(GetJobDec());
			result.Add(GetShipment());
			return result;
		}

		#region CusHawb

		internal BusinessObject GetCusHawb()
		{
			return HouseBill;
		}

		CusMAWB MasterBill
		{
			get
			{
				if (masterBill == null)
				{
					masterBill = Factory.New<CusMAWB>();
					masterBill.CM_MAWB = "MAWB101";
					masterBill.CM_RL_NKDischargePort = "AUSYD";
					masterBill.CM_FlightNo = "QF123";
					masterBill.CM_ArrivalDate = new ZDateTime(2009, 10, 13, 11, 34, 0);
				}
				return masterBill;
			}
		}
		CusMAWB masterBill;

		CusHAWB HouseBill
		{
			get
			{
				if (houseBill == null)
				{
					houseBill = MasterBill.ChildBills.AddNew();
					houseBill.CS_HAWB = "HAWB1011";
					houseBill.CS_RL_NKOrigin = "SGSIN";
					houseBill.CS_RL_NKDestination = "AUSYD";
					houseBill.CS_GoodsDescription = "BOOKS";
					houseBill.CS_GoodsValue = 525.25m;
					houseBill.CS_RX_NKGoodsCurrency = Core.Constants.CurrencyCodes.Australia;
				}
				return houseBill;
			}
		}
		CusHAWB houseBill;

		#endregion

		#region JobDeclaration

		internal BusinessObject GetJobDec()
		{
			return GetNewJobDecWithEntry(Events.CustomsEntryStatus, "CLR", "11223344", "FRM", JobMessageTypeList.Codes.Export, Core.Constants.TransportModes.Air);
		}

		JobDeclaration GetNewJobDecWithEntry(Event entryEvent, ZString entryStatus, ZString entryNumber, ZString subMesgType, ZString mesgType, ZString transportMode)
		{
			JobDeclaration jobDec = GetNewDeclaration(subMesgType, mesgType, transportMode);
			jobDec.JE_MasterBill = "0813333";
			jobDec.JE_HouseBill = "HOUSEBILL";
			jobDec.DeclarationNumber = entryNumber;
			jobDec.JE_EntryStatus = entryStatus;

			CusEntryHeader entryHeader = jobDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = entryNumber;
			entryHeader.CH_EntryStatus = entryStatus;

			StmALog log = jobDec.Logs.AddNew(entryEvent, entryStatus);

			JobComInvoiceHeader comInv = jobDec.Invoices.AddNew();
			comInv.JZ_InvoiceNumber = "INV000222111";
			comInv.JZ_InvoiceAmount = 525.25m;
			comInv.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;

			Factory.Save();
			return jobDec;
		}

		JobDeclaration GetNewDeclaration(ZString messageSubType, ZString messageType, ZString transportMode)
		{
			JobDeclaration jobDec = JobDeclaration.New(Factory);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			if (messageType == JobMessageTypeList.Codes.Import)
			{
				jobDec.JE_RL_NKOrigin = "NZAKL";
				jobDec.JE_RL_NKFinalDestination = "AUSYD";
				shipment.JS_RL_NKOrigin = "NZAKL";
				shipment.JS_RL_NKDestination = "AUSYD";
			}
			else if (messageType == JobMessageTypeList.Codes.Export)
			{
				jobDec.JE_RL_NKOrigin = "AUSYD";
				jobDec.JE_RL_NKFinalDestination = "NZAKL";
				shipment.JS_RL_NKOrigin = "AUSYD";
				shipment.JS_RL_NKDestination = "NZAKL";
			}

			jobDec.JE_TransportMode = transportMode;
			jobDec.JE_MessageType = messageType;
			jobDec.JE_MessageSubType = messageSubType;
			jobDec.JE_VoyageFlightNo = "QF123";
			jobDec.JE_DateOfArrival = new ZDateTime(2009, 10, 13, 11, 34, 0);
			jobDec.JE_GoodsDescription = "BOOKS";

			jobDec.JE_JS = shipment.PK;
			ErrorReporter.Clear();

			return jobDec;
		}

		#endregion

		static TGEDataRegistry rego
		{
			get { return TGEDataRegistry.Instance; }
		}

		internal ForwardingShipment GetShipment()
		{
			return FindOrCreateForwardingShipment("S00001111", Core.Constants.TransportModes.Air);
		}

		public override ForwardingShipment FindOrCreateForwardingShipment(ZString uniqueConsignRef, ZString transportMode)
		{
			ForwardingShipment shipment = base.FindOrCreateForwardingShipment(uniqueConsignRef, transportMode);
			ForwardingConsol consol = base.FindOrCreateForwardingConsol("C00001111", transportMode, Core.Constants.AgentType.Agent, Core.Constants.ContainerModes.FCL);
			consol.Shipments.Add(shipment);
			return shipment;
		}

		protected override void SetSharedShipmentCore(ForwardingShipment shipment)
		{
			shipment.ConsignorDocumentaryAddress.OrganisationPK = SharedConsignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = SharedConsignee.PK;
			shipment.JS_HouseBill = "HOUSEBILL";
			SharedOrigin.RL_IATA = "IAT";
			SharedDestination.RL_IATA = "IAT";
			shipment.JS_RL_NKOrigin = SharedOrigin.RL_Code;
			shipment.JS_RL_NKDestination = SharedDestination.RL_Code;

			PackLine packline = shipment.OuterPackLines.AddNew();
			packline.JL_Length = packline.JL_Width = packline.JL_Height = 1;

			shipment.JS_ActualWeight = 1;
			shipment.JS_ActualVolume = 1;
			shipment.JS_ActualChargeable = 1;
			shipment.JS_OuterPacks = 1;
			shipment.JS_GoodsDescription = "BOOKS";
			shipment.JS_INCO = Core.Constants.IncoTerms.FreeOnBoard;
			shipment.JS_RS_NKServiceLevel = "STD";
			shipment.JS_GoodsValue = 525.25m;
			shipment.CustomsEntryNumber = "3333";

			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2001, 1, 1);
			shipment.JS_E_DEP = new ZDateTime(2001, 1, 1);
			shipment.JS_E_ARV = new ZDateTime(2001, 1, 1);
			shipment.JS_SystemCreateTimeUtc = new ZDateTime(2001, 1, 1);
			shipment.JS_OH_DeliveryAgent = SharedDeliveryAgent.PK;

			new JobHeader.Loader(shipment).TryCreateWithoutMutexForTestOnly();
			shipment.Job.LocalChargesPK = SharedConsignee.PK;
			shipment.Job.JH_GB = GlbBranch.CurrentBranch.PK;

			JobRequiredDocument requiredDoc = shipment.DocsAndCartage.RequiredDocuments.AddNew();
			requiredDoc.EQ_DocCategory = Core.Constants.ReferenceTypes.SupplyChainLogistics;
			requiredDoc.EQ_DateReceived = new ZDateTimeOffset(2009, 7, 27);
			requiredDoc.EQ_DocType = "EXD";
			requiredDoc.EQ_DocCategory = "SCL";
		}

		protected override void SetSharedConsolCore(ForwardingConsol consol)
		{
			consol.JK_RL_NKLoadPort = SharedOrigin.RL_Code;
			consol.JK_RL_NKDischargePort = SharedDestination.RL_Code;

			NUnit.Framework.Assertion.AssertNotNull(consol.Transports.MostInterestingTransport);
			consol.Transports.MostInterestingTransport.JW_IsLinked = ZBool.False;
			consol.Transports.MostInterestingTransport.JW_VoyageFlight = "QF123";
			consol.Transports.MostInterestingTransport.JW_RL_NKLoadPort = SharedOrigin.RL_Code;
			consol.Transports.MostInterestingTransport.JW_RL_NKDiscPort = SharedDestination.RL_Code;
			consol.Transports.MostInterestingTransport.JW_ETD = new ZDateTime(2009, 10, 13);
			consol.Transports.MostInterestingTransport.JW_ETA = new ZDateTime(2009, 10, 14);
			consol.Transports.MostInterestingTransport.JW_ATD = new ZDateTime(2009, 10, 13, 11, 34, 0);
			consol.MasterBillMAWB = "3333";

			consol.SetDefaultSendingForwarderAddress(Factory.NewWithValidTestData<OrgHeader>());
			consol.SetDefaultReceivingForwarderAddress(Factory.NewWithValidTestData<OrgHeader>());
			consol.SetDefaultShippingLineAddress(SharedShippingLine);

			consol.JK_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			ForwardingContainer container = consol.Containers.AddNew();
			container.JC_ContainerMode = Core.Constants.ContainerModes.FCL;
			container.JC_ContainerNum = "ABCD0101011";
			container.JC_SealNum = "SEAL123";
		}

		internal NotificationBuffer Notifications
		{
			get { return notifications ?? (notifications = new NotificationBuffer()); }
		}
		NotificationBuffer notifications;
	}
}
