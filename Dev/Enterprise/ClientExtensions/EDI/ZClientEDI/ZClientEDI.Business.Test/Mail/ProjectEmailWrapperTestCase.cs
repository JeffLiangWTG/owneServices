using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Mail.Business.Test
{
	[TestedType(typeof(ProjectEmailWrapper))]
	sealed class ProjectEmailWrapperTestCase : CustomerServiceEmailBaseWrapperTestCase<ProjectEmailWrapper>
	{
		protected override ProjectEmailWrapper GetDocumentWrapper()
		{
			var project = Factory.NewWithValidTestData<EDIProject>();
			return ProjectEmailWrapper.New(project, Factory);
		}
	}

	[TestedType(typeof(ProjectEmailWrapper))]
	class ProjectEmailWrapperTest : DocumentWrapperTestCase
	{
		public void TestCurrentStaffNameAndTitle()
		{
			var anotherStaff = Factory.NewWithValidTestData<GlbStaff>();
			anotherStaff.GS_FullName = "Jenny Nguyen";
			Factory.Save();

			using (Env.SetTemporaryUserContext(anotherStaff.PK.ToGuid(), GlbBranch.CurrentBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				AssertEquals("CurrentStaffNameAndTitle", "Jenny Nguyen", Wrapper.CurrentStaffNameAndTitle);
				anotherStaff.GS_Title = "Associate Developer";
				Factory.Save();
				AssertEquals("CurrentStaffNameAndTitle", "Jenny Nguyen<br />Associate Developer", Wrapper.CurrentStaffNameAndTitle);
			}
		}

		public void TestDefaultEmailAddress()
		{
			AssertEquals("DefaultEmailAddress", Project.OverridingDefaultFromEmailAddress, Wrapper.DefaultEmailAddress);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var project = Factory.NewWithValidTestData<EDIProject>();
			return new DocumentWrapper[] { ProjectEmailWrapper.New(project, Factory) };
		}

		ProjectEmailWrapper Wrapper
		{
			get { return (ProjectEmailWrapper)base.Wrappers[0]; }
		}

		EDIProject Project
		{
			get { return Wrapper.WrappedObject; }
		}
	}
}
