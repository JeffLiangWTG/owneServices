using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IT.GUI.Testing;

sealed class MessageUserControlTest : TestCaseWithFactory
{
	public void TestSetUpEntryHeaderColumns()
	{
		using (var control = new MessageUserControl())
		{
			var entriesBoundGrid = (ZGrid)control.Controls.Find("EntriesBoundGrid", true).SingleOrDefault();
			AssertNotNull(entriesBoundGrid);
			AssertEquals("Visible ColumnStyles Count", 22, entriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Count(x => x.IsVisible));

			void AssertColumnOrder(ZString columnName, ZInt index)
			{
				var columnStyleList = entriesBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
				var columnStyle = columnStyleList.SingleOrDefault(x => x.ColumnName == columnName);
				AssertNotNull($"{columnName} not null", columnStyle);
				Assert($"{columnName}", columnStyle.IsVisible);
				AssertEquals(index, Array.IndexOf(columnStyleList, columnStyle));
			}

			CombineAssertions("Column Styles", () =>
			{
				AssertColumnOrder(CusEntryHeader.Schema.EntryTypeFriendlyName, 0);
				AssertColumnOrder(CusEntryHeader.Schema.CH_BGMReference, 1);
				AssertColumnOrder("CusEntryNumber+CE_IssueDate", 2);
				AssertColumnOrder(CusEntryHeader.Schema.CH_IncoTerm, 3);
				AssertColumnOrder(CusEntryHeader.Schema.PackagesCount, 4);
				AssertColumnOrder(CusEntryHeader.Schema.InvoiceAmount, 5);
				AssertColumnOrder(CusEntryHeader.Schema.InvoiceAmountCurrency, 6);
				AssertColumnOrder(CusEntryHeader.Schema.CH_FreightAdjustment, 7);
				AssertColumnOrder(CusEntryHeader.Schema.Duty, 8);
				AssertColumnOrder(CusEntryHeader.Schema.VAT, 9);
				AssertColumnOrder(CusEntryHeader.Schema.CH_Status, 10);
				AssertColumnOrder(CusEntryHeader.Schema.MessageStatusDescription, 11);
				AssertColumnOrder(CusEntryHeader.Schema.CH_EntryStatus, 12);
				AssertColumnOrder(CusEntryHeader.Schema.EntryHeaderStatusDescription, 13);
				AssertColumnOrder(CusEntryHeader.Schema.CH_MessageType, 14);
				AssertColumnOrder(CusEntryHeader.Schema.CH_MessageTypeDescription, 15);
				AssertColumnOrder(CusEntryHeader.Schema.EntryNumber, 16);
				AssertColumnOrder("CusEntryNumber+CE_EntryLineReference", 17);
				AssertColumnOrder(CusEntryHeader.Schema.CH_EntrySubmittedDate, 18);
				AssertColumnOrder(CusEntryHeader.Schema.MovementReferenceNumber, 19);
				AssertColumnOrder("EntryNumbersProvider+ReleaseInfo+CE_EntryNum", 20);
				AssertColumnOrder(CusEntryHeader.Schema.CH_EntryReleaseDate, 21);
			});
		}
	}

	public void TestSetUpEntryLinesColumns()
	{
		var testCases = new List<(string ColumnName, bool IsVisible)>
		{
			new ("CL_LineNumber", true),
			new ("LineSubmissionStatusDescription", true),
			new ("FormattedTariff", true),
			new ("EffectiveDescription", true),
			new (CusEntryLine.Schema.EffectiveGrossWeightKg, true),
			new (CusEntryLine.Schema.EffectiveNetWeightKg, true),
			new (CusEntryLine.Schema.EffectiveCustomsWeightKg, true),
			new (CusEntryLine.Schema.EffectiveSupplementaryQuantity, true),
			new (CusEntryLine.Schema.NumberOfPackages, true),
			new ("DutyAmount", true),
			new ("CL_DutyPercent", true),
			new ("GSTVATAmount", true),
			new ("GSTVATDeferred", true),
			new ("CL_CustomsValue", true),
			new ("CL_StatisticalValue", true),
			new (CusEntryLine.Schema.ZG_LinesValue, true),
			new (CusEntryLine.Schema.LinesValueInInvoiceCurrency, true),
			new (CusEntryLine.Schema.ZG_AdjustmentAmount, true),
			new ("ProcedureCodeWithoutConcession", true),
			new ("CountryOfOriginCode", true),
			new ("PreferenceCode", true),
			new ("ValuationMethod", true),
			new ("PackageType", true),
			new (CusEntryLine.Schema.SteelType, true),
			new ("QuotaOrderNumber", true),
			new (CusEntryLine.Schema.ReleaseCode, true),
			new (CusEntryLine.Schema.ReleaseDate, true),
		};

		using (var userControl = new MessageUserControl())
		{
			var entryLineGrid = (ZGrid)userControl.Controls.Find("EntryLineGrid", true).SingleOrDefault();
			AssertNotNull(entryLineGrid);

			var columnStyleList = entryLineGrid.ColumnStyles.Cast<ZGridColumnInfo>();
			AssertEquals("Column list length should equal test case count", testCases.Count, columnStyleList.Count());

			var actualColumnOrder = string.Join(", ", columnStyleList.Select(x => x.ColumnName));
			var expectedColumnOrder = string.Join(", ", testCases.Select(x => x.ColumnName));
			AssertEquals("Column Order", expectedColumnOrder, actualColumnOrder);

			CombineAssertions("Validate columns", () =>
			{
				foreach (var testCase in testCases)
				{
					var columnName = testCase.ColumnName;
					var columnStyle = entryLineGrid.GetColumnStyle(columnName);
					AssertNotNull($"User control should have '{columnName}' column", columnStyle);
					AssertEquals($"Column '{columnName}' visibility should be '{testCase.IsVisible}'", testCase.IsVisible, columnStyle.IsVisible);
				}
			});
		}
	}

