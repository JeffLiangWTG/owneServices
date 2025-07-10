using Enterprise.Registry.Business.Web.WebAPI;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing.Web.WebAPI
{
	[TestedType(typeof(WebAPIRegistry))]
	sealed class WebAPIRegistryTest : RegistryItemSetTestCase<WebAPIRegistry>
	{
	}
}
