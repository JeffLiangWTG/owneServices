using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Ccsuk.Testing
{
	[TestedType(typeof(StandAloneFsrEnquiryForm))]
	class StandAloneFsrEnquiryTests : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var messageInDatabase = Factory.New<StandAloneFsrEnquiry>();
			messageInDatabase.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			messageInDatabase.EM_ReceiveTransmit = "RCV"; // just to make it shut the hell up
			Factory.Save();
			messageInDatabase.EM_ReceiveTransmit = "TRX";
			Factory.Save();
			messageInDatabase.Reload();
			return new StandAloneFsrEnquiryForm(messageInDatabase);
		}

		public void TestRequeryOption()
		{
			var standAloneFsrEnquiry = Factory.New<StandAloneFsrEnquiry>();
			using (var form = new StandAloneFsrEnquiryFormForTest(standAloneFsrEnquiry))
			{
				var menu = form.ActionsMenuItemExposed;
				AssertNotContains("Query", menu.MenuItems[menu.MenuItems.Count - 1].Text);
			}
			var response = Factory.New<EDIMessage>();
			standAloneFsrEnquiry.EM_LinkedObject = response;
			using (var form = new StandAloneFsrEnquiryFormForTest(standAloneFsrEnquiry))
			{
				var menu = form.ActionsMenuItemExposed;
				var reQueryMenu = menu.MenuItems[menu.MenuItems.Count - 1];
				AssertContains("Query", reQueryMenu.Text);
				reQueryMenu.PerformClick();
				AssertEquals("The 'new' form was shown upon clicking the menu button", "StandAloneFsrEnquiryFormForNew", Enterprise.ZArchitecture.GUI.ZFormModaliser.LastFormShownDialogForTest.Name);
			}
		}

		class StandAloneFsrEnquiryFormForTest : StandAloneFsrEnquiryForm
		{
			public StandAloneFsrEnquiryFormForTest(StandAloneFsrEnquiry ediMessage)
				: base(ediMessage)
			{ }

			public Menu ActionsMenuItemExposed
			{
				get { return base.ActionsMenuItem; }
			}
		}
	}

	[TestedType(typeof(StandAloneFsrEnquiryFormForNew))]
	class StandAloneFsrEnquiryFormForNewTests : ZFormBasherTest
	{
		public void TestLicenceHitShedThenAgent()
		{
			var factory = new BusinessObjectFactory();
			RunTestLicenceHit("CUKAIR98LHRBAC", Environment.Env.Licence.AirCcsukShed, LicenceLoginResponse.Granted, factory);
			AssertNotNull("Created message OK", factory.LoadTop1<EDIMessage>(new ZQuery()));

			factory = new BusinessObjectFactory();
			RunTestLicenceHit("CUKFFW98000LXA", Environment.Env.Licence.AirCcsuk, LicenceLoginResponse.Granted, factory);
			AssertNotNull("Created message OK", factory.LoadTop1<EDIMessage>(new ZQuery()));
		}

		void RunTestLicenceHit(string pima, LicenceCheckpoint checkpoint, LicenceLoginResponse? expectedResultAfterCreatingMessage, BusinessObjectFactory factory)
		{
			LicencingAndShedRestrictionsTests.MakeBadgeAndCredentials(false);
			var npbo = new NonPersistentStandAloneFsrEnquiryForNew(factory);
			var manager = new NewStandAloneFsrEnquiryManager(npbo);
			using (var form = new StandAloneFsrEnquiryFormForNew(manager))
			{
				form.Show();
				npbo.PIMA = pima;
				npbo.MAWB = "11122222222";
				AssertEquals(false, form.GetLicenceLoginResultForTest().HasValue);
				form.CreateButton.PerformClick();
				Assert(!expectedResultAfterCreatingMessage.HasValue || expectedResultAfterCreatingMessage == form.GetLicenceLoginResultForTest().Value);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new StandAloneFsrEnquiryFormForNew(new NewStandAloneFsrEnquiryManager(new NonPersistentStandAloneFsrEnquiryForNew(Factory)));
		}
	}
}
