#if DEBUG
using System.Collections;
using System.ComponentModel;
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	class MockMetaDataType
	{
		public const string TestElementListMemberType = "MetaDataElementListMemberInfo.Test.TestElementListMemberType";

		public static void RegisterTypes()
		{
			MetaDataType.RegisterMetaDataType(new MetaDataListElementMemberType(TestElementListMemberType, typeof(int)));
		}
	}

	public class MetaDataElementListMemberTypeTests : TestCase
	{
		static MetaDataElementListMemberTypeTests()
		{ MockMetaDataType.RegisterTypes(); }

		public void TestInvalidElementListMember()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(TestObj))["PropertyWithList"];
			MetaDataType metaDataType = MetaDataType.GetMetaDataType(
				MockMetaDataType.TestElementListMemberType);

			string validationResults =
				metaDataType.ValidateMetaDataValue(typeof(TestObj), property, "SplatyBodgedPropertyName");
			Assert("Should have errors to report", validationResults.IndexOf("not find") != -1);
		}

		public void TestElementListWithUnknownElementType()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(TestObj))["PropertyWithListOfNoElementType"];
			MetaDataType metaDataType = MetaDataType.GetMetaDataType(
				MockMetaDataType.TestElementListMemberType);

			string validationResults =
				metaDataType.ValidateMetaDataValue(typeof(TestObj), property, "DoesntMatter");
			Assert("Should have 'unknown element type' to report", validationResults.IndexOf("not determine") != -1);
		}

		public void TestElementListWithWrongDataMemberPropertyType()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(TestObj))["PropertyWithList"];
			MetaDataType metaDataType = MetaDataType.GetMetaDataType(
				MockMetaDataType.TestElementListMemberType);

			string validationResults =
				metaDataType.ValidateMetaDataValue(typeof(TestObj), property, "ElementPropertyWithWrongType");
			Assert("Should have 'wrong data member type' to report", validationResults.IndexOf("not of correct type") != -1);
		}

		public void TestOkayElementListMember()
		{
			PropertyDescriptor property = TypeDescriptor.GetProperties(typeof(TestObj))["PropertyWithList"];
			MetaDataType metaDataType = MetaDataType.GetMetaDataType(
				MockMetaDataType.TestElementListMemberType);

			string validationResults =
				metaDataType.ValidateMetaDataValue(typeof(TestObj), property, "ElementProperty");
			AssertNull("Should have no errors to report", validationResults);
		}
	}

	class TestObj
	{
		[List("List")]
		public int PropertyWithList
		{ get { return 0; } }

		[List("ListOfNoElementType")]
		public int PropertyWithListOfNoElementType
		{ get { return 0; } }

		public TestCollection List
		{ get { return null; } }

		public IList ListOfNoElementType
		{ get { return null; } }
	}

	class TestCollection : CollectionBase
	{
		public TestCollectionElement this[int i]
		{ get { return null; } }
	}

	class TestCollectionElement
	{
		public int ElementProperty
		{ get { return 0; } }

		public string ElementPropertyWithWrongType
		{ get { return null; } }
	}
}
#endif
