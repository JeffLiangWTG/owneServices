using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.EU.H7.GUI.Testing;

[TestedType(typeof(OverrideUserControl))]
public class OverrideUserControlTest : TestCaseWithFactory
{
	public void TestOverrideCheckBox()
	{
		using (var control = new OverrideUserControl())
		{
			var checkbox = control.Controls.Find("overrideCheckBox", true).FirstOrDefault() as ZCheckBox;
			AssertNotNull("overrideCheckBox should be added", checkbox);
			AssertEquals("BindTo should be empty", string.Empty, checkbox.BindTo);

			control.BindToOverride = "OverrideMessageType";
			AssertEquals("BindTo should be set", "OverrideMessageType", checkbox.BindTo);
		}
	}

	public void TestCodeDropEdit()
	{
		using (var control = new OverrideUserControl())
		{
			var dropEdit = control.Controls.Find("codeDropEdit", true).FirstOrDefault() as ZDropEdit;
			AssertNotNull("codeDropEdit should be added", dropEdit);
			AssertEquals("BindTo should be empty", string.Empty, dropEdit.BindTo);

			control.BindToCode = "MessageType";
			AssertEquals("BindTo should be set", "MessageType", dropEdit.BindTo);
		}
	}
}
