using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.ProgressForm
{
	[TestedType(typeof(ProcessStatusProgressForm))]
	public class ProcessStatusProgressFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ProcessStatusProgressForm();
		}

		protected override bool AllowFormSizeFixed => true;
	}

	public class ProcessStatusProgressFormTest : TestCase
	{
		public void TestModifyStatusAndPercentComplete()
		{
			using (var myForm = new DummyProgressForm())
			{
				myForm.SetStatusAndPercentComplete("Wrong data", 22);
				AssertEquals(55, myForm.PercentComplete);
				AssertEquals("Dummy Status", myForm.Status);
			}
		}
	}

	public class DummyProgressForm : ProcessStatusProgressForm
	{
		protected override void ModifyStatusAndPercentComplete(ref string status, ref int percentComplete)
		{
			status = "Dummy Status";
			percentComplete = 55;
		}
	}
}
