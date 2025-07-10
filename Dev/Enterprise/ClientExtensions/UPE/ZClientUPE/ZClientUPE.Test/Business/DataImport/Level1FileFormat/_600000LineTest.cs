using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _600000LineTest : TestCase
	{
		public void TestShortTrackingNumber()
		{
			AssertEquals("65744051475", _600000Line.ShortTrackingNumber);
		}

		public void TestLongTrackingNumber()
		{
			AssertEquals("1ZE01E706643404080", _600000Line.LongTrackingNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_600000Line = new _600000Line("US2795AU9639040422              DE01E70GJRGB60000065744051475    5  LBS         USD         USD          USD                             C0000051867QF12            NN1ZE01E706643404080                 5      AKE35125QF                                                                                                                                                                ");
		}

		_600000Line _600000Line;
	}
}
