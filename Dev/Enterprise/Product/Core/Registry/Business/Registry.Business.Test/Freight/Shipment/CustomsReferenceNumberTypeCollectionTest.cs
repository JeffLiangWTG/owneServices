using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CustomsReferenceNumberTypeCollection))]
	sealed class CustomsReferenceNumberTypeCollectionTest : RegistryBusinessObjectCollectionTestCase<CustomsReferenceNumberTypeCollection>
	{
		public void TestAddSetsParent()
		{
			CustomsReferenceNumberTypeCollection collection = new CustomsReferenceNumberTypeCollection();
			CustomsReferenceNumberType customsReferenceNumberType1 = collection.AddNew();

			AssertEquals(collection, customsReferenceNumberType1.Parent);

			CustomsReferenceNumberType customsReferenceNumberType2 = new CustomsReferenceNumberType();
			collection.Add(customsReferenceNumberType2);

			AssertEquals(collection, customsReferenceNumberType1.Parent);
		}

		public void TestAddSystemDefined()
		{
			var collection = new CustomsReferenceNumberTypeCollection();
			var customsReferenceNumberType1 = collection.AddSystemDefined("A", (NoResString)"B", isUnique: true);

			AssertEquals("Code", "A", customsReferenceNumberType1.Code);
			AssertEquals("Description", "B", customsReferenceNumberType1.Description);
			Assert("IsUnique", customsReferenceNumberType1.IsUnique);
			AssertEquals("IsAutomation", false, customsReferenceNumberType1.IsAutomation);
			Assert("SystemDefined", customsReferenceNumberType1.SystemDefined);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override CustomsReferenceNumberTypeCollection GetCollectionToTest()
		{
			return new CustomsReferenceNumberTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CustomsReferenceNumberType();
		}
	}
}
