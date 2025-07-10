using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(SupportIncidentProcessTaskCollection))]
	public class SupportIncidentProcessTaskCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaults()
		{
			SupportIncident featureRequest = Factory.NewWithValidTestData<SupportIncident>();
			featureRequest.SetupForProjectFeatureRequest();
			EDIProject project = Factory.NewWithValidTestData<EDIProject>();
			featureRequest.RelatedProjectPK = project.PK;
			SupportIncidentProcessTaskCollection collection = featureRequest.WorkflowItems;

			OrgHeader client = Factory.New<OrgHeader>();
			client.Contacts.AddNew();

			featureRequest.IM_OH_Client = client.PK;
			featureRequest.IM_OA_BranchAddress = client.MainAddress.PK;
			featureRequest.IM_OC_Contact = client.Contacts[0].PK;

			GlbStaff staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "AAA";
			project.WKP_GS_NKProjectManager = staff1.GS_Code;

			GlbStaff staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "BBB";
			featureRequest.IM_GS_NKSpecifiedBy = staff2.GS_Code;

			SupportIncidentProcessTask task1 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			task1.P9_Type = "PJM";
			SupportIncidentProcessTask task2 = Factory.NewWithValidTestData<SupportIncidentProcessTask>();
			task2.P9_Type = "BSC";

			collection.SetDefaultsForNewTask(task1, false);
			collection.SetDefaultsForNewTask(task2, false);

			AssertEquals(staff1.GS_Code, task1.P9_GS_NKAssignedStaffMember);
			AssertEquals(staff2.GS_Code, task2.P9_GS_NKAssignedStaffMember);

			AssertEquals(client.PK, task1.OrganisationPK);
			AssertEquals(client.MainAddress.PK, task1.P9_OA);
			AssertEquals(client.Contacts[0].PK, task1.P9_OC);
		}

		public void TestIndexer()
		{
			AssertEquals(typeof(SupportIncidentProcessTask), Collection.AddNew().GetType());
			AssertEquals(typeof(SupportIncidentProcessTask), Collection[0].GetType());
		}

		protected new SupportIncidentProcessTaskCollection Collection
		{
			get { return (SupportIncidentProcessTaskCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<SupportIncident>().WorkflowItems;
		}
	}
}
