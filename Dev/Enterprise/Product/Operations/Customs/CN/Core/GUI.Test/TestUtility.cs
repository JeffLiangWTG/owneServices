using System;
using System.Linq;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CN.GUI
{
	public static class TestUtility
	{
		public static void AssertControlExistance(ZUserControl parent, string name, string dataBinding = null, Func<object> extraAssert = null)
		{
			AssertControlExistance<Control>(parent, name, dataBinding, extraAssert);
		}

		public static T AssertControlExistance<T>(ZUserControl parent, string name, string dataBinding = null, Func<object> extraAssert = null) where T : Control
		{
			var bindingSource = parent.BindingSource;

			var control = parent.Controls.Find(name, true).FirstOrDefault();
			Assertion.AssertNotNull(name + " should be found", control);
			if (control != null && dataBinding != null)
			{
				Assertion.AssertEquals("Binding member of " + name + " should be " + dataBinding, dataBinding, bindingSource.GetBindingMember(control));
			}
			extraAssert?.Invoke();

			return (T)control;
		}

		public static CustomsEntriesAndEntryLinesUserControl FindCustomsEntriesAndEntryLinesUserControl(this JobDeclarationForm form)
		{
			var brokerageUserControl = form.CustomsBrokerageUserControl;
			brokerageUserControl.LoadMessageTabPage();
			brokerageUserControl.MainTabControl.SelectedTab = brokerageUserControl.MessagesTabPage;
			return brokerageUserControl.MessagesTabPage.Controls.Find("CustomsEntriesAndEntryLinesUserControl", true).FirstOrDefault() as CustomsEntriesAndEntryLinesUserControl;
		}

		public static InvoiceLineUserControl FindInvoiceLineUserControl(this JobDeclarationForm form)
		{
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
			brokerageControl.LoadInvoiceLinesTabPage();
			return brokerageControl.InvoiceLinesUserControl as InvoiceLineUserControl;
		}

		public static EntryInstructionUserControl FindEntryInstructionUserControl(this JobDeclarationForm form)
		{
			var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
			brokerageControl.LoadEntryInstructionDetailsTabPage();
			var entryInstructionTabPage = brokerageControl.EntryInstructionDetailsTabPage;
			brokerageControl.MainTabControl.SelectedTab = entryInstructionTabPage;
			return entryInstructionTabPage.Controls[0] as EntryInstructionUserControl;
		}

		public static CustomsSupplierHeaderUserControl FindCustomsSupplierHeaderUserControl(this JobDeclarationForm form)
		{
			var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
			brokerageControl.LoadInvoicesTabPage();
			return brokerageControl.SupplierHeaderUserControl as CustomsSupplierHeaderUserControl;
		}
	}
}

