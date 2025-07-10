using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class SeaCargoMessageProcessorTest : TestCaseWithFactory
	{
		public void TestSetSACFlag()
		{
			var consol = GetValidConsol();
			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment1.JS_PackingMode = Core.Constants.ContainerModes.Bulk;
			shipment1.Logs.AddNew(Events.DataImport, "AU Declaration Style: SAC");
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "N111100004";
			var packLine = shipment1.OuterPackLines.AddNew();
			packLine.JL_JC = container.PK;

			var processor = new SeaCargoMessageProcessor(new NotificationBuffer());
			processor.Process(new SeaCargoProcessorJobForConsol(consol));

			var oceanBillFilter = new ZQuery(CusSCAOceanBillSchema.CB_ParentId, consol.PK);
			var oceanBill = Factory.LoadTop1<CusSCAOceanBill>(oceanBillFilter);
			AssertNotNull(oceanBill);

			var cusSCAHouseFilter = new ZQuery(CusSCAPivotSchema.CV_CA, Factory.LoadTop1<CusSCAHouse>(new ZQuery(CusSCAHouseSchema.CA_JS, shipment1.PK)).PK);
			var cusSCAPivot = Factory.LoadTop1<CusSCAPivot>(cusSCAHouseFilter);
			AssertNotNull(cusSCAPivot);
			Assert(cusSCAPivot.CV_IsSAC);
		}

		public void TestProcessAcceptableJob()
		{
			var buffer = new NotificationBuffer();
			var processor = new SeaCargoMessageProcessor(buffer);

			var consol = GetValidConsol();
			consol.JK_MasterBillNum = "BILL123";
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			Factory.Save();

			var oceanBill = CusSCAOceanBill.Load(consol);
			AssertNull("CusSCAOceanBill should not be created", oceanBill);

			processor.Process(new SeaCargoProcessorJobForConsol(consol));

			oceanBill = CusSCAOceanBill.Load(consol);
			AssertNotNull("CusSCAOceanBill should be created", oceanBill);
			AssertEquals("HouseBills count", 1, oceanBill.HouseBills.Count);
			AssertEquals("HouseBill.Messages.Count", 1, oceanBill.HouseBills[0].Messages.Count);
			AssertContains("Messages were sent", "Sea Cargo Messages for BILL123 were sent.", buffer.AsString);
		}

		public void TestProcessAcceptableJobWithoutSave()
		{
			var buffer = new NotificationBuffer();
			var processor = new SeaCargoMessageProcessor(buffer);

			var consol = GetValidConsol();
			consol.JK_MasterBillNum = "BILL123";
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			AssertNull("CusSCAOceanBill should not be created", CusSCAOceanBill.Load(consol));

			processor.Process(new SeaCargoProcessorJobForConsol(consol), false);

			var factory2 = new BusinessObjectFactory();
			var consolIn2 = factory2.Load<ForwardingConsol>(consol.PK);
			AssertNull("Consol not created", consolIn2);

			Factory.Save();
			consolIn2 = factory2.Load<ForwardingConsol>(consol.PK);
			var oceanBill = CusSCAOceanBill.Load(consolIn2);
			AssertNotNull("CusSCAOceanBill should be created", oceanBill);
			AssertEquals("HouseBills count", 1, oceanBill.HouseBills.Count);
			AssertEquals("HouseBill.Messages.Count", 1, oceanBill.HouseBills[0].Messages.Count);
			AssertContains("Messages were sent", "Sea Cargo Messages for BILL123 were sent.", buffer.AsString);
		}

		public void TestProcessAcceptableJobWithExistingOceanBill()
		{
			var buffer = new NotificationBuffer();
			var processor = new SeaCargoMessageProcessor(buffer);

			var consol = GetValidConsol();
			consol.JK_MasterBillNum = "BILL123";
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			new CMRSeaCargoSynchroniser(consol).SynchroniseOceanBill();
			Factory.Save();

			var oceanBill = CusSCAOceanBill.Load(consol);
			AssertNotNull("CusSCAOceanBill should be found", oceanBill);
			AssertEquals("HouseBills count", 1, oceanBill.HouseBills.Count);
			AssertEquals("HouseBill.Messages.Count", 0, oceanBill.HouseBills[0].Messages.Count);

			processor.Process(new SeaCargoProcessorJobForConsol(consol));

			oceanBill = CusSCAOceanBill.Load(consol);
			AssertEquals("HouseBill.Messages.Count", 1, oceanBill.HouseBills[0].Messages.Count);
			AssertContains("Message is sent", "Sea Cargo Messages for BILL123 were sent.", buffer.AsString);
			AssertNotContains("'Message not sent' is not reported", "Sea Cargo Messages for BILL123 were not sent.", buffer.AsString);

			processor.Process(new SeaCargoProcessorJobForConsol(consol));
			AssertContains("Message is not sent on a second pass", "Sea Cargo Messages for BILL123 were not sent.", buffer.AsString);
		}

		public void TestProcessAcceptableJobWithLockedMutex()
		{
			var buffer = new NotificationBuffer();
			var processor = new SeaCargoMessageProcessor(buffer);

			var consol = GetValidConsol();
			consol.JK_MasterBillNum = "BILL123";
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			Factory.Save();
			AssertNull("OceanBill should not be created", CusSCAOceanBill.Load(consol));

			var mutex = CusSCAOceanBill.CreateMutexForConsol(consol.PK, Core.Constants.CountryCodes.Australia);
			mutex.Lock();

			try
			{
				processor.Process(new SeaCargoProcessorJobForConsol(consol));

				var currentUser = StaticCurrentFetcher.Instance.CurrentUser;
				var lockedMessage = $"Could not send Sea Cargo messages for BILL123 as a lock could not be obtained on the master. Probably caused by user {currentUser.GS_FullName} sending messages for this Job at this time.";

				var notifications = buffer.AsString;
				AssertContains("Ocean Bill is Locked", lockedMessage, notifications);
				AssertContains("Message not sent", $"Sea Cargo Messages for BILL123 were not sent.", notifications);
			}
			finally
			{
				mutex.Unlock();
			}
		}

		public void TestProcessNotAcceptableJob()
		{
			var buffer = new NotificationBuffer();
			var processor = new SeaCargoMessageProcessor(buffer);
			var consol = GetInvalidConsol();
			consol.JK_MasterBillNum = "BILL123";

			AssertNotContains("Sea Cargo Messages were not sent", "Sea Cargo Messages for BILL123 were not sent.", buffer.AsString);

			processor.Process(new SeaCargoProcessorJobForConsol(consol));

			AssertContains("Sea Cargo Messages were not sent", "Sea Cargo Messages for BILL123 were not sent.", buffer.AsString);
		}

		public void TestConcurrencyErrorDoesNotCauseException()
		{
			var buffer = new NotificationBuffer();
			var processor = new SeaCargoMessageProcessor(buffer);

			var consol = GetValidConsol();
			consol.JK_MasterBillNum = "BILL123";
			var shipment = consol.Shipments.AddNew();
			shipment.FillWithValidTestData();
			var oceanBill = CusSCAOceanBill.Load(consol);
			AssertNull("CusSCAOceanBill should not be created", oceanBill);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var consolInFactory2 = factory2.Load<ForwardingConsol>(consol.PK);
			consolInFactory2.JK_AgentsReference = "AAA";
			factory2.Save();

			consol.JK_AgentsReference = "BBB";
			processor.Process(new SeaCargoProcessorJobForConsol(consol));

			oceanBill = CusSCAOceanBill.Load(consol);
			AssertEquals("HouseBill.Messages.Count", 0, oceanBill.HouseBills[0].Messages.Count);
			Assert(buffer.HasErrors);
			AssertEquals("Sea Cargo Messages for BILL123 were not sent, error during saving.", buffer.Events[2].Message);
		}

		protected override void SetUp()
		{
			base.SetUp();
			SetupPrivateKeyFile();
		}

		ForwardingConsol GetValidConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("1 Test Transport created by default", 1, consol.Transports.Count);
			var trasport = consol.Transports[0];

			trasport.JW_RL_NKLoadPort = GetPortFor(Core.Constants.CountryCodes.SouthAfrica).RL_Code;
			trasport.JW_RL_NKDiscPort = GetPortFor(Core.Constants.CountryCodes.Australia).RL_Code;

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_LloydsNumber = "1234567";

			trasport.JW_Vessel = vessel.RV_Code;
			trasport.JW_VoyageFlight = "VOY555";

			consol.SetDefaultShippingLineAddress(Factory.LoadTop1<OrgHeader>(new ZQuery()));
			consol.ShippingLine.LocalPrincipalID = "21003980130";

			return consol;
		}

		ForwardingConsol GetInvalidConsol()
		{
			var consol = GetValidConsol();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			return consol;
		}

		RefUNLOCO GetPortFor(ZString country) => Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, country));

		void SetupPrivateKeyFile()
		{
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			GlbCompany.CurrentCompany.OrgProxy.PrimaryRegistrationNumber.Number = "21003980130";
		}
	}
}
