using CargoWise.Types;
using Enterprise.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.Services.OperationalActions.Business.Testing
{
	static class OperationalActionMenuEditableHelperTest
	{
		public static void TestEditingMode(IOperationalActionMenuEditable menuEditable)
		{
			Assertion.AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, menuEditable.EditingMode);
			Assertion.AssertEquals("ReadOnly", false, menuEditable.ReadOnly);
			Assertion.AssertEquals("SystemDefinedInfo.ReadOnly", true, menuEditable.SystemDefinedInfo.ReadOnly);
			menuEditable.SystemDefinedInfo.Value = ZBool.True;
			Assertion.AssertEquals("ReadOnly", true, menuEditable.ReadOnly);
			menuEditable.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			Assertion.AssertEquals("ReadOnly", true, menuEditable.ReadOnly);
			menuEditable.SystemDefinedInfo.Value = ZBool.False;
			Assertion.AssertEquals("ReadOnly", false, menuEditable.ReadOnly);
			Assertion.AssertEquals("SystemDefinedInfo.ReadOnly", true, menuEditable.SystemDefinedInfo.ReadOnly);
			menuEditable.ReadOnly = true;
			Assertion.AssertEquals("ReadOnly", true, menuEditable.ReadOnly);
			menuEditable.ReadOnly = false;
			Assertion.AssertEquals("ReadOnly", false, menuEditable.ReadOnly);
			Assertion.AssertEquals("SystemDefinedInfo.ReadOnly", true, menuEditable.SystemDefinedInfo.ReadOnly);
			menuEditable.EditingMode = MenuEditingMode.AllowAll;
			menuEditable.SystemDefinedInfo.Value = ZBool.True;
			Assertion.AssertEquals("ReadOnly", false, menuEditable.ReadOnly);
			Assertion.AssertEquals("SystemDefinedInfo.ReadOnly", false, menuEditable.SystemDefinedInfo.ReadOnly);
		}
	}
}
