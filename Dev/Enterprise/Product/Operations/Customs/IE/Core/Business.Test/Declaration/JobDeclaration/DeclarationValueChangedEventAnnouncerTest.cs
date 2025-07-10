using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.Testing
{
	public class DeclarationValueChangedEventAnnouncerTest : TestCaseWithFactory
	{
		public void TestAnnounceValueChangedEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			int changedEventCalled = 0;

			using (var announcer = new DeclarationValueChangedAnnouncer(declaration))
			{
				announcer.OnValueChanged += new EventHandler(delegate
				{ changedEventCalled++; });

				declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._80;
				AssertEquals("should have been incremented", 1, changedEventCalled);
			}
			changedEventCalled = 0;
			declaration.ZG_BorderTransportMeans = ExportBorderTransportMeansList.Codes._81;
			AssertEquals("should not have been incremented as event should be unhooked on disposal of the announcer", 0, changedEventCalled);
		}
	}
}
