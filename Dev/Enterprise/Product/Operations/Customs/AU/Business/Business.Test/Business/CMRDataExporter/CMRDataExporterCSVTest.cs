using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public abstract class CMRDataExporterCSVTest : NonPersistentBusinessObjectTestCase
	{
		protected abstract CMRDataExporterCSV GetNewExporter();

		protected abstract string ExpectedCSVResult { get; }

		protected abstract string ExpectedFileName { get; }

		protected abstract string ExpectedMailSubject { get; }

		protected abstract bool IsDocManagerSupported { get; }

		protected virtual string ExpectedBodyText
		{ get { return ""; } }

		public void TestExpectedFileName()
		{
			CMRDataExporterCSV exporter = GetNewExporter();
			var generatedData = exporter.Generate();
			AssertEquals("Correct filename", true, generatedData.FileName.StartsWith(ExpectedFileName));
		}

		public void TestExpectedFileNameSuffix()
		{
			var exporter = GetNewExporter();
			var generatedData = exporter.Generate();
			Assert("Contain suffix", generatedData.FileName.EndsWith(CMRDataExporterCSV.SanitizingFileName(exporter.FileNameSuffix) + ".csv"));
		}

		public void TestExpectedMailSubject()
		{
			CMRDataExporterCSV exporter = GetNewExporter();
			AssertEquals("Correct Mail Subject", exporter.MailSubject, ExpectedMailSubject);
		}

		public void TestExpectedBodyText()
		{
			CMRDataExporterCSV exporter = GetNewExporter();
			AssertEquals("Correct Mail Body", exporter.BodyText, ExpectedBodyText);
		}

		public void TestExpectedCSVResult()
		{
			CMRDataExporterCSV exporter = GetNewExporter();
			var generatedData = exporter.Generate();
			AssertEquals("Output is correct", ExpectedCSVResult, generatedData.Content);
		}

		public void TestSaveToFile()
		{
			CMRDataExporterCSV exporter = GetNewExporter();
			string tempFile = Env.GetTempFileName();
			File.Delete(tempFile);

			exporter.Generate();
			try
			{
				exporter.SaveToFile(File.OpenWrite(tempFile));
				AssertEquals("Should write the file successfully", true, File.Exists(tempFile));
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		public void TestAttachToeDocs()
		{
			if (IsDocManagerSupported)
			{
				var exporter = GetNewExporter();
				exporter.Generate();
				AssertEquals("No eDoc attached", 0, ((IDocManagerSupport)exporter.BizObj).DocManagerInfo.AllEDocs.Count);
				exporter.AttachToeDocs();
				AssertEquals("1 eDoc attached", 1, ((IDocManagerSupport)exporter.BizObj).DocManagerInfo.AllEDocs.Count);
			}
			else
			{
				Assert(true);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewExporter();
		}
	}
}
