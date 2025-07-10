using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using CargoWiseOne.ResourceStrings.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[TestedType(typeof(SortOrder))]
	sealed class SortOrderTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SortOrder(String.Empty, String.Empty);
		}

		public void TestDisplayName()
		{
			AssertEquals("disp", new SortOrder("disp", "x").DisplayName);
		}

		public void TestText()
		{
			var sortOrder = new SortOrder("disp", "x");
			using (var mockRes = Res.UseMockData())
			{
				const string displayNameLocalized = "虚设名称";
				mockRes.SetResourceGetter(new ResourceStringGetter(k => new ResourceStringData(k, displayNameLocalized)));
				sortOrder.DisplayNameLocalizedData = mockRes.Get("");
				AssertEquals("TestBizObj.Text", "虚设名称", sortOrder.Text);
			}
		}

		public void TestFieldList()
		{
			AssertEquals("field", new SortOrder("x", "field").FieldList);
		}

		public void TestJsonConverter()
		{
			var sortOrder = new SortOrder("a", "b");

			var result = JsonConverterHelper.Serialize(sortOrder);
			var deserialisedField = JsonConverterHelper.Deserialize<SortOrder>(result);

			AssertEquals("DisplayName", sortOrder.DisplayName, deserialisedField.DisplayName);
			AssertEquals("FieldList", sortOrder.FieldList, deserialisedField.FieldList);
			AssertEquals("Selected", sortOrder.Selected, deserialisedField.Selected);
		}
	}
}
