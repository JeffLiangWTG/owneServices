using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DocumentMenu.Testing.DocumentCommandTest;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	[TestedType(typeof(JobDocumentRecipientConfigurationForm))]
	sealed class JobDocumentRecipientConfigurationFormTest : ZFormBasherTest
	{
		#region Filtering Tests

		public void TestFilterFindButton()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = organisation.Contacts.AddNew();
			contact.FillWithValidTestData();
			var orgDocument = contact.Documents.AddNew();
			orgDocument.OD_DocumentGroup = ContactType.All.Code;

			Factory.Save();

			using (var form = GetFormToBashCore() as JobDocumentRecipientConfigurationForm)
			{
				form.Show();

				AssertEquals("Pre-condition", 0, form.Configuration.OrgDocumentRecipients.Count);

				form.FilterFindButton.PerformClick();
				AssertEquals("No filters selected by default, the grid should be empty", 0, form.Configuration.OrgDocumentRecipients.Count);

				form.FilterOrganisationtFindBox.CodeBox.Text = organisation.OH_Code;
				form.FilterOrganisationtFindBox.CommitBoundValue();
				form.FilterFindButton.PerformClick();
				AssertEquals(1, form.Configuration.OrgDocumentRecipients.Count);
				AssertCollectionContains(form.Configuration.OrgDocumentRecipients.Cast<JobDocumentRecipientWrapperBase>(), r => r.WrappedBizoPK == orgDocument.PK);
			}
		}

		public void TestFilterClearButton()
		{
			var document = Factory.New<StmMenuItem>();
			document.SU_MenuName = "Name";
			document.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document.SU_BusinessContext = documentSupportable.DocumentSupporter.BusinessContext.ToString();
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_MenuPath = "Menu/Path";

			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			using (var form = GetFormToBashCore() as JobDocumentRecipientConfigurationForm)
			{
				form.Show();
				form.FilterDocumentFindBox.CodeBox.Text = document.DocumentId;
				form.FilterDocumentFindBox.CommitBoundValue();
				form.FilterOrganisationtFindBox.CodeBox.Text = organisation.OH_Code;
				form.FilterOrganisationtFindBox.CommitBoundValue();
				form.FilterDocumentGroupDropEdit.Text = ContactType.Receivables.Code;
				form.FilterDocumentGroupDropEdit.CommitBoundValue();

				Assert("Pre-conditon", string.Equals(document.DocumentId, form.FilterDocumentFindBox.CodeBox.Text, StringComparison.OrdinalIgnoreCase));
				AssertEquals("Pre-conditon", organisation.OH_Code, form.FilterOrganisationtFindBox.CodeBox.Text);
				AssertEquals("Pre-conditon", ContactType.Receivables.Code, form.FilterDocumentGroupDropEdit.Text);
				AssertEquals("Pre-conditon", document.PK, configuration.DocumentPKFilter);
				AssertEquals("Pre-conditon", organisation.PK, configuration.OrganisationPKFilter);
				AssertEquals("Pre-conditon", ContactType.Receivables.Code, configuration.DocumentGroupFilter);

				form.FilterClearButton.PerformClick();
				AssertEquals(string.Empty, form.FilterDocumentFindBox.CodeBox.Text);
				AssertEquals(string.Empty, form.FilterOrganisationtFindBox.CodeBox.Text);
				AssertEquals(string.Empty, form.FilterDocumentGroupDropEdit.Text);
				Assert(configuration.DocumentPKFilter.IsEmpty);
				Assert(configuration.OrganisationPKFilter.IsEmpty);
				Assert(configuration.DocumentGroupFilter.IsEmpty);
			}
		}

		public void TestBindToFilters()
		{
			var document = Factory.New<StmMenuItem>();
			document.SU_MenuName = "Name";
			document.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			document.SU_BusinessContext = documentSupportable.DocumentSupporter.BusinessContext.ToString();
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_MenuPath = "Menu/Path";

			var organisation = Factory.NewWithValidTestData<OrgHeader>();

			Factory.Save();

			using (var form = GetFormToBashCore() as JobDocumentRecipientConfigurationForm)
			{
				form.Show();

				AssertEquals("FilterDocumentFindBox.BindTo", nameof(configuration.DocumentPKFilter), form.FilterDocumentFindBox.BindTo);
				AssertEquals("FilterOrganisationtFindBox.BindTo", nameof(configuration.OrganisationPKFilter), form.FilterOrganisationtFindBox.BindTo);
				AssertEquals("FilterDocumentGroupDropEdit.BindTo", nameof(configuration.DocumentGroupFilter), form.FilterDocumentGroupDropEdit.BindTo);

				Assert("Pre-conditon", configuration.DocumentPKFilter.IsEmpty);
				Assert("Pre-conditon", configuration.OrganisationPKFilter.IsEmpty);
				Assert("Pre-conditon", configuration.DocumentGroupFilter.IsEmpty);

				form.FilterDocumentFindBox.CodeBox.Text = document.DocumentId;
				form.FilterDocumentFindBox.CommitBoundValue();
				AssertEquals(document.PK, configuration.DocumentPKFilter);
				Assert(configuration.OrganisationPKFilter.IsEmpty);
				Assert(configuration.DocumentGroupFilter.IsEmpty);

				form.FilterOrganisationtFindBox.CodeBox.Text = organisation.OH_Code;
				form.FilterOrganisationtFindBox.CommitBoundValue();
				AssertEquals(document.PK, configuration.DocumentPKFilter);
				AssertEquals(organisation.PK, configuration.OrganisationPKFilter);
				Assert(configuration.DocumentGroupFilter.IsEmpty);

				form.FilterDocumentGroupDropEdit.Text = ContactType.Receivables.Code;
				form.FilterDocumentGroupDropEdit.CommitBoundValue();
				AssertEquals(document.PK, configuration.DocumentPKFilter);
				AssertEquals(organisation.PK, configuration.OrganisationPKFilter);
				AssertEquals(ContactType.Receivables.Code, configuration.DocumentGroupFilter);
			}
		}

		#endregion

		#region Additional Columns Tests

		[RequiresSTA]
		public void TestJobDocumentRecipientsGridContainsEmailMacroColumn()
		{
			using (var form = GetFormToBashCore() as JobDocumentRecipientConfigurationForm)
			{
				form.Show();

				var macroColumn = form.JobDocumentRecipientsGrid.ColumnStyles.OfType<ZMacrosFindBoxColumnStyleInfo>().First();
				AssertNotNull(macroColumn);
				AssertNotNull(macroColumn.Roots);
			}
		}

		public void TestJobDocumentRecipientsGridContainsEmailRecipientsColumns()
		{
			using (var form = GetFormToBashCore() as JobDocumentRecipientConfigurationForm)
			{
				form.Show();

				var emailColumns = form.JobDocumentRecipientsGrid.ColumnStyles.OfType<NonPersistentCopyRecipientsColumnStyleInfo<JobDocumentRecipientWrapperBase>>().ToList();
				AssertEquals(3, emailColumns.Count);
				var emailToColumn = emailColumns.Single(c => c.ColumnName == nameof(JobDocumentRecipientWrapperBase.EmailToRecipientsAsString));
				var carbonCopyColumn = emailColumns.Single(c => c.ColumnName == nameof(JobDocumentRecipientWrapperBase.CarbonCopyRecipientsAsString));
				var blindCarbonCopyColumn = emailColumns.Single(c => c.ColumnName == nameof(JobDocumentRecipientWrapperBase.BlindCarbonCopyRecipientsAsString));

				var recipient = form.Configuration.JobDocumentRecipients.AddNew();
				recipient.EmailToRecipientsAsString = "a@a.aa";
				recipient.CarbonCopyRecipientsAsString = "b@b.bb";
				recipient.BlindCarbonCopyRecipientsAsString = "c@c.cc";
				form.Configuration.JobDocumentRecipients.RefreshBinding();

				var cell = new DataGridCell(0, form.JobDocumentRecipientsGrid.ColumnStyles.IndexOf(emailToColumn));
				AssertEquals("a@a.aa", form.JobDocumentRecipientsGrid[cell].ToString());

				cell = new DataGridCell(0, form.JobDocumentRecipientsGrid.ColumnStyles.IndexOf(carbonCopyColumn));
				AssertEquals("b@b.bb", form.JobDocumentRecipientsGrid[cell].ToString());

				cell = new DataGridCell(0, form.JobDocumentRecipientsGrid.ColumnStyles.IndexOf(blindCarbonCopyColumn));
				AssertEquals("c@c.cc", form.JobDocumentRecipientsGrid[cell].ToString());
			}
		}

		#endregion

		#region Suggestions Tests

		[RequiresSTA]
		public void TestHideSuggestedOrganisationsIfUnavailable()
		{
			using (var form = new JobDocumentRecipientConfigurationForm(configuration))
			{
				form.Show();
				Assert("Pre-condition", !form.Configuration.SupportsSuggestions);
				Assert(form.MainSplitContainer.Panel1Collapsed);
				Assert(!form.MainSplitContainer.Panel1.Visible);
			}

			configuration.SuggestedOrganisations.Add(new SuggestedOrganisation((NoResString)"Consignee", Factory.New<OrgHeader>()));
			using (var form = new JobDocumentRecipientConfigurationForm(configuration))
			{
				form.Show();
				Assert("Pre-condition", form.Configuration.SupportsSuggestions);
				Assert(!form.MainSplitContainer.Panel1Collapsed);
				Assert(form.MainSplitContainer.Panel1.Visible);
			}
		}

		#endregion

		#region Exclude Org Document Test

		public void TestExcludeOrgDocument()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = organisation.Contacts.AddNew();
			contact.FillWithValidTestData();
			var orgDocument = contact.Documents.AddNew();
			orgDocument.OD_DocumentGroup = ContactType.All.Code;

			Factory.Save();

			using (var form = GetFormToBashCore() as JobDocumentRecipientConfigurationForm)
			{
				form.Show();
				AssertEquals("Pre-condition", 0, form.Configuration.OrgDocumentRecipients.Count);

				var menuItem = form.OrgDocumentRecipientsGrid.ContextMenu.MenuItems[0];
				AssertEquals("Exclude", menuItem.Text);

				form.OrgDocumentRecipientsGrid.ContextMenu.DoPopup();
				Assert("No records in the grid yet, so Exclude should be disabled", !menuItem.Enabled);

				form.FilterOrganisationtFindBox.CodeBox.Text = organisation.OH_Code;
				form.FilterOrganisationtFindBox.CommitBoundValue();
				form.FilterFindButton.PerformClick();
				AssertEquals("Pre-condition", 1, form.Configuration.OrgDocumentRecipients.Count);

				form.OrgDocumentRecipientsGrid.Select(0);
				form.OrgDocumentRecipientsGrid.ContextMenu.DoPopup();
				Assert("One record in the grid, so Exclude should be enabled", menuItem.Enabled);
				AssertEquals(0, form.Configuration.JobDocumentRecipients.Count);

				menuItem.PerformClick();
				AssertEquals(1, form.Configuration.JobDocumentRecipients.Count);
				var exclusionWrapper = form.Configuration.JobDocumentRecipients[0] as JobDocumentRecipientWrapperForJobDocumentExclusion;
				AssertEquals(orgDocument.PK, exclusionWrapper.Exclusion.JDE_OD_Document);

				form.OrgDocumentRecipientsGrid.ContextMenu.DoPopup();
				Assert("One record in the grid but it is already excluded, so Exclude should be disabled", !menuItem.Enabled);
			}
		}
		public void TestOrganisationDefaultRecipientsMultipleExclusion()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			var contact = organisation.Contacts.AddNew();
			contact.FillWithValidTestData();

			var orgDocument = contact.Documents.AddNew();
			orgDocument.OD_DocumentGroup = ContactType.All.Code;

			Factory.Save();

			using var form = GetFormToBashCore() as JobDocumentRecipientConfigurationForm;
			form.Show();
			form.FilterOrganisationtFindBox.CodeBox.Text = organisation.OH_Code;
			form.FilterOrganisationtFindBox.CommitBoundValue();
			form.FilterFindButton.PerformClick();

			var dataSource = (JobDocumentRecipientConfiguration)form.OrgDocumentRecipientsGrid.DataSource;
			var orgDocumentRecipients = dataSource.OrgDocumentRecipients;
			orgDocumentRecipients.Add(new JobDocumentRecipientWrapperForOrgDocument(orgDocument, configuration));
			orgDocumentRecipients.Add(new JobDocumentRecipientWrapperForOrgDocument(orgDocument, configuration));
			orgDocumentRecipients.Add(new JobDocumentRecipientWrapperForOrgDocument(orgDocument, configuration));

			form.OrgDocumentRecipientsGrid.Select(0);
			form.OrgDocumentRecipientsGrid.Select(1);
			form.OrgDocumentRecipientsGrid.Select(2);
			form.OrgDocumentRecipientsGrid.ContextMenu.DoPopup();

			var menuItem = form.OrgDocumentRecipientsGrid.ContextMenu.MenuItems[0];
			menuItem.PerformClick();
			AssertEquals(3, form.Configuration.JobDocumentRecipients.Count);
		}

		#endregion

		public void TestJobDocumentRecipientsGrid_AllowReadOnlyRowsToBeDeleted()
		{
			using (var form = new JobDocumentRecipientConfigurationForm(configuration))
			{
				Assert(form.JobDocumentRecipientsGrid.AllowReadOnlyRowsToBeDeleted);
			}
		}

		public void TestAllowNew()
		{
			using (var form = new JobDocumentRecipientConfigurationForm(configuration))
			{
				AssertEquals(false, ((IPostingButtonsProvider)form).AllowNew);
			}
		}

		public void TestFormHeading()
		{
			using (var form = new JobDocumentRecipientConfigurationForm(configuration))
			{
				AssertEquals("Setup Job Specific Recipients - DummyBizo", form.FormHeading);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore() => new JobDocumentRecipientConfigurationForm(configuration);

		protected override void SetUp()
		{
			base.SetUp();
			documentSupportable = Factory.New<DocDummyBusinessObject>();
			configuration = JobDocumentRecipientConfiguration.New(documentSupportable);
		}

		DocDummyBusinessObject documentSupportable;
		JobDocumentRecipientConfiguration configuration;

		#endregion
	}
}
