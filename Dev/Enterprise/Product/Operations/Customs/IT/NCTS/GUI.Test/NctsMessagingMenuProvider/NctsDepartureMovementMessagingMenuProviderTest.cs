using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.DataTransfer;
using Enterprise.Customs.EU.NCTS.GUI;
using Enterprise.Customs.IT.Business;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.Customs.IT.NCTS.Business;
using Enterprise.Customs.IT.NCTS.Business.Testing;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.GUI.Testing;

sealed class NctsDepartureMovementMessagingMenuProviderTest : TestCaseWithFactory
{
	public void TestMessagingMenuProviderType()
	{
		var departureNctsHeader = Factory.NewDepartureNctsHeader();
		using (var nctsForm = new NctsMovementForm(departureNctsHeader))
		{
			var springLoadedNctsMessagingMenuProvider = NctsMessagingMenuProvider.New(departureNctsHeader, new NctsHeaderUniversalMessagingHelper(), nctsForm);
			AssertType<NctsDepartureMovementMessagingMenuProvider>(springLoadedNctsMessagingMenuProvider);
		}
	}

	public void TestIsSendDepartureDeclarationMenuAllowed()
	{
		var nctsHeader = Factory.NewDepartureNctsHeader();
		using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
		{
			var providerForTest = new NctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);
			AssertEquals("IsSendDepartureDeclarationMenuAllowed", false, providerForTest.IsSendDepartureDeclarationMenuAllowedExposed());
		}
	}

	public void TestSendCustomsMessagesMenuItem()
	{
		var messagingMenuProvider = new NctsDepartureMovementMessagingMenuProvider(Factory.NewDepartureNctsHeader());
		var sendCustomsMessagesMenuItem = messagingMenuProvider.CreateMenuItems().SingleOrDefault(x => x.Caption == "Send Customs Messages");
		AssertNotNull("'Send Customs Messages' menu item", sendCustomsMessagesMenuItem);
	}

	[TestDate(2019, 11, 24)]
	[RequiresSTA]
	public void TestSendCustomsMessagesMenuItemClick()
	{
		var nctsHeader = PrepareNctsDeclarationToSend();
		Factory.Save();

		using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var messagingMenuProvider = new NctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);
			var sendCustomsMessagesMenuItem = messagingMenuProvider.CreateMenuItems().SingleOrDefault(x => x.Caption == "Send Customs Messages");
			sendCustomsMessagesMenuItem.PerformClick();

			AssertType<Phase4MessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(form =>
			{
				var messageSendingForm = (Phase4MessageSendingForm)form;
				var messageSendingObjectParent = (NctsHeaderDepartureMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
				messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;

				var sendButton = messageSendingForm.FindSingle<ZButton>("SendSplitButton");
				sendButton.Enabled = true;
				sendButton.PerformClick();
			});

			sendCustomsMessagesMenuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Number of messages on the NctsHeader", 1, nctsHeader.Messages.Count);
				AssertEquals("NctsHeader.HasChanges", false, nctsHeader.HasChanges);
			});
		}
	}

	[RequiresSTA]
	public void TestSendCustomsMessagesMenuItemClick_WhenMessageSendingFails()
	{
		var nctsHeader = PrepareNctsDeclarationToSend();
		Factory.Save();

		using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			var messagingMenuProvider = new NctsDepartureMovementMessagingMenuProviderForSendingFailureTest(nctsHeader, nctsMovementForm);
			var sendCustomsMessagesMenuItem = messagingMenuProvider.CreateMenuItems().SingleOrDefault(x => x.Caption == "Send Customs Messages");
			sendCustomsMessagesMenuItem.PerformClick();

			AssertType<Phase4MessageSendingForm>(ZFormModaliser.LastFormShownDialogForTest);

			ZFormModaliser.ShowDialogsInTest = true;
			ZFormModaliser.SetDelegateToCallOnFormShown(form =>
			{
				var messageSendingForm = (Phase4MessageSendingForm)form;
				var messageSendingObjectParent = (NctsHeaderDepartureMessageSendingObjectParent)messageSendingForm.MessageSendingObjectParent;
				messageSendingObjectParent.CustomsMessageSendingMode = CustomsMessageSendingModeList.Codes.AutomaticProcedure;
				messageSendingObjectParent.SendingObjectsCollection[0].ShouldSend = true;

				var sendButton = messageSendingForm.FindSingle<ZButton>("SendSplitButton");
				sendButton.Enabled = true;
				sendButton.PerformClick();
			});

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			sendCustomsMessagesMenuItem.PerformClick();

			CombineAssertions(() =>
			{
				AssertContains("ZSaveException", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("BH_MessageStatus", "", nctsHeader.BH_MessageStatus);
				AssertEquals("When Message Sending fails, No new ITEDIMessages expected", 0, Factory.Load<ITEDIMessage>(new ZQuery()).Length);
			});
		}
	}

	public void TestRunPreSaveDeclarationBeforeSendCustomsMessageIfNeeded()
	{
		var nctsHeader = PrepareNctsDeclarationToSend();

		using (var nctsMovementForm = new NctsMovementForm(nctsHeader))
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var messagingMenuProvider = new NctsDepartureMovementMessagingMenuProviderForTest(nctsHeader, nctsMovementForm);
			var sendCustomsMessagesMenuItem = messagingMenuProvider.CreateMenuItems().SingleOrDefault(x => x.Caption == "Send Customs Messages");

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			sendCustomsMessagesMenuItem.PerformClick();

			AssertEquals("User message confirmation expected", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Header not saved", false, nctsHeader.IsInDatabase);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			sendCustomsMessagesMenuItem.PerformClick();

			AssertEquals("User message confirmation expected", "The Job has not yet been saved. Do you want to save and proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Header saved", true, nctsHeader.IsInDatabase);
		}
	}

	NctsHeader PrepareNctsDeclarationToSend()
	{
		Factory.New<OrgHeader>().OH_Code = "DEC1";
		Factory.Save();

		var company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
		var declarantTaxNumber = "11111111111";
		ITCustomsNumberViewStmNumsWrapperTestHelper.SetupTestCustomsDeclarationsNumberRange(company, TestDateAttribute.Date.Year, declarantTaxNumber, currentValue: 1);

		new AccountCollectionTestBuilder(GlbCompany.CurrentCompany.PK.ToGuid())
			.AppendAccount("11111111111-001", "1234")
			.AppendAccountDetail("1234-DEC1", "DEC1")
			.Build();
		Factory.Save();

		var nctsHeader = Factory.NewDepartureNctsHeader();
		nctsHeader.BH_CustomsProfile = "1234-DEC1";
		return nctsHeader;
	}
}

