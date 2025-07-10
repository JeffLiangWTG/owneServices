using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AirCargoMessageHelperTest : TestCaseWithFactory
	{
		[TestDate(2005, 5, 1)]
		public void TestSetSACFlag()
		{
			consol.Shipments[0].Logs.AddNew(Events.DataImport, "AU Declaration Style: SAC");
			using (var job = new AirCargoProcessorJobForConsol(consol))
			{
				processor.Process(job);
			}

			var cusMAWBFilter = new ZQuery(CusMAWBSchema.CM_JK, consol.PK);
			var cusMawb = Factory.LoadTop1<CusMAWB>(cusMAWBFilter);
			AssertNotNull(cusMawb);

			var cusHawb = CusHAWB.Load(consol.Shipments[0]);
			AssertNotNull(cusHawb);
			Assert(cusHawb.CS_IsSelfAssessedClearance);

			cusHawb = CusHAWB.Load(consol.Shipments[1]);
			AssertNotNull(cusHawb);
			Assert(!cusHawb.CS_IsSelfAssessedClearance);

			AssertEquals(2, processor.GeneratedHouseMessages.Count());
		}

		public void TestProcess_MawbHasErrors()
		{
			var testConsol = Factory.New<ForwardingConsol>();
			testConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			testConsol.Shipments.AddNew();

			using (var job = new AirCargoProcessorJobForConsol(testConsol))
			{
				processor.Process(job);
				Assert(!job.IsAcceptable);
			}
			AssertEquals(1, notify.Events.Length);
			var displayMessages = notify.Events[0].Message;
			var expected = "The final discharge port is not an Australian port.";

			Assert("\"" + displayMessages + "\" should contain \"" + expected + "\"", displayMessages.Contains(expected));
		}

		public void TestProcess_MawbDoesNotHaveErrors()
		{
			using (var job = new AirCargoProcessorJobForConsol(consol))
			{
				processor.Process(job);
				Assert(job.IsAcceptable);
			}
			Assert(!notify.HasErrors);
		}

		public void TestProcess_NotInAu()
		{
			var transport = consol.Transports[0];
			// Hit the getter or the SailingManager may go spastic when you first try to change something.
			// if the test still passes after removing this line then it has probably been fixed.
			Assert(!transport.JW_RL_NKDiscPort.IsEmpty);

			transport.JW_RL_NKDiscPort = "USJFK";

			using (var job = new AirCargoProcessorJobForConsol(consol))
			{
				processor.Process(job);
				Assert(!job.IsAcceptable);
			}
			AssertEquals(1, notify.Events.Length);
			Assert(notify.Events[0].Message.Contains("The final discharge port is not an Australian port."));
		}

		public void TestProcess_NotAir()
		{
			consol.JK_TransportMode = "SEA";
			consol.JK_MasterBillNum = consol.JK_MasterBillNum;
			using (var job = new AirCargoProcessorJobForConsol(consol))
			{
				processor.Process(job);
				Assert(!job.IsAcceptable);
			}
			AssertEquals(1, notify.Events.Length);
			AssertEquals("Error: This is not an Air Cargo.", notify.Events[0].Message);
		}

		public void TestProcess_NoMAWB()
		{
			using (var mutexForConsol = CusMAWB.CreateMutexForConsol(consol.PK, GlbCompany.CurrentCompany.GC_RN_NKCountryCode))
			{
				mutexForConsol.Lock();

				using (var job = new AirCargoProcessorJobForConsol(consol))
				{
					processor.Process(job);
					Assert(!job.IsAcceptable);
				}
				AssertEquals(1, notify.Events.Length);
				AssertEquals("Error: A Customs master bill record cannot be created as someone else is trying to create a master bill for this consol.", notify.Events[0].Message);
			}
		}

		[TestDate(2005, 5, 1)]
		public void TestConcurrencyErrorDoesNotCauseException()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;

			var processorWithFactory = new HouseBillsCargoMessageProcessor(notify);
			using (var job = new AirCargoProcessorJobForConsol(consol))
			{
				var factory2 = new BusinessObjectFactory();
				factory2.RefreshEnabled = false;
				var consolInFactory2 = factory2.Load<ForwardingConsol>(consol.PK);
				consolInFactory2.JK_AgentsReference = "AAA";
				factory2.Save();
				consol.JK_AgentsReference = "BBB";

				processorWithFactory.Process(job);

				Assert(job.IsAcceptable);
				Assert(notify.HasErrors);
				AssertEquals("Messages were not sent, error during saving.", notify.Events[2].Message);
			}
		}

		ZString currentRegNo;
		OrgHeader currentCompany;
		BusinessObjectFactory currentCompanyFactory;
		protected override void SetUp()
		{
			base.SetUp();

			currentCompanyFactory = new BusinessObjectFactory();
			currentRegNo = GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number;
			currentCompany = currentCompanyFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.OrgProxy.PK);
			currentCompany.PrimaryRegistrationNumber.Number = "21 003 980 130 123";
			currentCompanyFactory.Save();

			consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "MA1235";
			consol.JK_AgentType = "AGT";
			consol.JK_TransportMode = "AIR";
			consol.JK_ConsolMode = "LSE";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "GBLHR";
			transport.JW_RL_NKDiscPort = "AUBNE";
			transport.JW_VoyageFlight = "QF123";
			consol.MasterBillAirlinePrefix = "081";
			consol.MasterBillMAWB = "11111111";
			transport.JW_ETD = new ZDateTime(2004, 5, 23);
			transport.JW_ATD = new ZDateTime(2004, 5, 24);
			transport.JW_ETA = new ZDateTime(2004, 5, 29);
			consol.JK_PrepaidCollect = "PPD";

			shipment1 = consol.Shipments.AddNew();
			shipment1.JS_PackingMode = "LCL";
			shipment1.JS_TransportMode = "AIR";
			shipment1.ConsignorPK = OrgHeader.LoadFromCode(Factory, "WESCOM").PK;
			shipment1.ConsigneePK = OrgHeader.LoadFromCode(Factory, "MONMED").PK;
			shipment1.JS_GoodsDescription = "Downsized Developers";
			shipment1.JS_HouseBill = "HA1235";
			shipment1.JS_RL_NKOrigin = "GBLHR";
			shipment1.JS_RL_NKDestination = "AUBNE";
			shipment1.JS_OH_DeliveryAgent = OrgHeader.LoadFromCode(Factory, "AUSCON").PK;
			shipment1.JS_ActualWeight = 1;
			shipment1.JS_OuterPacks = 1;

			shipment2 = consol.Shipments.AddNew();
			shipment2.JS_PackingMode = "LCL";
			shipment2.JS_TransportMode = "AIR";
			shipment2.ConsignorPK = OrgHeader.LoadFromCode(Factory, "WESCOM").PK;
			shipment2.ConsigneePK = OrgHeader.LoadFromCode(Factory, "MONMED").PK;
			shipment2.JS_GoodsDescription = "Downsized Developers";
			shipment2.JS_HouseBill = "HA15";
			shipment2.JS_RL_NKOrigin = "GBLHR";
			shipment2.JS_RL_NKDestination = "AUBNE";
			shipment2.JS_OH_DeliveryAgent = OrgHeader.LoadFromCode(Factory, "AUSCON").PK;
			shipment2.JS_ActualWeight = 1;
			shipment2.JS_OuterPacks = 1;

			consol2 = Factory.New<ForwardingConsol>();
			consol2.FillWithValidTestData();
			consol2.MasterBillMAWB = "mawb123";
			consol2.JK_RL_NKPortOfFirstArrival = "AUSYD";

			shipment21 = consol2.Shipments.AddNew();
			shipment21.FillWithValidTestData();
			shipment21.JS_HouseBill = "hawb1";
			shipment21.JS_RL_NKOrigin = "GBLHR";
			shipment21.JS_RL_NKDestination = "AUSYD";

			shipment22 = consol2.Shipments.AddNew();
			shipment22.FillWithValidTestData();
			shipment22.JS_HouseBill = "hawb2";
			shipment22.JS_RL_NKOrigin = "GBLHR";
			shipment22.JS_RL_NKDestination = "AUSYD";

			Factory.Save();

			notify = new NotificationBuffer();
			processor = new HouseBillsCargoMessageProcessor(notify);
		}

		protected override void TearDown()
		{
			currentCompany.PrimaryRegistrationNumber.Number = currentRegNo;
			currentCompanyFactory.Save();
			base.TearDown();
		}

		HouseBillsCargoMessageProcessor processor;

		ForwardingConsol consol;
		ForwardingConsol consol2;
		CommonShipment shipment1;
		CommonShipment shipment2;
		CommonShipment shipment22;
		CommonShipment shipment21;
		NotificationBuffer notify;
	}
}
