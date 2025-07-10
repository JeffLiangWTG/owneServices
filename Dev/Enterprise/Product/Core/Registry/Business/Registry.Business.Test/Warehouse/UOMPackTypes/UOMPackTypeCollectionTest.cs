using System.Linq;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(UOMPackTypeCollection))]
	sealed class UOMPackTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<UOMPackTypeCollection>
	{
		#region AllowNew

		public void TestAllowNewAndRemoveFromCollection()
		{
			var collection = UOMPackTypeCollection.GetDefault();
			AssertEquals(3, collection.Count);

			AssertEquals(false, collection.AllowNew);
			AssertEquals(false, collection.AllowRemove);
		}

		#endregion

		#region TestGetDefault

		public void TestGetDefault()
		{
			var defaultValues = UOMPackTypeCollection.GetDefault().Cast<UOMPackType>().OrderBy(l => l.Code).ToArray();
			AssertPackType(defaultValues[0], UOMPackTypesList.Codes.Case, UOMPackTypesList.Descriptions.Case, 1);
			AssertPackType(defaultValues[1], UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Descriptions.Pallet, 1);
			AssertPackType(defaultValues[2], UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Descriptions.SplitCase, 1);
		}

		void AssertPackType(UOMPackType packType, string expectedCode, string expectedDescription, int expectedNumberOfLabels)
		{
			AssertEquals(expectedCode, packType.Code);
			AssertEquals(expectedDescription, packType.Description);
			AssertEquals(expectedNumberOfLabels, packType.NumberOfLabels);
		}

		#endregion

		#region Implmentation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override UOMPackTypeCollection GetCollectionToTest()
		{
			return new UOMPackTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new UOMPackType();
		}

		#endregion
	}
}
