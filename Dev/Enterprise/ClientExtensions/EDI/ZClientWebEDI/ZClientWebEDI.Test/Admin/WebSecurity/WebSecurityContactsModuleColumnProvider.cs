using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(WebSecurityGridColumnProviders.WebSecurityContactsModuleColumnProvider))]
	public class WebSecurityContactsModuleColumnProvider : GridColumnProviderBaseTest
	{
		protected override GridColumnProvider GetNewTestProvider() => new WebSecurityGridColumnProviders.WebSecurityContactsModuleColumnProvider();
	}
}
