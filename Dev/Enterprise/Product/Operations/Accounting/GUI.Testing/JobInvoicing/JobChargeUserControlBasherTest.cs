using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.GUI.JobInvoicing.Testing
{
	[TestedType(typeof(TestFormForJobChargeUserControl))]
	public class JobChargeUserControlBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			Job newJob = Factory.NewJobForTesting<Job>();
			return new TestFormForJobChargeUserControl(newJob);
		}

		public void TestCaption()
		{
			var testForm = GetFormToBashCore();
			using (testForm)
			{
				testForm.Show();
				ExposeAllTabPages(testForm);
				BashControlCaption(testForm);
			}
		}

		void BashControlCaption(Control controlToBash)
		{
			var controlToSkip = new List<string>
			{
				"LocalAgentDeclaredCostAmountCalcEdit",
				"LocalAgentDeclaredSellAmountCalcEdit"
			};

			var controls = new ArrayList(controlToBash.Controls);
			foreach (Control control in controls)
			{
				BashControlCaption(control);
			}

			var renderer = controlToBash.GetExtension<ZLabelCaptionRenderer>();
			if (renderer != null && renderer.captionMeasurement != null && !controlToSkip.Contains(controlToBash.Name))
			{
				AssertNotNull($"Control: {controlToBash.Name} failed to measure Caption, please check the Caption of the control is displayed correctly", renderer.captionMeasurement.Caption);
			}
		}

		class TestFormForJobChargeUserControl : ZForm
		{
			public TestFormForJobChargeUserControl(Job newJob)
				: base(newJob)
			{
				this.CaptionRenderingEnabled = true;
				var userControl = new JobChargeUserControl();
				userControl.Bind(newJob);
				Controls.Add(userControl);
				this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(new Size(userControl.Size.Width, userControl.Size.Height), false) + CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(new Size(20, 20));
				userControl.AllowOverlap(MainStatusBar);
				userControl.Show();
			}
		}
	}
}
