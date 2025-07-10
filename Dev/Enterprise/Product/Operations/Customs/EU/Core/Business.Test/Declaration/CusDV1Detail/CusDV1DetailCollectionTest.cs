using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing.Declaration
{
	[TestedType(typeof(CusDV1DetailCollection))]
	class CusDV1DetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSequenceNumberForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = declaration.DV1Details;
			CombineAssertions(() =>
			{
				var item1 = collection.AddNew();
				AssertEquals("item1.Sequence", (ZShort)1, item1.Sequence);

				var item2 = collection.AddNew();
				AssertEquals("item2.Sequence", (ZShort)2, item2.Sequence);
				AssertEquals("item1.Sequence after adding item2", (ZShort)1, item1.Sequence);
			});
		}

		public void TestSequencesAreOrderedOnLoaded()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = declaration.DV1Details;
			collection.AddNew();
			collection.AddNew();
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedCollection = newFactory.Load<JobDeclaration>(declaration.PK).DV1Details;
			AssertArrayEqualsByElements("Sequences are always ordered regardless of BOs", new ZShort[] { 1, 2 }, reloadedCollection.Cast<CusDV1Detail>().Select(x => x.Sequence).ToArray());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new CusDV1DetailCollection(declaration);
		}
	}
}
