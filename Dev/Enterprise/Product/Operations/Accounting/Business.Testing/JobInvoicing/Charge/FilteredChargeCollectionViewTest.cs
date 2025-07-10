using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(FilteredChargeCollectionView))]
	public class FilteredChargeCollectionViewTest : BusinessObjectCollectionViewTestCase<FilteredChargeCollectionView>
	{
		#region TestIsThisPartOfTheCollection

		public void TestJobChargeNotOverLoaded_ChargesLoadSuspender()
		{
			var jobPKs = new List<ZGuid>();
			for (var i = 0; i < 10; i++)
			{
				var shipement = TestObjectCreator.CreateShipment(string.Format("S0000123{0}", i));
				var job = TestObjectCreator.CreateJob(shipement);
				TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
				jobPKs.Add(job.PK);
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var mainJob = newFactory.NewJobWithValidTestDataForTesting<Job>();
			mainJob.Charges.IncludeChargesFromJobs(jobPKs.ToArray());
			mainJob.Charges.Load();
			AssertEquals("Charges from 10 saved Jobs", 10, mainJob.Charges.Count);

			newFactory.ResetDatabaseLoadCount();
			_ = new FilteredChargeCollectionView(mainJob.Charges);
			AssertEquals("Only one DynamicBusinessObjectCollection Load to get Orgs from Charges in Db because individual job Charges collections are not loaded", 1, newFactory.DatabaseLoadCount);
		}

		public void TestIsThisPartOfTheCollection()
		{
			GlbStaff testUser = Factory.NewWithValidTestData<GlbStaff>();
			testUser.GS_IsController = false;

			GlbCompany testCompany = Factory.NewWithValidTestData<GlbCompany>();
			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			using (Env.SetTemporaryUserContext(testUser.GS_LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				var testShipment1 = TestObjectCreator.CreateShipment("S00001234");
				var testJob1 = TestObjectCreator.CreateJob(testShipment1);
				var testCharge1 = TestObjectCreator.CreateCharge(testJob1, TestObjectCreator.CC1, 100m, 100m);
				var testCharge2 = TestObjectCreator.CreateCharge(testJob1, TestObjectCreator.CC2, 100m, 100m);

				Factory.Save();

				BusinessObjectFactory securityFactory = new BusinessObjectFactory();

				SecurityCore security = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK);
				SecurityCore security2 = new UserLoginController().GetSecurityForUser(Env.CurrentUser.LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK);

				GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
				loginSecurity.GU_GB = Env.CurrentBranch.PK;
				loginSecurity.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity.GU_GS = Env.CurrentUser.PK;
				loginSecurity.GU_SecurityRight = security.Login.Code;
				GlbSecurity loginSecurity2 = securityFactory.New<GlbSecurity>();
				loginSecurity2.GU_GB = testBranch.PK;
				loginSecurity2.GU_GE = Env.CurrentDepartment.PK;
				loginSecurity2.GU_GS = Env.CurrentUser.PK;
				loginSecurity2.GU_SecurityRight = security2.Login.Code;

				GlbSecurity invoicingSecurity = securityFactory.New<GlbSecurity>();
				invoicingSecurity.GU_GB = Env.CurrentBranch.PK;
				invoicingSecurity.GU_GE = Env.CurrentDepartment.PK;
				invoicingSecurity.GU_GS = Env.CurrentUser.PK;
				invoicingSecurity.GU_SecurityRight = security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCharges).Code;

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCharges).IsAllowed = true;
				security2.Login.IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = true;
				loginSecurity2.GU_SecurityItemIsAllowed = false;

				securityFactory.Save();

				testJob1.ChargesFilteredByChargeViewingPermission.Rebuild();
				AssertEquals("should show all charges", 2, testJob1.Charges.Count);
				AssertEquals("should show all charges", 2, testJob1.ChargesFilteredByChargeViewingPermission.Count);

				loginSecurity.GU_SecurityItemIsAllowed = true;
				security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainShipmentJobInvoicing, SecurityCore.AllowViewCharges).IsAllowed = false;
				invoicingSecurity.GU_SecurityItemIsAllowed = false;
				securityFactory.Save();

				testJob1.ChargesFilteredByChargeViewingPermission.Rebuild();
				AssertEquals("should show all charges", 2, testJob1.Charges.Count);
				AssertEquals("should show all charges", 2, testJob1.ChargesFilteredByChargeViewingPermission.Count);

				testCharge1.JR_GB = testBranch.PK;
				Factory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
				Factory.Save();

				testJob1.ChargesFilteredByChargeViewingPermission.Rebuild();
				AssertEquals("should show all charges", 2, testJob1.Charges.Count);
				AssertEquals("should show only testCharge2", 1, testJob1.ChargesFilteredByChargeViewingPermission.Count);
			}
		}

		public void TestCollectionViewSecurityWithBatchProcessor()
		{
			TestObjectCreator.CreateTestPeriods(ZDateTime.Now.AddDays(-1));

			GlbStaff serviceUser = Factory.LoadTop1<GlbStaff>(new ZQuery(GlbStaffSchema.GS_LoginName, User.ServiceUserName));

			var testBranch = TestObjectCreator.CreateBranch("TBR", "Test Branch", GlbCompany.CurrentCompany);
			Factory.Save();

			var testShipment = TestObjectCreator.CreateShipment("S00001234");
			var testJob = TestObjectCreator.CreateJob(testShipment);
			var testCharge1 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC1, 100m, 100m);
			var testCharge2 = TestObjectCreator.CreateCharge(testJob, TestObjectCreator.CC2, 100m, 100m);
			testCharge2.JR_GB = testBranch.PK;

			Factory.Save();

			BusinessObjectFactory securityFactory = new BusinessObjectFactory();
			GlbGroup glbGroup = securityFactory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));

			GlbGroupLink groupLink = securityFactory.New<GlbGroupLink>();
			groupLink.GK_GG = glbGroup.PK;
			groupLink.GK_GS = serviceUser.PK;
			groupLink.GK_MembershipType = "";

			GlbSecurity loginSecurity = securityFactory.New<GlbSecurity>();
			loginSecurity.GU_GB = ZGuid.Empty;
			loginSecurity.GU_GE = ZGuid.Empty;
			loginSecurity.GU_GS = ZGuid.Empty;
			loginSecurity.GU_SecurityRight = "Login";
			loginSecurity.GU_GG = glbGroup.PK;
			loginSecurity.GU_SecurityItemIsAllowed = false;

			GlbSecurity viewSecurity = securityFactory.New<GlbSecurity>();
			viewSecurity.GU_GB = ZGuid.Empty;
			viewSecurity.GU_GE = ZGuid.Empty;
			viewSecurity.GU_GS = ZGuid.Empty;
			viewSecurity.GU_SecurityRight = "MaintainShipmentJobInvoicingInvViewCharges";
			viewSecurity.GU_GG = glbGroup.PK;
			viewSecurity.GU_SecurityItemIsAllowed = false;

			securityFactory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			newFactory.ClearCachedValue<bool>(("Login BRN:" + testBranch.GB_Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
			newFactory.ClearCachedValue<bool>(("Login BRN:" + Env.CurrentBranch.Code + " DEP:" + GlbDepartment.CurrentDepartment.GE_Code));
			newFactory.Save();
			using (Env.SetTemporaryUserContext(serviceUser.GS_LoginName, testBranch.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				var job = new Job.Loader(newFactory, testShipment).Load(true, false);

				job.ChargesFilteredByChargeViewingPermission.Rebuild();
				AssertEquals("should show all charges", 2, job.ChargesFilteredByChargeViewingPermission.Count);
			}
		}

		#endregion

		#region Implementation

		protected override FilteredChargeCollectionView GetCollectionToTest()
		{
			var job = Factory.NewJobWithValidTestDataForTesting<Job>();
			var chargeCollection = new ChargeCollection(job);
			return new FilteredChargeCollectionView(chargeCollection);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<Charge>();
		}

		protected TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
