using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.GUI.Testing
{
	[TestedType(typeof(DEGuaranteeForm))]
	sealed class DEGuaranteeFormTest : ZFormBasherTest
	{
		public void TestSendAccessCodesToCustomsMenuItem()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();

			using (var form = new DEGuaranteeForm(guaranteeHeader))
			{
				form.Show();
				Application.DoEvents();

				var actionsMenuItem = form.Menu.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Actio&ns");
				var sendCodeMenuItem = actionsMenuItem.MenuItems.Cast<MenuItem>().Single(x => x.Text == "Send Access Code to Customs");

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(AssertType<SendAccessCodeForm>);

				sendCodeMenuItem.PerformClick();
			}
		}

		protected override Form GetFormToBashCore()
		{
			var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
			return new DEGuaranteeForm(guaranteeHeader);
		}
	}
}
