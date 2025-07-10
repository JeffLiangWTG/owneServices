using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class ExcelManagerTest : TestCaseWithFactory
	{
		//heavy test. uncomment to find faulty reports
		//class ExcelManagerForTest : ExcelManager
		//{
		//    internal ExcelManagerForTest(Form parentForm)
		//        :base(parentForm)
		//    { }

		//    public void OpenTemplateBook_Exposed(string filePath)
		//    {
		//        base.OpenTemplateBook(filePath);
		//    }
		//}

		//public void TestOpenAllReports()
		//{
		//    ExcelManagerForTest manager = new ExcelManagerForTest(null);
		//    string basePath = Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\ExcelTemplates\Reports");
		//    string[] files = Directory.GetFiles(basePath, "*.xls", SearchOption.AllDirectories);
		//    StringCollectionX badFiles = new StringCollectionX();
		//    manager.ExcelApplication = new Excel.ApplicationClass();
		//    foreach (string file in files)
		//    {
		//        try
		//        {
		//            manager.OpenTemplateBook_Exposed(file);
		//        }
		//        catch(Exception ex)
		//        {
		//            Exception inner = ex;
		//            while (inner.InnerException != null)
		//            {
		//                inner = inner.InnerException;
		//            }
		//            badFiles.Add(file + "\t\t" + inner.Message);
		//        }
		//    }
		//    manager.ExcelApplication.Quit();
		//    GC.Collect();
		//    AssertEquals(string.Join("\r\n", badFiles.ToArray()), 0, badFiles.Count);
		//}

		[GuiTest]
		public void TestComExceptionAreUserFriendlyDisplayed()
		{
			using (var form = new Form())
			{
				var manager = new ExcelManager(form);
				var fakeFilename = "ThisIsAFakeFilename";
				Assert("Precondition: File.Exists(xlsFile)", !File.Exists(fakeFilename));

				try
				{
					manager.Edit(fakeFilename, -1, -1);
				}
				finally
				{
					var excelApplication = manager.ExcelApplication;
					if (excelApplication != null)
					{
						if ((excelApplication.ActiveWorkbook != null))
						{
							excelApplication.ActiveWorkbook.GetType().InvokeMember("Close", System.Reflection.BindingFlags.InvokeMethod, null, excelApplication.ActiveWorkbook, new object[] { false, Missing.Value, Missing.Value }, manager.ForceEnglishUSLocale);
						}
						excelApplication.Quit();
					}
				}

				Assert(UnitTestUserNotification.Instance.LastMessage.Text.StartsWith("There was a problem opening this document. Details:"));
			}
		}

		[GuiTest]
		public void TestLocaleIndependenceJapanese()
		{
			AssertLocaleIndependence("ja");
		}

		[GuiTest]
		public void TestLocaleIndependenceGerman()
		{
			AssertLocaleIndependence("de");
		}

		[GuiTest]
		public void TestLocaleIndependenceIcelandic()
		{
			AssertLocaleIndependence("is");
		}

		[GuiTest]
		public void TestLocaleIndependenceFrench()
		{
			AssertLocaleIndependence("fr");
		}

		[GuiTest]
		public void TestLocaleIndependenceEnglish()
		{
			AssertLocaleIndependence("en");
		}

		void AssertLocaleIndependence(string locale)
		{
			var wasCultureInfo = Thread.CurrentThread.CurrentUICulture;
			try
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(locale);
				AssertNotEquals(wasCultureInfo.Name, Thread.CurrentThread.CurrentUICulture.Name);

				using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly))
				using (var form = new Form())
				{
					var manager = new ExcelManager(form);
					var testXls = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					AssertEquals("Precondition: File.Exists(xlsFile)", true, File.Exists(testXls));

					//if anything goes wrong we will get an exception
					try
					{
						manager.Edit(testXls, -1, -1);
					}
					finally
					{
						var excelApplication = manager.ExcelApplication;
						if (excelApplication != null)
						{
							if (excelApplication.ActiveWorkbook != null)
							{
								excelApplication.ActiveWorkbook.GetType().InvokeMember("Close", BindingFlags.InvokeMethod, null, excelApplication.ActiveWorkbook, new object[] { false, Missing.Value, Missing.Value }, manager.ForceEnglishUSLocale);
							}

							excelApplication.Quit();
						}
					}
					//if we get to here its all OK. 
				}
			}
			finally
			{
				Thread.CurrentThread.CurrentUICulture = wasCultureInfo;
			}
		}

		[GuiTest]
		public void TestInvalidCastExceptionAreUserFriendlyDisplayed()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly))
			using (var form = new ZForm())
			{
				var manager = new ExcelManagerInvalidCastExceptionTest(form);
				var lastMessageText = "";

				UnitTestUserNotification.Instance.ClearMessages();
				try
				{
					manager.Edit("", -1, -1);

					lastMessageText = UnitTestUserNotification.Instance.LastMessage.Text;
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessages();

					var excelApplication = manager.ExcelApplication;
					if (excelApplication != null)
					{
						if (excelApplication.ActiveWorkbook != null)
						{
							excelApplication.ActiveWorkbook.GetType().InvokeMember("Close", System.Reflection.BindingFlags.InvokeMethod, null, excelApplication.ActiveWorkbook, new object[] { false, Missing.Value, Missing.Value }, manager.ForceEnglishUSLocale);
						}

						excelApplication.Quit();
					}
				}

				AssertEquals("InvalidCastException are user friendly displayed", "A problem occurred while running Excel. The Windows registry version value of Excel is different to its installed version.\r\nIt is recommended that you completely uninstall Excel and then reinstall it.", lastMessageText);
			}
		}

		class ExcelManagerInvalidCastExceptionTest : ExcelManager
		{
			internal ExcelManagerInvalidCastExceptionTest(Form parentForm) : base(parentForm)
			{
			}

			protected override void OpenTemplateBook(string filePath)
			{
				throw new InvalidCastException("Unable to cast COM object of type 'Excel.ApplicationClass' to interface type 'Excel._Application'. This operation failed because the QueryInterface call on the COM component for the interface with IID '{000208D5-0000-0000-C000-000000000046}' failed due to the following error: Library not registered. (Exception from HRESULT: 0x8002801D (TYPE_E_LIBNOTREGISTERED)).");
			}
		}
	}
}
