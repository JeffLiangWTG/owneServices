using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _500000LineCollectionTest : TestCase
	{
		public void TestIndexor()
		{
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5000001   EA 40' COAX CABLE (WIRING)                                                                                 4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("40' COAX CABLE (WIRING)", _500000LineCollection[0].Description);
		}

		public void TestAdd()
		{
			_500000LineCollection.Add(new _500000Line("US2795AU9639040422              DE01E70GJRGB60000065744051475    5  LBS         USD         USD          USD                             C0000051867QF12            NN1ZE01E706643404080                 5      AKE35125QF                                                                                                                                                                "));
			AssertEquals(1, _500000LineCollection.Count);
		}

		public void TestAddRange()
		{
			_500000LineCollection range = new _500000LineCollection();
			range.Add(new _500000Line("US2795AU9639040422              DE01E70GJRGB60000065744051475    5  LBS         USD         USD          USD                             C0000051867QF12            NN1ZE01E706643404080                 5      AKE35125QF                                                                                                                                                                "));
			range.Add(new _500000Line("US2795AU9639040422              DE01E70GJRGB60000065744051475    5  LBS         USD         USD          USD                             C0000051867QF12            NN1ZE01E706643404080                 5      AKE35125QF                                                                                                                                                                "));
			_500000LineCollection.AddRange(range);
			AssertEquals(2, _500000LineCollection.Count);
		}

		public void TestGoodsDescription()
		{
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5000001                                                                                                              4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA 40' COAX CABLE (WIRING)                                                                                 4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("40' COAX CABLE (WIRING)", _500000LineCollection.GoodsDescription);
		}

		public void TestGoodsDescription_RemoveSpecificWords()
		{
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA sAmPle OF something                                                                                     4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("OF SOMETHING", _500000LineCollection.GoodsDescription);
			_500000LineCollection = new _500000LineCollection();
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA sAmPles OF something                                                                                    4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("OF SOMETHING", _500000LineCollection.GoodsDescription);
			_500000LineCollection = new _500000LineCollection();
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA sAmPles OF sample                                                                                       4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("OF", _500000LineCollection.GoodsDescription);
			_500000LineCollection = new _500000LineCollection();
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA sAmPles NCV sample                                                                                      4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("", _500000LineCollection.GoodsDescription);
			_500000LineCollection = new _500000LineCollection();
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA consolidation of goods                                                                                  4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("CONSOLIDATION OF GOODS", _500000LineCollection.GoodsDescription);
			_500000LineCollection = new _500000LineCollection();
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA consol of goods                                                                                         4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("OF GOODS", _500000LineCollection.GoodsDescription);
			_500000LineCollection = new _500000LineCollection();
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA something consolidated                                                                                  4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("SOMETHING CONSOLIDATED", _500000LineCollection.GoodsDescription);
			_500000LineCollection = new _500000LineCollection();
			_500000LineCollection.Add(new _500000Line("CA1399AU9639050704              DAA20618HSNR5010001   EA something consol-idated                                                                                 4000      CAD17515               CA8544300000                           COAXT-40                                                                                                                                         "));
			AssertEquals("SOMETHING -IDATED", _500000LineCollection.GoodsDescription);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_500000LineCollection = new _500000LineCollection();
		}

		_500000LineCollection _500000LineCollection;
	}
}
