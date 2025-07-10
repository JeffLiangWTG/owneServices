using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(WebSecurityGridColumnProviders.SecurityGridColumnProvider))]
	public class WebSecuritySecurityGridColumnProviderTest : GridColumnProviderBaseTest
	{
		protected override GridColumnProvider GetNewTestProvider() => new WebSecurityGridColumnProviders.SecurityGridColumnProvider();
	}
}
