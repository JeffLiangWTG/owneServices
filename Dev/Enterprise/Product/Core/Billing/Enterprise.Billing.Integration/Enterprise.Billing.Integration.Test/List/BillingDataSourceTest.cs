using System;
using NUnit.Framework;

namespace Enterprise.Billing.Integration.Test
{
	public class BillingDataSourceTest : TestCase
	{
		public void TestAllBillingDataSourceValid()
		{
			Type type = typeof(BillingDataSource);
			foreach (var p in type.GetFields())
			{
				var databaseValue = p.GetValue(null).ToString();
				Assert(!String.IsNullOrEmpty(databaseValue));
			}
		}
	}
}
