using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.EU.GUI.Testing
{
	public abstract class MessageUserControlForVirtualPropertiesTest<T> : TestCaseWithFactory
		where T : MessageUserControl, new()
	{
		[RequiresSTA]
		public void TestNewLineDetailsTabPage()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var control = new T())
			{
				form.Controls.Add(control);
				control.JobDeclaration = declaration;
				form.Show();

				var newEntryDetailsTabPage = control.NewEntryDetailsTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("DynamicLayoutApplied", DefaultDynamicLayoutApplied, typeof(T).GetProperty("DynamicLayoutApplied", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(control));
					if (DefaultDynamicLayoutApplied)
					{
						AssertEquals($"{DefaultDynamicLayoutApplied}-NewEntryDetailsTabPage", true, newEntryDetailsTabPage.TabVisible);
					}
					else
					{
						AssertEquals($"{DefaultDynamicLayoutApplied}-NewEntryDetailsTabPage", false, newEntryDetailsTabPage.TabVisible);
					}
				});
			}
		}

		protected virtual ZBool DefaultDynamicLayoutApplied => ZBool.False;
	}

	sealed class MessageUserControlTest : MessageUserControlForVirtualPropertiesTest<MessageUserControl>
	{
		public void TestNewLineDetailsTabPage_NotVisible()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var messageUserControl = new MessageUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				AssertEquals("NewEntryDetailsTabPage", false, messageUserControl.NewEntryDetailsTabPage.TabVisible);
			}
		}

		[RequiresSTA]
		public void TestSetupEntryHeaderColumns()
		{
			var testDec = Factory.New<JobDeclaration>();

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("User control should have Duty column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.Duty]);
					AssertNotNull("User control should have VAT column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.VAT]);

					var entryStatusColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus];
					AssertNotNull("User control should have CH_EntryStatus column", entryStatusColumn);
					AssertEquals("CH_EntryStatus column is not visible", false, entryStatusColumn.IsVisible);

					var issueDateColumn = userControl.EntriesBoundGrid.Columns["CusEntryNumber+CE_IssueDate"];
					AssertNotNull("User control should have Entry Number Issue Date column", issueDateColumn);
					AssertEquals("Issue Date column is not mandatory", false, issueDateColumn.IsMandatory);

					var entrySubmittedDateColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate];
					AssertNotNull("User control should have CH_EntrySubmittedDate column", entrySubmittedDateColumn);
					AssertEquals("CH_EntrySubmittedDate column is not visible", false, entrySubmittedDateColumn.IsVisible);

					var statusColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status];
					AssertNotNull("User control should have CH_Status column", statusColumn);
					AssertEquals("CH_Status column is not visible", false, statusColumn.IsVisible);

					var statusDescriptionColumn = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription];
					AssertNotNull("User control should have MessageStatusDescription column", statusDescriptionColumn);
					AssertEquals("MessageStatusDescription column is not visible", false, statusDescriptionColumn.IsVisible);

					AssertNotNull("User control should have CH_MessageType column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType]);
					AssertNotNull("User control should have CH_MessageTypeDescription column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription]);
					AssertNotNull("User control should have EntryHeaderStatusDescription column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription]);
					AssertNotNull("User control should have CH_EntryReleaseDate column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryReleaseDate]);
					AssertNotNull("User control should have DeclarationUCR column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationUCR]);

					var entryTypeColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryTypeFriendlyName);
					AssertNotNull("User control should have EntryTypeFriendlyName column", entryTypeColumn);
					AssertEquals("EntryTypeFriendlyName should be visible", true, entryTypeColumn.IsVisible);

					var totalDutiesAndTaxesAmount = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_TotalPaid);
					AssertNotNull("User control should have CH_TotalPaid column", totalDutiesAndTaxesAmount);
					AssertEquals("CH_TotalPaid should not be visible by default", false, totalDutiesAndTaxesAmount.IsVisible);
					AssertEquals("CH_TotalPaid should be readonly", true, totalDutiesAndTaxesAmount.IsReadOnly);

					var exitStatusColumn = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ExitedStatus);
					AssertNotNull("User control should have CH_ExitedStatus column", exitStatusColumn);
					AssertEquals("CH_ExitedStatus should be visible by default for export declarations", true, exitStatusColumn.IsVisible);
					AssertEquals("CH_ExitedStatus should be readonly", true, exitStatusColumn.IsReadOnly);

					AssertNotNull("Context menu to set entry failed transmission exists", userControl.EntriesBoundGrid.ContextMenu.MenuItems.FindByText("Set Entry as Failed from Transmission"));
				});
			}
		}

		public void TestEntryLineAdditionalDataUserControlType()
		{
			var testDec = Factory.New<JobDeclaration>();

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				var entryLineAdditionalDataUserControl = userControl.Controls.Find(nameof(EntryLineAdditionalDataUserControl), true).FirstOrDefault();

				AssertNotNull(entryLineAdditionalDataUserControl);
				AssertEquals(typeof(EntryLineAdditionalDataUserControl), entryLineAdditionalDataUserControl.GetType());
			}
		}

		public void TestChangeGridColumnsVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var totalDutiesAndTaxesAmount = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_TotalPaid];
				var exitStatus = userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_ExitedStatus];

				CombineAssertions(() =>
				{
					AssertNotNull("CH_TotalPaid should not be null", totalDutiesAndTaxesAmount);
					AssertNotNull("CH_ExitedStatus should not be null", exitStatus);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					AssertEquals("CH_TotalPaid should be visible", true, totalDutiesAndTaxesAmount.IsVisible);
					AssertEquals("CH_ExitedStatus should not be visible", false, exitStatus.IsVisible);

					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					AssertEquals("CH_TotalPaid should not be visible", false, totalDutiesAndTaxesAmount.IsVisible);
					AssertEquals("CH_ExitedStatus should be visible", true, exitStatus.IsVisible);
				});
			}
		}

		public void TestAmendmentReasonLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var amendmentReasonLabel = userControl.FindSingle<ZLabel>("AmendmentReasonLabel");
				AssertEquals("Caption", "Reason For Amendment / Invalidation", amendmentReasonLabel.CaptionResourceString.Caption);
			}
		}
	}
}
