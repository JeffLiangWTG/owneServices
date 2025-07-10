using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
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

				declaration.ZA_CustShipNoOverride_Hidden = true;
				AssertEquals("should have been incremented", 1, changedEventCalled);

				declaration.JE_ToOrder = true;
				AssertEquals("should have been incremented", 2, changedEventCalled);
				declaration.ZA_CustShipNoOverride_Hidden = false;
				declaration.JE_ToOrder = false;
			}
			changedEventCalled = 0;
			declaration.ZA_CustShipNoOverride_Hidden = true;
			AssertEquals("should not have been incremented as event should be unhooked on disposal of the announcer", 0, changedEventCalled);

			declaration.JE_ToOrder = true;
			AssertEquals("should not have been incremented as event should be unhooked on disposal of the announcer", 0, changedEventCalled);
		}
	}
}
