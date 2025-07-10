using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusSCADepotHouse))]
	sealed class CusSCADepotHouseTest : EnterpriseBusinessObjectTestCase
	{
		#region Static Loaders

		public void TestLoad()
		{
			#region Create Test Objects

			var container1 = Factory.New<CusSCADepotContainer>();
			container1.CJ_Voyage = "123";
			container1.CJ_LloydsNumber = "54321";

			var container2 = Factory.New<CusSCADepotContainer>();
			container2.CJ_Voyage = "321";
			container2.CJ_LloydsNumber = "54321";

			var container3 = Factory.New<CusSCADepotContainer>();
			container3.CJ_Voyage = "123";
			container3.CJ_LloydsNumber = "12345";

			var house11 = container1.HouseBills.AddNew();
			house11.CX_HouseBill = "HOUSE1";
			house11.CX_Status = "FOO";

			var house12 = container1.HouseBills.AddNew();
			house12.CX_HouseBill = "OCEAN1";
			house12.CX_Status = "FOO";

			var house21 = container2.HouseBills.AddNew();
			house21.CX_HouseBill = "HOUSE1";
			house21.CX_Status = "FOO";

			var house22 = container2.HouseBills.AddNew();
			house22.CX_HouseBill = "HOUSE1";
			house22.CX_Status = "BAR";

			var house31 = container3.HouseBills.AddNew();
			house31.CX_HouseBill = "House2";
			house31.CX_Status = "BAR";

			var house32 = container3.HouseBills.AddNew();
			house32.CX_HouseBill = "House3";
			house32.CX_Status = "FOO";

			#endregion

			AssertEquals(house11, CusSCADepotHouse.Load(Factory, "HOUSE1", ZString.Empty, "FOO", "54321", "123"));
			AssertEquals(house12, CusSCADepotHouse.Load(Factory, ZString.Empty, "OCEAN1", "FOO", "54321", "123"));
			AssertEquals(house21, CusSCADepotHouse.Load(Factory, "HOUSE1", ZString.Empty, "FOO", "54321", "321"));
			AssertEquals(house22, CusSCADepotHouse.Load(Factory, "HOUSE1", ZString.Empty, "BAR", "54321", "321"));
			AssertEquals(house31, CusSCADepotHouse.Load(Factory, "HOUSE2", ZString.Empty, "BAR", "12345", "123"));
			AssertEquals(house32, CusSCADepotHouse.Load(Factory, "HOUSE3", ZString.Empty, "FOO", "12345", "123"));
			AssertNull(CusSCADepotHouse.Load(Factory, "HOUSE4", ZString.Empty, "FOO", "54321", "123"));
			AssertNull(CusSCADepotHouse.Load(Factory, "HOUSE1", ZString.Empty, "FAP", "54321", "123"));
			AssertNull(CusSCADepotHouse.Load(Factory, "HOUSE1", ZString.Empty, "FOO", "99999", "123"));
			AssertNull(CusSCADepotHouse.Load(Factory, "HOUSE1", ZString.Empty, "FOO", "54321", "999"));
			AssertNull(CusSCADepotHouse.Load(Factory, ZString.Empty, "OCEAN2", "FOO", "54321", "123"));
		}

		#endregion

		#region Related Business Objects

		public void TestContainer()
		{
			var container = Factory.New<CusSCADepotContainer>();
			var house = container.HouseBills.AddNew();
			AssertEquals("Container is present", container, house.Container);
			house.CX_CJ = ZGuid.Empty;
			AssertNull("Container is null", house.Container);
		}

		#endregion

		#region Business Object Overrides

		[ExpectNoExceptions()]
		public void TestSupportsClone()
		{
			var house = Factory.New<CusSCADepotHouse>();
			house.Clone();
		}

		#endregion

		public void TestOutturnableLines()
		{
			var house = Factory.New<CusSCADepotHouse>();
			AssertEquals(0, house.OutturnableLines.Length);

			var underbond = house.Underbonds.AddNew();
			AssertEquals(0, house.OutturnableLines.Length);

			var outturn = underbond.Outturns.AddNew();
			AssertEquals(0, house.OutturnableLines.Length);

			outturn.Parent = house;
			AssertEquals(1, house.OutturnableLines.Length);
		}

		public void TestUsesTranshipmentPortOnUnderbond()
		{
			var house = Factory.New<CusSCADepotHouse>();
			AssertEquals(false, ((ICusUnderbondDependentCollectionParent)house).UsesTranshipmentPortOnUnderbond);
			AssertEquals(ZString.Empty, ((ICusUnderbondDependentCollectionParent)house).DefaultTranshipmentPort);
		}

		public void TestEquals()
		{
			var house1 = Factory.New<CusSCADepotHouse>();
			var house2 = Factory.New<CusSCADepotHouse>();
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));

			house1.CX_ClientID = TestClientID1;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_ClientID = TestClientID1;
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));
			house2.CX_ClientID = TestClientID2;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_ClientID = TestClientID1;

			house1.CX_Damaged = 2;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_Damaged = 2;
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));
			house2.CX_Damaged = 0;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_Damaged = 2;

			house1.CX_HouseBill = TestHouseBill1;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_HouseBill = TestHouseBill1;
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));
			house2.CX_HouseBill = TestHouseBill2;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_HouseBill = TestHouseBill1;

			house1.CX_PackageCount = 30;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_PackageCount = 30;
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));
			house2.CX_PackageCount = 31;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_PackageCount = 30;

			house1.CX_Pillaged = 1;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_Pillaged = 1;
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));
			house2.CX_Pillaged = 5;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_Pillaged = 1;

			house1.CX_Short = 4;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_Short = 4;
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));
			house2.CX_Short = 1;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_Short = 4;

			house1.CX_Surplus = 2;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_Surplus = 2;
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));
			house2.CX_Surplus = 4;
			AssertEquals("Houses not Equal", false, house1.MessageDataEqual(house2));
			house2.CX_Surplus = 2;
		}

		public void TestCopyPersistentValuesFrom()
		{
			CusSCADepotHouse house1 = Factory.New<CusSCADepotHouse>();
			CusSCADepotHouse house2 = Factory.New<CusSCADepotHouse>();
			house2.CopyPersistentValuesFrom(house1);
			AssertEquals("Houses Equal", true, house1.MessageDataEqual(house2));
		}

		public void TestCanSendWithoutDelay()
		{
			CusSCADepotHouse house = Factory.New<CusSCADepotHouse>();
			Assert(((ICusUnderbondDependentCollectionParent)house).CanSendWithoutDelay);
		}

		public void TestIsAutoLogged()
		{
			CusSCADepotHouse house1 = Factory.New<CusSCADepotHouse>();
			Factory.Save();
			StmALog addedEvent = house1.Logs.MostRecentLogByEventTime(Events.AddedARecordToTheSystem);
			AssertNotNull("Failed to create Added event", addedEvent);
		}

		[ExpectNoExceptions()]
		public void TestFromShipment()
		{
			CFSShipment shipment = Factory.NewWithValidTestData<CFSShipment>();
			shipment.JS_OH_HandledOnBehalfOfForwarder = CreateForwarderWithoutCMP();
			CusSCADepotHouse house = Factory.New<CusSCADepotHouse>();
			house.FromShipment(shipment);
		}

		public void TestIOutturnableLinePackagesManifested()
		{
			CusSCADepotHouse house = Factory.New<CusSCADepotHouse>();
			house.CX_PackageCount = 20;
			AssertEquals(20, ((IOutturnableLine)house).PackagesManifested);
		}

		#region Implementation

		const string TestClientID1 = "C239910192";
		const string TestClientID2 = "C101920910";
		const string TestHouseBill1 = "3839290298";
		const string TestHouseBill2 = "742939";

		ZGuid CreateForwarderWithoutCMP()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_FullName = "Test Forwarder Without Customs Code";
			header.MainAddress.OA_Address1 = "Test Address 1";
			header.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			return header.PK;
		}

		#endregion
	}
}
