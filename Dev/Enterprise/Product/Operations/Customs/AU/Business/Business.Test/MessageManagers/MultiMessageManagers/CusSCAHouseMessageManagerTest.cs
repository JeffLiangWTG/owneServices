using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusSCAHouseMessageManagerTest : SeaCargoTestCase
	{
		public void TestAllMessageManagers()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			var pivot = house.Pivot.AddNew();
			var container = oceanBill.Containers.AddNew();
			pivot.CV_CN = container.PK;
			((ICusUnderbondDependentCollectionParent)container).Underbonds.AddNew();
			var allMessageManagers = new CusSCAHouseMessageManagerForTest(house).GetAllMessageManagers();
			AssertEquals("AllMessageManagers.Length", 2, allMessageManagers.Length);
			AssertEquals("AllMessageManagers[0]", typeof(CusSCAHouseSEACRManager), allMessageManagers[0].GetType());
			AssertEquals("AllMessageManagers[1]", typeof(CusUnderbondUBMREQManager), allMessageManagers[1].GetType());
		}

		public void TestAllMessageManagersForNullOceanBill()
		{
			AssertEquals("Length", 0, new CusSCAHouseMessageManagerForTest(null, null).GetAllMessageManagers().Length);
		}

		public void TestSettingHouseBillSetsOceanBill()
		{
			var oceanBill = Factory.New<CusSCAOceanBill>();
			var house = oceanBill.HouseBills.AddNew();
			var manager = new CusSCAHouseMessageManager(null, null);
			manager.HouseBill = house;
			AssertEquals("OceanBill", oceanBill, manager.OceanBill);
		}

		public void TestMessagingApplicationName()
		{
			var manager = new CusSCAHouseMessageManager(null, null);
			AssertEquals("MessagingApplicationName", "Sea Cargo", manager.MessagingApplicationName);
		}

		public void TestCanSend()
		{
			GlbBranch.CurrentBranch.OrgProxy.PrimaryRegistrationNumber.Number = "11001872966";
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();
			var manager = new CusSCAHouseMessageManagerForTest(null, null);
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

			AssertEquals("Can Send on original should fail . only thing invalid should be oceanbill shipping line", false, manager.CanSendOriginal(new SendsMessagesToCustomsShutterUpperer(false), manager.AllMessageManagers));
			AssertEquals("Can Send on amend should fail . only thing invalid should be oceanbill shipping line", false, manager.CanAmend(new SendsMessagesToCustomsShutterUpperer(false), manager.AllMessageManagers));
			AssertEquals("Can Send on withdrawl should fail . only thing invalid should be oceanbill shipping line", false, manager.CanWithdraw(new SendsMessagesToCustomsShutterUpperer(false), manager.AllMessageManagers));

			oceanBill.CB_OH_ShippingLine = CreateShippingLine("A shipping").PK;

			AssertEquals("Can Send on original should work", true, manager.CanSendOriginal(new SendsMessagesToCustomsShutterUpperer(), manager.AllMessageManagers));
			AssertEquals("Can Send on amend should work", true, manager.CanAmend(new SendsMessagesToCustomsShutterUpperer(), manager.AllMessageManagers));
			AssertEquals("Can Send on withdrawl should work", true, manager.CanWithdraw(new SendsMessagesToCustomsShutterUpperer(), manager.AllMessageManagers));
		}

		public void TestOceanBillDetailsValid()
		{
			var manager = new CusSCAHouseMessageManager(null, null);
			var oceanBill = CreateOceanBill();
			oceanBill.CB_ResponsiblePartyID = "82091234089";
			manager.OceanBill = oceanBill;
			AssertEquals("No Message errors on Ocean Bill", true, manager.OceanBillDetailsValid(new SendsMessagesToCustomsShutterUpperer()));

			var house = oceanBill.HouseBills.AddNew();
			house.Validation.ValidateCA_OH_Consignee();
			AssertEquals("No Message errors on Ocean Bill but Housebills have errors", true, manager.OceanBillDetailsValid(new SendsMessagesToCustomsShutterUpperer()));

			oceanBill.Validation.ValidateAll();
			oceanBill.CB_OceanBill = string.Empty;
			AssertEquals("Message errors exist on Ocean Bill", false, manager.OceanBillDetailsValid(new SendsMessagesToCustomsShutterUpperer(false)));
		}

		protected override void SetUp()
		{
			Env.Registry.AUCustomsImportsMessagingMode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			base.SetUp();
		}

		internal class CusSCAHouseMessageManagerForTest : CusSCAHouseMessageManager
		{
			public CusSCAHouseMessageManagerForTest(CusSCAHouse house) : base(house)
			{
			}

			public CusSCAHouseMessageManagerForTest(CusSCAHouse house, ZString ownerCompanyABN) : base(house, ownerCompanyABN)
			{
			}

			internal new SingleMessageManager[] GetAllMessageManagers() => base.GetAllMessageManagers();

			internal new bool CanSendOriginal(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend) => base.CanSendOriginal(sender, managersToSend);

			internal new SingleMessageManager[] AllMessageManagers => base.AllMessageManagers;

			internal new bool CanAmend(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managers) => base.CanAmend(sender, managers);

			internal new bool CanWithdraw(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managers) => base.CanWithdraw(sender, managers);
		}
	}
}
