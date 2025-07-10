using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngine.Testing;
using Enterprise.Integration.RemoteDesktopServices;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Test.Winzor
{
	class WinzorExcelManagerTest : ZArchitecture.GUI.Testing.RemoteDesktopServicesTest
	{
		[ExpectNoExceptions]
		public void TestOpenReadOnlyTemplate()
		{
			TestTemplate("TestReadOnlyTemplate", true);
		}

		[ExpectNoExceptions]
		public void TestOpenReadWriteTemplate()
		{
			TestTemplate("TestReadWriteTemplate", false);
		}

		void TestTemplate(string templateName, bool readOnly)
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
					form.Show();
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

					var dummyRemoteExcelManager = new DummyRemoteExcelManager(form);
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

		class DummyRemoteExcelManager : IExcelManager
		{
			internal WinzorExcelManager RemoteExcelManager;

			internal DummyRemoteExcelManager(Form form)
			{
				RemoteExcelManager = new WinzorExcelManagerForTests(form, true);
			}

			public virtual void Edit(string workingFile, int row, int column)
			{
				RemoteExcelManager.Edit(workingFile, row, column);
				Thread.Sleep(TimeSpan.FromSeconds(2));
				CloseExcel();
			}

			protected void CloseExcel()
			{
				ExcelClosed(DialogResult.OK);
			}

			public event ExcelClosedEventHandler ExcelClosed;

			public bool IsSupported { get; set; }
		}

		class WinzorExcelManagerForTests : WinzorExcelManager
		{
			public WinzorExcelManagerForTests(Form parentForm, bool needToOpenFile) : base(parentForm)
			{
				this.needToOpenFile = needToOpenFile;
			}

			readonly bool needToOpenFile;

			protected override bool OpenFile(IRemoteFile file)
			{
				return !needToOpenFile || base.OpenFile(file);
			}
		}
	}
}
