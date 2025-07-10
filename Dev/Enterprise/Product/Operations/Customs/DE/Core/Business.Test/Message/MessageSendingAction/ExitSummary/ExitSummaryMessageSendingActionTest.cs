using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ExitSummaryMessageSendingAction))]
	class ExitSummaryMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDetails()
		{
			AssertEquals("M12345678", messageSendingAction.Details);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("R12345678", messageSendingAction.ReferenceNumber);
		}

		public void TestStatus()
		{
			AssertEquals("AAA", messageSendingAction.Status);
		}

		public void TestStatusDescription_InvalidStatus()
		{
			AssertEquals("Shouldn't throw exception", ZString.Empty, messageSendingAction.StatusDescription);
		}

		public void TestStatusDescription()
		{
			CreateExportCustomsStatus();
			AssertEquals("Valid status", "AAA DESC", messageSendingAction.StatusDescription);
		}

		public void TestMessageType_ReadOnly()
		{
			AssertEquals(false, messageSendingAction.MessageTypeInfo.ReadOnly);
		}

		public void TestMessageType_MaxLength()
		{
			AssertEquals(3, messageSendingAction.MessageTypeInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ExitSummaryMessageSendingAction(exitDetail);
		}

		protected override void SetUp()
		{
			base.SetUp();

			exitHeader = Factory.New<CusExitControlHeader>();
			exitHeader.CEH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			exitHeader.CEH_ReferenceNumber = "R12345678";
			exitDetail = exitHeader.CusExitDetails.AddNew();
			exitDetail.CED_MovementReferenceNumber = "M12345678";
			exitDetail.CED_Status = "AAA";
			messageSendingAction = (ExitSummaryMessageSendingAction)GetNewBusinessObject();
		}
		ExitSummaryMessageSendingAction messageSendingAction;
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
