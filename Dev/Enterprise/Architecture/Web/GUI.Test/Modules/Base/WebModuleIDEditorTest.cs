using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.Internal.Testing
{
	sealed class WebModuleIDEditorTest : TestCase
	{
		public void TestGetModuleIDs()
		{
			AssertEquals(WebModuleIDs.All, new WebModuleIDEditor().GetModuleIDsInternal());
		}
	}
}
