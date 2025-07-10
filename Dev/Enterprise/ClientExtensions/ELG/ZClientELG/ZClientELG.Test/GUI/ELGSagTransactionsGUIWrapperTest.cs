using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.ELG.Testing
{
	[TestedType(typeof(ELGExportGUIWrapper))]
	public class ELGSagTransactionsGUIWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestInitialDirectory()
		{
			string dirPath = TempForTest.TempPath;
			DataTransferSwitchRegistryBusinessObject currentRegistrySettings = TestHelper.GetValidDataTransferSwitchRegistryBusinessObject(ZDateTime.Now.AddMinutes(-30));
			currentRegistrySettings.Directory = dirPath;
			DataTransferSwitchRegistryItemTest.UpdateValue(ELGDataRegistry.Instance.SagDataTransferSwitchRegistryItem, currentRegistrySettings);
			AssertEquals(dirPath, GUIWrapper.InitialDirectory);
		}

		public void TestFormCaption()
		{
			AssertEquals("Export Transactions in Sage Format", GUIWrapper.FormCaption);
		}

		public void TestDataExporter()
		{
			AssertNotNull(GUIWrapper.DataExporter);
			AccountingTransactionsDataExporter exporter = GUIWrapper.DataExporter;
			AssertEquals("Expected type of SagARExporter", typeof(SagARExporter), exporter.GetType());
			AssertSame("Data Exporter should be lazy loaded", exporter, GUIWrapper.DataExporter);
		}

		[TestDate(2005, 12, 8)]
		public void TestExport()
		{
			TestHelper.SetValidRegistryAll();
			TempDirectory.DeleteDirectory(ELGDataRegistry.Instance.SagExportDirectory);
			OrgHeader org = TestHelper.AddOrg();
			Factory.Save();
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(org.OH_Code, "AUD", "AR", org.OH_Code);
			TestHelper.SetValidRegistrySageAccountCodeCollectionItem(org.OH_Code, "AUD", "AP", org.OH_Code);
			TestHelper.NewPMG();
			TestHelper.NewInvoices(ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GUIWrapper.Export();
			FileInfo[] filesInDir = DirInfo.GetFiles();
			Assert("Files created", filesInDir.Length >= 1);
			string expectedMessage = "Batch " + GUIWrapper.DataExporter.FilterProvider.CurrentBatchNo + " was exported successfully.";
			ZString lastMessage = UnitTestUserNotification.Instance.LastMessage.Text;
			Assert("User notification message should contain the message: " + expectedMessage, lastMessage.Contains(expectedMessage));
			string filename = Path.Combine(ELGDataRegistry.Instance.SagExportDirectory, GUIWrapper.Filename);
			AssertEquals("File should be created/exported", true, File.Exists(filename));
			Assert("User Notification of File created should contain: " + expectedMessage, lastMessage.Contains(GUIWrapper.Filename));
			TempDirectory.DeleteDirectory(ELGDataRegistry.Instance.SagExportDirectory);
		}

		#region NonPersistentBusinessObjectTestCase
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ELGExportGUIWrapper(Factory);
		}

		#endregion
		DirectoryInfo DirInfo
		{
			get
			{
				return dirInfo ?? (dirInfo = new DirectoryInfo(ELGDataRegistry.Instance.SagExportDirectory));
			}
		}

		DirectoryInfo dirInfo;
		ELGExportGUIWrapperTestClass GUIWrapper
		{
			get
			{
				return guiWrapper ?? (guiWrapper = new ELGExportGUIWrapperTestClass(Factory));
			}
		}

		ELGExportGUIWrapperTestClass guiWrapper;
		ELGTestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new ELGTestHelper(Factory));
			}
		}

		ELGTestHelper testHelper;
		class ELGExportGUIWrapperTestClass : ELGExportGUIWrapper
		{
			public ELGExportGUIWrapperTestClass(BusinessObjectFactory factory) : base(factory)
			{
			}

			public new string FileExtention
			{
				get
				{
					return base.FileExtention;
				}
			}

			public new string DialogFilter
			{
				get
				{
					return base.DialogFilter;
				}
			}

			public new string InitialDirectory
			{
				get
				{
					return base.InitialDirectory;
				}
			}

			public new AccountingTransactionsDataExporter DataExporter
			{
				get
				{
					return base.DataExporter;
				}
			}

			public string Filename
			{
				get
				{
					return base.FileName;
				}
			}

			public DialogResult ExposedDialogResult = DialogResult.OK;
		}
	}
}
