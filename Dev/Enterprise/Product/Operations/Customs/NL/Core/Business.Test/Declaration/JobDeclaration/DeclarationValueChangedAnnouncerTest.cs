using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

class DeclarationValueChangedAnnouncerTest : TestCaseWithFactory
{
	public void TestAnnounceValueChangedEvent()
	{
		var declaration = Factory.New<JobDeclaration>();
		int changedEventCalled = 0;

		using (var announcer = new DeclarationValueChangedAnnouncer(declaration))
		{
			announcer.OnValueChanged += new EventHandler(delegate
			{
				changedEventCalled++;
			});

			declaration.JE_TransportModeInland = TransportTypeList.Codes.Mail;
			AssertEquals("should have been incremented", 1, changedEventCalled);
		}

		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		AssertEquals("Announcer is disposed and counter should stay same", 1, changedEventCalled);
	}
}
