using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(MultistepProgressForm))]
	public class MultistepProgressFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new MultistepProgressForm(1);
		}

		protected override bool AllowFormSizeFixed => true;
	}

	public class MultistepProgressFormGeneralTest : TestCase
	{
		public void TestModifyStatusAndPercentComplete()
		{
			using (var form = new DummyMultistepProgressForm(3))
			{
				var status = "Importing 1 record from 5";
				var percentComplete = 90;
				form.TestModifyStatusAndPercentComplete(ref status, ref percentComplete);
				AssertEquals("Step 1 of 3: Importing 1 record from 5", status);
				AssertEquals(30, percentComplete);

				status = "Processing 5 record from 10";
				percentComplete = 50;
				form.TestModifyStatusAndPercentComplete(ref status, ref percentComplete);
				AssertEquals("Step 2 of 3: Processing 5 record from 10", status);
				AssertEquals(50, percentComplete);

				status = "Validating 2 record from 15";
				percentComplete = 30;
				form.TestModifyStatusAndPercentComplete(ref status, ref percentComplete);
				AssertEquals("Step 3 of 3: Validating 2 record from 15", status);
				AssertEquals(76, percentComplete);
			}
		}

		class DummyMultistepProgressForm : MultistepProgressForm
		{
			public DummyMultistepProgressForm(int stepNumber)
				: base(stepNumber)
			{
			}

			public void TestModifyStatusAndPercentComplete(ref string status, ref int percentComplete)
			{
				ModifyStatusAndPercentComplete(ref status, ref percentComplete);
			}
		}
	}
}
