using System.Linq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.UserControls.Testing
{
	public static class TestHelper
	{
		public static void AssertControlExists(ZUserControl parent, string controlName, string bindingMember, bool checkIsVisible = false)
		{
			var control = parent.Controls.Find(controlName, true).FirstOrDefault();
			Assertion.AssertNotNull($"{controlName} should exist on {parent.Name}.", control);
			Assertion.AssertEquals($"Binding member of {controlName} should be {bindingMember}.", bindingMember, parent.BindingSource.GetBindingMember(control));

			if (checkIsVisible)
			{
				Assertion.Assert($"{controlName} should be visible.", control.Visible);
			}
		}
	}
}
