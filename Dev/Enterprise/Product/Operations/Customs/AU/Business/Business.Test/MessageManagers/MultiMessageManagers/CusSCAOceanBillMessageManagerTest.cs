using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAOceanBillMessageManagerTest : SeaCargoTestCase
	{
		public void TestAllMessageManagers()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			oceanBill.HouseBills.AddNew();
			oceanBill.HouseBills.AddNew();
			var container = oceanBill.Containers.AddNew();
			((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();
			var allMessageManagers = new CusSCAOceanBillMessageManagerForTest(oceanBill).GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 3, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(CusSCAHouseSEACRManager), allMessageManagers[0].GetType());
			AssertEquals("AllMessageManagers[1]", typeof(CusSCAHouseSEACRManager), allMessageManagers[1].GetType());
			AssertEquals("AllMessageManagers[2]", typeof(CusUnderbondUBMREQManager), allMessageManagers[2].GetType());
		}

		public void TestAllMessageManagersForNullOceanBill()
		{
			AssertEquals("Length", 0, new CusSCAOceanBillMessageManagerForTest(null, null).GetAllMessageManagers().Length);
		}

		public void TestMessagesArePreloadedBeforeSending()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();

			var messagePKs = new List<ZGuid>();
			for (var i = 0; i < 101; i++)
			{
				var house = oceanBill.HouseBills.AddNew();
				var message = house.Messages.AddNew(typeof(CMRSEACRMessage));
				messagePKs.Add(message.PK);
			}

			Factory.Save();

			AssertDBHits(oceanBill, messagePKs);

			var factoryNew = new BusinessObjectFactory();
			var oceanBillReloaded = factoryNew.Load<CusSCAOceanBill>(oceanBill.PK);
			AssertDBHits(oceanBillReloaded, messagePKs);
		}

		public void TestMessagingApplicationName()
		{
			AssertEquals("MessagingApplicationName", "Sea Cargo", new CusSCAOceanBillMessageManager(null, null).MessagingApplicationName);
		}

		public void TestCanSend()
		{
			GlbBranch.CurrentBranch.OrgProxy.PrimaryRegistrationNumber.Number = "11001872966";
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			var manager = new CusSCAOceanBillMessageManagerForTest(null, null);
			var oceanBill = CreateOceanBill();
			manager.OceanBill = oceanBill;
			oceanBill.CB_OH_ShippingLine = ZGuid.Empty;

			var container = oceanBill.Containers.AddNew();
			var house = oceanBill.HouseBills.AddNew();
			var pivot = container.Pivots.AddNew();
			house.Pivot.Add(pivot);
			EnsureValidContainer(container, ContainerNumber1);
			EnsureValidPivot(pivot, "Pivot line 1");
			EnsureValidHouse(house, "HouseBill32");
			oceanBill.CB_PrincipalID = "";

			AssertEquals("Can Send on original should fail . only thing invalid should be oceanbill shipping line", false, manager.CanSendOriginalExposed(new SendsMessagesToCustomsShutterUpperer(false), manager.AllMessageManagers));
			AssertEquals("Can Send on amend should fail . only thing invalid should be oceanbill shipping line", false, manager.CanAmend(new SendsMessagesToCustomsShutterUpperer(false), manager.AllMessageManagers));
			AssertEquals("Can Send on withdrawl should fail . only thing invalid should be oceanbill shipping line", false, manager.CanWithdraw(new SendsMessagesToCustomsShutterUpperer(false), manager.AllMessageManagers));

			oceanBill.CB_OH_ShippingLine = CreateShippingLine("A shipping").PK;

			AssertEquals("Can Send on original should work", true, manager.CanSendOriginalExposed(new SendsMessagesToCustomsShutterUpperer(), manager.AllMessageManagers));
			AssertEquals("Can Send on amend should work", true, manager.CanAmend(new SendsMessagesToCustomsShutterUpperer(), manager.AllMessageManagers));
			AssertEquals("Can Send on withdrawl should work", true, manager.CanWithdraw(new SendsMessagesToCustomsShutterUpperer(), manager.AllMessageManagers));
		}

		public void TestOceanBillDetailsValid()
		{
			var manager = new CusSCAOceanBillMessageManager(null, null);
			var oceanBill = CreateOceanBill();
			oceanBill.CB_ResponsiblePartyID = "82091234089";
			manager.OceanBill = oceanBill;
			AssertEquals("No Message errors on Ocean Bill", true, manager.OceanBillDetailsValid(new SendsMessagesToCustomsShutterUpperer()));

			var house = oceanBill.HouseBills.AddNew();
			house.Validation.ValidateCA_OH_Consignee();
			AssertEquals("No Message errors on Ocean Bill but Housebills have errors", true, manager.OceanBillDetailsValid(new SendsMessagesToCustomsShutterUpperer()));

			oceanBill.Validation.ValidateAll();
			oceanBill.CB_OceanBill = ZString.Empty;
			AssertEquals("Message errors exist on Ocean Bill", false, manager.OceanBillDetailsValid(new SendsMessagesToCustomsShutterUpperer(false)));
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		void AssertDBHits(CusSCAOceanBill oceanBill, IEnumerable<ZGuid> messagePKs)
		{
			var factory = oceanBill.Factory;

			var manager = new CusSCAOceanBillMessageManagerForTest(oceanBill);
			var sender = new Customs.Business.SendsMessagesToCustomsShutterUpperer(false);
			sender.ReturnAllForWhichMessagesShouldWeSend = true;

			var tableHitsbeforeSend = factory.GetTableHitCount(EDIMessage.Schema.TableName);
			manager.SendOriginalMessages(sender);
			var tableHitsafterSend = factory.GetTableHitCount(EDIMessage.Schema.TableName);
			AssertEquals("edimessages retrieved from DB in batches of 100", 2, tableHitsafterSend - tableHitsbeforeSend);

			var ediMessages = factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.PK, messagePKs));
			var tableHitsafterReload = factory.GetTableHitCount(EDIMessage.Schema.TableName);
			AssertEquals("edimessages have been cached", 0, tableHitsafterReload - tableHitsafterSend);
			AssertEquals(101, ediMessages.Length);
		}

		sealed class CusSCAOceanBillMessageManagerForTest : CusSCAOceanBillMessageManager
		{
			public CusSCAOceanBillMessageManagerForTest(CusSCAOceanBill oceanBill) : base(oceanBill)
			{
			}

			public CusSCAOceanBillMessageManagerForTest(CusSCAOceanBill oceanBill, ZString ownerCompanyABN) : base(oceanBill, ownerCompanyABN)
			{
			}

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();

			internal bool CanSendOriginalExposed(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend) => base.CanSendOriginal(sender, managersToSend);

			internal new bool CanAmend(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managers) => base.CanAmend(sender, managers);

			internal new bool CanWithdraw(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managers) => base.CanWithdraw(sender, managers);

			internal new SingleMessageManager[] AllMessageManagers => base.AllMessageManagers;

			protected override int PreloadBatchSize => 100;

			protected override bool CanSendOriginal(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend) => true;
		}
	}
}
