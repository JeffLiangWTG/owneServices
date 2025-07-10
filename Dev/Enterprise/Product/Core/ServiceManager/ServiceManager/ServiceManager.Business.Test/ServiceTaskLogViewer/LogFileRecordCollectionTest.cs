using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;

namespace Enterprise.ServiceManager.Business.Testing
{
	[TestedType(typeof(LogFileRecordCollection))]
	sealed class LogFileRecordCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LogFileRecordCollection>
	{
		public void TestLoad()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var testFilesDirectory = Path.GetDirectoryName(resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.AD_20070720.TXT", "AD_20070720.TXT"));
				resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.ADC_20070720.TXT", "ADC_20070720.TXT");
				resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.DBM_20070720.TXT", "DBM_20070720.TXT");
				resourceRetriever.SaveResourceToFile("Enterprise.ServiceManager.Business.Testing.ServiceTaskLogViewer.TestFiles.LWK_20070720.TXT", "LWK_20070720.TXT");
				var collection = new LogFileRecordCollection();
				collection.Sort("Name");
				collection.Load(new List<ILogViewerDataProvider>() { new LogViewerDataProviderFactory.LocalLogViewerDataProvider(testFilesDirectory, null) });

				AssertEquals(4, collection.Count);
				AssertEquals("AD_20070720.TXT", collection[0].Name);
				AssertEquals(0, collection[0].EventList.Count);
				AssertEquals("ADC_20070720.TXT", collection[1].Name);
				AssertEquals(0, collection[1].EventList.Count);
				AssertEquals("DBM_20070720.TXT", collection[2].Name);
				AssertEquals(7, collection[2].EventList.Count);
				AssertEquals("LWK_20070720.TXT", collection[3].Name);
				AssertEquals(19, collection[3].EventList.Count);
			}
		}

		protected override LogFileRecordCollection GetCollectionToTest()
		{
			return new LogFileRecordCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return LogFileRecordTest.GetNewLogFileRecord();
		}
	}
}
