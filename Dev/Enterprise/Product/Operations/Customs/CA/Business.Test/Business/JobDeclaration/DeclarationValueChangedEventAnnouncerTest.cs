using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.CA.Business.Testing
{
	sealed class DeclarationValueChangedEventAnnouncerTest : TestCaseWithFactory
	{
		public void TestAnnounceValueChangedEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			int changedEventCalled = 0;

			using (var announcer = new DeclarationValueChangedAnnouncer(declaration))
			{
				announcer.OnValueChanged += new EventHandler(delegate
				{ changedEventCalled++; });

				declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
				AssertEquals("should have been incremented", 1, changedEventCalled);
			}
			changedEventCalled = 0;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			AssertEquals("should not have been incremented as event should be unhooked on disposal of the announcer", 0, changedEventCalled);
		}
	}
}
