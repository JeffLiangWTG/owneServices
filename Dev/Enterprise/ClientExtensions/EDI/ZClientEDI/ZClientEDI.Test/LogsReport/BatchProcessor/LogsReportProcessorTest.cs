using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.DocumentScanning.Business;
using Enterprise.MasterFiles.Business.LogsReport;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LogsReporting.BatchProcessor.Testing
{
	public class LogsReportProcessorTest : TestCaseWithFactory
	{
		[TestDate(2001, 8, 9)]
		public void TestProcess()
		{
			var incident = Factory.NewWithValidTestData<SupportIncident>();
			incident.IM_IncidentNumber = "CS01234567";
			Factory.Save();
			LogsReport report = new LogsReport("CS01234567", "ABC", new byte[] { 0x01, 0x23, 0x45 });
			LogsReportProcessor processor = new LogsReportProcessor();
			processor.Process(report.XML);
			var factory2 = new BusinessObjectFactory();
			var incidentReloaded = factory2.Load<SupportIncident>(incident.PK);
			var eDoc = (StorageFile)incidentReloaded.DocManagerInfo.AllEDocs[0];
			AssertEquals("ZIP", eDoc.SC_DataType);
			AssertEquals("INT", eDoc.SC_DocType);
			AssertEquals("ABC logs 20010809", eDoc.SC_FileName);
			AssertEquals(false, eDoc.SC_IsPublished);
			AssertEquals(3, eDoc.SC_ImageData.Length);
			AssertEquals((byte)0x01, eDoc.SC_ImageData[0]);
			AssertEquals((byte)0x23, eDoc.SC_ImageData[1]);
			AssertEquals((byte)0x45, eDoc.SC_ImageData[2]);
		}
	}
}
