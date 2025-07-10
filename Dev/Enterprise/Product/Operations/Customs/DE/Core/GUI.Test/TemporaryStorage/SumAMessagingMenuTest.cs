using System;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.CusTempStorage;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	public class SumAMessagingMenuTest : TestCaseWithFactory
	{
		public void TestFormAddsMenu()
		{
			var cusTempStorageDec = Factory.NewWithValidTestData<CUSPRLCusTempStorageDec>();
			cusTempStorageDec.STH_SJH = header.PK;
			cusTempStorageDec.STH_DeclarationType = TemporaryStorageDeclarationTypeList.Codes.CustomsPresentationLedger;

			using (var sumAForm = new TemporaryStorageForm(header))
			{
				var topLevelMenu = sumAForm.Menu.MenuItems.FindByText(Messages);
				AssertNotNull("Messages menu exists", topLevelMenu);
				var declarationMenu = topLevelMenu.MenuItems.FindByText("SumA Declaration");
				AssertNotNull("SumA Declaration menu exists", declarationMenu);
				CombineAssertions(() =>
				{
					var finalSumAWithoutPreliminaryMenu = declarationMenu.MenuItems.FindByText(FinalSumAWithoutPreliminaryMenuName);
					AssertNotNull("Final SumA without Preliminary menu exists", finalSumAWithoutPreliminaryMenu);
					var finalSumAWithPreliminaryMenu = declarationMenu.MenuItems.FindByText(FinalSumAWithAPreliminaryMenuName);
					AssertNotNull("Final SumA with Preliminary menu exists", finalSumAWithPreliminaryMenu);
					var preliminarySumAMenu = declarationMenu.MenuItems.FindByText(PreliminarySumAMenuName);
					AssertNotNull("SumA Preliminary menu exists", preliminarySumAMenu);
					var amendmentOfPreliminarySumAMenu = declarationMenu.MenuItems.FindByText(PreliminarySumAAmendmentMenuName);
					AssertNotNull("SumA Preliminary Amend menu exists", amendmentOfPreliminarySumAMenu);

					AssertMenuClickIsConnected(FinalSumAWithoutPreliminaryMenuName, () => finalSumAWithoutPreliminaryMenu.PerformClick());
					AssertMenuClickIsConnected(FinalSumAWithAPreliminaryMenuName, () => finalSumAWithPreliminaryMenu.PerformClick());
					AssertMenuClickIsConnected(PreliminarySumAMenuName, () => preliminarySumAMenu.PerformClick());
					AssertMenuClickIsConnected(PreliminarySumAAmendmentMenuName, () => amendmentOfPreliminarySumAMenu.PerformClick());
				});
			}

			void AssertMenuClickIsConnected(ZString message, Action menuClick)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				menuClick();
				AssertEquals(message + " is connected", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals(message + " has no added messages", 0, header.CUSPRLCusTempStorageDec.Messages.Count);
			}
		}

		public void TestSendFinalSumAWithoutPRLMessage()
		{
			AssertCUSPRLWarningsAndSendMessage(header, FinalSumAWithoutPreliminaryMenuName);
			SendCUSPRL(header, FinalSumAWithoutPreliminaryMenuName);
		}

		public void TestSendPreliminarySumAMessage()
		{
			AssertCUSPRLWarningsAndSendMessage(header, PreliminarySumAMenuName);
			SendCUSPRL(header, PreliminarySumAMenuName);
		}

		public void TestSendFinalSumAWithAPreliminaryMessage()
		{
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				var topLevelMenu = sumAForm.Menu.MenuItems.FindByText(Messages);
				var sendMenuItem = topLevelMenu.MenuItems.FindByText(FinalSumAWithAPreliminaryMenuName, true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Factory.Save();

				CombineAssertions(() =>
				{
					sendMenuItem.PerformClick();
					AssertEquals("No declaration", true, UnitTestUserNotification.Instance.LastMessage.WasError);

					var cusprlStorageDec = CUSPRLCusTempStorageDec.New(header);
					Factory.Save();
					sendMenuItem.PerformClick();
					AssertEquals("No lines", true, UnitTestUserNotification.Instance.LastMessage.WasError);

					cusprlStorageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
					cusprlStorageDec.CusTempStorageLines.AddNew();
					Factory.Save();
					sendMenuItem.PerformClick();
					AssertEquals("Declaration waiting response", true, UnitTestUserNotification.Instance.LastMessage.WasError);

					cusprlStorageDec.STH_MessageStatus = ZString.Empty;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
					ZFormModaliser.ShowDialogsInTest = true;
					ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
					{
						var dialog = (MessageSendingForm<FinalSumAWithAPreliminaryMessageSendingActionParent>)obj;
						((FinalSumAWithAPreliminaryMessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0]).ShouldSend = false;
					});
					cusprlStorageDec.CusTempStorageLines.AddNew();
					sendMenuItem.PerformClick();
					AssertEquals(1, header.CUSPRLCusTempStorageDec.Messages.Count);

					var message = (EDIMessage)header.CUSPRLCusTempStorageDec.Messages.Single();
					var xDoc = XDocument.Parse(message.EM_MessageText);
					AssertEquals("GoodsItems count", 1, xDoc.Descendants("GoodsItem").Count());
				});
			}
		}

		public void TestSendAmendmentOfPRLSumAMessage()
		{
			AssertCUSPRLWarningsAndSendMessage(header, PreliminarySumAAmendmentMenuName, line =>
			{
				line.TSL_GoodsDescription = "LINE 2 GOODS";
				line.TSL_IsModified = true;
				line.TSL_CustomsStatus = CustomsStatusList.Codes.PAC;
			});
			SendCUSPRL(header, PreliminarySumAAmendmentMenuName);

			var header2 = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			var decOfHeader2 = CUSPRLCusTempStorageDec.New(header2);
			decOfHeader2.CusTempStorageLines.AddNew();
			Factory.Save();

			SendCUSPRL(header2, PreliminarySumAAmendmentMenuName, jobHeader =>
			{
				AssertEquals(true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("All Lines are already finalized or there are no changes to be sent."));
				AssertEquals(0, jobHeader.CUSPRLCusTempStorageDec.Messages.Count);
			});
		}

		public void TestSubsequentMessagesCannotBeSent()
		{
			var cusprlStorageDec = CUSPRLCusTempStorageDec.New(header);
			cusprlStorageDec.CusTempStorageLines.AddNew();
			Factory.Save();

			using (var sumAForm = new TemporaryStorageForm(header))
			{
				var topLevelMenu = sumAForm.Menu.MenuItems.FindByText(Messages);
				var finalSumAWithoutPreliminaryMenu = topLevelMenu.MenuItems.FindByText(FinalSumAWithoutPreliminaryMenuName, true);
				CombineAssertions(() =>
				{
					AssertEquals("Status Not Sent", Common.Shared.MessageStatusList.Codes.NotSent, cusprlStorageDec.STH_MessageStatus);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					Environment.Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed = true;
					finalSumAWithoutPreliminaryMenu.PerformClick();
					AssertEquals("Message has been sent", 1, cusprlStorageDec.Messages.Count);
					AssertEquals("Status updated to sent", Common.Shared.MessageStatusList.Codes.Sent, cusprlStorageDec.STH_MessageStatus);

					finalSumAWithoutPreliminaryMenu.PerformClick();
					AssertEquals("Cannot Send multiple messages", true, UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("No extra message sent", 1, cusprlStorageDec.Messages.Count);
				});
			}
		}

		void AssertCUSPRLWarningsAndSendMessage(CusTempStorageJobHeader jobHeader, ZString menuItemName, Action<CUSPRLCusTempStorageLine> lineModifier = null)
		{
			using (var sumAForm = new TemporaryStorageForm(jobHeader))
			{
				var topLevelMenu = sumAForm.Menu.MenuItems.FindByText(Messages);
				var sendMenuItem = topLevelMenu.MenuItems.FindByText(menuItemName, true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Factory.Save();

				CombineAssertions(() =>
				{
					sendMenuItem.PerformClick();
					AssertEquals("No declaration", true, UnitTestUserNotification.Instance.LastMessage.WasError);

					var cusprlStorageDec = CUSPRLCusTempStorageDec.New(jobHeader);
					Factory.Save();
					sendMenuItem.PerformClick();
					AssertEquals("No lines", true, UnitTestUserNotification.Instance.LastMessage.WasError);

					var cusprlStorageLine = cusprlStorageDec.CusTempStorageLines.AddNew();
					lineModifier?.Invoke(cusprlStorageLine);
					Environment.Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed = false;
					Factory.Save();
					sendMenuItem.PerformClick();
					AssertContains("Has message errors and send with message errors is not allowed", "Please fix the following message errors before sending any messages", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					Environment.Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed = true;
					sendMenuItem.PerformClick();
					AssertEquals("Has message errors and send with message errors is allowed", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("It is likely that your message(s) will be rejected"));
					AssertEquals("Message created", 1, jobHeader.CUSPRLCusTempStorageDec.Messages.Count);

					cusprlStorageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.NotSent;
					Factory.Save();
				});
			}
		}

		void SendCUSPRL(CusTempStorageJobHeader jobHeader, ZString menuItemName, Action<CusTempStorageJobHeader> assertion = null)
		{
			var newFactory = new BusinessObjectFactory();
			newFactory.SuspendValidation();
			var reloadedHeader = newFactory.Load<CusTempStorageJobHeader>(jobHeader.PK);

			using (var sumAForm = new TemporaryStorageForm(reloadedHeader))
			{
				var topLevelMenu = sumAForm.Menu.MenuItems.FindByText(Messages);
				var sendMenuItem = topLevelMenu.MenuItems.FindByText(menuItemName, true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Environment.Env.Security.CustomsTemporaryStorageSendWithMessageErrors.IsAllowed = false;
				sendMenuItem.PerformClick();
				assertion = assertion ?? DefaultSendCUSPRLAssertion;
				assertion.Invoke(jobHeader);
			}
		}

		void DefaultSendCUSPRLAssertion(CusTempStorageJobHeader jobHeader)
		{
			AssertEquals("Validation is Suspended so no Message Errors", true, UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageContainingThisText("The message has been sent"));
			AssertEquals(2, jobHeader.CUSPRLCusTempStorageDec.Messages.Count);
		}

		public void TestAmendmentMenuChangeCustodyInformation()
		{
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				TestAmendmentMenuClick(sumAForm, "Change Custody Information", () => header.CHGTSTCusTempStorageDecs.AddNew());
			}
		}

		public void TestAmendmentChangeDisposalEntitledTrader()
		{
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				TestAmendmentMenuClick(sumAForm, "Change Disposal Entitled Trader", () => header.CHGOFFCusTempStorageDecs.AddNew());
			}
		}

		public void TestAmendmentChangeOwnerReference()
		{
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				TestAmendmentMenuClick(sumAForm, "Change Owner Reference", () => header.CHGSPOCusTempStorageDecs.AddNew());
			}
		}

		public void TestAmendmentChangeConsolidation()
		{
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				TestAmendmentMenuClick(sumAForm, "Consolidation", () => header.PRLCONCusTempStorageDecs.AddNew());
			}
		}

		public void TestAmendmentSplit()
		{
			using (var sumAForm = new TemporaryStorageForm(header))
			{
				TestAmendmentMenuClick(sumAForm, "Split", () => header.CUSPCSCusTempStorageDecs.AddNew());
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			setTemporaryCurrentUser = Factory.SetTemporaryCurrentUser("Senior Logistics Manager", "Owen Daniels", "+61 2 8001 2200");
		}
		CusTempStorageJobHeader header;
		IDisposable setTemporaryCurrentUser;

		protected override void TearDown()
		{
			base.TearDown();
			setTemporaryCurrentUser.Dispose();
		}

		void TestAmendmentMenuClick(TemporaryStorageForm sumAForm, ZString amendmentMenuToTest, Func<CusTempStorageDec> createDec)
		{
			var topLevelMenu = sumAForm.Menu.MenuItems.FindByText(Messages);
			var amendmentsMenu = topLevelMenu.MenuItems.FindByText("Amendments", true);
			var amendmentSubMenu = amendmentsMenu.MenuItems.FindByText(amendmentMenuToTest);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			amendmentSubMenu.PerformClick();
			CombineAssertions(() =>
			{
				AssertEquals("There are no decs selected", $"Please enter at least one declaration in Amendments > {amendmentMenuToTest}.", UnitTestUserNotification.Instance.LastMessage.Text);

				var dec = createDec();
				dec.STH_IdentificationIndicator = TemporaryStorageIdentificationIndicatorList.Codes.REG;

				amendmentSubMenu.PerformClick();
				AssertEquals("Not Saved", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormClosing(obj =>
				{
					var dialog = (MessageSendingForm<MessageSendingActionParent>)obj;
					((MessageSendingAction)dialog.MessageSendingObjectParent.SendingObjectsCollection[0]).ShouldSend = true;
				});

				amendmentSubMenu.PerformClick();
				AssertEquals("Declaration has no lines", true, UnitTestUserNotification.Instance.LastMessage.WasError);

				dec.CusTempStorageLines.AddNew();
				dec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
				Factory.Save();
				amendmentSubMenu.PerformClick();
				AssertEquals("Declaration waiting response", true, UnitTestUserNotification.Instance.LastMessage.WasError);

				dec.STH_MessageStatus = ZString.Empty;
				Factory.Save();
				amendmentSubMenu.PerformClick();
				AssertEquals("Message sent", 1, dec.Messages.Count);
			});
		}

		const string FinalSumAWithoutPreliminaryMenuName = "Final SumA without Preliminary";
		const string PreliminarySumAMenuName = "Preliminary SumA";
		const string FinalSumAWithAPreliminaryMenuName = "Final SumA with a Preliminary";
		const string PreliminarySumAAmendmentMenuName = "Amendment of Preliminary SumA";
		const string Messages = "Messages";
	}
}
