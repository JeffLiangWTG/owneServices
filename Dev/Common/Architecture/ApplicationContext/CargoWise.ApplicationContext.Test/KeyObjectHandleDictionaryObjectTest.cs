using System.Collections;
using NUnit.Framework;

namespace CargoWise.Application.Testing
{
	class KeyObjectHandleDictionaryObjectTest : TestCase
	{
		public void TestCreateInstance()
		{
			Hashtable dictionary = (Hashtable)ObjectFactory.Get("TestKeyTypeDictionary");
			AssertEquals(typeof(AuthorizedInterfaceReference), dictionary["Key"]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Configure(IocConfigurationTests.TestConfigurationLocation);
		}

		protected override void TearDown()
		{
			ObjectFactory.Unconfigure(IocConfigurationTests.TestConfigurationLocation);
		}
	}
}
