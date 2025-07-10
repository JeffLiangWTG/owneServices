using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	[TestedType(typeof(CcsukGenralMessageForm))]
	class CcsukGenralMessageFormBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new CcsukGenralMessageForm(Factory.New<GenralEdiMessage>());
		}

		public void TestReply()
		{
			var inboundInterchange = EDIInterchange.CreateNewInterchangeFromString(Factory, "UNB+UNOA:2+CUKAIR98LHRCWE:IATA+CUKFFW98000CAR:IATA+120511:1108+944'UNH+950+GENRAL:0:912:UN+14A5F03D618F4FD19195B677DC248663'BGM+TXT:ZZZ'MSG+USER'FTX+AAA+++WIS TO CAR'UNT+5+950'UNZ+1+944'");
			Factory.Save();
			var genral = Factory.LoadTop1<GenralEdiMessage>(new ZQuery());
			genral.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			using (var form = new CcsukGenralMessageFormForTest(genral))
			{
				var menu = form.ActionsMenuItemExposed;
				AssertNotContains("Reply", menu.MenuItems[menu.MenuItems.Count - 1].Text);
			}
			genral.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			using (var form = new CcsukGenralMessageFormForTest(genral))
			{
				var menu = form.ActionsMenuItemExposed;
				var replyMenu = menu.MenuItems[menu.MenuItems.Count - 1];
				AssertContains("Reply", replyMenu.Text);
				AssertEquals("No 'new' form yet shown", null, ZArchitecture.GUI.ZFormModaliser.LastFormShownDialogForTest);
				replyMenu.PerformClick();
				AssertType<CcsukGenralMessageFormForNew>("The 'new' form was shown upon clicking the menu button", ZArchitecture.GUI.ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestLicenceHitAgent()
		{
			RunTestLicenceHit("CUKFFW98000LXA", Environment.Env.Licence.AirCcsuk, LicenceLoginResponse.Granted);
		}

		public void TestLicenceHitShed()
		{
			RunTestLicenceHit("CUKAIR98LHRBAC", Environment.Env.Licence.AirCcsukShed, LicenceLoginResponse.Granted);
			AssertNotNull("Created message OK", Factory.LoadTop1<EDIMessage>(new ZQuery()));
		}

		void RunTestLicenceHit(string pima, LicenceCheckpoint checkpoint, LicenceLoginResponse? expectedResultAfterCreatingMessage)
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			var manager = new NewGenralMessageManager(npbo);
			using (var form = new CcsukGenralMessageFormForNew(manager))
			{
				form.Show();
				npbo.Airport = "LHR";
				npbo.ShedOrBadge = "SLS";
				npbo.SendingProfile = pima;
				AssertEquals(false, form.GetLicenceLoginResultForTest().HasValue);
				form.CreateButton.PerformClick();
				Assert(!expectedResultAfterCreatingMessage.HasValue || expectedResultAfterCreatingMessage == form.GetLicenceLoginResultForTest().Value);
			}
		}
	}

	class CcsukGenralMessageFormForTest : CcsukGenralMessageForm
	{
		public CcsukGenralMessageFormForTest(GenralEdiMessage genralEdiMessage)
			: base(genralEdiMessage)
		{ }

		public Menu ActionsMenuItemExposed
		{
			get { return base.ActionsMenuItem; }
		}
	}

	[TestedType(typeof(CcsukGenralMessageFormForNew))]
	class CcsukGenralMessageFormForNewBasher : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var npbo = new NonPersistentGenralEdiMessageForNew(Factory);
			var manager = new NewGenralMessageManager(npbo);
			return new CcsukGenralMessageFormForNew(manager);
		}
	}
}
