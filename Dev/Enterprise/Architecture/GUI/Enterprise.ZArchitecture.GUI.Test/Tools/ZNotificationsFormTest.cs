using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.DevTools.Testing
{
	class ZNotificationsFormTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestFormLoads()
		{
			using (var form = new ZNotificationsForm(Factory.New<DummyBusinessObject>()))
			{
				form.Show();
			}
		}
	}
}