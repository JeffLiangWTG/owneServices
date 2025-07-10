using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Business.Testing
{
	class GbDeclarationSenderChooserTest : TestCaseWithFactory
	{
		public void TestEdcsAlerts()
		{
			AddBadgeCodeToRegistry("NES", "", GatewayList.Codes.NES, "EXP");
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew().InvoiceLines.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.JE_CustomsProfile = "NES";
			var messageFunction = new CusdecMessageFunction.New();
			var shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			var sender = new GbDeclarationSenderChooserWithNoFinalSend();

			sender.Send(declaration, shutUp, messageFunction);
			AssertEquals(0, shutUp.PastYesNoQuestionsAsked.Count);

			GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddHours(-4).ToDateTime());
			GBCustomsDataRegistry.Instance.EdcsWtgAlertString.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "EDCS is dodgy");
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			shutUp.AnswerToContinueWithAction = true; // yes
			var canSend = sender.SendAndIndicateIfASenderWasSelected(declaration, shutUp, messageFunction);
			AssertContains("EDCS is dodgy", shutUp.PastYesNoQuestionsAsked[0]);
			AssertEquals(true, canSend);

			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			shutUp.AnswerToContinueWithAction = false; // no
			canSend = sender.SendAndIndicateIfASenderWasSelected(declaration, shutUp, messageFunction);
			AssertContains("EDCS is dodgy", shutUp.PastYesNoQuestionsAsked[0]);
			AssertEquals(false, canSend);

			GBCustomsDataRegistry.Instance.EdcsWtgAlertTime.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddHours(-7).ToDateTime());  // more than 6 hours ago 
			shutUp = new SendsMessagesToCustomsShutterUpperer(false);
			sender.Send(declaration, shutUp, messageFunction);
			AssertEquals(0, shutUp.PastYesNoQuestionsAsked.Count);
		}

		void AddBadgeCodeToRegistry(string badgeCode, string portCode, string cspCode, string direction)
		{
			BadgeCodeSettingCollection badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, Guid.Empty, Guid.Empty);
			BadgeCodeSetting badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = badgeCode;
			badgeCodeSetting.RL_PortCode = portCode;
			badgeCodeSetting.CSPCode = cspCode;
			badgeCodeSetting.Direction = direction;
			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, badgeCodeSettings);

			var credentials = GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var credential = credentials.AddNew();
			credential.BadgeCode = badgeCode;
			credential.Username = "ME";
			credential.Password = "PW";
			credential.Company = badgeCode;
			credential.Printer = "Printer";
			GBCustomsDataRegistry.Instance.Credentials.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, credentials);
		}
	}

	class GbDeclarationSenderChooserWithNoFinalSend : GbDeclarationSenderChooser
	{
		protected override void DoFinalSend(BaseJobDeclaration declaration, ISendsMessagesToCustoms sendToCustoms, CusdecMessageFunction how, IDeclarationMessageSender sender)
		{
			this.sender = sender;
		}

		internal bool SendAndIndicateIfASenderWasSelected(BaseJobDeclaration declaration, ISendsMessagesToCustoms sendToCustoms, CusdecMessageFunction how)
		{
			sender = null;
			base.Send(declaration, sendToCustoms, how);
			return sender != null;
		}
		IDeclarationMessageSender sender;
	}
}
