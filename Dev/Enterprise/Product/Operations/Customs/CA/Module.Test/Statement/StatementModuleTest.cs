using System;
using System.Windows.Forms;
using Enterprise.Customs.CA.GUI;
using Enterprise.Customs.CA.Registry;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.CA.Module.Testing
{
	public abstract class StatementModuleTest : ZModuleBasherTest
	{
		public void TestImportUniversalTransactionBatchXMLFilesMenu()
		{
			var menuText = "Import Universal Transaction Batch XML";

			using (var module = (ZFilterGridModule)GetModule())
			using (var form = new ZForm())
			using (CACustomsDataRegistry.Instance.EnableImportUniversalTransactionBatchXMLFiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var filterControl = module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();

				Application.DoEvents();

				var menu = module.DataTransferMenuItem.MenuItems.FindByText(menuText);
				AssertNotNull("Should create the menu when the value of EnableImportUniversalTransactionBatchXMLFiles is enable.", menu);

				menu.PerformClick();
				AssertEquals(typeof(TransactionBatchXmlDataImportForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}

			using (var module = (ZFilterGridModule)GetModule())
			using (var form = new ZForm())
			using (CACustomsDataRegistry.Instance.EnableImportUniversalTransactionBatchXMLFiles.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var filterControl = module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();

				Application.DoEvents();

				var menu = module.DataTransferMenuItem.MenuItems.FindByText(menuText);
				AssertNull("Should not create the menu when the value of EnableImportUniversalTransactionBatchXMLFiles is disable.", menu);
			}
		}

		public void TestSecurityCheckPoint()
		{
			using (var module = GetModule())
			{
				AssertEquals("SecurityCheckpoint", ExpectedSecurityCheckpoint, module.SecurityCheckpoint);
			}
		}

		public virtual void TestFilterControl()
		{
			using (var module = GetModule())
			{
				AssertEquals("Filter control type", typeof(StatementFilterControl), module.EmbeddedControl.GetType());
			}
		}

		protected abstract SecurityCheckpoint ExpectedSecurityCheckpoint { get; }

		protected override string CountryCode => Core.Constants.CountryCodes.Canada;
	}
}
