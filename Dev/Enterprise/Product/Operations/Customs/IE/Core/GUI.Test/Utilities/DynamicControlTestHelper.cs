using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IE.GUI.Testing
{
	public static class DynamicControlTestHelper
	{
		public static void AssertTabPageCaptionAndUserControlType<TControl>(BusinessObject businessObject, string tabPageName, string userControlName, string expectedCaption, Type expectedUserControlType)
			where TControl : Control, new()
		{
			using (var form = businessObject == null ? new ZForm() : new ZForm(businessObject))
			using (var control = new TControl())
			{
				form.Controls.Add(control);
				form.Show();

				var tabPage = control.FindSingle<ZTabPage>(tabPageName);
				tabPage.Show();
				var foundUserControl = control.FindSingle<ZDynamicControlCreationUserControl>(userControlName);
				Assertion.AssertEquals(expectedCaption, tabPage.CaptionResourceString.Caption);
				Assertion.AssertEquals(expectedUserControlType, foundUserControl.UserControlType);
			}
		}

		public static void AssertTabPageCaptionAndUserControlType<TControl>(string tabPageName, string userControlName, string expectedCaption, Type expectedUserControlType) where TControl : Control, new()
		{
			AssertTabPageCaptionAndUserControlType<TControl>(null, tabPageName, userControlName, expectedCaption, expectedUserControlType);
		}
	}
}
