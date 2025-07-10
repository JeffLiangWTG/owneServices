using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

sealed class DeclarationValueChangedAnnouncerTest : TestCaseWithFactory
{
	public void TestAnnounceValueChangedEvent()
	{
		var declaration = Factory.New<JobDeclaration>();
		int changedEventCalled = 0;

		using (var announcer = new DeclarationValueChangedAnnouncer(declaration))
		{
			announcer.OnValueChanged += new EventHandler(delegate
			{ changedEventCalled++; });

			declaration.MessageVersion = "XML";
			AssertEquals("should have been incremented", 1, changedEventCalled);
		}
	}
}
