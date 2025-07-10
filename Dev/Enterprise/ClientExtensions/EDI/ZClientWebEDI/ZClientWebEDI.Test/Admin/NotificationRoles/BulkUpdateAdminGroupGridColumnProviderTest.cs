using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(BulkUpdateAdminGroupGridColumnProvider))]
	public class BulkUpdateAdminGroupGridColumnProviderTest : GridColumnProviderBaseTest
	{
		protected override GridColumnProvider GetNewTestProvider() => new BulkUpdateAdminGroupGridColumnProvider();
	}
}
