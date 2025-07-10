using System.Data;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DescriptionPropertyAttributeTest : TestCaseWithFactory
	{
		public void TestDescriptionFromBusinessObject()
		{
			var testDescription = new ZString("MyTestDescription");
			var bizO = Factory.New(typeof(DummyBusinessObject)) as DummyBusinessObject;
			bizO.Z0_Description = testDescription;
			AssertEquals(testDescription, DescriptionPropertyAttribute.DescriptionFromBusinessObject(bizO));
			AssertEquals("Z0_Description", DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(DummyBusinessObject)));
		}

		public void TestDescriptionFromBusinessObjectWithMultilingualString()
		{
			var testDescription = (NoResString)"MyTestDescription";
			var bizO = Factory.New(typeof(DummyBusinessObjectWithMultilingualString)) as DummyBusinessObjectWithMultilingualString;
			bizO.Description = testDescription;
			AssertEquals(testDescription, DescriptionPropertyAttribute.DescriptionFromBusinessObject(bizO));
			AssertEquals("Description", DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(DummyBusinessObjectWithMultilingualString)));
		}

		public void TestDescriptionFromBusinessObjectWithTranslatableField()
		{
			var bizO = Factory.New<TranslatableDataFieldTestCase.DummyWithTranslatable>();
			bizO.Z0_Description = "Boom";
			var resKey = bizO.Z0_DescriptionInfo.CustomizableDataResourceStrings.GetMultilingualString(bizO, "Boom").ResourceKey;
			AssertEquals("Boom", DescriptionPropertyAttribute.DescriptionFromBusinessObject(bizO));
			using (Res.TemporarilySwitchLanguage("ZH-CN"))
			using (var mockRes = Res.UseMockData())
			{
				mockRes.Put(resKey, new ResourceStringData(resKey, "咚"));
				AssertEquals("咚", DescriptionPropertyAttribute.DescriptionFromBusinessObject(bizO));
			}
		}

		public void TestDescriptionPropertyNameFromTypeWithNoMultilingual()
		{
			AssertEquals("Z0_Description", DescriptionPropertyAttribute.DescriptionPropertyNameFromTypeWithNoMultilingual(typeof(DummyBusinessObjectWithMultilingualStringProperty)));
		}

		#region TestGetProperty

		public void TestGetProperty()
		{
			var obj = new ClassWithDescriptionProperty();
			obj.MaiProperty = "Dat Property";

			var property = DescriptionPropertyAttribute.GetProperty(typeof(ClassWithDescriptionProperty), typeof(DescriptionPropertyAttribute));
			AssertNotNull(property);
			AssertEquals("Dat Property", property.GetValue(obj, null));
		}

		[DescriptionProperty("MaiProperty")]
		class ClassWithDescriptionProperty
		{
			public string MaiProperty { get; set; }
		}

		#endregion

		#region TestCanBeReferencedByDescription

		public void TestCanBeReferencedByDescription()
		{
			Assert("Disabled for non-supported types", !DescriptionPropertyAttribute.CanBeReferencedByDescription(typeof(object)));
			Assert("Disabled by default", !DescriptionPropertyAttribute.CanBeReferencedByDescription(typeof(DummyBusinessObjectWithMultilingualString)));

			Assert("Enabled", DescriptionPropertyAttribute.CanBeReferencedByDescription(typeof(DummyWithDescription)));
		}

		[CodeProperty("Z0_Code"), DescriptionProperty("Z0_Description", CanBeReferencedBy = true)]
		class DummyWithDescription : DummyBusinessObject
		{
			public DummyWithDescription(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }
		}

		#endregion
	}
}
