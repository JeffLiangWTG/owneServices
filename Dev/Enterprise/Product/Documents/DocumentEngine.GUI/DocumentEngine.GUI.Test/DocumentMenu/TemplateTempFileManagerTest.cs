using System;
using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	sealed class TemplateTempFileManagerTest : TestCaseWithFactory
	{
		public void TestSetupWorkingFileForUnmodifiableExcelTemplates()
		{
			var tempFileName = Temp.GetTempFileName();
			try
			{
				var template = Factory.New<StmTemplateBase>();
				template.ReadOnly = true;
				template.SO_IsSystemDefined = ZBool.True;
				template.SO_Name = "Test Template";
				template.SO_ExcelTemplatePath = tempFileName;
				template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
				template.IsCheckedOutByMe = false;

				AssertEquals("Precondition: template.CanModifyExcelTemplate", false, template.CanModifyExcelTemplate);

				var preSetupFileAttributes = File.GetAttributes(template.ExcelTemplateFullPath);
				var fileManager = new TemplateTempFileManager(template);
				var workingFile = fileManager.SetupWorkingFile();

				AssertEquals("File.GetAttributes(workingFile)", FileAttributes.ReadOnly, File.GetAttributes(workingFile));

				fileManager.CleanupWorkingFile();

				AssertEquals("Precondition: workingFile", template.ExcelTemplateFullPath, workingFile);
				AssertEquals("File.GetAttributes(workingFile)", preSetupFileAttributes, File.GetAttributes(workingFile));
			}
			finally
			{
				File.Delete(tempFileName);
			}
		}

		public void TestSetupWorkingFile_WithSO_ExcelTemplatePath_GetsProperExtension_XLS()
		{
			AssertSetupWorkingFile_SO_ExcelTemplatePath("Test.xls", ".xls");
		}

		public void TestSetupWorkingFile_WithSO_ExcelTemplatePath_GetsProperExtension_XLSX()
		{
			AssertSetupWorkingFile_SO_ExcelTemplatePath("Test.xlsx", ".xlsx");
		}

		public void TestSetupWorkingFile_WithEmptySO_ExcelTemplatePath_UsesDefaultXLSExtention()
		{
			AssertSetupWorkingFile_SO_ExcelTemplatePath(string.Empty, ".xls");
		}

		void AssertSetupWorkingFile_SO_ExcelTemplatePath(string templatePath, string expectedExtension)
		{
			var workingFile = string.Empty;

			var template = Factory.New<StmTemplateBase>();
			template.ReadOnly = true;
			template.SO_IsSystemDefined = ZBool.False;
			template.SO_Name = "Test Template";
			template.SO_ExcelTemplatePath = templatePath;
			template.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[Hello World]
{A}-[#EndOfReport]");
			template.IsCheckedOutByMe = false;

			var fileManager = new TemplateTempFileManager(template);
			workingFile = fileManager.SetupWorkingFile();
			fileManager.CleanupWorkingFile();

			Assert(Path.GetExtension(workingFile).Equals(expectedExtension, StringComparison.InvariantCultureIgnoreCase));
		}

		[ExpectNoExceptions]
		public void TestCleanupWorkingFile_DoesntThrowWhenFileNotFound()
		{
			var tempFileName = "I am a fake path";
			var template = Factory.New<StmTemplateBase>();
			template.SO_ExcelTemplatePath = tempFileName;
			template.SO_IsSystemDefined = ZBool.True;

			var fileManager = new TemplateTempFileManager(template);
			fileManager.SetupWorkingFile();

			Assert(!File.Exists(tempFileName));

			fileManager.CleanupWorkingFile();
		}
	}
}
