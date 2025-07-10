using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(LocalPartNumberCollection))]
	sealed class LocalPartNumberCollectionTest : ActiveBusinessObjectCollectionTestCase<LocalPartNumberCollection>
	{
		public override void TestAddNew()
		{
			base.TestAddNew();
			var goodsCatalogs = Factory.New<CusGoodsCatalog>();
			var collection = new LocalPartNumberCollection(goodsCatalogs);
			var goodsCatalogProductionInfo = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CGI_Type", CusGoodsCatalogProductionInfoTypeList.Codes.LPN, goodsCatalogProductionInfo.CGI_Type);
				AssertEquals("CGI_CGC_Catalog", goodsCatalogs.PK, goodsCatalogProductionInfo.CGI_CGC_Catalog);
			});
		}

		public void TestAllowNew()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var collection = new LocalPartNumberCollectionForTesting(goodsCatalog);
			Assert(!collection.AllowNew);
		}

		public void TestFind()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var collection = new LocalPartNumberCollection(goodsCatalog);
			var partNumber1 = collection.AddNew();
			partNumber1.CGI_Reference = "1234";
			var partNumber2 = collection.AddNew();
			partNumber2.CGI_Reference = "5678";
			AssertContainsExactElementsInAnyOrder(new[] { partNumber1 }, collection.Find("1234"));
			AssertContainsExactElementsInAnyOrder(new[] { partNumber2 }, collection.Find("5678"));

			partNumber2.CGI_Reference = "1234";
			AssertContainsExactElementsInAnyOrder(new[] { partNumber1, partNumber2 }, collection.Find("1234"));

			Assert(!collection.Find("123").Any());
		}

		public void TestAddLocalPartNumberIfNotExists()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var collection = new LocalPartNumberCollection(goodsCatalog);

			collection.AddLocalPartNumberIfNotExists("1234");
			AssertEquals(1, collection.Count);
			AssertEquals("1234", collection[0].CGI_Reference);

			collection.AddLocalPartNumberIfNotExists("1234");
			AssertEquals(1, collection.Count);

			collection.AddLocalPartNumberIfNotExists("5678");
			AssertEquals(2, collection.Count);
			AssertEquals("1234", collection[0].CGI_Reference);
			AssertEquals("5678", collection[1].CGI_Reference);
		}

		public void TestRemoveLocalPartNumberIfExists()
		{
			var goodsCatalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var collection = new LocalPartNumberCollection(goodsCatalog);

			collection.AddLocalPartNumberIfNotExists("1234");
			collection.AddLocalPartNumberIfNotExists("5678");
			AssertEquals(2, collection.Count);
			AssertEquals("1234", collection[0].CGI_Reference);
			AssertEquals("5678", collection[1].CGI_Reference);

			collection.RemoveLocalPartNumberIfExists("9876");
			AssertEquals(2, collection.Count);

			collection.RemoveLocalPartNumberIfExists("1234");
			AssertEquals(1, collection.Count);
			AssertEquals("5678", collection[0].CGI_Reference);
		}

		public void TestUpdateLocalPartNumber()
		{
			var catalog = Factory.NewWithValidTestData<CusGoodsCatalog>();
			var collection = new LocalPartNumberCollection(catalog);

			collection.UpdateLocalPartNumber(ZString.Empty, "1234");
			collection.UpdateLocalPartNumber(ZString.Empty, "5678");

			var localPartNumber1 = collection[0];
			var localPartNumber2 = collection[1];
			AssertLocalPartNumbers("Two new Local Part Numbers added", collection, new[] { "1234", "5678" });
			AssertNull("CID event should not be added when add new Local Part Number", catalog.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier));

			collection.UpdateLocalPartNumber("9876", "0123");
			collection.UpdateLocalPartNumber("5678", "5555");
			AssertLocalPartNumbers("One Local Part Number updated", collection, new[] { "1234", "5555" });
			Assert("Local Part Number 1 should not be deleted", !localPartNumber1.IsDeleted);
			Assert("Local Part Number 2 should not be deleted", !localPartNumber2.IsDeleted);
			AssertNull("CID event should not be added when AuthorityIdentifier is empty", catalog.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier));

			catalog.CGC_AuthorityIdentifier = "1";
			collection.UpdateLocalPartNumber("1234", "0123");
			AssertLocalPartNumbers("One Local Part Number updated", collection, new[] { "0123", "5555" });
			Assert("Local Part Number 1 should not be deleted", !localPartNumber1.IsDeleted);
			Assert("Local Part Number 2 should not be deleted", !localPartNumber2.IsDeleted);
			AssertEquals("|DES=A new Local Part Number was added because the Product Code was changed from 1234 to 0123.", catalog.Logs.MostRecentLogByPostedTime(Events.ChangeOfIdentifier).SL_Reference);

			collection.UpdateLocalPartNumber("0123", ZString.Empty);
			AssertLocalPartNumbers("One Local Part Number updated", collection, new[] { "5555" });
			Assert("Local Part Number 1 should not be deleted", localPartNumber1.IsDeleted);

			void AssertLocalPartNumbers(string message, LocalPartNumberCollection collection, params string[] localPartNumbers)
			{
				AssertContainsExactElementsInExactOrder(message, localPartNumbers, collection.Select(x => x.CGI_Reference));
			}
		}

		protected override LocalPartNumberCollection GetCollectionToTest()
		{
			var goodsCatalogs = Factory.New<CusGoodsCatalog>();
			return goodsCatalogs.LocalPartNumbers;
		}

		class LocalPartNumberCollectionForTesting : LocalPartNumberCollection
		{
			public LocalPartNumberCollectionForTesting(CusGoodsCatalog parent) : base(parent)
			{
			}

			public new bool AllowNew => base.AllowNew;
		}
	}
}
