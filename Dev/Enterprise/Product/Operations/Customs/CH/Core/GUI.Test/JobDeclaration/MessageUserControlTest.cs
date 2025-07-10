using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.Common.CH;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(MessageUserControl))]
sealed class MessageUserControlTest : TestCaseWithFactory
{
	public void TestTaxOrFeeTabPage()
	{
		using (var userControl = new MessageUserControl())
		{
			var tab = (ZTabPage)userControl.Controls.Find("TaxOrFeeTabPage", true).SingleOrDefault();
			AssertNotNull("Tax Or Fee tab page exists", tab);
		}
	}

	public void TestEntryLinesMessagesTabPages_Import() => AssertEntryLinesMessagesTabPages(CHJobMessageTypeList.Codes.Import, new[] { "MessageTabPage", "EComMessageTabPage", "EntryHeaderChargesTabPage", "EntryLinesTabPage" });

	public void TestEntryLinesMessagesTabPages_Export() => AssertEntryLinesMessagesTabPages(CHJobMessageTypeList.Codes.Export, new[] { "MessageTabPage", "EntryLinesTabPage" });

	public void TestEntryLinesMessagesTabPages_EDA() => AssertEntryLinesMessagesTabPages(CHJobMessageTypeList.Codes.ExportDeclarationActivation, new[] { "MessageTabPage", "EntryLinesTabPage" });

	void AssertEntryLinesMessagesTabPages(string messageType, string[] expectedTabPages)
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = messageType;

		using (var form = new ZForm(declaration))
		using (var userControl = new MessageUserControl())
		{
			form.Controls.Add(userControl);
			form.Show();

			var entryLinesMessagesTabControl = (ZTabControl)userControl.Controls.Find("EntryLinesMessagesTabControl", true).SingleOrDefault();
			AssertContainsExactElementsInExactOrder("TabPages in order", expectedTabPages, entryLinesMessagesTabControl.TabPages.Cast<ZTabPage>().Select(x => x.Name));

			var nonEComMessagesUserControl = entryLinesMessagesTabControl.TabPages[0].Controls[0] as ZUserControl;
			AssertEquals("CustomsEntryHeaders.NonEComMessages", userControl.BindingSource.GetBindingMember(nonEComMessagesUserControl));

			if(messageType == CHJobMessageTypeList.Codes.Import)
			{
				var eComMessagesUserControl = entryLinesMessagesTabControl.TabPages[1].Controls[0] as ZUserControl;
				AssertType<EComMessagesTabUserControl>(eComMessagesUserControl);
				AssertEquals("CustomsEntryHeaders.EComMessages", userControl.BindingSource.GetBindingMember(eComMessagesUserControl));
			}
		}
	}

	public void TestEntriesBoundGridColumns()
	{
		using (var userControl = new MessageUserControl())
		{
			var index = 0;
			var entriesBoundGridColumnStyles = userControl.EntriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.MovementReferenceNumber, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.CH_BGMReference, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.CH_EntryStatus, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.EntryHeaderStatusDescription, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.CH_Status, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.MessageStatusDescription, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.CH_PhaseStatus, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.PhaseStatusDescription, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.SelectionResult, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.SelectionResultDescription, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.CH_EntrySubmittedDate, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.CH_EntryReleaseDate, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.AccessCode, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.PackagesCount, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.TotalDutyAmount, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, CusEntryHeader.Schema.GSTAmount, index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, nameof(CusEntryHeader.ConfirmedDuty), index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, nameof(CusEntryHeader.ConfirmedVAT), index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, nameof(CusEntryHeader.EComMessageStatusDescription), index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, nameof(CusEntryHeader.MovementReferenceNumberIssueDate), index++);
				UserControlTestHelper.AssertColumnStyles(entriesBoundGridColumnStyles, nameof(CusEntryHeader.MovementReferenceNumberExpiryDate), index++);
			});
		}
	}

	public void TestEntryLineGridGridColumns()
	{
		using (var userControl = new MessageUserControlForTest())
		{
			var index = 0;
			var invoiceChargesGridColumnStyle = userControl.EntryLineGridExposed.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, CusEntryLine.Schema.CL_LineNumber, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, CusEntryLine.Schema.LineSubmissionStatusDescription, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, CusEntryLine.Schema.FormattedTariff, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, CusEntryLine.Schema.EffectiveDescription, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, CusEntryLine.Schema.DutyAmount, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, CusEntryLine.Schema.GSTVATAmount, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, nameof(CusEntryLine.ConfirmedDuty), index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, nameof(CusEntryLine.ConfirmedVAT), index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, CusEntryLine.Schema.CL_CustomsValue, index++);
				UserControlTestHelper.AssertColumnStyles(invoiceChargesGridColumnStyle, CusEntryLine.Schema.CL_StatisticalValue, index++);
			});
		}
	}

	public void TestEntryHeaderChargesGridColumns()
	{
		using (var userControl = new MessageUserControlForTest())
		{
			var index = 0;
			var entryHeaderChargesGridColumnStyle = userControl.EntryHeaderChargesGridExposed.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();

			CombineAssertions(() =>
			{
				UserControlTestHelper.AssertColumnStyles(entryHeaderChargesGridColumnStyle, CusEntryHeaderCharges.Schema.C1_ChargeType, index++);
				UserControlTestHelper.AssertColumnStyles(entryHeaderChargesGridColumnStyle, nameof(CusEntryHeaderCharges.ChargeTypeDescription), index++);
				UserControlTestHelper.AssertColumnStyles(entryHeaderChargesGridColumnStyle, CusEntryHeaderCharges.Schema.C1_ChargeAmount, index++);
			});
		}
	}
}

#region MessageUserControlForTest

public class MessageUserControlForTest : MessageUserControl
{
	public MessageUserControlForTest()
		: base()
	{
	}

	public ZGrid EntryLineGridExposed => EntryLineGrid;
	public ZGrid EntryHeaderChargesGridExposed => EntryHeaderChargesGrid;
}
#endregion
