using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Testing
{
	[TestedType(typeof(MyAccountHostingSiteLandingPageUrlRegistryItem))]
	public class MyAccountHostingSiteLandingPageUrlRegistryItemTest : StronglyTypedRegistryItemTestCaseWithFactory<MyAccountHostingSiteLandingPageUrlCollection>
	{
		protected override StronglyTypedRegistryItem<MyAccountHostingSiteLandingPageUrlCollection, MyAccountHostingSiteLandingPageUrlCollection> GetNewRegistryItem()
		{
			return new MyAccountHostingSiteLandingPageUrlRegistryItem("", null, null, null, new MyAccountHostingSiteLandingPageUrlCollection());
		}
	}
}
