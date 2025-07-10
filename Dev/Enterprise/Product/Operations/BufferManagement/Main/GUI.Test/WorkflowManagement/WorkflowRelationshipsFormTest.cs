using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.BufferManagement.GUI.Test
{
	[TestedType(typeof(WorkflowRelationshipsForm))]
	class WorkflowRelationshipsFormTest : ZFormBasherTest
	{
		public void TestFormVerb()
		{
			var workflow = Factory.NewWithValidTestData<ProcessHeader>();

			using (var form = new WorkflowRelationshipsForm(workflow))
			{
				AssertEquals("Workflow Relationships", form.FormCaption);
			}
		}

		#region Implementation

		public override void TestMinimumSizeNotTooBig()
		{
			Assert("725px high is too small. RJW approved 1080p for BMS.", true);
		}

		protected override Form GetFormToBashCore()
		{
			var workflow = ProcessJobHeader.GetForParent(Factory.NewWithValidTestData<OrgHeader>(), Factory).ProcessHeaders.AddNew();
			Factory.Save();

			return new WorkflowRelationshipsForm(workflow);
		}

		#endregion
	}
}
