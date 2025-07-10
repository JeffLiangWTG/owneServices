using System;
using CargoWise.Application;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Client.Test
{
	public class PAVEHttpClientFactoryTest : TransactionedTestCase
	{
		public void TestShouldCreateHttpClient_WithGlowBaseUri()
		{
			var baseGlowServiceRootUrl = "https://bla/doIt";
			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, baseGlowServiceRootUrl);
			var paveHttpClientFactory = ObjectFactory.Get<IPAVEHttpClientFactory>();
			var paveHttpClient = paveHttpClientFactory.Create();

			AssertType<PAVEHttpClientFactory>(paveHttpClientFactory);
			AssertType<PAVEHttpClient>(paveHttpClient);
			AssertEquals(baseGlowServiceRootUrl + "/", PAVEHttpClientFactory.BaseUri);
		}

		public void TestShouldNotCreateHttpClient_WhenNoGlowBaseUri()
		{
			var paveHttpClientFactory = ObjectFactory.Get<IPAVEHttpClientFactory>();
			var paveHttpClient = paveHttpClientFactory.Create();

			AssertType<PAVEHttpClientFactory>(paveHttpClientFactory);
			AssertNull(paveHttpClient);

			GlowRegistry.Instance.GlowServiceUriRegistryItem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, string.Empty);
			paveHttpClient = paveHttpClientFactory.Create();

			AssertNull(paveHttpClient);
		}
	}
}
