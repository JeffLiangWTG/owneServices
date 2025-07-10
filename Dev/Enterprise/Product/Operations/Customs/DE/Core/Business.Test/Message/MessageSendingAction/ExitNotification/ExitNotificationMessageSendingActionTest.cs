using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitNotificationMessageSendingAction))]
	class ExitNotificationMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldSend()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Send?", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.ShouldSend)).Caption);
				AssertEquals(true, messageSendingAction.ShouldSend);
			});
		}

		public void TestReferenceNumber()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Reference Number", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.ReferenceNumber)).Caption);
				AssertEquals("R12345678", messageSendingAction.ReferenceNumber);
			});
		}

		public void TestStatus()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Status", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.Status)).Caption);
				AssertEquals("AAA", messageSendingAction.Status);
			});
		}

		public void TestStatusDescription()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Status Description", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.StatusDescription)).Caption);
				AssertEquals("AAA DESC", messageSendingAction.StatusDescription);
			});
		}

		public void TestMessageType()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Message Type", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.MessageType)).Caption);
				AssertEquals("NOT", messageSendingAction.MessageType);
			});
		}

		public void TestMissingQuantity()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Missing Quantity", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.MissingQuantity)).Caption);
				AssertEquals(false, messageSendingAction.MissingQuantity);
			});
		}

		public void TestFinalization()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Finalization", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.Finalization)).Caption);
				AssertEquals(true, messageSendingAction.Finalization);
			});
		}

		public void TestRedirection()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Redirection", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.Redirection)).Caption);
				AssertEquals(true, messageSendingAction.Redirection);
			});
		}

		public void TestIntendedExitCustomsOffice()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Intended Exit Customs Office", DataBoundResourceStrings.GetDataForProperty(typeof(ExitNotificationMessageSendingAction), nameof(ExitNotificationMessageSendingAction.IntendedExitCustomsOffice)).Caption);
				AssertEquals("DE001234", messageSendingAction.IntendedExitCustomsOffice);
			});
		}

		public void TestRedirection_IntendedExitCustomsOfficeMandatoryNotification()
		{
			CombineAssertions(() =>
			{
				messageSendingAction.IntendedExitCustomsOffice = ZString.Empty;
				messageSendingAction.Redirection = ZBool.True;
				AssertHasMessageErrorContaining("Redirection = True -> IntendedExitCustomsOffice mandatory", messageSendingAction.IntendedExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);

				messageSendingAction.Redirection = ZBool.False;
				AssertNoMessageErrorContaining("Redirection = False -> IntendedExitCustomsOffice not mandatory", messageSendingAction.IntendedExitCustomsOfficeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestRedirection_IntendedExitCustomsOfficeReadOnly()
		{
			CombineAssertions(() =>
			{
				messageSendingAction.Redirection = ZBool.True;
				AssertEquals("Redirection = True -> IntendedExitCustomsOffice enabled", false, messageSendingAction.IntendedExitCustomsOfficeInfo.ReadOnly);

				messageSendingAction.Redirection = ZBool.False;
				AssertEquals("Redirection = False -> IntendedExitCustomsOffice read-only", true, messageSendingAction.IntendedExitCustomsOfficeInfo.ReadOnly);
			});
		}

		public void TestRedirection_IntendedExitCustomsOfficeCleared()
		{
			CombineAssertions(() =>
			{
				AssertEquals("IntendedExitCustomsOffice initially", "DE001234", messageSendingAction.IntendedExitCustomsOffice);

				messageSendingAction.Redirection = ZBool.False;
				AssertEquals("Redirection = False -> IntendedExitCustomsOffice cleared ", ZString.Empty, messageSendingAction.IntendedExitCustomsOffice);
			});
		}

		public void TestItems()
		{
			AssertEquals(2, messageSendingAction.Items.Count);
		}

		public void TestPackages()
		{
			AssertEquals(2, messageSendingAction.Packages.Count);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExitNotificationMessageSendingAction(exitDetail);
		}

		protected override void SetUp()
		{
			base.SetUp();

			CreateExportCustomsStatus();

			exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			exitHeader.CEH_ReferenceNumber = "R12345678";
			exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = "M12345678";
			exitDetail.CED_Status = "AAA";

			exitDetail.CusExitItems.AddNew().Packages.AddNew();
			exitDetail.CusExitItems.AddNew().Packages.AddNew();
			messageSendingAction = (ExitNotificationMessageSendingAction)GetNewBusinessObject();
			messageSendingAction.ShouldSend = true;
			messageSendingAction.Finalization = true;
			messageSendingAction.Redirection = true;
			messageSendingAction.IntendedExitCustomsOffice = "DE001234";
		}
		ExitNotificationMessageSendingAction messageSendingAction;
		CusExitControlHeader exitHeader;
		CusExitDetail exitDetail;

		void CreateExportCustomsStatus()
		{
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "CSTEX DESC");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, "AAA", "AAA DESC", startDate, endDate);
			Factory.Save();
		}
	}
}
