using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;

namespace Enterprise.Accounting.Business.ConsolCosting.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Accounting.Business.ConsolCosting;
	using Enterprise.Environment;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.MasterFiles.Business.Testing;
	using Enterprise.Security;
	using NUnit.Framework;

	[TestedType(typeof(ApportionmentSplitChargeFilteredCollection))]
	public class ApportionmentSplitChargeFilteredCollectionTest : BusinessObjectCollectionViewTestCase<ApportionmentSplitChargeFilteredCollection>
	{
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
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
				var cost = apps.CostsCollection.TryAddNew();
				cost.E6_AC_ChargeCode = TestObjectCreator.CC1.PK;
				cost.E6_LocalCostAmount = 200;
				cost.E6_OSCostAmount = 200;
				AssertEquals("PreCondition", 1, apps.CostsCollection.Count);
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
				apps.CostsCollection[0].FilteredApportionmentCharges.Rebuild();
				AssertEquals("Should contain 2 charge", 2, apps.CostsCollection[0].ApportionmentCharges.Count);
				AssertEquals("Should contain 2 charge", 2, apps.CostsCollection[0].FilteredApportionmentCharges.Count);
				loginSecurity.GU_SecurityItemIsAllowed = true;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();
				apps.CostsCollection[0].FilteredApportionmentCharges.Rebuild();
				AssertEquals("Should contain 2 charge", 2, apps.CostsCollection[0].ApportionmentCharges.Count);
				AssertEquals("Should contain 2 charge", 2, apps.CostsCollection[0].FilteredApportionmentCharges.Count);
				cost.ApportionmentCharges[0].JR_GB = testBranch.PK;
				Factory.Save();
				apps.CostsCollection[0].FilteredApportionmentCharges.Rebuild();
				AssertEquals("Should contain 2 charge", 2, apps.CostsCollection[0].ApportionmentCharges.Count);
				AssertEquals("Should contain only one charge", 1, apps.CostsCollection[0].FilteredApportionmentCharges.Count);
			}
		}

		protected override ApportionmentSplitChargeFilteredCollection GetCollectionToTest()
		{
			var cost = Factory.New<JobConsolCost>();
			var collectionWithACost = new ApportionmentSplitChargeCollection(cost);
			return new ApportionmentSplitChargeFilteredCollection(collectionWithACost);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ApportionSplitCharge>();
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
