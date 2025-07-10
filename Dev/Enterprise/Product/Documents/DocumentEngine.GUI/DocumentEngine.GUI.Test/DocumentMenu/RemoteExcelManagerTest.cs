using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.RemoteDesktopServices.Server;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class RemoteExcelManagerTest : RemoteDesktopServices.Testing.RemoteDesktopServicesTest
	{
		[ExpectNoExceptions]
		public void TestReadOnlyTemplate()
		{
			TestTemplate("TestReadOnlyTemplate", true);
		}

		[ExpectNoExceptions]
		public void TestReadWriteTemplate()
		{
			TestTemplate("TestReadWriteTemplate", false);
		}

		void TestTemplate(string templateName, bool readOnly, bool remoteFile = false)
		{
			try
			{
				var workingFileName = Path.Combine(Temp.TempPath, templateName + "DAT.xls");
				using (var resourceRetriever = new EmbeddedResourceRetriever())
				{
					var tempDirectoryPath = resourceRetriever.SaveAllResourcesToFiles();
					var workingFileNameDat = Path.Combine(tempDirectoryPath, "Enterprise.DocumentEngine.GUI.Testing." + templateName + ".xls");

					if (!File.Exists(workingFileName))
					{
						File.Copy(workingFileNameDat, workingFileName);
					}
				}

				if (readOnly)
				{
					//Make sure TestReadOnlyTemplate.xls is readonly
					if (!File.GetAttributes(workingFileName).HasFlag(FileAttributes.ReadOnly))
					{
						File.SetAttributes(workingFileName, FileAttributes.ReadOnly);
					}
				}
				else
				{
					//Make sure TestReadWriteTemplate.xls is writable
					if (!File.GetAttributes(workingFileName).HasFlag(FileAttributes.Normal))
					{
						File.SetAttributes(workingFileName, FileAttributes.Normal);
					}
				}

				using (var form = new Form())
				{
					var template = Factory.New<StmTemplateBase>();
					template.SO_IsSystemDefined = ZBool.True;
					template.SO_Name = "Unit Test";
					template.SO_ExcelTemplatePath = workingFileName;
					template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
		@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
					template.IsCheckedOutByMe = false;
					template.ReadOnly = readOnly;

					var dummyRemoteExcelManager = new DummyRemoteExcelManager(form, templateName, remoteFile);
					dummyRemoteExcelManager.IsSupported = true;

					var templateEditor = new TemplateEditor(template, form);
					templateEditor.SetExcelManagerForTesting(dummyRemoteExcelManager);
					templateEditor.Edit();
				}
			}
			finally
			{
				//Cleaning created files
				string[] fileNames = Directory.GetFiles(Temp.TempPath, templateName + "*.xls");
				foreach (var filename in fileNames)
				{
					if (!File.GetAttributes(filename).HasFlag(FileAttributes.Normal))
					{
						File.SetAttributes(filename, FileAttributes.Normal);
					}
					File.Delete(filename);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestEditingForm_Closed()
		{
			TestTemplate("TestReadWriteTemplate", false, true);
		}

		public void TestEditTemplateWithNullTemplate()
		{
			var templateName = Core.Constants.SectionRepositoryTemplateNames.User;
			using (var form = new Form())
			{
				var template = Factory.New<StmTemplateBase>();
				template.SO_Name = templateName;
				template.SO_Template = null;

				var dummyRemoteExcelManager = new DummyRemoteExcelManagerNotOpeningFile(form, templateName, false);
				dummyRemoteExcelManager.IsSupported = true;

				var templateEditor = new TemplateEditor(template, form);
				templateEditor.SetExcelManagerForTesting(dummyRemoteExcelManager);
				AssertExceptionThrown<ExcelInterfaceException>(templateEditor.Edit);

				var okButton = (ZButton)((RemoteExcelManagerForTests)dummyRemoteExcelManager.RemoteExcelManager).EditingForm.Controls.Find("OKBtn", true)[0];
				okButton.PerformClick();
				AssertNoExceptionThrown(okButton.PerformClick);
			}
		}

		class RemoteExcelManagerForTests : RemoteExcelManager
		{
			public RemoteExcelManagerForTests(Form parentForm, bool needToOpenFile) : base(parentForm)
			{
				this.needToOpenFile = needToOpenFile;
			}

			readonly bool needToOpenFile;

			public EditingExcelForm EditingForm => editingForm;

			protected override bool OpenFile(RemoteFile file)
			{
				return !needToOpenFile || RunInAnotherThreadWithTimeout(() => base.OpenFile(file), false);
			}

			protected override byte[] FetchFileData(RemoteFile file)
			{
				return RunInAnotherThreadWithTimeout(() => base.FetchFileData(file), null);
			}

			T RunInAnotherThreadWithTimeout<T>(Func<T> f, T valueOnFailure)
			{
				try
				{
					var task = Task.Run(f);
					return task.Wait(TimeSpan.FromSeconds(5)) ?
						task.Result :
						valueOnFailure;
				}
				catch (Exception ex) when (ex.Find<COMException>() != null)
				{
					return valueOnFailure;
				}
			}
		}

		#region Implementation

		class DummyRemoteExcelManager : IExcelManager
		{
			internal RemoteExcelManager RemoteExcelManager;
			readonly string templateName;

			internal DummyRemoteExcelManager(Form form, string templateName, bool remote)
			{
				this.RemoteExcelManager = new RemoteExcelManagerForTests(form, true);
				this.templateName = templateName;
				this.remote = remote;
			}

			Mock<RemoteFile> remoteFileMoq;
			readonly bool remote;

			public virtual void Edit(string workingFile, int row, int column)
			{
				RemoteExcelManager.Edit(workingFile, row, column);
				if (remote)
				{
					remoteFileMoq = new Mock<RemoteFile>(workingFile, File.ReadAllBytes(workingFile), false, false) { CallBase = true };
					remoteFileMoq.Setup(m => m.FetchFileData()).Returns((byte[])null);
				}

				Thread.Sleep(TimeSpan.FromSeconds(2));

				CloseTemplate(templateName);
				CloseExcel();

				if (remoteFileMoq != null)
				{
					RemoteExcelManager.SetRemoteFile(remoteFileMoq.Object);
				}

				RemoteExcelManager.CloseEditingFormForTest();
			}

			protected void CloseExcel()
			{
				ExcelClosed(DialogResult.OK);
			}

			public event ExcelClosedEventHandler ExcelClosed;

			public bool IsSupported { get; set; }
		}

		class DummyRemoteExcelManagerNotOpeningFile : DummyRemoteExcelManager
		{
			internal DummyRemoteExcelManagerNotOpeningFile(Form form, string templateName, bool remote)
				: base(form, templateName, remote)
			{
				this.RemoteExcelManager = new RemoteExcelManagerForTests(form, false);
			}

			public override void Edit(string workingFile, int row, int column)
			{
				RemoteExcelManager.Edit(workingFile, row, column);
				CloseExcel();
			}
		}

		static void CloseTemplate(string templateName)
		{
			Excel.Application instance = GetRunningExcelInstance();

			if (instance != null)
			{
				Excel.Workbooks workbooks = instance.Workbooks;
				Excel.Workbook testWorkbook = GetWorkBookByName(workbooks, templateName);

				if (testWorkbook != null)
				{
					HandleCOMException(() => testWorkbook.Close(false, null, false));
					Marshal.ReleaseComObject(testWorkbook);
					testWorkbook = null;
				}

				int workbooksCount = workbooks != null ? workbooks.Count : 0;

				if (workbooks != null)
				{
					if (workbooksCount == 0)
					{
						HandleCOMException(() => workbooks.Close());
					}
					Marshal.ReleaseComObject(workbooks);
					workbooks = null;
				}

				if (workbooksCount == 0)
				{
					HandleCOMException(() => instance.Quit());
				}
				Marshal.ReleaseComObject(instance);
				instance = null;

#pragma warning disable CW1056 // Do Not Use GC.Collect()
				GC.Collect();
#pragma warning restore CW1056 // Do Not Use GC.Collect()
				GC.WaitForPendingFinalizers();
			}
		}

		static void HandleCOMException(Action action)
		{
			try
			{
				action();
			}
			catch (COMException) { }
		}

		static Excel.Application GetRunningExcelInstance()
		{
			Excel.Application instance = null;
			//Application not running, or can't get a handle
			HandleCOMException(() => instance = GetActiveObject("Excel.Application") as Excel.Application);

			return instance;
		}

		static Excel.Workbook GetWorkBookByName(Excel.Workbooks workbooks, string workbookName)
		{
			Excel.Workbook testWorkBook = null;
			if (workbooks != null)
			{
				foreach (var workbook in workbooks)
				{
					var excelWorkbook = workbook as Excel.Workbook;
					if (excelWorkbook != null && excelWorkbook.Name.Contains(workbookName))
					{
						testWorkBook = excelWorkbook;
						break;
					}
				}
			}
			return testWorkBook;
		}

		#endregion

		// Copy-paste from .NETFramework\v4.8\mscorlib.dll because .NET does not have Marshal.GetActiveObject method
		#region GetActiveObjectSupport

		[SecurityCritical]
		static object GetActiveObject(string progID)
		{
			object ppunk;
			Guid clsid;
			try
			{
				CLSIDFromProgIDEx(progID, out clsid);
			}
			catch (Exception)
			{
				CLSIDFromProgID(progID, out clsid);
			}

			GetActiveObject(ref clsid, IntPtr.Zero, out ppunk);
			return ppunk;
		}

		[DllImport("ole32.dll", PreserveSig = false)]
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		static extern void CLSIDFromProgIDEx([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

		[DllImport("ole32.dll", PreserveSig = false)]
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		static extern void CLSIDFromProgID([MarshalAs(UnmanagedType.LPWStr)] string progId, out Guid clsid);

		[DllImport("oleaut32.dll", PreserveSig = false)]
		[SuppressUnmanagedCodeSecurity]
		[SecurityCritical]
		static extern void GetActiveObject(ref Guid rclsid, IntPtr reserved, [MarshalAs(UnmanagedType.Interface)] out object ppunk);
		#endregion
	}
}
