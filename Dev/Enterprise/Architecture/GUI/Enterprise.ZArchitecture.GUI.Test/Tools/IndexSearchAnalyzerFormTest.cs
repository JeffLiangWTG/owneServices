using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI;
using GlowIndexQueryService.Business;

namespace Enterprise.ZArchitecture.DevTools.Testing
{
	sealed class IndexSearchAnalyzerFormTest : TestCaseWithFactory
	{
		public void TestIndexSearchAnalyzerForm_ShouldShowCorrectControls()
		{
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			using (var form = new IndexSearchAnalyzerForm(glowIndexQueryParam))
			{
				form.Show();
				Application.DoEvents();

				var runQueryButton = form.FindSingle<KButton>("RunQueryButton");
				var openUrlButton = form.FindSingle<KButton>("OpenUrlButton");
				var formatUriCheckBox = form.FindSingle<KCheckBox>("FormatUriCheckBox");
				var queryStatusTextBox = form.FindSingle<KTextBox>("QueryStatusTextBox");
				var resultsTextBox = form.FindSingle<KTextBox>("ResultsTextBox");
				var indexSearchUriTextBox = form.FindSingle<KTextBox>("IndexSearchUriTextBox");

				Assert(runQueryButton.Enabled);
				Assert(openUrlButton.Enabled);
				Assert(formatUriCheckBox.Checked);
				AssertEquals("", queryStatusTextBox.Text);
				AssertEquals("", resultsTextBox.Text);
				AssertEquals(@"https://localhost/Glow/odata/Index/EntityInfos?$top=50&$filter=
(
	Code eq 'WY'
)
AND
EntityType eq 'IGlbStaff'
", indexSearchUriTextBox.Text);
			}
		}

		public void TestIndexSearchAnalyzerForm_ShouldShowFormattedUri()
		{
			var stringTerm = new Term("CODE", "WY");
			var boolTerm = new Term("IsSystemAccount", "true");
			var emptyQuery = new EmptyQuery();
			var equalQueryWithQuotesAndNotExact = new EqualQuery(stringTerm, true, false);
			var equalQueryWithQuotesAndExact = new EqualQuery(stringTerm, true, true);
			var equalQueryWithoutQuotesAndNotExact = new EqualQuery(boolTerm, false, false);
			var notEqualQueryWithQuotes = new NotEqualQuery(stringTerm);
			var notEqualQueryWithoutQuotes = new NotEqualQuery(boolTerm, false);
			var prefixQuery = new PrefixQuery(stringTerm);
			var isBlankQueryWithIncludeEmptyString = new IsBlankQuery(stringTerm);
			var isBlankQueryWithOUTIncludeEmptyString = new IsBlankQuery(stringTerm, false);
			var isNotBlankQueryWithIncludeEmptyString = new IsNotBlankQuery(stringTerm);
			var isNotBlankQueryWithoutIncludeEmptyString = new IsNotBlankQuery(stringTerm, false);
			var booleanQueryWith1Level = new BooleanQuery(BooleanOperator.And, equalQueryWithQuotesAndNotExact, equalQueryWithQuotesAndExact, equalQueryWithoutQuotesAndNotExact, notEqualQueryWithQuotes, notEqualQueryWithoutQuotes, prefixQuery);
			var booleanQueryWith2Levels = new BooleanQuery(BooleanOperator.And, booleanQueryWith1Level, isBlankQueryWithIncludeEmptyString, isBlankQueryWithOUTIncludeEmptyString);
			var booleanQueryWith3Levels = new BooleanQuery(BooleanOperator.Or, booleanQueryWith2Levels, isNotBlankQueryWithIncludeEmptyString, isNotBlankQueryWithoutIncludeEmptyString);
			var notQuery = new NotQuery(booleanQueryWith3Levels);

			var indexQueryParam = new GlowIndexQueryParam(new List<IGlowQuery>() { notQuery, emptyQuery }, "IGlbStaff");
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			using (var form = new IndexSearchAnalyzerForm(indexQueryParam))
			{
				form.Show();
				Application.DoEvents();
				var indexSearchUriTextBox = form.FindSingle<KTextBox>("IndexSearchUriTextBox");
				AssertEquals(@"https://localhost/Glow/odata/Index/EntityInfos?$top=50&$filter=
(
	not
	(
		(
			(
				(
					CODE eq 'WY'
				)
				AND
				(
					CODE eq '""WY""'
				)
				AND
				(
					IsSystemAccount eq true
				)
				AND
				(
					CODE ne 'WY'
				)
				AND
				(
					IsSystemAccount ne true
				)
				AND
				(
					startswith
					(
						CODE,'WY'
					)
				)
			)
			AND
			(
				(
					CODE eq null
				)
				OR
				(
					CODE eq ''
				)
			)
			AND
			(
				CODE eq null
			)
		)
		OR
		(
			(
				CODE ne null
			)
			AND
			(
				CODE ne ''
			)
		)
		OR
		(
			CODE ne null
		)
	)
)
AND
EntityType eq 'IGlbStaff'
", indexSearchUriTextBox.Text);
			}
		}

		public void TestRunQuery_WhenGlowServiceIsUnavailable_ShouldDisplayErrorMessage()
		{
			IndexSearchFilterHelper.ResetIndexQueryEngine();
			using (GlowRegistry.Instance.GlowUseIndexingServiceForGlobalSearch.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ""))
			using (var form = new IndexSearchAnalyzerForm(glowIndexQueryParam))
			{
				form.Show();
				Application.DoEvents();

				var runQueryButton = form.FindSingle<KButton>("RunQueryButton");
				runQueryButton.PerformClick();
				Application.DoEvents();

				var queryStatusTextBox = form.FindSingle<KTextBox>("QueryStatusTextBox");
				var resultsTextBox = form.FindSingle<KTextBox>("ResultsTextBox");

				AssertEquals("Query Failed.", queryStatusTextBox.Text);
				AssertEquals("Unknown Error: Invalid URI: The format of the URI could not be determined.", resultsTextBox.Text);
				AssertEquals(System.Drawing.Color.Red, resultsTextBox.ForeColor);
				Assert(runQueryButton.Enabled);
				AssertEquals(Cursors.Default, form.Cursor);
			}
		}

		public void TestRunQuery_WhenGlowServiceIsAvailable()
		{
			using (new GlowIndexQueryEngineMock())
			using (GlowRegistry.Instance.GlowMaximumNumberOfModuleFiltersSearchResults.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 10))
			using (var form = new IndexSearchAnalyzerForm(glowIndexQueryParam))
			{
				form.Show();
				Application.DoEvents();

				var runQueryButton = form.FindSingle<KButton>("RunQueryButton");
				var queryStatusTextBox = form.FindSingle<KTextBox>("QueryStatusTextBox");
				var resultsTextBox = form.FindSingle<KTextBox>("ResultsTextBox");
				runQueryButton.PerformClick();
				Application.DoEvents();

				var resultPanel = form.FindSingle<KPanel>("ResultPanel");
				var dataTableGridView = resultPanel.FindSingle<DataTableGridView>();
				AssertEquals("Query Completed.", queryStatusTextBox.Text);
				AssertEquals($"1 record(s) returned, the max count of records is 10 ({GlowRegistry.Instance.GlowMaximumNumberOfModuleFiltersSearchResults.HumanReadableRegistryPath()})", resultsTextBox.Text);
				AssertNotNull(dataTableGridView);

				var visibleMenuItems = new string[] { "Select All", "Copy", "Copy With Headers", "Copy As CSV" };
				var invisibleMenuItems = new string[] { "Copy As Insert SQL" };
				foreach (var visibleMenuItem in visibleMenuItems)
				{
					AssertNotNull(dataTableGridView.ExtensionMenu.MenuItems.FindByText(visibleMenuItem));
				}
				foreach (var invisibleMenuItem in invisibleMenuItems)
				{
					AssertNull(dataTableGridView.ExtensionMenu.MenuItems.FindByText(invisibleMenuItem));
				}
			}
		}

		public void TestOpenInBrowser()
		{
			using (GlowRegistry.Instance.GlowServiceUriRegistryItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://localhost/Glow"))
			using (var form = new IndexSearchAnalyzerForm(glowIndexQueryParam))
			{
				form.Show();
				Application.DoEvents();

				var openUrlButton = form.FindSingle<KButton>("OpenUrlButton");
				openUrlButton.PerformClick();
				Application.DoEvents();

				AssertEquals(@"https://localhost/Glow/odata/Index/EntityInfos?$top=50&$filter=(Code eq 'WY') and EntityType eq 'IGlbStaff'", WebUrlLauncher.LastUrlLaunched);
				WebUrlLauncher.ClearLastUrlLaunched();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			var equalQuery = new EqualQuery(new Term("Code", "WY"));
			glowIndexQueryParam = new GlowIndexQueryParam(new List<IGlowQuery>() { equalQuery }, "IGlbStaff");
		}

		GlowIndexQueryParam glowIndexQueryParam;
	}
}
