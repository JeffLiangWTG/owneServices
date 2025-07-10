using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(WebSecurityGridColumnProviders.ContactGridColumnProvider))]
	public class WebSecurityContactGridColumnProviderTest : GridColumnProviderBaseTest
	{
		protected override GridColumnProvider GetNewTestProvider() => new WebSecurityGridColumnProviders.ContactGridColumnProvider();
	}
}
