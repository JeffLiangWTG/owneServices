using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class UserControlDeciderTest : TestCase
	{
		public void TestBooleanUserControl()
		{
			using (var userControl = (ZCheckBox)UserControlDecider.CreateControl(FieldType.Boolean, null, "bindingField", ""))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
			}
		}

		public void TestDateUserControl()
		{
			using (var userControl = (ZDateEdit)UserControlDecider.CreateControl(FieldType.Date, null, "bindingField", ""))
			{
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, userControl.DateTimeFormat);
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
			}
		}

		public void TestDateTimeUserControl()
		{
			using (var userControl = (ZDateEdit)UserControlDecider.CreateControl(FieldType.DateTime, null, "bindingField", ""))
			{
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, userControl.DateTimeFormat);
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
			}
		}

		public void TestDateTimeOffsetUserControl()
		{
			using (var userControl = (ZDateTimeOffsetEdit)UserControlDecider.CreateControl(FieldType.DateTimeOffset, null, "bindingField", ""))
			{
				AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Long, userControl.DateTimeFormat);
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
			}
		}

		public void TestByteUserControl()
		{
			using (var userControl = (ZCalcEdit)UserControlDecider.CreateControl(FieldType.Byte, null, "bindingField", ""))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("Decimals", 0, userControl.Decimals);
			}
		}

		public void TestDecimalUserControl()
		{
			using (var userControl = (ZCalcEdit)UserControlDecider.CreateControl(FieldType.Decimal, null, "bindingField", ""))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("Decimals", 6, userControl.Decimals);
			}
		}

		public void TestGuidUserControl()
		{
			using (var userControl = (ZGuidFindBox)UserControlDecider.CreateControl(FieldType.Guid, ModuleIDs.RefCountry, "bindingField", "bindingListField"))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("BindTo", "bindingListField", userControl.BindToList);
				AssertEquals("ModuleID", ModuleIDs.RefCountry, userControl.ModuleID);
			}
		}

		public void TestGuidDropEditUserControl()
		{
			using (var userControl = (ZGuidDropEdit)UserControlDecider.CreateControl(FieldType.GuidDropEdit, null, "bindingField", "bindingListField"))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("BindTo", "bindingListField", userControl.BindToList);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, userControl.CharacterCasing);
			}
		}

		public void TestIntegerUserControl()
		{
			using (var userControl = (ZCalcEdit)UserControlDecider.CreateControl(FieldType.Integer, null, "bindingField", ""))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("Decimals", 0, userControl.Decimals);
			}
		}

		public void TestOrganisationGuidUserControl()
		{
			using (var userControl = (ZGuidFindBox)UserControlDecider.CreateControl(FieldType.OrganisationGuid, ModuleIDs.RefCountry, "bindingField", "bindingListField"))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("BindTo", "bindingListField", userControl.BindToList);
				AssertEquals("ModuleID", ModuleIDs.RefCountry, userControl.ModuleID);
			}
		}

		public void TestTextUserControl()
		{
			using (var userControl = (ZTextBox)UserControlDecider.CreateControl(FieldType.Text, null, "bindingField", ""))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, userControl.CharacterCasing);
			}
		}

		public void TestTextCodeFindBoxUserControl()
		{
			using (var userControl = (ZCodeFindBox)UserControlDecider.CreateControl(FieldType.TextCodeFindBox, ModuleIDs.RefCountry, "bindingField", "bindingListField"))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("BindTo", "bindingListField", userControl.BindToList);
				AssertEquals("ModuleID", ModuleIDs.RefCountry, userControl.ModuleID);
			}
		}

		public void TestTextDropEditUserControl()
		{
			using (var userControl = (ZDropEdit)UserControlDecider.CreateControl(FieldType.TextDropEdit, ModuleIDs.RefCountry, "bindingField", "bindingListField"))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("BindTo", "bindingListField", userControl.BindToList);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, userControl.CharacterCasing);
			}
		}

		public void TestTextMultiLineUserControl()
		{
			using (var userControl = (ZTextBox)UserControlDecider.CreateControl(FieldType.TextMultiLine, null, "bindingField", ""))
			{
				AssertEquals("BindTo", "bindingField", userControl.BindTo);
				AssertEquals("CharacterCasing", CharacterCasing.Normal, userControl.CharacterCasing);
				AssertEquals("Multiline", true, userControl.Multiline);
				AssertEquals("ScrollBars", ScrollBars.Both, userControl.ScrollBars);
				AssertEquals("AcceptsReturn", true, userControl.AcceptsReturn);
			}
		}
	}
}
