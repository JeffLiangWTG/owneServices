using Enterprise.ZArchitecture.Web.Security.Configuration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Security.Testing
{
	public abstract class SecureWebPageItemSettingCollectionTest : TestCase
	{
		public void TestIndexOf()
		{
			SecureWebPageItemSettingCollection collection = GetNewCollection();
			AssertEquals("No settings", 0, collection.Count);

			AssertEquals("Index of Null", -1, collection.IndexOf((SecureWebPageItemSetting)null));
			SecureWebPageItemSetting newSetting = GetNewSetting();
			AssertEquals("Setting not found", -1, collection.IndexOf(newSetting));
			AssertEquals("Setting not found by path", -1, collection.IndexOf(newSetting.Path));
		}

		#region Implementation

		protected abstract SecureWebPageItemSettingCollection GetNewCollection();
		protected abstract SecureWebPageItemSetting GetNewSetting();

		#endregion
	}
}
