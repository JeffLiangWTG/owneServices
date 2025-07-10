using System;
using CargoWiseOne.ResourceStrings;

namespace CargoWise.EntityFramework.Testing
{
	using System.Collections.Generic;
	using System.ComponentModel;
	using NUnit.Framework;

	public static class DataBoundResourceStringsTestExtensions
	{
		public static void AssertDataBoundResourceStringsWithMultipleResourceKey(this TestCaseWithFactory testCase, ZPropertyInfo info, string multipleResourceKey, string caption, string shortCaption = "", string mediumCaption = "", string fullDescription = "", IDataBoundBusinessObject dataBoundBusinessObject = null) =>
			AssertDataBoundResourceStringsWithMultipleResourceKey(testCase, info, string.IsNullOrEmpty(multipleResourceKey) ? null : new[] { multipleResourceKey }, caption, shortCaption, mediumCaption, fullDescription, dataBoundBusinessObject);

		public static void AssertDataBoundResourceStringsWithMultipleResourceKey(this TestCaseWithFactory testCase, ZPropertyInfo info, IReadOnlyList<string> multipleResourceKeys, string caption, string shortCaption = "", string mediumCaption = "", string fullDescription = "", IDataBoundBusinessObject dataBoundBusinessObject = null)
		{
			var captionResourceString = DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(info, dataBoundBusinessObject, multipleResourceKeys);
			TestCase.AssertEquals("Caption", caption, captionResourceString.Caption);
			TestCase.AssertEquals("ShortCaption", shortCaption, captionResourceString.ShortCaption);
			TestCase.AssertEquals("MediumCaption", mediumCaption, captionResourceString.MediumCaption);
			TestCase.AssertEquals("FullDescription", fullDescription, captionResourceString.FullDescription);
		}
	}

	sealed class DataBoundResourceStringsTest : TestCaseWithFactory
	{
		public void TestGetColumnDescriptiveName()
		{
			AssertEquals("Description", DataBoundResourceStrings.GetColumnDescriptiveName("DummyBizo", "Z0_Description"));
			AssertEquals("Full Name", DataBoundResourceStrings.GetColumnDescriptiveName("OrgHeader", "OH_FullName"));
			AssertEquals("OrgHeader|OH_HelloEveryone", DataBoundResourceStrings.GetColumnDescriptiveName("OrgHeader", "OH_HelloEveryone"));
		}

		public void TestGetTableDescriptiveName()
		{
			AssertEquals("Contact", DataBoundResourceStrings.GetTableDescriptiveName("OrgContact"));
		}

		public void TestMultipleResourceStringData()
		{
			AssertEquals("[5/34] Description", DataBoundResourceStrings.GetDataForProperty(typeof(DummyBusinessObjectSupportMultipleResourceStringData1), "Z0_Description", new[] { "DummyBusinessObject|ECC" }).Caption);
			AssertEquals("Number", DataBoundResourceStrings.GetDataForProperty(typeof(DummyBusinessObjectSupportMultipleResourceStringData1), "Z0_Number", new[] { "DummyBusinessObject|ECC" }).Caption);

			AssertEquals("[21] Description", DataBoundResourceStrings.GetDataForProperty(typeof(DummyBusinessObjectSupportMultipleResourceStringData1), "Z0_Description", new[] { null, string.Empty, "UNKNOWN", "DummyBusinessObject|SAD", "DummyBusinessObject|ECC" }).Caption);
			AssertEquals("[5/34] Description", DataBoundResourceStrings.GetDataForProperty(typeof(DummyBusinessObjectSupportMultipleResourceStringData1), "Z0_Description", new[] { null, string.Empty, "UNKNOWN", "DummyBusinessObject|ECC", "DummyBusinessObject|SAD" }).Caption);

			var descriptor = TypeDescriptor.GetProperties(typeof(DummyBusinessObjectSupportMultipleResourceStringData1))["Z0_Description"];
			AssertEquals("[21] Description", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(descriptor, "DummyBusinessObject|SAD", null).Caption);
			AssertEquals("[54] Test", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(descriptor, "", null).Caption);
			AssertEquals("[54] Test", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(descriptor, (string)null, null).Caption);

			AssertEquals("[21] Description", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(descriptor, null, new[] { null, string.Empty, "UNKNOWN", "DummyBusinessObject|SAD", "DummyBusinessObject|ECC" }).Caption);
			AssertEquals("[5/34] Description", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(descriptor, null, new[] { null, string.Empty, "UNKNOWN", "DummyBusinessObject|ECC", "DummyBusinessObject|SAD" }).Caption);
		}

		public void TestIsApplicable()
		{
			var dummyBO = Factory.New<DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable>();
			var businessObject = new DataBoundBusinessObject(dummyBO);
			AssertEquals("Description for Z0_Number when DummyValue is 0", "Number", DataBoundResourceStrings.GetDataForProperty(dummyBO.Z0_NumberInfo, businessObject: businessObject)?.Caption);

			dummyBO.SetDummyValue(1);
			AssertEquals("Description for Z0_Number when DummyValue is 1", "Number_Description_True", DataBoundResourceStrings.GetDataForProperty(dummyBO.Z0_NumberInfo, businessObject: businessObject)?.Caption);
		}

		public void TestIsApplicable_WithMultipleKey()
		{
			var dummyBO = Factory.New<DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable>();
			var resourceString = GetResourceStringData(new[] { "DummyMultipleKeyAnother" });
			AssertEquals("When Multiple Resource Key and Dummy Value is 0", "Description_4", resourceString?.Caption);

			dummyBO.SetDummyValue(1);
			resourceString = GetResourceStringData(new[] { "DummyMultipleKey" });
			AssertEquals("When Multiple Resource Key and Dummy Value is 1", "Description_1", resourceString?.Caption);

			resourceString = GetResourceStringData(Array.Empty<string>());
			AssertEquals("When No Multiple Resource Key and Dummy Value is 1", "Description_2", resourceString?.Caption);

			dummyBO.SetDummyValue(0);
			resourceString = GetResourceStringData(Array.Empty<string>());
			AssertEquals("When No Multiple Resource Key and Dummy Value is 0", "Description_3", resourceString?.Caption);

			ResourceStringData GetResourceStringData(string[] multipleKeys)
			{
				var businessObject = new DataBoundBusinessObject(dummyBO);
				return DataBoundResourceStrings.GetDataForProperty(typeof(DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable), "Z0_Description", businessObject, multipleKeys);
			}
		}

		public void TestCaption_WithMultipleKeyAndIsApplicable()
		{
			var dummyBO = Factory.New<DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable>();
			AssertEquals("When MultipleKeys And Not Applicable", "Date_None", GetResourceStringData()?.Caption);

			dummyBO.SetDummyValue(1);
			AssertEquals("When Dummy Value is 1", "Date_One", GetResourceStringData()?.Caption);

			dummyBO.SetDummyValue(2);
			AssertEquals("When Dummy Value is 2", "Date_Two", GetResourceStringData()?.Caption);

			dummyBO.SetDummyValue(11);
			AssertEquals("When Dummy Value is other than 1 or 2", "Date_None", GetResourceStringData()?.Caption);

			ResourceStringData GetResourceStringData()
			{
				var businessObject = new DataBoundBusinessObject(dummyBO);
				return DataBoundResourceStrings.GetDataForProperty(typeof(DummyBusinessObjectSupportWithResourceStringDataWithIsApplicable), "Z0_AnotherDate", businessObject, new[] { "DummyMultipleKey" });
			}
		}
	}
}
