using System;
using Enterprise.DocumentVisualizer.Business;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class CustomFindBoxMetaDataTest : TestCase
	{
		public void TestIdentifier()
		{
			var metadata = new CustomFindBoxMetaData(() => null);
			AssertEquals("Identifier is the same as instance Id", CustomFindBoxMetaData.Identifier, metadata.Id);
		}

		public void TestCustomFindBoxProvider()
		{
			Func<object> provider = () => null;

			var metadata = new CustomFindBoxMetaData(provider);
			AssertEquals("CustomFindBoxProvider", provider, metadata.CustomFindBoxProvider);
			AssertEquals("CustomFindBoxProvider is same as Value", metadata.CustomFindBoxProvider, metadata.Value);
			Assert("CustomFindBoxProvider ShowDescription is true by default", metadata.ShowDescription);

			metadata = new CustomFindBoxMetaData(provider, false);
			Assert("CustomFindBoxProvider ShowDescription is false", !metadata.ShowDescription);
		}
	}
}