	public void TestGroupedPreviousDocumentsUserControl()
	{
		using (var userControl = new MessageUserControl())
		{
			var entryLinesMessagesTabControl = userControl.FindSingleOrDefault<ZTabControl>("EntryLinesMessagesTabControl");
			AssertNotNull(nameof(entryLinesMessagesTabControl), entryLinesMessagesTabControl);

			var allM2LinesTabPage = userControl.FindSingleOrDefault<ZTabPage>("AllM2LinesTabPage");
			AssertNotNull(nameof(allM2LinesTabPage), allM2LinesTabPage);

			entryLinesMessagesTabControl.SelectTab(allM2LinesTabPage);
			var groupedPreviousDocumentsUserControl = allM2LinesTabPage.FindSingleOrDefault<GroupedPreviousDocumentsUserControl>("GroupedPreviousDocumentsUserControl");
			AssertNotNull(nameof(groupedPreviousDocumentsUserControl), groupedPreviousDocumentsUserControl);
			AssertEquals($"{nameof(groupedPreviousDocumentsUserControl)} visibility", true, groupedPreviousDocumentsUserControl.Visible);
		}
	}

	public void TestAllM2LinesTabPage()
	{
		using (var userControl = new MessageUserControl())
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			userControl.SetDataBinding(declaration, "");
			var allM2LinesTabPage = GetAllM2LinesTabPageControl();
			AssertNull("AllM2LinesTabPage, for IMP", allM2LinesTabPage);

			declaration.JE_MessageType = "EXP";
			declaration.MessageVersion = "TXT";
			allM2LinesTabPage = GetAllM2LinesTabPageControl();
			AssertEquals("AllM2LinesTabPage, Visibility for EXP with UCC6=false", true, allM2LinesTabPage.TabVisible);

			declaration.MessageVersion = "XML";
			allM2LinesTabPage = GetAllM2LinesTabPageControl();
			AssertNull("AllM2LinesTabPage, for EXP with UCC6=true", allM2LinesTabPage);

			ZTabPage GetAllM2LinesTabPageControl() => userControl.FindSingleOrDefault<ZTabPage>("AllM2LinesTabPage");
		}
	}

	public void TestEntryLineAdditionalDataUserControlType()
	{
		using (var userControl = new MessageUserControl())
		{
			var entryLineAdditionalDataUserControl = userControl.Controls.Find("EntryLineAdditionalDataUserControl", true).FirstOrDefault();
			AssertNotNull(entryLineAdditionalDataUserControl);
			AssertEquals(typeof(EntryLineAdditionalDataUserControl), entryLineAdditionalDataUserControl.GetType());
		}
	}

	public void TestEntryLinesMessagesTabControlPages()
	{
		using (var userControl = new MessageUserControl())
		{
			var tabControl = (ZTabControl)userControl.Controls.Find("EntryLinesMessagesTabControl", true).FirstOrDefault();
			CombineAssertions(() =>
			{
				AssertNotNull(tabControl);
				AssertEquals("NewEntryDetailsTabPage", tabControl.TabPages[0].Name);
				AssertEquals("EntryLinesTabPage", tabControl.TabPages[1].Name);
				AssertEquals("AllM2LinesTabPage", tabControl.TabPages[2].Name);
				AssertEquals("MessageTabPage", tabControl.TabPages[3].Name);
			});
		}
	}
}
