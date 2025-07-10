using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class EDIMenuTest : TestCaseForAttachGUI
{
	public void TestSendCustomsMessagesMenuItemVisible()
	{
		void testVisibilityWhenApplicationCodeIsBthOrItf()
		{
			using (var formForTest = new JobDeclarationFormForTest(declaration))
			{
				formForTest.Show();
				var menu = formForTest.EDIMenu;

				var submitBaseMenuItem = menu.MenuItems.FindByText("Submit");
				var sendCustomsMessagesNewMenuItem = menu.MenuItems.FindByText("Send Customs Messages");
				AssertNotNull("Send Customs Messages new menu item", sendCustomsMessagesNewMenuItem);
				AssertNotNull("Submit base menu item", submitBaseMenuItem);

				declaration.JE_ApplicationCode = "BLT";
				menu.RefreshMenu();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("(BLT) - Send Customs Messages new menu item visible state", true, sendCustomsMessagesNewMenuItem.Visible);
				AssertEquals("(BLT) - Submit base menu item visible state", false, submitBaseMenuItem.Visible);

				declaration.JE_ApplicationCode = "ITF";
				menu.RefreshMenu();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("(ITF) - Send to Customs new menu item visible state", false, sendCustomsMessagesNewMenuItem.Visible);
				AssertEquals("(ITF) - Submit base menu item visible state", true, submitBaseMenuItem.Visible);
			}
		}

		void testVisibility(bool isBuiltInSubmission)
		{
			using (var formForTest = new JobDeclarationFormForTest(declaration))
			{
				formForTest.Show();
				var menu = formForTest.EDIMenu;

				var submitBaseMenuItem = menu.MenuItems.FindByText("Submit");
				var sendCustomsMessagesNewMenuItem = menu.MenuItems.FindByText("Send Customs Messages");
				AssertNotNull("Send Customs Messages new menu item", sendCustomsMessagesNewMenuItem);
				AssertNotNull("Submit base menu item", submitBaseMenuItem);

				menu.RefreshMenu();
				menu.OnPopup(EventArgs.Empty);
				AssertEquals("Send Customs Messages new menu item visible state", isBuiltInSubmission, sendCustomsMessagesNewMenuItem.Visible);
				AssertEquals("Submit base menu item visible state", !isBuiltInSubmission, submitBaseMenuItem.Visible);
			}
		}

		CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "BTH", });
		declaration = Factory.New<JobDeclaration>();
		testVisibilityWhenApplicationCodeIsBthOrItf();

		CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "BIT", });
		declaration = Factory.New<JobDeclaration>();
		testVisibilityWhenApplicationCodeIsBthOrItf();

		CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "ITF", });
		declaration = Factory.New<JobDeclaration>();
		testVisibility(isBuiltInSubmission: false);

		CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new LocalCountryCustomsInterface() { RecipientID = "RecipientID", SubmissionType = "BLT", });
		declaration = Factory.New<JobDeclaration>();
		testVisibility(isBuiltInSubmission: true);
	}

	public void TestDisplayGenerateEntriesMenuOption()
	{
		using (var formForTest = new JobDeclarationFormForTest(declaration))
		{
			formForTest.Show();
			var menu = formForTest.EDIMenu;

			Assert("DisplayGenerateEntriesMenuOption should be always", menu.DisplayGenerateEntriesMenuOption);
		}
	}

	[TestDate(2019, 11, 24)]
	public void TestSendCustomsMessagesMenuItemClick()
	{
		OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All);

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		using (var formForTest = new JobDeclarationFormForTest(declaration))
		{
			formForTest.Show();
			using (var menu = formForTest.EDIMenu)
			{
				var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				declaration.JE_MessageType = "IMP";
				declaration.JE_ScreeningStatus = "UNK";
				declaration.JE_CustomsProfile = "TEST-DEC1";
				Assert("Pre-condition", declaration.HasChanges);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				sendCustomsMessagesMenuItem.PerformClick();
				AssertContains("You can't merge this entry because there are no invoice headers.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(declaration.HasChanges);

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				var mergedLine1 = entryHeader1.MergedLines.AddNew();
				invoiceLine1.JI_CL = mergedLine1.PK;
				invoiceLine1.JI_CEI = entryInstruction1.PK;

				var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader2 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				var mergedLine2 = entryHeader2.MergedLines.AddNew();
				invoiceLine2.JI_CL = mergedLine2.PK;
				invoiceLine2.JI_CEI = entryInstruction2.PK;

				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				var declarantTaxNumber = "11111111111";
				ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, TestDateAttribute.Date.Year, declarantTaxNumber, currentValue: 1);
				new AccountCollectionTestBuilder(company.PK)
					.AppendAccount("11111111111-321", "Test")
					.AppendAccountDetail("TEST-DEC1", "DEC1")
					.Build();

				sendCustomsMessagesMenuItem.PerformClick();
				AssertEquals("The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				var decWrapper = menu.GetJobDeclarationMessageSendingObjectParentExposed(declaration);
				decWrapper.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
				AssertEquals("Sending objects count", 2, decWrapper.SendingObjectsCollection.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				sendCustomsMessagesMenuItem.PerformClick();

				AssertContains("Unable to submit message due to Denied Party Screening cancellation.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(typeof(DocumentLoginForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

				declaration.JE_ScreeningStatus = "CLR";
				Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(form
					=> ConfigureMessageSendingObjectAndClickSendButton(form, CustomsMessageSendingModeList.Codes.AutomaticProcedure));

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				sendCustomsMessagesMenuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertContains("", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(typeof(MessageSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

					AssertEquals("Has one message", 1, entryHeader1.Messages.Count);
					AssertEquals("No message", 0, entryHeader2.Messages.Count);

					AssertEquals("Declaration.HasChanges", false, declaration.HasChanges);
				});

				ZFormModaliser.ClearDelegateToCallBeforeShowingFormsOrDialogs();
				ZFormModaliser.SetDelegateToCallOnFormShown(form
					=> ConfigureMessageSendingObjectAndClickSendButton(form, CustomsMessageSendingModeList.Codes.FallbackProcedure));
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = "pratiche.001";
				sendCustomsMessagesMenuItem.PerformClick();
				AssertEquals(typeof(SaveFileDialog), ZFormModaliser.LastCommonDialogShownDialogForTest.GetType());
			}
		}
	}

	[TestDate(2019, 11, 24)]
	public void TestSendToCustoms_ShouldCheckCredit()
	{
		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		using (var formForTest = new JobDeclarationFormForTest(declaration))
		{
			using (var menu = formForTest.EDIMenu)
			{
				var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");

				declaration.JE_MessageType = "IMP";

				var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader = declaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine = invoice1.InvoiceLines.AddNew();
				var mergedLine = entryHeader.MergedLines.AddNew();
				invoiceLine.JI_CL = mergedLine.PK;
				invoiceLine.JI_CEI = entryInstruction.PK;

				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, TestDateAttribute.Date.Year, "11111111111", currentValue: 1);
				new AccountCollectionTestBuilder(company.PK)
					.AppendAccount("11111111111-321", "Test")
					.AppendAccountDetail("TEST-DEC1", "DEC1")
					.Build();

				Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				ZFormModaliser.SetDelegateToCallOnFormShown(form
					=>
				{
					var messageSendingForm = (MessageSendingForm)form;
					var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
					messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
					messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
				});

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

				var testCases = new[]
				{
					(isCheckPass: false, expectedMessageCount: 0, expectedMessageText: "Submit message with credit restriction canceled."),
					(isCheckPass: true, expectedMessageCount: 1, expectedMessageText: "Message sent successfully"),
				};

				foreach (var (isCheckPass, expectedMessageCount, expectedMessageText) in testCases)
				{
					using (CheckCreditTestHelper.WithCreditCheck(Factory, declaration, isCheckPass))
					{
						AssertEquals($"Credit check passed: {isCheckPass}, Precondition", 0, entryHeader.Messages.Count);
						declaration.JE_CustomsProfile = "TEST-DEC1";

						sendCustomsMessagesMenuItem.PerformClick();

						CombineAssertions($"Credit check passed: {isCheckPass}", () =>
						{
							AssertEquals("Last message after sending", expectedMessageText, UnitTestUserNotification.Instance.LastMessage.Text);
							AssertEquals("Messages count after sending", expectedMessageCount, entryHeader.Messages.Count);
						});
					}
				}
			}
		}
	}

	public void TestRefreshMenuWhenDeclarationIsNotCreatedYet()
	{
		var menu = new EDIMenu();
		AssertNull("PRE-CONDITION", menu.Declaration);

		var sendCustomsMessagesNewMenuItem = menu.MenuItems.FindByText("Send Customs Messages");
		AssertNoExceptionThrown("Should not throw any exception", () => menu.RefreshMenu());
		AssertEquals(nameof(sendCustomsMessagesNewMenuItem.Visible), false, sendCustomsMessagesNewMenuItem.Visible);
	}

	public void TestSendCustomsMessagesTriggersMergeAndShowsSavePopupOnlyIfRequired()
	{
		using (var formForTest = new JobDeclarationFormForTest(declaration))
		using (var menu = formForTest.EDIMenu)
		{
			var menuItem = menu.MenuItems.FindByText("Send Customs Messages");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			declaration.Invoices.AddNew().InvoiceLines.AddNew().JI_Tariff = "ABC";
			CombineAssertions("PRE-CONDITIONS 1 (the first merge has not yet been done)", () => AssertInvolvedProperties(expectedIsMergeDone: false, expectedRequiresMerge: false));
			menuItem.PerformClick();
			CombineAssertions("POST-CONDITIONS 1 (the first merge has been done)", () => AssertInvolvedProperties(expectedIsMergeDone: true, expectedRequiresMerge: false, expectedLastNotificationMessage: "The Job has not yet been saved. Do you want to save and proceed?"));

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			declaration.JE_AgentsReference = "XYZ";
			CombineAssertions("PRE-CONDITIONS 2 (the second merge will be triggered)", () => AssertInvolvedProperties(expectedIsMergeDone: true, expectedRequiresMerge: true));
			menuItem.PerformClick();
			CombineAssertions("POST-CONDITIONS 2 (the second merge has been triggered and job saved)", () =>
			{
				AssertInvolvedProperties(expectedIsMergeDone: true, expectedRequiresMerge: false, expectedLastNotificationMessage: "The Job has not yet been saved. Do you want to save and proceed?");
				AssertEquals("Declaration HasChanges", false, declaration.HasChanges);
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			CombineAssertions("PRE-CONDITIONS 3 (another merge is not required)", () => AssertInvolvedProperties(expectedIsMergeDone: true, expectedRequiresMerge: false));
			menuItem.PerformClick();
			CombineAssertions("POST-CONDITIONS 3 (another merge and save popup has not been triggered)", () => AssertInvolvedProperties(expectedIsMergeDone: true, expectedRequiresMerge: false));
		}

		void AssertInvolvedProperties(bool expectedIsMergeDone, bool expectedRequiresMerge, string expectedLastNotificationMessage = null)
		{
			AssertEquals("IsMergeDone", expectedIsMergeDone, declaration.IsMergeDone);
			AssertEquals("RequiresMerge", expectedRequiresMerge, declaration.MergeManager.RequiresMerge);
			AssertEquals("LastMessage Text", expectedLastNotificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestNonUcc6MessageSendingForm()
	{
		using (var ediMenu = new EDIMenuForTest())
		{
			ediMenu.Declaration = declaration;
			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration);

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			using (var messageSendingForm = ediMenu.GetNewMessageSendingFormExposed(sendingObjectParent))
			{
				AssertType<MessageSendingForm>("Message Sending Form Type", messageSendingForm);
			}
		}
	}

	public void TestUcc6AndWtgInternalSystemMessageSendingForm()
	{
		using (var ediMenu = new EDIMenuForTest())
		{
			ediMenu.Declaration = declaration;
			var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration);

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			using (var messageSendingForm = ediMenu.GetNewMessageSendingFormExposed(sendingObjectParent))
			{
				AssertType<MessageSendingFormUcc6>("Message Sending Form Type", messageSendingForm);
			}
		}
	}

	public void TestGetNewMessageGeneratorForSad()
	{
		declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		using (var ediMenu = new EDIMenuForTest())
		{
			ediMenu.Declaration = declaration;

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration);
				var messageGenerator = ediMenu.GetNewMessageGeneratorExposed(sendingObjectParent);
				AssertEquals("Message Generator Type", true, messageGenerator is SadOutgoingCustomsMessageCreationStrategy);
			}
		}
	}

	public void TestGetNewMessageGeneratorForAidaXml()
	{
		declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		using (var ediMenu = new EDIMenuForTest())
		{
			ediMenu.Declaration = declaration;

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration);
				var messageGenerator = ediMenu.GetNewMessageGeneratorExposed(sendingObjectParent);
				AssertType<AidaXmlOutgoingCustomsMessageCreationStrategy>("Message Generator Type", messageGenerator);
			}
		}
	}

	public void TestJobDeclarationMessageSendingObjectParentForSad()
	{
		declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		using (var ediMenu = new EDIMenuForTest())
		{
			ediMenu.Declaration = declaration;

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration);
				AssertType<JobDeclarationMessageSendingObjectParent>("SendingObjectParent Type", sendingObjectParent);
			}
		}
	}

	public void TestJobDeclarationMessageSendingObjectParentForAidaXml()
	{
		declaration.CustomsEntryHeaders.AddNew().MergedLines.AddNew();
		using (var ediMenu = new EDIMenuForTest())
		{
			ediMenu.Declaration = declaration;

			using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				var sendingObjectParent = ediMenu.GetJobDeclarationMessageSendingObjectParentExposed(declaration);
				AssertType<Ucc6JobDeclarationMessageSendingObjectParent>("SendingObjectParent Type", sendingObjectParent);
			}
		}
	}

	public void TestSendCustomsMessagesRollback_WhenSendingProcessFails()
	{
		OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All);

		using (EU.Business.Testing.ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
		using (var formForTest = new JobDeclarationFormForTest(declaration))
		{
			formForTest.Show();
			using (var menu = formForTest.EDIMenuForFailureSendingTest)
			{
				var sendCustomsMessagesMenuItem = (ZMenuItem)menu.MenuItems.FindByText("Send Customs Messages");
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				declaration.JE_MessageType = "IMP";
				declaration.JE_ScreeningStatus = "CLR";
				declaration.JE_CustomsProfile = "TEST-DEC1";

				var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
				var entryHeader1 = declaration.CustomsEntryHeaders.AddNew();
				entryHeader1.CH_CEI_Instruction = entryInstruction1.PK;
				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				var mergedLine1 = entryHeader1.MergedLines.AddNew();
				invoiceLine1.JI_CL = mergedLine1.PK;
				invoiceLine1.JI_CEI = entryInstruction1.PK;

				var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				var declarantTaxNumber = "11111111111";
				ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, TestDateAttribute.Date.Year, declarantTaxNumber, currentValue: 1);
				new AccountCollectionTestBuilder(company.PK)
					.AppendAccount("11111111111-321", "Test")
					.AppendAccountDetail("TEST-DEC1", "DEC1")
					.Build();

				Factory.Save();

				ZFormModaliser.ShowDialogsInTest = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.SetDelegateToCallOnFormShown(form =>
				{
					var messageSendingForm = (MessageSendingForm)form;
					var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
					messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
					messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;

					var sendButton = messageSendingForm.FindSingle<ZButton>("SendSplitButton");
					sendButton.Enabled = true;
					sendButton.PerformClick();
				});

				sendCustomsMessagesMenuItem.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals("ZSaveException", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals("CH_Status", "", entryHeader1.CH_Status);
					AssertEquals("When Message Sending fails, No new ITEDIMessages expected", 0, Factory.Load<ITEDIMessage>(new ZQuery()).Length);
				});
			}
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
		declaration.JE_MessageType = "IMP";
		declaration.JE_ApplicationCode = "BLT";
	}

	JobDeclaration declaration;

	void ConfigureMessageSendingObjectAndClickSendButton(object form, ZString customsMessageSendingMode)
	{
		var messageSendingForm = (MessageSendingForm)form;
		var messageSendingObjectParent = (JobDeclarationMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
		messageSendingObjectParent.CustomsMessageSendingMode = customsMessageSendingMode;
		messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;
		messageSendingObjectParent.SendingObjectsCollection[1].ShouldSend = false;

		var sendButton = messageSendingForm.FindSingle<ZButton>("SendSplitButton");
		sendButton.Enabled = true;
		sendButton.PerformClick();
	}
}

