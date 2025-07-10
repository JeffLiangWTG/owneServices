using System.IO;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	public abstract class DocumentCellCalculationsTestCase : TestCaseWithFactory
	{
		public void TestCantUseFileNameOnAutoTester()
		{
			AssertEquals("Cannot specify a template filename when checking in, you must supply a valid menu name", false, IsFile(GetMenuName()));
		}

		protected abstract BusinessObject GetNewBusinessObject();
		protected abstract DataContext GetDataContext();
		protected abstract string GetBusinessContext();
		protected abstract string GetMenuName();
		#region Implementation

		DirectoryInfo fTempOutputPath;
		ExcelInterface fExcelInterface;
		protected ExcelWorkSheet Worksheet;

		protected override void SetUp()
		{
			base.SetUp();
			SetupDocumentTest();
		}

		protected virtual void SetupDocumentTest()
		{
			int i = 0;
			string tempPath;
			do
			{
				tempPath = Env.TempPath + nameof(DocumentCellCalculationsTestCase) + i + "\\";
				if (Directory.Exists(tempPath))
				{
					try
					{
						Directory.Delete(tempPath, true);
					}
					catch
					{
					}
				}
			}
			while (Directory.Exists(tempPath));
			fTempOutputPath = Directory.CreateDirectory(tempPath);

			BusinessObject bO = GetNewBusinessObject();
			DocumentPack pack = GetNewDocumentPack(bO);
			using (PrintTask task = new PrintTask())
			{
				task.Add(pack);
				DeliveryInstructions instructions = new DeliveryInstructions();
				instructions.Destination = DeliveryInstructionDestination.Disk;
				instructions.OutputDirectory = tempPath;
				task.Run(instructions);
				AssertEquals("Output file count", 1, fTempOutputPath.GetFiles().Length);
				fExcelInterface = new ExcelInterface();
				fExcelInterface.LoadExcelFile(fTempOutputPath.GetFiles()[0].FullName);
				Worksheet = fExcelInterface.WorkSheets[0];
			}
		}

		protected override void TearDown()
		{
			if (Worksheet != null)
			{
				Worksheet.Dispose();
			}

			if (fExcelInterface != null)
			{
				fExcelInterface.Dispose();
			}

			if (fTempOutputPath.Exists)
			{
				try
				{
					fTempOutputPath.Delete(true);
				}
				catch
				{
					Thread.Sleep(5000);
					fTempOutputPath.Delete(true);
				}
			}
			base.TearDown();
		}

		protected bool IsFile(string menuName)
		{
			return File.Exists(menuName);
		}

		protected DocumentPack GetNewDocumentPack(BusinessObject bO)
		{
			string menuNameOrFileName = GetMenuName();
			DocumentPack result;

			if (IsFile(menuNameOrFileName))
			{
				result = new DocumentPack();
				ExcelTemplate template = new ExcelTemplateForUnitTesting(menuNameOrFileName, TestFilesSubFolder.ReportTestFiles);
				DocumentWrapper docWrapper = GetNewBusinessObjectWrapper(bO);
				Report report = new Report(result, template, docWrapper, "test document", null, DocumentDirection.ANY, false);
				result.Add(report);
			}
			else
			{
				DocumentZQuery filter = new DocumentZQuery(GetBusinessContext(), menuNameOrFileName);
				DocumentCommand[] menuItems = (DocumentCommand[])Factory.Load(typeof(DocumentCommand), filter);
				Assert("Ambiguous match on menu name " + menuNameOrFileName, menuItems.Length <= 1);
				Assert("Could not find menu name " + menuNameOrFileName, menuItems.Length == 1);

				result = new DocumentPack(menuItems[0], (IDocumentSupportable)bO, null, null);
			}
			return result;
		}

		protected virtual DocumentWrapper GetNewBusinessObjectWrapper(BusinessObject bO)
		{
			return DocumentWrapperFactory.CreateWrapper(GetDataContext(), bO);
		}

		#endregion
	}
}
