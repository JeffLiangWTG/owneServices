using System.Collections.Specialized;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	public class SecureURLHashProviderTest : TestCase
	{
		public void TestGetSecurityHash()
		{
			var items = new NameValueCollection();
			items.Add("item1", "zubin");
			items.Add("item2", "rakhsh");
			items.Add("item3", "zay");
			items.Add("item4", "wrong");
			items.Add("item5", "lola");
			var result = SecureURLHashProvider.GetSecurityHash(items, "item1", "item2", "item3", "item5");
			AssertEquals("+XbFndlnKpEA/3H9UmHLaRHWQjZXeUktC", result);
			result = SecureURLHashProvider.GetSecurityHash(items, "item1");
			AssertEquals("+QJpJtR4qJOYtdN6rzwo+aEvSWeOem7O9", result);
		}

		public void TestCreateDataSecuredBySecurityHash()
		{
			var items = new NameValueCollection();
			items.Add("item1", "zubin");
			items.Add("item2", "rakhsh");
			items.Add("item3", "zay");
			items.Add("item4", "wrong");
			items.Add("item5", "lola");
			var result = SecureURLHashProvider.CreateDataSecuredBySecurityHash(items, "item1", "item2", "item3", "item5");
			AssertEquals("item1=zubin&item2=rakhsh&item3=zay&item5=lola&", result);
			result = SecureURLHashProvider.CreateDataSecuredBySecurityHash(items, "item1");
			AssertEquals("item1=zubin&", result);
		}
	}
}