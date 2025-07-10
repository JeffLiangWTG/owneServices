using System.Drawing;
using System.Web.UI.WebControls;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.GUI.WebControls.Testing;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips
{
	sealed class ZFilterLayoutDropDownListTest : ZDropDownListTest
	{
		#region TestMakeCurrentFilterUnsaved

		public void TestMakeCurrentFilterUnsaved()
		{
			AssertNull("Precondition", DropDown.SelectedItem);
			DropDown.MakeCurrentFilterUnsaved(); // ensure no exception when SelectedItem is null

			ListItem unselectedItem = new ListItem("oink");
			ListItem selectedItem = new ListItem("moo");
			DropDown.Items.Add(unselectedItem);
			DropDown.Items.Add(selectedItem);
			DropDown.SelectedIndex = 1;
			DropDown.MakeCurrentFilterUnsaved();

			AssertNull(unselectedItem.Attributes["style"]);
			AssertEquals("color:" + KnownColor.GrayText, selectedItem.Attributes["style"]);
		}

		#endregion

		#region TestOnClientChange

		public void TestOnClientChange()
		{
			AssertEquals("Precondition", "", DropDown.OnClientChange);

			DropDown.OnClientChange = "DoSomething()";
			AssertEquals("DoSomething()", DropDown.OnClientChange);
		}

		#endregion

		#region TestRebind

		public void TestRebind()
		{
			ZString code1 = DummyWithList.DummyList[0].Code;
			ZString code2 = DummyWithList.DummyList[1].Code;
			AssertEquals("Precondition", false, code1 == code2);

			DummyWithList.Z0_Code = code1;
			DummyWithList.Z0_Description = code2;

			DropDown.BindTo = DummyBizoSchema.Constants.Z0_Code;
			DropDown.BindToList = "DummyList";
			DropDown.Bind(DummyWithList);
			AssertEquals(code1, DropDown.SelectedValue);

			DropDown.BindTo = DummyBizoSchema.Constants.Z0_Description;
			DropDown.Rebind();
			AssertEquals(code2, DropDown.SelectedValue);
		}

		#endregion

		#region Implementation

		new ZFilterLayoutDropDownList DropDown
		{
			get { return fDropDown ?? (fDropDown = new ZFilterLayoutDropDownList()); }
		}

		ZFilterLayoutDropDownList fDropDown;

		#endregion
	}
}
