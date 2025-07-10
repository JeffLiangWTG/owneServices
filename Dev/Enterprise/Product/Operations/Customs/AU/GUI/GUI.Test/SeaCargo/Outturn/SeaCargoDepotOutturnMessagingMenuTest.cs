using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	sealed class SeaCargoDepotOutturnMessagingMenuTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestConstructor()
		{
			var header = CusOutturnHeader.New(Factory);
			var manager = new CusOutturnHeaderMessageManager(header);
			using (var menu = new SeaCargoDepotOutturnMessagingMenu(manager))
			{
				AssertNotNull(menu);
			}
		}
	}
}