#region JobDeclarationFormForTest

public class JobDeclarationFormForTest : JobDeclarationForm
{
	public JobDeclarationFormForTest(JobDeclaration declaration)
		: base(declaration)
	{
	}

	protected override Customs.GUI.IEDIMenu GetNewTopLevelMenuCore() => EDIMenu;

	EDIMenuForTest ediMenu;
	public EDIMenuForTest EDIMenu => ediMenu ?? (ediMenu = new EDIMenuForTest());

	internal EDIMenuForFailureSendingTest EDIMenuForFailureSendingTest => eDIMenuForFailureSendingTest ?? (eDIMenuForFailureSendingTest = GetNewEDIMenuForFailureSendingTest());
	EDIMenuForFailureSendingTest eDIMenuForFailureSendingTest;

	EDIMenuForFailureSendingTest GetNewEDIMenuForFailureSendingTest()
	{
		var menu = new EDIMenuForFailureSendingTest();
		menu.Declaration = Declaration;
		return menu;
	}
}

#endregion

#region EDIMenuForTest

public class EDIMenuForTest : EDIMenu
{
	public JobDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParentExposed(JobDeclaration declaration)
		=> GetJobDeclarationMessageSendingObjectParent(declaration);

	public new bool DisplayGenerateEntriesMenuOption => base.DisplayGenerateEntriesMenuOption;

	public Form GetNewMessageSendingFormExposed(JobDeclarationMessageSendingObjectParent sendingObjectParent) => GetNewMessageSendingForm(sendingObjectParent);

	public IOutgoingCustomsMessageCreationStrategy GetNewMessageGeneratorExposed(JobDeclarationMessageSendingObjectParent sendingObjectParent)
		=> GetMessageCreationStrategy(Declaration.Factory, "ATM", sendingObjectParent.SendingObjectsCollection[0]);
}

class EDIMenuForFailureSendingTest : EDIMenuForTest
{
	protected override IOutgoingCustomsMessageCreationStrategy GetMessageCreationStrategy(BusinessObjectFactory factory, ZString customsMessageSendingMode, Business.JobDeclarationMessageSendingObject sendingObject)
	{
		var messageCreationStrategyMock = new Mock<IOutgoingCustomsMessageCreationStrategy>();

		messageCreationStrategyMock.Setup(x => x.GenerateMessage()).Throws(new ZSaveException(new FriendlyDataForTestException("ZSaveException", "Debug Message"), factory));
		return messageCreationStrategyMock.Object;
	}
}

#endregion
