using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class BrokeragePlugInTest : Customs.GUI.PlugIn.Testing.BaseBrokeragePlugInAbstractTest
	{
		public void TestImportJobDeclaration()
		{
			var shipment = Factory.New<ForwardingShipment>();
			using (BrokeragePlugIn plugIn = new BrokeragePlugIn(shipment))
			{
				AssertEquals("ImportJobDeclaration for CA", typeof(Business.ImportJobDeclaration), plugIn.CreateDeclarationHelperInternal.GetNewImportJobDeclaration(shipment).GetType());
			}
		}

		public void TestBrokerageControlIsCorrectType()
		{
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertType<CustomsBrokerageUserControl>(plugin.UserControl);
			}
		}

		public void TestMenuIsCorrectType()
		{
			using (BrokeragePlugIn plugin = new BrokeragePlugIn(Factory.New<ForwardingShipment>()))
			{
				AssertEquals(typeof(EDIMenu), plugin.TopLevelMenu.GetType());
			}
		}

		public void TestShowPreSaveDialogsCore()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				var shipment = Factory.New<ForwardingShipment>();
				var testJobDeclaration = Factory.New<JobDeclaration>();
				testJobDeclaration.JE_JS = shipment.PK;
				testJobDeclaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				testJobDeclaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddMonths(2);
				var invoice = testJobDeclaration.Invoices.AddNew();
				var line = invoice.JobComInvoiceLines.AddNew();
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Enterprise.Core.Constants.Weight.Kilograms);
				testJobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				testJobDeclaration.DoMerge();
				Factory.Save();

				var b3entryHeader = testJobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				var testmessage = b3entryHeader.Messages.AddNew();
				testmessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testmessage.EM_Status = "QUE";
				testmessage.EM_ReceiveTransmit = "TRX";
				var existingScheduledTime = System.DateTime.UtcNow.AddDays(2);
				var existingScheduledTimeTranslated = (ZDateTime)EnvProxy.Instance.Time.GetLocalTimeFromUtc(existingScheduledTime);
				testmessage.EM_HeldUntilDate = existingScheduledTime;
				testmessage.EM_IsActive = true;
				line.DutiesAndTaxes.DeleteAll();
				Factory.Save();

				using (BrokeragePlugIn testPlugIn = new BrokeragePlugIn(shipment))
				{
					CombineAssertions(() =>
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						testJobDeclaration.HasChanges = false;
						testPlugIn.ShowPreSaveDialogsCore();

						var expectedMessage = string.Format("You have made changes when there is a deferred Entry message to be sent at {0}. The previous message will be canceled and a new Entry message will be scheduled to replace it.", existingScheduledTimeTranslated);
						Assert("won't call AskForB3ActionIfDeferredMessageExists", !UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));
						AssertEquals(false, b3entryHeader.NeedCancelDeferredB3CADMessage);
						AssertEquals(false, b3entryHeader.NeedResendDeferredB3CADMessage);
						AssertEquals(false, b3entryHeader.NeedResendB3CADMessageSilent);
						AssertEquals(ZDateTime.Empty, b3entryHeader.OriginalDeferredB3CADMessageTime);

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						testJobDeclaration.HasChanges = true;
						testPlugIn.ShowPreSaveDialogsCore();

						Assert("will call AskForB3ActionIfDeferredMessageExists", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));
						AssertEquals(true, b3entryHeader.NeedCancelDeferredB3CADMessage);
						AssertEquals(true, b3entryHeader.NeedResendDeferredB3CADMessage);
						AssertEquals(true, b3entryHeader.NeedResendB3CADMessageSilent);
						AssertEquals(b3entryHeader.DeferredB3MessageTime, b3entryHeader.OriginalDeferredB3CADMessageTime);
					});
				}
			}
		}

		public void TestOnSaveCompletedOrAbort()
		{
			using (TransactionNumberTestHelper.SetupCompanyASECNumberForTest())
			{
				TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
				var shipment = Factory.New<ForwardingShipment>();
				var testJobDeclaration = Factory.New<JobDeclaration>();
				testJobDeclaration.JE_JS = shipment.PK;
				testJobDeclaration.JE_MessageType = Enterprise.Customs.Business.JobMessageTypeList.Codes.Import;
				testJobDeclaration.JE_EntryAuthorisationDate = ZDateTime.Now.AddMonths(2);
				var invoice = testJobDeclaration.Invoices.AddNew();
				var line = invoice.JobComInvoiceLines.AddNew();
				JobComInvoiceLineTestHelper.FillInvoiceLine(line, 3, 1, 1000, 1000, 666, 333, Enterprise.Core.Constants.Weight.Kilograms);
				testJobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				testJobDeclaration.DoMerge();
				Factory.Save();
				using (BrokeragePlugIn testPlugIn = new BrokeragePlugIn(shipment))
				{
					CombineAssertions(() =>
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testPlugIn.ShowPreSaveDialogs();
						testPlugIn.OnSaveCompletedOrAborted(false);
						Assert("Not saving, not triggering", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						testPlugIn.ShowPreSaveDialogs();
						testPlugIn.OnSaveCompletedOrAborted(true);
						Assert("Saving, not triggering", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));
					});
				}

				var b3entryHeader = testJobDeclaration.GetEntryHeaderFor(MessageTypeList.Codes.B3CUSDEC);
				var testmessage = b3entryHeader.Messages.AddNew();
				testmessage.EM_MessageType = MessageTypeList.Codes.B3CUSDEC;
				testmessage.EM_Status = "QUE";
				testmessage.EM_ReceiveTransmit = "TRX";
				var existingScheduledTime = System.DateTime.UtcNow.AddDays(2);
				var existingScheduledTimeTranslated = (ZDateTime)EnvProxy.Instance.Time.GetLocalTimeFromUtc(existingScheduledTime);
				testmessage.EM_HeldUntilDate = existingScheduledTime;
				testmessage.EM_IsActive = true;
				line.DutiesAndTaxes.DeleteAll();
				Factory.Save();
				using (BrokeragePlugIn testPlugIn = new BrokeragePlugIn(shipment))
				{
					CombineAssertions(() =>
					{
						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
						testJobDeclaration.HasChanges = true;
						testPlugIn.ShowPreSaveDialogs();

						var expectedMessage = string.Format("You have made changes when there is a deferred Entry message to be sent at {0}. The previous message will be canceled and a new Entry message will be scheduled to replace it.", existingScheduledTimeTranslated);
						Assert("Saving, triggering", UnitTestUserNotification.Instance.PreviousMessages.ContainsMessageWithThisText(expectedMessage));

						Factory.Save();
						testPlugIn.OnSaveCompletedOrAborted(true);
						AssertEquals("Saving, triggering, Say Yes",
							string.Format("System cannot send a B3 Message for Declaration S00001000 message as The Network Client ID is not configured,\r\nin the registry for Company - EDI, Branch - BNE. Please contact your System Administrator.", existingScheduledTimeTranslated),
							UnitTestUserNotification.Instance.LastMessage.Text);
					});
				}
			}
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugInToTest() => new BrokeragePlugInForBashTesting(Shipment);

		sealed class BrokeragePlugInForBashTesting : BrokeragePlugIn
		{
			public BrokeragePlugInForBashTesting(ForwardingShipment shipment)
				: base(shipment)
			{
			}
		}
	}
}
