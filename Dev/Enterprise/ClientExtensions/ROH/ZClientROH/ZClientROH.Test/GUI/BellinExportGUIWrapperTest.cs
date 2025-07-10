using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Client.Rohlig.Bellin;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig.GUI
{
	[TestedType(typeof(BellinExportGUIWrapper))]
	public class BellinExportGUIWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFormCaption()
		{
			BellinExportGUIWrapper gUIWrapper = new BellinExportGUIWrapper(Factory);
			AssertEquals("Form Caption", Constants.BellinCaption, gUIWrapper.FormCaption);
		}

		public void TestDialogFilter()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			AssertEquals("Form Caption", "CSV Files | *.csv", gUIWrapper.DialogFilter);
		}

		public void TestFileExtension()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			AssertEquals("Form Caption", ".csv", gUIWrapper.FileExtention);
		}

		public void TestDateExporter()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			AssertEquals("Should be a BellinDataExporter", typeof(BellinDataExporter), gUIWrapper.DataExporter.GetType());
			AccountingTransactionsDataExporter exporter = gUIWrapper.DataExporter;
			AssertSame("Export was not Lazy Loaded", exporter, gUIWrapper.DataExporter);
		}

		public void TestIsOKToExport()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			try
			{
				ZString expectedMessage = "Must Specify Date From and Date To";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				gUIWrapper.Export();
				AssertEquals("Expected Error Message did not appear or was incorrect", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("File should NOT be created", false, File.Exists(gUIWrapper.ExportedFile));
				gUIWrapper.DateFrom = ZDateTime.Now;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				gUIWrapper.Export();
				AssertEquals("Expected Error Message did not appear or was incorrect", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("File should NOT be created", false, File.Exists(gUIWrapper.ExportedFile));
				DeleteIfExists(gUIWrapper.ExportedFile);
				gUIWrapper.DateFrom = ZDateTime.Empty;
				gUIWrapper.DateTo = ZDateTime.Now;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				gUIWrapper.Export();
				AssertEquals("Expected Error Message did not appear or was incorrect", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("File should NOT be created", false, File.Exists(gUIWrapper.ExportedFile));
				gUIWrapper.DateFrom = ZDateTime.Today.AddDays(-1);
				TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
				helper.Invoice.AH_InvoiceDate = ZDateTime.Today;
				gUIWrapper.DateTo = ZDateTime.Today.AddDays(1);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				Factory.Save();
				gUIWrapper.Export();
				Assert("Should be no Date From/To error message", expectedMessage != UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("File should be created", File.Exists(gUIWrapper.ExportedFile));
			}
			finally
			{
				DeleteIfExists(gUIWrapper.ExportedFile);
			}
		}

		public void TestAPCreditorGroup()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			gUIWrapper.DataExporter.FilterProvider.AccountGroup = ZGuid.NewZGuid();
			AssertEquals("APCredit Group", gUIWrapper.DataExporter.FilterProvider.AccountGroup, gUIWrapper.APCreditorGroup);
		}

		public void TestOrgHeadersList()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			OrganisationsFindBoxCollection collection = gUIWrapper.OrgHeadersList;
			AssertSame("Collection was not Lazy Loaded", collection, gUIWrapper.OrgHeadersList);
			AssertEquals("Should be a Creditors Collection", typeof(CreditorCollection), gUIWrapper.OrgHeadersList.GetType());
		}

		public void TestAccountCreditorGroup()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			OrgCreditorGroupCollection collection = gUIWrapper.AccountCreditorGroup;
			AssertSame("Collection was not Lazy Loaded", collection, gUIWrapper.AccountCreditorGroup);
		}

		public void TestCollectionIsReadOnly()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			AssertEquals("Selected Organisations should not be readonly", false, gUIWrapper.SelectedOrganisations.ReadOnly);
		}

		public void TestCurrentBatchNumberCanBeSpecified()
		{
			BellinExportGUIWrapperTestClass gUIWrapper = new BellinExportGUIWrapperTestClass(Factory);
			try
			{
				gUIWrapper.DateFrom = ZDateTime.Today.AddDays(-1);
				TransactionExportTestDataHelper helper = new TransactionExportTestDataHelper(Factory);
				helper.ObjectCreator.CreateGenExportBatchSequenceHeader(1, helper.Invoice.PK, 1);
				helper.Invoice.AH_InvoiceDate = ZDateTime.Today;
				gUIWrapper.DateTo = ZDateTime.Today.AddDays(1);
				gUIWrapper.DataExporter.FilterProvider.CurrentBatchNo = 1;
				Factory.Save();
				gUIWrapper.Export();
				Assert("File should be created", File.Exists(gUIWrapper.ExportedFile));
				AssertEquals("Batch Number Should be 1", 1, gUIWrapper.DataExporter.FilterProvider.CurrentBatchNo);
			}
			finally
			{
				DeleteIfExists(gUIWrapper.ExportedFile);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new BellinExportGUIWrapper(Factory);
		}

		#region Implementation
		class BellinExportGUIWrapperTestClass : BellinExportGUIWrapper
		{
			public BellinExportGUIWrapperTestClass(BusinessObjectFactory factory) : base(factory)
			{
			}

			public string ExportedFile;
			protected override DialogResult ShowDialog(IFileDialog dialog)
			{
				ExportedFile = Env.GetTempFileName(Env.TempPath, "CSV");
				File.Create(ExportedFile).Close();
				(dialog as ZSaveFileDialog).FileName = ExportedFile;
				return DialogResult.OK;
			}

			public new AccountingTransactionsDataExporter DataExporter
			{
				get
				{
					return base.DataExporter;
				}
			}

			public new string DialogFilter
			{
				get
				{
					return base.DialogFilter;
				}
			}

			public new string FileExtention
			{
				get
				{
					return base.FileExtention;
				}
			}
		}
		#endregion
	}
}
