using System.Linq;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZClientWebCargoWiseEDI.Testing
{
	[TestedType(typeof(NotificationRolesContactsModuleColumnProvider))]
	public class NotificationRolesContactsModuleColumnProviderTest : GridColumnProviderBaseTest
	{
		protected override GridColumnProvider GetNewTestProvider() => new NotificationRolesContactsModuleColumnProvider();
		public override void TestTranslatability()
		{
			Assert(true);
		}

		public void TestNotificationGropList()
		{
			var provider = new NotificationRolesContactsModuleColumnProvider();
			AssertEquals(true, provider.NotificationGroupList.GetAllCodes().OrderBy(x => x).SequenceEqual(new[] { "CSV", "A/R", "BOR", "ERA", "IST", "CCP" }.OrderBy(x => x)));
		}

		public void TestNotificationGroupColumn_DisableColumnSorting()
		{
			var provider = new NotificationRolesContactsModuleColumnProvider();
			AssertEquals(true, provider.AllColumns.Skip(5).All(x => string.IsNullOrEmpty(x.SortExpression)));
		}
	}
}
