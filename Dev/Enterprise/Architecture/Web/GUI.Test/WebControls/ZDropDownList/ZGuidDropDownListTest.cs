using System;
using System.Web.UI;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZGuidDropDownListTest : ZDropDownListTest
	{
		#region Setup

		protected override Control GetNewControl()
		{
			return new ZGuidDropDownList();
		}

		#endregion

		public void TestBindToGUIDField()
		{
			DropDown.BindTo = "Z0_Guid";
			DropDown.DataValueField = "PK";
			DropDown.BindToList = "DummyList";
			Page.Controls.Add(DropDown);
			DummyWithList.Z0_Guid = (Guid)DummyWithList.DummyList[3].PK;
			DropDown.Bind(DummyWithList);

			AssertEquals(DummyWithList.Z0_Guid.ToString(), DropDown.SelectedValue);

			Page.IsPostBack = true;

			DropDown.SelectedValue = DummyWithList.DummyList[1].PK.ToString();
			DropDown.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Guid.ToString(), DropDown.SelectedValue);

			DropDown.SelectedValue = DummyWithList.DummyList[3].PK.ToString();
			DropDown.Bind(DummyWithList);
			AssertEquals(DummyWithList.Z0_Guid.ToString(), DropDown.SelectedValue);
		}

		public void TestWrongValue()
		{
			DropDown.SelectedValue = "abc";
			AssertEquals("Selected value should be empty", "", DropDown.SelectedValue);
		}

		protected override System.Collections.Generic.Dictionary<Pair, bool> ExpectedLoadPostData()
		{
			var result = base.ExpectedLoadPostData();
			result.Add(new Pair(Guid.Empty, string.Empty), false);
			result.Add(new Pair(string.Empty, Guid.Empty), false);
			result.Add(new Pair(Guid.Empty, Guid.Empty), false);
			result.Add(new Pair(Guid.NewGuid(), string.Empty), true);
			result.Add(new Pair(string.Empty, Guid.NewGuid()), true);
			var value = Guid.NewGuid();
			result.Add(new Pair(value, value), false);
			return result;
		}
	}
}
