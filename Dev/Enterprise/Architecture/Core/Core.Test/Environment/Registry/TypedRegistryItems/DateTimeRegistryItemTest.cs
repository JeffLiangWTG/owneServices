using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	[TestedType(typeof(DateTimeRegistryItem))]
	public class DateTimeRegistryItemTest : StronglyTypedRegistryItemTestCase<DateTime>
	{
		public void TestConstructor_WithParamAsIRegistryItem()
		{
			RegistryItemImpl registry = new RegistryItemImpl("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryDataTypes.DateTimeType, RegistryStorageFlags.All);
			DateTimeRegistryItem item = new DateTimeRegistryItem(registry);
			AssertRegistryIsCorrectlyPopulated(item, "Name", "Category", "Caption", "Hint", RegistryStorageFlags.All, RegistryOptions.Default, DateTime.MinValue);
		}

		public void TestConstructor_With5Param()
		{
			DateTimeRegistryItem item = new DateTimeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.BranchDepartment);
			AssertRegistryIsCorrectlyPopulated(item, "Name", "Category", "Caption", "Hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.Default, DateTime.MinValue);
		}

		public void TestConstructor_With6Param_Options()
		{
			DateTimeRegistryItem item = new DateTimeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.IsOnlyForDevelopers);
			AssertRegistryIsCorrectlyPopulated(item, "Name", "Category", "Caption", "Hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.IsOnlyForDevelopers, DateTime.MinValue);
		}

		public void TestConstructor_With6Param_DefaultValue()
		{
			DateTimeRegistryItem item = new DateTimeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.BranchDepartment, new DateTime(2005, 8, 2));
			AssertRegistryIsCorrectlyPopulated(item, "Name", "Category", "Caption", "Hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.Default, new DateTime(2005, 8, 2));
		}

		public void TestConstructor_With7Param()
		{
			DateTimeRegistryItem item = new DateTimeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.NotCached, new DateTime(2005, 10, 2));
			AssertRegistryIsCorrectlyPopulated(item, "Name", "Category", "Caption", "Hint", RegistryStorageFlags.BranchDepartment, RegistryOptions.NotCached, new DateTime(2005, 10, 2));
		}

		public void TestDateTimeFormat()
		{
			DateTimeRegistryItem item = new DateTimeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.BranchDepartment);
			DateTimeRegistryEditorInfo editorInfo = item.EditorInfo as DateTimeRegistryEditorInfo;
			AssertNotNull("EditorInfo", editorInfo);
			AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, editorInfo.DateTimeFormat);
		}

		public void TestSetValue_WithNullValue()
		{
			IRegistryItem item = new DateTimeRegistryItem("Name", (NoResString)"Category", (NoResString)"Caption", (NoResString)"Hint", RegistryStorageFlags.BranchDepartment);
			item.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, null);
			AssertEquals("Value should be default to to DateTime.MinValue", DateTime.MinValue, item.Value);
		}

		#region Implementation

		void AssertRegistryIsCorrectlyPopulated(DateTimeRegistryItem item, string name, string category, string caption, string hint, RegistryStorageFlags storage, RegistryOptions options, DateTime defaultValue)
		{
			AssertNotNull("Item", item);
			AssertEquals("Name", name, item.Name);
			AssertEquals("Category", category, item.Category);
			AssertEquals("Caption", caption, item.Caption);
			AssertEquals("DataType", RegistryDataTypes.DateTimeType, item.DataType);
			AssertEquals("Storage", storage, item.Storage);
			AssertEquals("Options", options, item.Options);
			AssertEquals("DefaultValue", defaultValue, item.DefaultValue);
			AssertEquals("Value", defaultValue, item.Value);
			//AssertEquals("DateTimeFormat", ZDateTimePickerFormat.Short, Item.DateTimeFormat);
		}

		protected override StronglyTypedRegistryItem<DateTime, DateTime> GetNewRegistryItem()
		{
			return new DateTimeRegistryItem("", null, null, null, RegistryStorageFlags.System);
		}

		#endregion
	}
}
