using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZEditForm))]
	sealed class ZEditFormTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var businessObject = Factory.New<DummyEnterpriseBusinessObject>();
			Factory.Save();
			return new DummyZEditForm(businessObject);
		}

		public class DummyZEditForm : ZEditForm
		{
			public DummyZEditForm(BusinessObject businessObject) : base(businessObject)
			{
				CaptionRenderingEnabled = true;
				this.CaptionResourceString = Enterprise.ZArchitecture.GUI.UserControls.Res.GetData("c4fb11ba-8559-4055-a1c5-e3287a2a173b", "Test");
			}

			public Control SaveButtonUserControlExposed => SaveButtonUserControl;
		}

		public void TestSaveButtonUserControlShouldAlignRight()
		{
			using (ComponentExtensions.SwitchToDesignMode())
			using (var form = GetFormToBashCore() as DummyZEditForm)
			{
				form.SaveButtonUserControlExposed.Location = new System.Drawing.Point(10, form.SaveButtonUserControlExposed.Location.Y);

				form.Show();

				AssertEquals(form.ClientSize.Width - form.SaveButtonUserControlExposed.Width, form.SaveButtonUserControlExposed.Location.X);
			}
		}

		#endregion
	}
}