class NctsDepartureMovementMessagingMenuProviderForTest : NctsDepartureMovementMessagingMenuProvider
{
	public NctsDepartureMovementMessagingMenuProviderForTest(NctsHeader header, NctsMovementForm nctsMovementForm) : base(header)
	{
		ParentForm = nctsMovementForm;
	}

	public NctsDepartureMovementMessagingMenuProviderForTest(NctsHeader header, NctsHeaderUniversalMessagingHelper ntcsHeaderUniversalMessagingHelper, NctsMovementForm nctsMovementForm) : base(header, ntcsHeaderUniversalMessagingHelper)
	{
		ParentForm = nctsMovementForm;
	}

	public bool IsSendDepartureDeclarationMenuAllowedExposed() => IsSendDepartureDeclarationMenuAllowed();

	public NctsHeaderDepartureMessageSendingObjectParent GetMessageSendingObjectParentExposed(NctsHeader nctsHeader) => GetMessageSendingObjectParent(nctsHeader);
}

class NctsDepartureMovementMessagingMenuProviderForSendingFailureTest : NctsDepartureMovementMessagingMenuProviderForTest
{
	public NctsDepartureMovementMessagingMenuProviderForSendingFailureTest(NctsHeader header, NctsMovementForm nctsMovementForm) : base(header, nctsMovementForm)
	{
	}

	protected override IOutgoingCustomsMessageCreationStrategy GetStrategy(BusinessObjectFactory factory, NctsHeaderDepartureMessageSendingObject sendingObject)
	{
		var outgoingCustomsMessageCreationStrategyMock = new Mock<IOutgoingCustomsMessageCreationStrategy>();
		outgoingCustomsMessageCreationStrategyMock.Setup(x => x.GenerateMessage()).Throws(new ZSaveException(new FriendlyDataForTestException("ZSaveException", "Debug Message"), factory));
		return outgoingCustomsMessageCreationStrategyMock.Object;
	}
}
