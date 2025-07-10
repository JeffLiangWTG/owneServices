using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer;
using Enterprise.Accounting.DataTransfer.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Client.AWH.Testing
{
	[TestedType(typeof(AWHARTransExportGUIWrapper))]
	public class AWHARTransExportGUIWrapperTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFormCaption()
		{
			AssertEquals("Format Caption", "Export Transactions To AWH File", GUIWrapper.FormCaption);
		}

		public void TestDataExporter()
		{
			AssertNotNull("Data Exporter", GUIWrapper.DataExporter);
			AssertEquals("Data Exporter", typeof(AWHARInvoiceDataExporter), GUIWrapper.DataExporter.GetType());
		}

		public void TestFileExtension()
		{
			AssertEquals("File Extension", ".csv", GUIWrapper.FileExtention);
		}

		public void TestInitialDirectory()
		{
			AWHDataRegistry.Instance.ARTransExportDirectory = "1234";
			AssertEquals("Initial Directory", "1234", GUIWrapper.InitialDirectory);
		}

		[TestDate(2007, 1, 1, 12, 0, 0)]
		public void TestExport()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				ZString expectedOutputFile = resourceRetriever.SaveResourceToFile("Prefix_0005_20070101120000.csv");
				TransactionExportTestDataHelper dataHelper = new TransactionExportTestDataHelper(Factory);
				CreateLSCCode(dataHelper);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("No File is found on the Export Directory", 0, Directory.GetFiles(ExportDirectory).Length);
				GUIWrapper.Export();
				ExportedFile = "Prefix_" + GUIWrapper.LastBatchNumber.ToString().PadLeft(4, '0') + "_20070101120000.csv";
				ZString exportedFileFullPath = Path.Combine(ExportDirectory, ExportedFile);
				AssertEquals("File should be created/exported", true, File.Exists(exportedFileFullPath));
				string fileCreatedMessage = string.Format("File {0} has been created.\r\n\r\nBatch {1} was exported successfully.\r\n\r\n", ExportedFile, GUIWrapper.LastBatchNumber);
				AssertUserNotificationMessage(fileCreatedMessage);
				AssertASCIIFilesHasSameData(expectedOutputFile, exportedFileFullPath);
			}
		}

		public void TestLSCCodeIsNotSet()
		{
			TransactionExportTestDataHelper dataHelper = new TransactionExportTestDataHelper(Factory);
			AssertEquals("No File is found on the Export Directory", 0, Directory.GetFiles(ExportDirectory).Length);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GUIWrapper.Export();
			AssertEquals("No File is created as LSC code is not set", 0, Directory.GetFiles(ExportDirectory).Length);
			string expectedMessage = "The Following Properties must be set and Batch " + GUIWrapper.LastBatchNumber.ToString() + " Needs to be Manually Recreated.\n" + AWHConstants.DashLine + "\r\nThe Following Debtors do not have Legacy Code set:\r\n" + AWHConstants.DashLine + "\r\n" + dataHelper.Header.OH_Code + "\r\n\r\n";
			AssertUserNotificationMessage(expectedMessage);
		}

		public void TestBranchIsNotSet()
		{
			TransactionExportTestDataHelper dataHelper = new TransactionExportTestDataHelper(Factory);
			CreateLSCCode(dataHelper);
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("LAA", "AAA"));
			AssertEquals("Preconditions: Current Branch is not 'LAA'", true, Env.CurrentBranch.Code != "LAA");
			AWHDataRegistry.Instance.BranchList = list;
			AssertEquals("No File is found on the Export Directory", 0, Directory.GetFiles(ExportDirectory).Length);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GUIWrapper.Export();
			AssertEquals("No File is created as Branch code is not set", 0, Directory.GetFiles(ExportDirectory).Length);
			string expectedMessage = "The Following Properties must be set and Batch " + GUIWrapper.LastBatchNumber.ToString() + " Needs to be Manually Recreated.\n" + AWHConstants.DashLine + "\r\nThe Following Branches do not have AWH Branch Code set:\r\n" + AWHConstants.DashLine + "\r\n" + Env.CurrentBranch.Code + "\r\n\r\n";
			AssertUserNotificationMessage(expectedMessage);
		}

		public void TestDeptIsNotSetCorrectly()
		{
			TransactionExportTestDataHelper dataHelper = new TransactionExportTestDataHelper(Factory);
			CreateLSCCode(dataHelper);
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.Add(new CodeDescriptionPair("LAA", "A"));
			AssertEquals("Preconditions: Current Dept is not 'LAA'", true, Env.CurrentDepartment.Code != "L");
			AWHDataRegistry.Instance.DepartmentList = list;
			AssertEquals("No File is found on the Export Directory", 0, Directory.GetFiles(ExportDirectory).Length);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GUIWrapper.Export();
			AssertEquals("No File is created as Department code is not set", 0, Directory.GetFiles(ExportDirectory).Length);
			string expectedMessage = "The Following Properties must be set and Batch " + GUIWrapper.LastBatchNumber.ToString() + " Needs to be Manually Recreated.\n" + AWHConstants.DashLine + "\r\nThe Following Departments do not have AWH Department Code set:\r\n" + AWHConstants.DashLine + "\r\n" + Env.CurrentDepartment.Code + "\r\n\r\n";
			AssertUserNotificationMessage(expectedMessage);
		}

		void AssertUserNotificationMessage(string expectedMessage)
		{
			bool messageFound = false;
			foreach (UnitTestUserNotification.PreviousMessage actualMessage in UnitTestUserNotification.Instance.PreviousMessages)
			{
				if (expectedMessage == actualMessage.Text)
				{
					messageFound = true;
					break;
				}
			}

			Assert(expectedMessage + " was not found", messageFound);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AWHARTransExportGUIWrapper(Factory);
		}

