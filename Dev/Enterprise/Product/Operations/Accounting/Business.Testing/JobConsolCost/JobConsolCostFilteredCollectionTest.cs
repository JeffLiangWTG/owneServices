using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Accounting.Business.ConsolCosting;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.Security;
	using NUnit.Framework;

	[TestedType(typeof(JobConsolCostFilteredCollection))]
	public class JobConsolCostFilteredCollectionTest : BusinessObjectCollectionViewTestCase<JobConsolCostFilteredCollection>
	{
		public void TestIsThisPartOfTheCollection()
		{
			var testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;
			Factory.Save();
			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var consol = Factory.New<ForwardingConsol>();
				var testShipment1 = TestObjectCreator.CreateShipment("S00001234");
				var testShipment2 = TestObjectCreator.CreateShipment("S00001256");
				var testJob1 = TestObjectCreator.CreateJob(testShipment1);
				var testJob2 = TestObjectCreator.CreateJob(testShipment2);
				consol.Shipments.Add(testShipment1);
				consol.Shipments.Add(testShipment2);
				var apps = new ApportionmentListing(Factory, consol);
				var cost1 = apps.CostsCollection.TryAddNew();
				cost1.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost1.E6_LocalCostAmount = 200;
				cost1.E6_OSCostAmount = 200;
				var cost2 = apps.CostsCollection.TryAddNew();
				cost2.E6_AC_ChargeCode = TestObjectCreator.CC2.PK;
				cost2.E6_LocalCostAmount = 100;
				cost2.E6_OSCostAmount = 100;
				Factory.Save();
				AssertEquals("PreCondition", 2, apps.CostsCollection.Count);
				AssertEquals("PreCondition", 2, apps.CostsCollection[0].ApportionmentCharges.Count);
				AssertEquals("PreCondition", 2, apps.CostsCollection[1].ApportionmentCharges.Count);
				var securityFactory = new BusinessObjectFactory();
				var security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				var security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);
				var loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				var loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;
				var invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.AllowViewCosts).Code;
				loginSecurity.GU_SecurityItemIsAllowed = true;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();
				apps.CostsFilteredCollection.Rebuild();
				AssertContainsExactElementsInAnyOrder("Should contain both cost", new[] { cost1, cost2 }, apps.CostsCollection);
				AssertContainsExactElementsInAnyOrder("Should contain both cost", new[] { cost1, cost2 }, apps.CostsFilteredCollection);
				loginSecurity.GU_SecurityItemIsAllowed = true;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();
				apps.CostsFilteredCollection.Rebuild();
				AssertContainsExactElementsInAnyOrder("Should contain both cost", new[] { cost1, cost2 }, apps.CostsCollection);
				AssertContainsExactElementsInAnyOrder("Should contain both cost", new[] { cost1, cost2 }, apps.CostsFilteredCollection);
				cost1.ApportionmentCharges[0].JR_GB = testBranch.PK;
				Factory.Save();
				apps.CostsFilteredCollection.Rebuild();
				AssertContainsExactElementsInAnyOrder("Should contain both cost", new[] { cost1, cost2 }, apps.CostsCollection);
				AssertContainsExactElementsInAnyOrder("Should contain only cost2", new[] { cost2 }, apps.CostsFilteredCollection);
				cost1.ApportionmentCharges[0].JR_OSCostAmt = 0;
				cost1.ApportionmentCharges[0].JR_LocalCostAmt = 0;
				cost1.ApportionmentCharges[1].JR_OSCostAmt = 0;
				cost1.ApportionmentCharges[1].JR_LocalCostAmt = 200;
				Factory.Save();
				apps.CostsFilteredCollection.Rebuild();
				AssertContainsExactElementsInAnyOrder("Should contain both cost", new[] { cost1, cost2 }, apps.CostsCollection);
				AssertContainsExactElementsInAnyOrder("Should contain both cost", new[] { cost1, cost2 }, apps.CostsFilteredCollection);
			}
		}

		protected override JobConsolCostFilteredCollection GetCollectionToTest()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var jobConsolCostCollection = new JobConsolCostCollection(Factory, consol);
			return new JobConsolCostFilteredCollection(jobConsolCostCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobConsolCost>();
		}

		protected TestObjectCreator TestObjectCreator
		{
			get
			{
				return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
			}
		}

		TestObjectCreator testObjectCreator;
	}
}
