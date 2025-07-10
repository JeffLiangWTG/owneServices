using System;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportLicenseMessageUserControlTest : TestCaseWithFactory
	{
		public void TestSetupEntryHeaderColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;

			using (var form = new ZForm(declaration))
			using (var userControl = new ImportLicenseMessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertNotNull("User control should have Entry Number column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryNumber]);
					AssertNotNull("User control should have Reference Number column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference]);
					AssertNotNull("User control should have Entry Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus]);
					AssertNotNull("User control should have Issue Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate]);
					AssertNotNull("User control should have Entry Submitted Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate]);
					AssertNotNull("User control should have Message Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status]);
					AssertNotNull("User control should have Message Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription]);
					AssertNull("User control should NOT have MRN column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumber]);
					AssertNotNull("User control should have Message Type column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType]);
					AssertNotNull("User control should have Message Type Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription]);
					AssertNotNull("User control should have Entry Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription]);
					AssertNull("User control should NOT have Message Declaration UCR column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.DeclarationUCR]);
					AssertNull("User control should NOT have Entry Access Key column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryAccessKey]);
					AssertNull("User control should NOT have Cargo Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_CargoStatus]);
					AssertNull("User control should NOT have Cargo Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CargoStatusDescription]);
					AssertNull("User control should NOT have Administrative Status column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_AdministrativeStatus]);
					AssertNull("User control should NOT have Administrative Status Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.AdministrativeStatusDescription]);
					AssertNull("User control should NOT have Risk Channel column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_RiskChannel]);
					AssertNull("User control should NOT have Risk Channel Description column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.RiskChannelDescription]);
					AssertNull("User control should NOT have Packages Count column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.PackagesCount]);
					AssertNotNull("User control should have Import License Identifier column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportLicenseIdentifier]);
					AssertNotNull("User control should have Shipment Expiry Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_ValidityILShipmentDate]);
					AssertNotNull("User control should have Dispatch Expiry Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_ValidityILDispatchDate]);
					AssertNotNull("User control should have Concession Date column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryReleaseDate]);
					AssertNotNull("User control should have Import Declaration JOB column", userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportDeclarationNumber]);
				});
			}
		}

		public void TestEntriesBoundGridColumnsVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (var form = new ZForm(declaration))
			using (var userControl = new ImportLicenseMessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("Entry Instruction column should be visible", true, userControl.EntriesBoundGrid.Columns[ImportLicenseMessageUserControl.Schema.EntryInstructionDescription].IsVisible);
					AssertEquals("Entry Number column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryNumber].IsVisible);
					AssertEquals("Reference Number column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_BGMReference].IsVisible);
					AssertEquals("Message Type column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageType].IsVisible);
					AssertEquals("Message Type Description column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_MessageTypeDescription].IsVisible);
					AssertEquals("Entry Status column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryStatus].IsVisible);
					AssertEquals("Entry Status Description column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.EntryHeaderStatusDescription].IsVisible);
					AssertEquals("Issue Date column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MovementReferenceNumberIssueDate].IsVisible);
					AssertEquals("Entry Submitted Date column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntrySubmittedDate].IsVisible);
					AssertEquals("Message Status column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_Status].IsVisible);
					AssertEquals("Message Status Description column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.MessageStatusDescription].IsVisible);
					AssertEquals("Import License Identifier column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportLicenseIdentifier].IsVisible);
					AssertEquals("Shipment Expiry Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_ValidityILShipmentDate].IsVisible);
					AssertEquals("Dispatch Expiry Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_ValidityILDispatchDate].IsVisible);
					AssertEquals("Concession Date column should be visible", true, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.CH_EntryReleaseDate].IsVisible);
					AssertEquals("Import Declaration JOB column should NOT be visible", false, userControl.EntriesBoundGrid.Columns[CusEntryHeader.Schema.ImportDeclarationNumber].IsVisible);
				});
			}
		}

		public void TestEntriesBoundGridColumnsCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (var form = new ZForm(declaration))
			using (var userControl = new ImportLicenseMessageUserControl())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("EntryInstructionDescription Caption should be", "Entry Instruction", userControl.EntriesBoundGrid.GetColumnStyle(ImportLicenseMessageUserControl.Schema.EntryInstructionDescription).CaptionResourceString.Caption);
					AssertEquals("EntryNumber Caption should be", "Import License", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryNumber).CaptionResourceString.Caption);
					AssertEquals("CH_EntrySubmittedDate Caption should be", "License Submitted Date", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntrySubmittedDate).CaptionResourceString.Caption);
					AssertEquals("MovementReferenceNumberIssueDate Caption should be", "License Issue Date", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MovementReferenceNumberIssueDate).CaptionResourceString.Caption);
					AssertEquals("CH_EntryStatus Caption should be", "License Status", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryStatus).CaptionResourceString.Caption);
					AssertEquals("EntryHeaderStatusDescription Caption should be", "License Status Description", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.EntryHeaderStatusDescription).CaptionResourceString.Caption);
					AssertEquals("CH_Status Caption should be", "Message Status", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_Status).CaptionResourceString.Caption);
					AssertEquals("MessageStatusDescription Caption should be", "Message Status Description", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.MessageStatusDescription).CaptionResourceString.Caption);
					AssertEquals("ImportLicenseIdentifier Caption should be", "License Id", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.ImportLicenseIdentifier).CaptionResourceString.Caption);
					AssertEquals("BR_ValidityILShipmentDate Caption should be", "Shipment Expiry Date", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ValidityILShipmentDate).CaptionResourceString.Caption);
					AssertEquals("BR_ValidityILDispatchDate Caption should be", "Dispatch Expiry Date", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_ValidityILDispatchDate).CaptionResourceString.Caption);
					AssertEquals("CH_EntryReleaseDate Caption should be", "Concession Date", userControl.EntriesBoundGrid.GetColumnStyle(CusEntryHeader.Schema.CH_EntryReleaseDate).CaptionResourceString.Caption);
				});
			}
		}

		public void TestEntryLineGridColumnsCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (var form = new ZForm(declaration))
			using (var userControl = new ImportLicenseMessageUserControlForTesting())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("LineSubmissionStatusDescription Caption should be", "Line Status", userControl.EntryLineGrid_Exposed.GetColumnStyle("LineSubmissionStatusDescription").CaptionResourceString.Caption);
					AssertEquals("FormattedTariff Caption should be", "Tariff", userControl.EntryLineGrid_Exposed.GetColumnStyle("FormattedTariff").CaptionResourceString.Caption);
					AssertEquals("EffectiveDescription Caption should be", "Description", userControl.EntryLineGrid_Exposed.GetColumnStyle("EffectiveDescription").CaptionResourceString.Caption);
					AssertEquals("CL_CustomsValue Caption should be", "Customs Value", userControl.EntryLineGrid_Exposed.GetColumnStyle("CL_CustomsValue").CaptionResourceString.Caption);
				});
			}
		}

		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			using (var form = new ZForm(declaration))
			using (var userControl = new ImportLicenseMessageUserControlForTesting())
			{
				userControl.Dock = DockStyle.Fill;
				userControl.JobDeclaration = declaration;
				form.Controls.Add(userControl);
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals("EntriesGroupBox Caption should be", "Licenses", userControl.EntriesGroupBox_Exposed.CaptionResourceString.Caption);
					AssertEquals("TariffCodeTextBox Caption should be", "Tariff Code:", userControl.TariffCodeTextBox.CaptionResourceString.Caption);
					AssertEquals("DescriptionTextBox Caption should be", "Description:", userControl.DescriptionTextBox.CaptionResourceString.Caption);
				});
			}
		}

		public void TestBaseMessagesTabUserControlType()
		{
			using (var control = new ImportLicenseMessageUserControlForTesting())
			{
				AssertEquals(typeof(MessagesTabUserControl), control.GetBaseMessagesTabUserControlTypeExposed());
			}
		}

		class ImportLicenseMessageUserControlForTesting : ImportLicenseMessageUserControl
		{
			public ZTabControl EntryLinesMessagesTabControl_Exposed => EntryLinesMessagesTabControl;
			public ZTabPage EntryLinesTabPage_Exposed => EntryLinesTabPage;
			public ZGroupBox ExtendedInfoGroupBox_Exposed => ExtendedInfoGroupBox;
			public ZGrid EntryLineGrid_Exposed => EntryLineGrid;
			public ZGroupBox EntriesGroupBox_Exposed => EntriesGroupBox;
			public Type GetBaseMessagesTabUserControlTypeExposed() => base.GetBaseMessagesTabUserControlType();
		}
	}
}
