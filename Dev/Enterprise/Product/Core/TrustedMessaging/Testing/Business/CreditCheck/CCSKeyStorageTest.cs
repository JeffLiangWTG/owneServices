using CargoWise.EntityFramework.Testing;
using Enterprise.TrustedMessaging.Business.CreditCheck;
using WTG.TrustedMessaging.Models;

namespace Enterprise.TrustedMessaging.Testing.Business.CreditCheck
{
	public class CCSKeyStorageTest : TestCaseWithFactory
	{
		public void TestSaveAndLoadSecretKey()
		{
			var keyStorage = new CCSKeyStorage();
			var key = new SecretKey() { Product = "product", RefId = "refId", Key = "key" };
			keyStorage.SaveSecretKey(key);

			var result = keyStorage.LoadSecretKey("product", "refId");
			AssertEquals("product", result.Product);
			AssertEquals("refId", result.RefId);
			AssertEquals("key", result.Key);

			result = keyStorage.LoadSecretKey("productNotExisted", "refIdNotExisted");
			AssertEquals("productNotExisted", result.Product);
			AssertEquals("refIdNotExisted", result.RefId);
			AssertNull(result.Key);
		}
	}
}
