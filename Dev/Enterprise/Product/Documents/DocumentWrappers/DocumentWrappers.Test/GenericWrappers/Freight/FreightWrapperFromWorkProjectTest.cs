using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWorkProject))]
	sealed class FreightWrapperFromWorkProjectTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var project = Factory.New<Project>();
			var wrapper = new FreightWrapperFromWorkProject(project, Factory);
			AssertEquals("TrackingBusinessObjectPK", project.PK, wrapper.TrackingBusinessObjectPK);
		}

		public void TestJobNumberShouldMatchProjectNumber()
		{
			var project = Factory.NewWithValidTestData<Project>();
			project.WKP_ProjectNumber = "PRJ00023";
			var wrapper = new FreightWrapperFromWorkProject(project, Factory);
			AssertEquals($"Expected wrappers job number to be PRJ00023 but was {wrapper.JobNumber}. SAD!", "PRJ00023", wrapper.JobNumber);
		}

		#region Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromWorkProject(project, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<Project>();
		}

		protected override bool IsCarrierUsed => false;

		public override void TestWrapperNotes()
		{
			AssertEquals("Notes", 1, Wrapper.Notes.Count);
		}

		public override void TestWrapperNotesIncludingRelated()
		{
			AssertEquals("Notes", 1, Wrapper.NotesIncludingRelated.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			project = GetNewBusinessObjectToWrap() as Project;
		}

		Project project;

		#endregion Implementation
	}
}
