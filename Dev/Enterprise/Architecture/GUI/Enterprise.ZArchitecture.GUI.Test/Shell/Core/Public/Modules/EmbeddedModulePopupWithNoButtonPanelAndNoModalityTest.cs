using System.Windows.Forms;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Modules
{
	sealed class EmbeddedModulePopupWithNoButtonPanelAndNoModalityTest : TransactionedTestCase
	{
		public void TestButtonPanelIsHidden()
		{
			using (var modulePopup = new EmbeddedModulePopupWithNoButtonPanelAndNoModalityForTest())
			{
				AssertEquals(false, modulePopup.ButtonPanel.Visible);
			}
		}

		public void TestFormsOpenNotModal()
		{
			using (var modulePopup = new EmbeddedModulePopupWithNoButtonPanelAndNoModalityForTest())
			{
				AssertNull(modulePopup.ParentForm);
			}
		}

		internal class EmbeddedModulePopupWithNoButtonPanelAndNoModalityForTest : ZFilterModule.EmbeddedModulePopupWithNoButtonPanelAndNoModality
		{
			public EmbeddedModulePopupWithNoButtonPanelAndNoModalityForTest()
				: base((ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
			}

			public new Panel ButtonPanel => base.ButtonPanel;
		}
	}
}
