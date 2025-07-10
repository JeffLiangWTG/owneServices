using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IL.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.GUI.Testing
{
	[TestedType(typeof(EntriesTabUserControl))]
	sealed class EntriesTabUserControlTest : TestCaseWithFactory
	{
		public void TestGetBaseMessagesTabUserControlType()
		{
			using (var control = new EntriesTabUserControl())
			{
				AssertEquals("MessagesTab is correct type", typeof(CustomsMessagingControl), control.BaseMessageUserControl.UserControlType);
			}
		}

		public void TestMessagesUserControlBindingPath()
		{
			using (var control = new EntriesTabUserControlForTest())
			{
				AssertEquals("Messages User Control Binding Path", "CustomsEntryHeaders", control.MessagesUserControlBindingPath_Exposed);
			}
		}

		public void TestEntryFeesTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var entriesTabUserControl = new EntriesTabUserControl())
			{
				form.Controls.Add(entriesTabUserControl);
				form.Show();

				var entryFeesTabPage = entriesTabUserControl.FindSingle<ZTabPage>("EntryFeesTabPage");
				entryFeesTabPage.Show();

				CombineAssertions(() =>
				{
					AssertEquals("EntryFeesTabPage visible", true, entryFeesTabPage.TabVisible);
					AssertEquals("Caption", "Entry Fees", entryFeesTabPage.CaptionResourceString.Caption);

					AssertEntryFeeGrid(entriesTabUserControl, "EntryFeesGrid", "Calculated");
					AssertEntryFeeGrid(entriesTabUserControl, "ConfirmedFeesGrid", "Confirmed");
				});
			}
		}

		public void TestEntryLineAdditionalDataUserControlType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.ActiveEntryHeaders.AddNew();

			using (var form = new ZForm(declaration))
			using (var entriesTabUserControl = new EntriesTabUserControl())
			{
				form.Controls.Add(entriesTabUserControl);
				form.Show();
				var entryLineAdditionalDataUserControl = entriesTabUserControl.Controls.Find(nameof(EntryLineAdditionalDataUserControl), true).FirstOrDefault();

				AssertNotNull(entryLineAdditionalDataUserControl);
				AssertEquals(typeof(EntryLineAdditionalDataUserControl), entryLineAdditionalDataUserControl.GetType());
			}
		}

		public void TestEntryLineGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var entriesTabUserControl = new EntriesTabUserControl())
			{
				form.Controls.Add(entriesTabUserControl);
				form.Show();
				var entryLineGrid = (ZGrid)entriesTabUserControl.Controls.Find("EntryLineGrid", true).FirstOrDefault();

				AssertNotNull(entryLineGrid);
				var columns = entryLineGrid.ColumnStyles.ToArray().Cast<ZGridColumnInfo>();
				AssertEquals("EntryLineGrid contain 9 columns", 9, columns.Count());
				AssertEntryLineGridColumn(columns, "CL_LineNumber", "Line Number", true, 75);
				AssertEntryLineGridColumn(columns, "LineSubmissionStatusDescription", "Status", true, 80);
				AssertEntryLineGridColumn(columns, "CL_AdValoremTariff", "Tariff", true, 80);
				AssertEntryLineGridColumn(columns, "CL_Description", "Description", true, 120);
				AssertEntryLineGridColumn(columns, "CL_ConfirmedCustomsValue", "Customs Value", true, 100);
				AssertEntryLineGridColumn(columns, "CL_ConfirmedStatisticalValue", "Statistical Value", true, 100);
				AssertEntryLineGridColumn(columns, "CL_InvoiceAmount", "Invoice Amount", false, 100);
				AssertEntryLineGridColumn(columns, "CL_RX_NKInvoiceAmountCurrency", "Currency", false, 60);
				AssertEntryLineGridColumn(columns, "CL_SystemLastEditTimeUtc", "Last Edited Time (UTC)", false, 125);
			}
		}

		static void AssertEntryLineGridColumn(IEnumerable<ZGridColumnInfo> columns, string columnName, string caption, bool isVisible, int width)
		{
			var culumn = columns.FirstOrDefault(x => x.ColumnName == columnName);
			AssertNotNull($"{columnName} column exist", culumn);
			AssertEquals($"{columnName} IsReadOnly", true, culumn.IsReadOnly);
			AssertEquals($"{columnName} Caption", caption, culumn.CaptionResourceString?.Caption);
			AssertEquals($"{columnName} IsVisible", isVisible, culumn.IsVisible);
			AssertEquals($"{columnName} Width", width, culumn.Width);
		}

		void AssertEntryFeeGrid(EntriesTabUserControl entriesTabUserControl, ZString gridName, ZString expectedCaption)
		{
			var entryFeesGrid = (ZGrid)entriesTabUserControl.Controls.Find(gridName, true).SingleOrDefault();
			AssertEquals(gridName + ": grid caption.", true, entryFeesGrid.CaptionText.Contains(expectedCaption));
			AssertEquals(gridName + ": grid caption is visible.", true, entryFeesGrid.CaptionVisible);
			AssertEquals(gridName + ": ConfirmedFeesGrid should be readonly.", gridName == "ConfirmedFeesGrid", entryFeesGrid.ReadOnly);
			AssertNotNull(gridName + ": ChargeType column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_ChargeType));
			AssertEquals(gridName + ": C1_ChargeType column caption.", "Fee Code", entryFeesGrid.GetColumnCaption(CusEntryHeaderCharges.Schema.C1_ChargeType));
			AssertNotNull(gridName + ": Amount column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_ChargeAmount));
			AssertEquals(gridName + ": Amount column caption.", "Amount", entryFeesGrid.GetColumnCaption(CusEntryHeaderCharges.Schema.C1_ChargeAmount));
			AssertNotNull(gridName + ": MethodOfPayment column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_MethodOfPayment));
			AssertEquals(gridName + ": MethodOfPayment column caption.", "Method Of Payment", entryFeesGrid.GetColumnCaption(CusEntryHeaderCharges.Schema.C1_MethodOfPayment));
			if (gridName == "EntryFeesGrid")
			{
				AssertNotNull("C1_RateOverrideReasonCode column", entryFeesGrid.GetColumnStyle(CusEntryHeaderCharges.Schema.C1_RateOverrideReasonCode));
				AssertEquals(gridName + ": C1_RateOverrideReasonCode column caption.", "Action", entryFeesGrid.GetColumnCaption(CusEntryHeaderCharges.Schema.C1_RateOverrideReasonCode));
			}
		}

		class EntriesTabUserControlForTest : EntriesTabUserControl
		{
			internal string MessagesUserControlBindingPath_Exposed => MessagesUserControlBindingPath;
		}
	}
}
