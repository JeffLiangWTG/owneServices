using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(TaxExemptionOrSpecificUseDutyRateUserControl))]
	sealed class SubsequentMessagesUserControlTest : TestCaseWithFactory
	{
		public class Columns
		{
			public ZString DisplayTitle;
			public ZString Names;
			public System.Type InfoType;
		}

		Columns CreateColumnsType(ZString displayTitle, ZString names, System.Type infoType)
		{
			var result = new Columns();
			result.DisplayTitle = displayTitle;
			result.Names = names;
			result.InfoType = infoType;
			return result;
		}

		public void TestApplyingTaxExemptionOrSpecificUseDutyRateGrid()
		{
			using (var control = new ImportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("ApplyingTaxExemptionOrSpecificUseDutyRateGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("EntryLineNo", nameof(MessageSendingEntryLineObject.EntryLineNo), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Message Status", nameof(MessageSendingEntryLineObject.MessageStatus), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Message Status Desc.", nameof(MessageSendingEntryLineObject.MessageStatusDesc), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Accepted Date", nameof(MessageSendingEntryLineObject.AcceptedDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("HS Code", nameof(MessageSendingEntryLineObject.HSCode), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Duty Reduction", nameof(MessageSendingEntryLineObject.DutyReduction), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Post Clearance YN", nameof(MessageSendingEntryLineObject.PostClearanceYN), typeof(ZTextBoxColumnStyleInfo)));
				Assert(grid, columnsList, false);
			}
		}
		public void TestSubsequentMessageRefundRequestGrid()
		{
			using (var control = new ImportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("SubsequentMessageRefundRequestGrid", true)[0];

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("Refund Declaration Number", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.RefundDeclarationNumber), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Nessage Status", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.MessageStatus5UL), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Nessage Status Desc.", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.MessageStatusDesc5UL), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Review Result", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.ReviewResult), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Review Result Desc.", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.ReviewResultDesc), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Accepted Date", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.AcceptedDate5UL), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Customs Disbursement Bill #", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.CustomsDisbursementBillNumber), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Refund Approval Date", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.RefundApprovalDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Refund Approval No.", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.RefundApprovalNumber), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Provision Date", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.ProvisionDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Provision No.", nameof(EDIMessageWrapper.MessageData5UL) + "+" + nameof(RefundRequest.ProvisionNo), typeof(ZTextBoxColumnStyleInfo)));
				Assert(grid, columnsList, false);
			}
		}

		public void TestAgreedRateForAllLinesUserControl()
		{
			using (var control = new ImportMessageUserControl())
			{
				var userControl = control.FindSingle<AgreedRateForAllLinesUserControl>();
				AssertNotNull(userControl.FindSingle<ZDropEdit>("MessageStatusDropEdit")); 
				AssertNotNull(userControl.FindSingle<ZDateEdit>("AcceptedDateEdit"));

				var grid = userControl.FindSingle<ZGrid>("AgreedRateForAllLinesAmendmentGrid");

				var columnsList = new List<Columns>();
				columnsList.Add(CreateColumnsType("Amend Sequence No", nameof(EDIMessageWrapper.ApplicationReference), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Message Status", nameof(EDIMessageWrapper.MessageStatus), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Message Status Desc.", nameof(EDIMessageWrapper.MessageStatusDescription), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Accepted Date", nameof(EDIMessageWrapper.GOVCBR5BBMessage) + "+" + nameof(Import5BBDetails.AcceptedDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Review Date", nameof(EDIMessageWrapper.GOVCBR5BBMessage) + "+" + nameof(Import5BBDetails.ReviewDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Review Result Desc.", nameof(EDIMessageWrapper.GOVCBR5BBMessage) + "+" + nameof(Import5BBDetails.ReviewResultDescription), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType("Amend Reason Description", nameof(EDIMessageWrapper.GOVCBR5BBMessage) + "+" + nameof(Import5BBDetails.AmendReasonDescription), typeof(ZTextBoxColumnStyleInfo)));
				Assert(grid, columnsList, true);
			}
		}

		void Assert(ZGrid grid, List<Columns> columnsList, bool isReadOnly)
		{
			foreach (Columns col in columnsList)
			{
				var checkColumn = grid.GetColumnStyle(col.Names);
				AssertNotNull(col.DisplayTitle + "Column exists", checkColumn);
				AssertEquals(col.InfoType, checkColumn.GetType());
				AssertEquals(isReadOnly, checkColumn.IsReadOnly);
			}
		}
	}
}
