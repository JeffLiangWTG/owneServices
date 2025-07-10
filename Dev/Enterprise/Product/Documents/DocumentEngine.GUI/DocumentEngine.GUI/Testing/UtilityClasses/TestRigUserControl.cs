#if DEBUG
using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating;
using Enterprise.DocumentEngine.GUI.RuntimeOptions;
using Enterprise.DocumentEngine.GUI.Testing.UtilityClasses;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentEngine.GUI.Unit_Testing_Utility_Classes
{
	[CargoWise.Windows.UI.Testing.SuppressFormDesignerAnalysis] // Has no Resx to stop the Compiler Warning about release builds.
	public partial class TestRigUserControl : UserControl
	{
		public TestRigUserControl()
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				this.FileTextBox.Text = Path.Combine(WTG.TestHelpers.TestCase.BaseSourcePath, @"Enterprise\\Product\\Documents\\ExcelTemplates\Documents\Statement Of Account.xls");
			}
		}

		void oButton1_Click(object sender, System.EventArgs e)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(new DocumentPack(), excelTemplate))
			{
				CollectionOfIFilter fc = rpt.FilterCollection;
				FilterField ff;

				ff = new DateField(factory);
				ff.FieldName = "XX_somedate";
				ff.DisplayName = "Some Date";
				fc.Add(ff);

				ff = new LookupField(factory);
				ff.FieldName = "XX_FK";
				ff.DisplayName = "Debtor";
				fc.Add(ff);

				ff = new TextRangeField(factory);
				ff.FieldName = "XX_Code";
				ff.DisplayName = "Code range";
				fc.Add(ff);

				ff = new TextField(factory);
				ff.FieldName = "XX_three";
				ff.DisplayName = "Some dummy field";
				fc.Add(ff);

				ff = new OptionGroup(factory);
				ff.FieldName = "XX_Thing";
				ff.DisplayName = "Type of animal";
				((OptionGroup)ff).AddOption("Canine", "DOG");
				((OptionGroup)ff).AddOption("Lupine", "WLF");
				((OptionGroup)ff).AddOption("Ovine", "SHP");
				((OptionGroup)ff).AddOption("Bovine", "COW");
				((OptionGroup)ff).AddOption("Feline", "CAT");
				fc.Add(ff);

				ff = new TextField(factory);
				ff.FieldName = "XX_four";
				ff.DisplayName = "Other field";
				fc.Add(ff);

				ff = new AccountingPeriodField(factory);
				ff.FieldName = "XX_Period";
				ff.DisplayName = "Posting Period";
				fc.Add(ff);

				ff = new TextField(factory);
				ff.FieldName = "XX_five";
				ff.DisplayName = "Other field";
				fc.Add(ff);

				SortOrderCollection soc = rpt.SortOrderCollection;
				soc.Add("Size (largest to smallest)", "xx_size desc");
				soc.Add("Weight (lightest to heaviest)", "xx_weight");
				soc.Add("Name", "xx_name");

				new RuntimeOptionsForm(rpt).Show();
			}
		}

		void ReportButton_Click(object sender, System.EventArgs e)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TestReport", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(new DocumentPack(), excelTemplate))
			{
				FilterField ff;

				ff = new TextRangeField(factory);
				ff.FieldName = "AccGLHeader.AG_AccountNum";
				ff.DisplayName = "Account number";
				rpt.FilterCollection.Add(ff);

				ff = new OptionGroup(factory);
				ff.FieldName = "AccGLHeader.AG_AccountType";
				ff.DisplayName = "Account type";
				((OptionGroup)ff).AddOption("Profit & loss", "P&L");
				((OptionGroup)ff).AddOption("Balance sheet", "BSH");
				((OptionGroup)ff).AddOption("Total level", "TTL");
				rpt.FilterCollection.Add(ff);

				SortOrderCollection soc = rpt.SortOrderCollection;
				Enterprise.DocumentEngine.RuntimeOptions.SortOrder so;

				so = new Enterprise.DocumentEngine.RuntimeOptions.SortOrder("Account number", "AccountNumber");
				so.Selected = true; // default
				soc.Add(so);

				so = new Enterprise.DocumentEngine.RuntimeOptions.SortOrder("Print sequence", "PrintSequence, AccountNumber");
				soc.Add(so);

				so = new Enterprise.DocumentEngine.RuntimeOptions.SortOrder("Account Type", "Type, AccountNumber");
				soc.Add(so);

				new RuntimeOptionsForm(rpt).Show();
			}
		}

		void TemplateReportButton_Click(object sender, System.EventArgs e)
		{
			ExcelTemplateForUnitTesting excelTemplate = new ExcelTemplateForUnitTesting("TemplateTestReport", TestFilesSubFolder.ReportTestFiles);
			using (Report rpt = new Report(new DocumentPack(), excelTemplate))
			{
				new RuntimeOptionsForm(rpt).Show();
			}
		}

		void oButton2_Click(object sender, System.EventArgs e)
		{
			ZOpenFileDialog openFileDialog1 = new ZOpenFileDialog();

			if (openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				string tempFile = Env.GetTempFileName();
				File.Copy(openFileDialog1.UnmappedFileName, tempFile, true);
				BusinessObjectFactory factory = new BusinessObjectFactory();
				ReportCommand testReport = factory.New<ReportCommand>();
				testReport.SU_MenuName = "Test Report Add";
				testReport.SU_IsPublished = true;
				testReport.SU_IsSystemDefined = true;
				DocumentEngine.DocumentPack pack = new DocumentEngine.DocumentPack(testReport);

				DocumentEngine.DeliveryInstructions instructions = new DocumentEngine.DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Preview;
				ExcelTemplateForUnitTesting template = new ExcelTemplateForUnitTesting(tempFile, TestFilesSubFolder.ReportTestFiles);
				string reportName = new FileInfo(openFileDialog1.UnmappedFileName).Name.Replace(".xls", "");
				DocumentEngine.Report report = new DocumentEngine.Report(pack, template, reportName, null, false);
				pack.Add(report);
				DocumentEngine.PrintTask task = new DocumentEngine.PrintTask();
				task.Add(pack);
				task.Run(Env.Security.None);
				task = null;
			}
		}

		void PrintTaskButton_Click(object sender, System.EventArgs e)
		{
			DocumentPack pack = new TestableDocumentPack();
			DeliveryInstructions instructions = new DeliveryInstructions();
			instructions.Destination = DeliveryInstructionDestination.Auto;

			ExcelTemplateForUnitTesting template = new ExcelTemplateForUnitTesting("RecipientName.xls", TestFilesSubFolder.ReportTestFiles);
			Report report = new Report(pack, template);
			pack.Add(report);

			PrintTask task = new PrintTask();
			task.Add(pack);

			task.Run(Env.Security.None);
		}

		void TemplateDialogButton_Click(object sender, System.EventArgs e)
		{
			if (TemplateOpenFileDialog.ShowDialog() == DialogResult.OK)
			{
				FileTextBox.Text = TemplateOpenFileDialog.UnmappedFileName;
			}
		}

		void UpdateTemplateButton_Click(object sender, System.EventArgs e)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			ZString message = "";

			StmTemplateBase stmTemplate = (StmTemplateBase)factory.LoadTop1(typeof(StmTemplateBase), new ZQuery(Enterprise.ZArchitecture.Schema.StmTemplateSchema.SO_Name, NameTextBox.Text));

			if (stmTemplate == null)
			{
				DialogResult @continue = Globals.Message.Show("This will create a new template with name: [" + NameTextBox.Text + "]. Do you want to continue?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
				if (@continue == DialogResult.Yes)
				{
					stmTemplate = (StmTemplateBase)factory.New(typeof(StmTemplateBase));
					stmTemplate.SO_Name = NameTextBox.Text;
					stmTemplate.SO_DataContext = DataContextTextBox.Text;
					stmTemplate.SO_IsSystemDefined = SystemDefinedCheckBox.Checked;
					stmTemplate.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(FileTextBox.Text);
					factory.Save();

					message = "Template Created";
				}
				else
				{
					message = "No template changes made.";
				}
			}
			else
			{
				stmTemplate.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(FileTextBox.Text);
				factory.Save();

				message = "Template Updated";
			}

			Globals.Message.Show(message);
		}

		void UpdateClientDocumentXMLButton_Click(object sender, System.EventArgs e)
		{
			new ClientDocumentsXMLUpdater().WriteAllCheckedOutClientTemplatesToXml();
			Globals.Message.Show("Completed Updating Client Document Xmls");
		}

		class TestableDocumentPack : DocumentPack
		{
			public override Enterprise.MasterFiles.Business.DocAutoDelivery AutoDocumentDelivery
			{
				get
				{
					return new MockAutoDocumentDelivery();
				}
			}
		}

		class MockAutoDocumentDelivery : Enterprise.MasterFiles.Business.DocAutoDelivery
		{
			public override DocDeliveryContactCollection GetDeliveryContacts(IStmMenuItem menuItem, DocumentSupporter deliveryFilter)
			{
				DocDeliveryContactCollection contacts = new DocDeliveryContactCollection(Factory);
				DocDeliveryContact contact = contacts.AddNew();
				contact.Name = "Lorenzo";
				contact.DeliveryMethod = "PRN";
				return contacts;
			}
		}

		void DocBuilderTemplateUpdaterButton_Click(object sender, EventArgs e)
		{
			var factory = new BusinessObjectFactory();
			var docBuilderTemplateUpdater = new DocBuilderTemplateUpdater(factory);

			using (var form = new DocBuilderTemplateUpdaterForm(docBuilderTemplateUpdater))
			{
				ZFormModaliser.ShowDialogAndDispose(form);
			}
		}

		void fixEmUppererButton_Click(object sender, EventArgs e)
		{
			string result = new ClientDocumentsXMLUpdater().ScanAndFixClientMenuPathsRemovingLeadingAndTrailingSlashes();
			Globals.Message.Show(result, "Completed Updating Client Document Xmls", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}

#endif
