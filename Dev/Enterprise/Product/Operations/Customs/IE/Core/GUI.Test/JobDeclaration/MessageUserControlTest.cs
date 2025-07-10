using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.EU.Business.EUCommonConstants;

namespace Enterprise.Customs.IE.GUI.Testing
{
	class MessageUserControlTest : TestCaseWithFactory
	{
		public void TestEntryLineAdditionalDataUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "V1";
			using (var userControl = new MessageUserControl(declaration))
			{
				AssertNotNull(userControl.FindSingleOrDefault<ImportEntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl"));
			}

			declaration.JE_ApplicationCode = "V2";
			using (var userControlUCC6 = new MessageUserControl(declaration))
			{
				AssertNotNull(userControlUCC6.FindSingleOrDefault<UCC6EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl"));
			}
		}

		public void TestEntryLineSelectionChangesAdditionalDataUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_ApplicationCode = "V1";

			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			var fee1 = entryLine1.Fees.AddNew();
			fee1.CF_BaseValue = 2500;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			var fee2 = entryLine2.Fees.AddNew();
			fee2.CF_BaseValue = 495;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl(declaration))
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var entryLineMessagesTabControl = userControl.FindSingle<ZTabControl>("EntryLinesMessagesTabControl");
				entryLineMessagesTabControl.SelectedTab = (ZTabPage)entryLineMessagesTabControl.TabPages["EntryLinesTabPage"];

				var entryLineGridListManager = entryLineMessagesTabControl.FindSingle<ZGrid>("EntryLineGrid").ListManager;
				var line1SelectIndex = entryLineGridListManager.List.Cast<CusEntryLine>().IndexOf(entryLine => entryLine.PK == entryLine1.PK);
				var line2SelectIndex = entryLineGridListManager.List.Cast<CusEntryLine>().IndexOf(entryLine => entryLine.PK == entryLine2.PK);
				Assert("Ensure EntryLine1 has been found", line1SelectIndex > -1);
				Assert("Ensure EntryLine2 has been found", line2SelectIndex > -1);

				// The UserControlType setter will cause multiple EntryLineTaxAndConfirmedFeeUserControl(IE&EU)-EntryLineDutyAndTaxGrid created when running in UT environment.
				// This is not an issue because the old EU EntryLineTaxAndConfirmedFeeUserControl will be disposed when crated in UI thread.
				var entryLineCalculatedDutiesAndTaxesGrids = entryLineMessagesTabControl.FindAll<ZGrid>(g => g.Name == "EntryLineDutyAndTaxGrid");
				var entryLineCalculatedDutiesAndTaxesGrid = entryLineCalculatedDutiesAndTaxesGrids.First();

				entryLineGridListManager.Position = line1SelectIndex;
				var selectedOnGrid = (CusEntryLineFee)entryLineCalculatedDutiesAndTaxesGrid.GetCurrent();
				CombineAssertions("When first entry line is selected", () =>
				{
					AssertEquals("PK of fee displayed on Calculated Duties And Taxes grid", fee1.PK, selectedOnGrid.PK);
					AssertEquals("Base Value of fee displayed on Calculated Duties And Taxes grid", 2500m, selectedOnGrid.CF_BaseValue);
				});

