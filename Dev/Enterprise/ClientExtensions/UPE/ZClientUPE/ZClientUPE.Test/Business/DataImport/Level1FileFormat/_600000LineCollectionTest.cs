using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _600000LineCollectionTest : TestCase
	{
		public void TestIndexor()
		{
			_600000LineCollection.Add(new _600000Line("US2795AU9639040422              DE01E70GJRGB60000065744051475    5  LBS         USD         USD          USD                             C0000051867QF12            NN1ZE01E706643404080                 5      AKE35125QF                                                                                                                                                                "));
			AssertEquals("65744051475", _600000LineCollection[0].ShortTrackingNumber);
		}

		public void TestAdd()
		{
			_600000LineCollection.Add(new _600000Line("US2795AU9639040422              DE01E70GJRGB60000065744051475    5  LBS         USD         USD          USD                             C0000051867QF12            NN1ZE01E706643404080                 5      AKE35125QF                                                                                                                                                                "));
			AssertEquals(1, _600000LineCollection.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_600000LineCollection = new _600000LineCollection();
		}

		_600000LineCollection _600000LineCollection;
	}
}
