using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.BR.GUI.Testing
{
	class ImportLicensesModuleButtonGridWithFactoryTest : TestCaseWithFactory
	{
		public void TestGetNewRecordAttacher()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var grid = new ImportLicensesModuleButtonGridForTesting())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.AttachButtonForTest.PerformClick();
				AssertType<ImportLicenseAttacher>(grid.Attacher);
			}
		}

		public void TestEdit()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			Factory.Save();

			using (var form = new ZForm(declaration))
			using (var grid = new ImportLicensesModuleButtonGridForTesting())
			{
				form.Controls.Add(grid);
				form.Show();

				var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

				var entryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_JE = licDeclaration.PK;
				entryInstruction.CEI_Description = "TEST1";

				var entryInstruction2 = licDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction2.CEI_JE = licDeclaration.PK;
				entryInstruction2.CEI_Description = "TEST2";

				declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction) });
				declaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction2) });

				grid.EditButtonForTest.PerformClick();
				AssertEquals("Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.", UnitTestUserNotification.Instance.LastMessage.Text);

				declaration.AttachedImportLicenseEntries.DeletePivotFor(entryInstruction2);
				Factory.Save();
				grid.EditButtonForTest.PerformClick();
				using (var formLic = grid.LicenseController.LastShownForm)
				{
					AssertType<JobDeclarationForm>(formLic);
				}
			}
		}

		public void TestDetachButton_Click()
		{
			var impDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			impDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;

			using (var form = new ZForm(impDeclaration))
			using (var grid = new ImportLicensesModuleButtonGridForTesting())
			{
				form.Controls.Add(grid);
				form.Show();

				grid.DetachButtonForTest.PerformClick();
				AssertEquals("Please select an item in the grid by first clicking in the left-hand gutter to highlight the whole row.", UnitTestUserNotification.Instance.LastMessage.Text);

				var licDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				licDeclaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;

				var entryInstruction = licDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_JE = licDeclaration.PK;
				entryInstruction.CEI_Description = "TEST1";

				var invHeader = licDeclaration.Invoices.AddNew();
				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_CEI = entryInstruction.PK;
				var entryHeader = licDeclaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_CEI_Instruction = entryInstruction.PK;
				var entryLine = entryHeader.AllEntryLines.AddNew();
				invLine.JI_CL = entryLine.PK;
				entryInstruction.EntryHeader.MovementReferenceNumberSetter("TST1", ZDateTime.Now);

				impDeclaration.AttachImportLicense(new[] { new ImportLicenseAttachingObject(entryInstruction) });
				Factory.Save();

				AssertEquals("Precondition:", 1, impDeclaration.AttachedImportLicenseEntries.Count);
				AssertEquals("Precondition:", 1, impDeclaration.Invoices.Count);
				AssertEquals("Precondition:", 1, impDeclaration.InvoiceLines.Count);

				grid.DetachButtonForTest.PerformClick();

				AssertEquals("One Import License Entry detached", 0, impDeclaration.AttachedImportLicenseEntries.Count);
				AssertEquals("One Invoice detached", 0, impDeclaration.Invoices.Count);
				AssertEquals("One Invoice Line detached", 0, impDeclaration.InvoiceLines.Count);
			}
		}

		class ImportLicensesModuleButtonGridForTesting : ImportLicensesModuleButtonGrid
		{
			public ImportLicensesModuleButtonGridForTesting()
			{
				ColumnStyles.Add(new ZTextBoxColumnStyleInfo() { ColumnName = "ImportLicenseJobNumber" });
				BindToGridList = "AttachedImportLicenseEntries";
				BindToFindBoxList = "PossibleImportLicenseDeclarationForAttachment_List";
				ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.BR.License;
			}

			public ZRecordAttacher Attacher { get; private set; }

			protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
			{
				Attacher = base.GetNewRecordAttacher(destinationCollection, findBoxList, moduleID);
				return Attacher;
			}
		}
	}

	[TestedType(typeof(ImportLicensesModuleButtonGrid))]
	class ImportLicensesModuleButtonGridTest : ZModuleButtonGridTestBase
	{
	}
}
