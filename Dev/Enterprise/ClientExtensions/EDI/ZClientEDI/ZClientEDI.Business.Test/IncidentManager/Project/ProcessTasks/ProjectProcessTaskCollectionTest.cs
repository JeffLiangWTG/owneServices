using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(ProjectProcessTaskCollection))]
	class ProjectProcessTaskCollectionTest : ProcessTaskCollectionTest<ProjectProcessTaskCollection>
	{
		IDisposable disposable;

		protected override void SetUp()
		{
			base.SetUp();
			disposable = ProcessTaskCollection.CanCreateTaskCollection();
		}

		protected override void TearDown()
		{
			base.TearDown();
			disposable.Dispose();
		}

		public void TestDefaults()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			client.Contacts.AddNew();

			Project.WKP_OA_ClientAddress = client.MainAddress.PK;
			Project.WKP_OC_Contact = client.Contacts[0].PK;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Project.WKP_GS_NKProjectManager = staff.GS_Code;

			var task = Collection.AddNew();
			AssertEquals(client.PK, task.OrganisationPK);
			AssertEquals(client.MainAddress.PK, task.P9_OA);
			AssertEquals(client.Contacts[0].PK, task.P9_OC);
			AssertEquals(staff.GS_Code, task.P9_GS_NKAssignedStaffMember);

			var taskWithCapability = Factory.NewWithValidTestData<ProjectProcessTask>();
			GlbCapability capability = Factory.New<GlbCapability>();
			capability.G4_Code = "COD";
			taskWithCapability.P9_G4_RequiredCapability = capability.PK;
			Collection.SetDefaultsForNewChildForTest(taskWithCapability, false);
			Collection.Add(taskWithCapability);
			AssertNotEquals(staff.GS_Code, taskWithCapability.P9_GS_NKAssignedStaffMember);
		}

		public void TestOriginCountry()
		{
			AssertEquals("Country should be empty", ZString.Empty, Collection.OriginCountry);
			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_RL_NKClosestPort = "AUSYD";

			Project.WKP_OA_ClientAddress = client.MainAddress.PK;
			AssertEquals("Country should be AU", "AU", Collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			AssertEquals("Country should be empty", ZString.Empty, Collection.DestinationCountry);

			OrgHeader client = Factory.NewWithValidTestData<OrgHeader>();
			client.OH_RL_NKClosestPort = "IDJKT";

			Project.WKP_OA_ClientAddress = client.MainAddress.PK;
			AssertEquals("Country should be ID", "ID", Collection.DestinationCountry);
		}

		protected new ProjectProcessTaskCollection Collection
		{
			get { return base.Collection; }
		}

		protected override ProjectProcessTaskCollection GetCollectionToTestCore()
		{
			return new ProjectProcessTaskCollection(Project);
		}

		EDIProject Project
		{
			get { return project ?? (project = Factory.New<EDIProject>()); }
		}

		EDIProject project;
	}
}