				entryLineGridListManager.Position = line2SelectIndex;
				selectedOnGrid = (CusEntryLineFee)entryLineCalculatedDutiesAndTaxesGrid.GetCurrent();
				CombineAssertions("When second entry line is selected", () =>
				{
					AssertEquals("PK of fee displayed on Calculated Duties And Taxes grid", fee2.PK, selectedOnGrid.PK);
					AssertEquals("Base Value of fee displayed on Calculated Duties And Taxes grid", 495m, selectedOnGrid.CF_BaseValue);
				});
			}
		}

		public void TestDateTimeFormat()
		{
			var testDec = Factory.New<JobDeclaration>();

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				AssertEquals("DateTimeFormat should be short", ZArchitecture.Core.ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryReleaseDate)).DateTimeFormat);
				AssertEquals("DateTimeFormat should be short", ZArchitecture.Core.ZDateTimePickerFormat.Short, ((ZDateEditColumnStyleInfo)userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MovementReferenceNumberIssueDate)).DateTimeFormat);
			}
		}

		public void TestChangeControlsVisibility()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "EXP";
			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				var entryLineAdditionalDataUserControl = userControl.FindSingle<EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl");
				AssertEquals("TaxOrFeeTabPage Not Visible for Export", false, entryLineAdditionalDataUserControl.TaxOrFeeTabPage.TabVisible);
			}

			testDec.JE_MessageType = "IMP";
			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				var entryLineAdditionalDataUserControl = userControl.FindSingle<EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl");
				AssertEquals("TaxOrFeeTabPage Visible for Import", true, entryLineAdditionalDataUserControl.TaxOrFeeTabPage.TabVisible);
			}
		}

		public void TestEntriesBoundGridVisibility()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "EXP";
			var issueDateColumnStyleName = $"{nameof(CusEntryNumber)}+{CusEntryNumber.Schema.CE_IssueDate}";

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				Assert("EntryNumber should not available as default", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).IsUnavailable);
				Assert("DeclarationUCR should not available as default", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.DeclarationUCR).IsUnavailable);
				Assert("VAT should not available as default when export", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.VAT).IsUnavailable);
				Assert("Duty should not available as default when export", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.Duty).IsUnavailable);
				Assert("Issue Date should available show as default when export", userControl.EntriesBoundGrid.GetColumnStyle(issueDateColumnStyleName).IsUnavailable);
				Assert("CRN (Customs Registration Number) is available when export", !userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CRN).IsUnavailable);
				var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				Assert("Entry Line Grid - DutyAmount is unavailable when export", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.DutyAmount).IsUnavailable);
				Assert("Entry Line Grid - CL_DutyPercent is unavailable when export", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_DutyPercent).IsUnavailable);
				Assert("Entry Line Grid - GSTVATAmount is unavailable when export", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.GSTVATAmount).IsUnavailable);
				Assert("Entry Line Grid - GSTVATDeferred is unavailable when export", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.GSTVATDeferred).IsUnavailable);
				Assert("Entry Line Grid - CL_CustomsValue is unavailable when export", entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CustomsValue).IsUnavailable);
			}

			testDec.JE_MessageType = "IMP";

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				Assert("EntryNumber should not available as default", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).IsUnavailable);
				Assert("DeclarationUCR should not available as default", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.DeclarationUCR).IsUnavailable);
				Assert("VAT should available as default when import", !userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.VAT).IsUnavailable);
				Assert("Duty should available as default when import", !userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.Duty).IsUnavailable);
				Assert("Issue Date available show as default when import", !userControl.EntriesBoundGrid.GetColumnStyle(issueDateColumnStyleName).IsUnavailable);
				Assert("CRN (Customs Registration Number) is available when import", !userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CRN).IsUnavailable);
				var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				Assert("Entry Line Grid - DutyAmount is available when import", !entryLineGrid.GetColumnStyle(CusEntryLine.Schema.DutyAmount).IsUnavailable);
				Assert("Entry Line Grid - CL_DutyPercent is available when import", !entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_DutyPercent).IsUnavailable);
				Assert("Entry Line Grid - GSTVATAmount is available when import", !entryLineGrid.GetColumnStyle(CusEntryLine.Schema.GSTVATAmount).IsUnavailable);
				Assert("Entry Line Grid - GSTVATDeferred is available when import", !entryLineGrid.GetColumnStyle(CusEntryLine.Schema.GSTVATDeferred).IsUnavailable);
				Assert("Entry Line Grid - CL_CustomsValue is available when import", !entryLineGrid.GetColumnStyle(CusEntryLine.Schema.CL_CustomsValue).IsUnavailable);
			}
		}

		public void TestCustomsRegistrationNumberColumnStyle()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "IMP";

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();
				var crnColumnStyle = userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CRN);
				CombineAssertions(() =>
				{
					AssertEquals("CRN should have correct caption 'Customs Registration Number'", "Customs Registration Number", crnColumnStyle.CaptionResourceString.Caption);
					AssertEquals("CRN should have a short caption 'CRN'", "CRN", crnColumnStyle.CaptionResourceString.ShortCaption);
					AssertEquals("CRN should be readonly", true, crnColumnStyle.IsReadOnly);
					AssertEquals("CRN should be visible", true, crnColumnStyle.IsVisible);
				});
			}
		}

		public void TestCaptionOfReferenceColumnStyle()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "EXP";

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				AssertEquals("CH_BGMReference.Caption", "LRN", userControl.EntriesBoundGrid.GetColumnCaption(CusEntryHeader.Schema.CH_BGMReference));
			}
		}

		public void TestMessageStatusDescriptionVisibility()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "EXP";

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();

				Assert("MessageStatusDescription should show as default", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MessageStatusDescription).IsVisible);
			}
		}

		public void TestMessagesTabUserControl()
		{
			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = "EXP";

			using (var form = new ZForm(testDec))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(testDec, ".");
				form.Show();
				var messageUserControl = userControl.FindSingle<ZDynamicControlCreationUserControl>("BaseMessageUserControl");
				AssertNotNull("MessagesTabUserControl", messageUserControl.FindSingleOrDefault<MessagesTabUserControl>());
			}
		}

		public void TestColumnsAndVisibilitiesH2AndNonH2()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			var h2Instruction = declaration.CustomsEntryInstructions.FirstOrDefault() ?? declaration.CustomsEntryInstructions.AddNew();
			h2Instruction.CEI_Style = ImportDeclarationTypeList.H2;
			var h2InvoiceLine = (JobComInvoiceLine)declaration.InvoiceLines.FirstOrDefault() ?? declaration.InvoiceLines.AddNew();
			h2InvoiceLine.JI_CEI = h2Instruction.PK;

			var h2Entry = declaration.ActiveEntryHeaders.AddNew();
			h2Entry.CH_CEI_Instruction = h2Instruction.PK;
			var h2EntryLine = h2Entry.MergedLines.AddNew();
			h2InvoiceLine.JI_CL = h2EntryLine.PK;

			var nonH2Instruction = declaration.CustomsEntryInstructions.AddNew();
			nonH2Instruction.CEI_Style = ImportDeclarationTypeList.H1;
			var nonH2InvoiceLine = declaration.InvoiceLines.AddNew();
			nonH2InvoiceLine.JI_CEI = nonH2Instruction.PK;

			var nonH2Entry = declaration.ActiveEntryHeaders.AddNew();
			nonH2Entry.CH_CEI_Instruction = nonH2Instruction.PK;
			var nonH2EntryLine = nonH2Entry.MergedLines.AddNew();
			nonH2InvoiceLine.JI_CL = nonH2EntryLine.PK;

			using (var form = new ZForm(declaration))
			using (var userControl = new MessageUserControl())
			{
				form.Controls.Add(userControl);
				form.SetDataBinding(declaration, ".");
				form.Show();

				var entriesGrid = userControl.EntriesBoundGrid;
				var entryLineGrid = userControl.FindSingle<ZGrid>("EntryLineGrid");
				var entryLineAdditionalDataUserControl = userControl.FindSingle<EntryLineAdditionalDataUserControl>("EntryLineAdditionalDataUserControl");
				var taxOrFeeTabPage = entryLineAdditionalDataUserControl.TaxOrFeeTabPage;

				var listManager = entriesGrid.ListManager;
				var h2SelectIndex = listManager.List.Cast<CusEntryHeader>().IndexOf(entryHeader => entryHeader.EntryInstruction.IsH2);
				var nonH2SelectIndex = listManager.List.Cast<CusEntryHeader>().IndexOf(entryHeader => !entryHeader.EntryInstruction.IsH2);
				AssertEquals("To make sure an H2 EntryHeader has been found.", true, h2SelectIndex > -1);
				AssertEquals("To make sure an non-H2 EntryHeader has been found.", true, nonH2SelectIndex > -1);

				listManager.Position = nonH2SelectIndex;
				listManager.Position = h2SelectIndex;
				AssertEquals("EntryLinesGrid has 5 columns when an H2 EntryHeader selected.", 5, entryLineGrid.Columns.Count);
				AssertEquals("TaxOrFeeTabPage hides when an H2 EntryHeader selected.", false, taxOrFeeTabPage.TabVisible);

				listManager.Position = nonH2SelectIndex;
				AssertEquals("EntryLinesGrid has 10 columns when an non-H2 EntryHeader selected.", 10, entryLineGrid.Columns.Count);
				AssertEquals("TaxOrFeeTabPage shows when an non-H2 EntryHeader selected.", true, taxOrFeeTabPage.TabVisible);
			}
		}
	}
}
