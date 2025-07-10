using System;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public static class EUDynamicControlTestHelper
	{
		public static void AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<TControl>(JobDeclaration declaration, string tabPageName, string userControlName, string expectedCaption, Type expectedUserControlType)
			where TControl : EUCustomsSupplierHeaderUserControl, new()
		{
			declaration.Invoices.AddNew();
			using (var frm = new ZForm(declaration))
			using (var userControl = new TControl())
			{
				frm.Controls.Add(userControl);
				userControl.JobDeclaration = declaration;
				userControl.SetDataBinding(declaration, "");
				frm.Show();

				var tabPage = userControl.FindSingle<ZTabPage>(tabPageName);
				tabPage.Show();
				Assertion.CombineAssertions(() =>
				{
					Assertion.AssertEquals("TabPage Visible", true, tabPage.TabVisible);
					Assertion.AssertEquals("TabPage Caption", expectedCaption, tabPage.CaptionResourceString.Caption);
					var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>(userControlName);
					Assertion.AssertEquals("UserControl type", expectedUserControlType, foundUserControl.UserControlType);
				});
			}
		}

		public static void AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<TControl>(JobDeclaration declaration, string tabPageName, string userControlName, string expectedCaption, Type expectedUserControlType)
			where TControl : EUInvoiceLineUserControl, new()
		{
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (var frm = new ZForm(declaration))
			using (var userControl = new TControl())
			{
				frm.Controls.Add(userControl);
				userControl.JobDeclaration = declaration;
				frm.Show();

				var tabPage = userControl.FindSingle<ZTabPage>(tabPageName);
				tabPage.Show();
				Assertion.CombineAssertions(() =>
				{
					Assertion.AssertEquals("TabPage Visible", true, tabPage.TabVisible);
					Assertion.AssertEquals("TabPage Caption", expectedCaption, tabPage.CaptionResourceString.Caption);
					var foundUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>(userControlName);
					Assertion.AssertEquals("UserControl type", expectedUserControlType, foundUserControl.UserControlType);
				});
			}
		}
	}
}
