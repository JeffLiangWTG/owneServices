using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(FaxPriceCollection))]
	internal sealed class FaxPriceCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<FaxPriceCollection>
	{
		public new void TestAddNew()
		{
			FaxPriceCollection collection = new FaxPriceCollection();
			FaxPrice item = collection.AddNew();

			AssertEquals(1, collection.Count);
			AssertEquals(item, collection[0]);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override FaxPriceCollection GetCollectionToTest()
		{
			return new FaxPriceCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new FaxPrice();
		}

		#endregion
	}
}
