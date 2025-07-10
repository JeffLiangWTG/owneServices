using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	public class BALProviderTest : TestCaseWithFactory
	{
		BALProvider provider;

		public void TestTimestamp()
		{
			provider = new BALProvider(new BALMessage { Timestamp = "1660211772103" });
			AssertEquals("1660211772103", provider.Timestamp);
		}

		public void TestTotal()
		{
			provider = new BALProvider(new BALMessage { Total = "11061096.00" });
			AssertEquals("11061096.00", provider.Total);
		}
		public void TestTotal_XlsxField() => typeof(BALProvider).TestXlsxField(nameof(BALProvider.Total), 2, "Total");

		public void TestCash()
		{
			provider = new BALProvider(new BALMessage { Cash = "9361096.00" });
			AssertEquals("9361096.00", provider.Cash);
		}
		public void TestCash_XlsxField() => typeof(BALProvider).TestXlsxField(nameof(BALProvider.Cash), 3, "Cash");

		public void TestDeferred()
		{
			provider = new BALProvider(new BALMessage { Deferred = "1700000.00" });
			AssertEquals("1700000.00", provider.Deferred);
		}
		public void TestDeferred_XlsxField() => typeof(BALProvider).TestXlsxField(nameof(BALProvider.Deferred), 4, "Deferred");
	}
}