#region Implementation
		AWHARTransExportGUIWrapperForTest GUIWrapper;
		string ExportDirectory;
		string ExportedFile;
		protected override void SetUp()
		{
			base.SetUp();
			GUIWrapper = new AWHARTransExportGUIWrapperForTest(Factory);
			ExportDirectory = Path.Combine(Env.TempPath, "Export");
			Directory.CreateDirectory(ExportDirectory);
			SetupRegistryItems();
			ExportedFile = "";
		}

		void CreateLSCCode(TransactionExportTestDataHelper dataHelper)
		{
			GlbStaff user1 = Factory.New<GlbStaff>();
			user1.GS_LoginName = "hello_user";
			user1.GS_FullName = "John David User";
			user1.StaffPlainTextPassword = "qwerty";
			user1.GS_UserAddress1 = "123 user st";
			user1.GS_City = "SYDNEY";
			user1.GS_Code = "DDD";
			user1.GS_EmailAddress = "user1@edifakeclient.cog";
			OrgCusCode lscCusCode = dataHelper.Header.CustomsCodes.AddNew();
			lscCusCode.OK_RN_NKCodeCountry = Env.CurrentCompany.Country.Code;
			lscCusCode.OK_CodeType = "LSC";
			lscCusCode.OK_CustomsRegNo = "CusRegNo";
			Factory.Save();
		}

		void SetupRegistryItems()
		{
			AWHDataRegistry.Instance.ARTransExportDirectory = ExportDirectory;
			AWHDataRegistry.Instance.ExportFilePrefix = "Prefix";
			CodeDescriptionPairList brhList = new CodeDescriptionPairList();
			brhList.Add(new CodeDescriptionPair(Env.CurrentBranch.Code, "ABC"));
			AWHDataRegistry.Instance.BranchList = brhList;
			CodeDescriptionPairList deptList = new CodeDescriptionPairList();
			deptList.Add(new CodeDescriptionPair(Env.CurrentDepartment.Code, "1"));
			AWHDataRegistry.Instance.DepartmentList = deptList;
		}

		protected override void TearDown()
		{
			base.TearDown();
			TempDirectory.DeleteDirectory(ExportDirectory);
		}

#region Setup
		class AWHARTransExportGUIWrapperForTest : AWHARTransExportGUIWrapper
		{
			public AWHARTransExportGUIWrapperForTest(BusinessObjectFactory factory) : base(factory)
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

			public new int LastBatchNumber
			{
				get
				{
					return base.LastBatchNumber;
				}
			}

			public DialogResult ExposedDialogResult = DialogResult.OK;
			protected override DialogResult ShowDialog(IFileDialog dialog)
			{
				if (ExposedDialogResult == DialogResult.OK)
				{
					using (Stream file = (dialog as ZSaveFileDialog).OpenFile())
					{
					}
				}

				return ExposedDialogResult;
			}
		}
#endregion
#endregion
	}
}
