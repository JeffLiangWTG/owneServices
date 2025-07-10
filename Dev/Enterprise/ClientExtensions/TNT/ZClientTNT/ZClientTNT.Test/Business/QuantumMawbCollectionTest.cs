using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	[TestedType(typeof(QuantumMawbCollection))]
	public class QuantumMawbCollectionTest : NonPersistentBusinessObjectCollectionTestCase<QuantumMawbCollection>
	{
		public void TestCollection()
		{
			NotificationBuffer notify = new NotificationBuffer();
			string tempSampleX2Path = TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name);
			QuantumMawbCollection collection = new QuantumMawbCollection(Factory, tempSampleX2Path);
			AssertEquals("Collection should be emtpy", 0, collection.Count);
			collection.LoadFromFile();
			AssertEquals("Elements in the collection", 6, collection.Count);
			Assert("IsLinkedToConsol", !collection[0].IsLinkedToConsol);
			AssertEquals("Element 0 MasterBill", "08135025185", collection[0].MasterBillNum);
			AssertEquals("Element 0 FlightNumber", "JL5772", collection[0].FlightNumber);
			AssertEquals("Element 1 MasterBill", "19933462310", collection[1].MasterBillNum);
			AssertEquals("Element 1 DepartureDate", new ZDateTime(2004, 08, 16), collection[1].DepartureDate);
			AssertEquals("Element 2 MasterBill", "08133462295", collection[2].MasterBillNum);
			AssertEquals("Element 2 FlightNumber", "PX4006", collection[2].FlightNumber);
			AssertEquals("Element 3 MasterBill", "08135025185", collection[3].MasterBillNum);
			AssertEquals("Element 3 FlightNumber", "PX004", collection[3].FlightNumber);
			AssertEquals("Element 4 MasterBill", "07405320851", collection[4].MasterBillNum);
			PortMatcher matcher = new PortMatcher(Factory, "MEL", notify, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			AssertEquals("Element 4 PortOfDischarge", matcher.Result, collection[4].PortOfDischarge);
			AssertEquals("Element 5 MasterBill", "70305321050", collection[5].MasterBillNum);
			matcher = new PortMatcher(Factory, "AKL", notify, GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			AssertEquals("Element 5 PortOfLoading", matcher.Result, collection[5].PortOfLoading);
		}

		[ExpectException(typeof(FileNotFoundException))]
		public void TestInexistingFileThrowsException()
		{
			string sampleInvalidExit2FilePath = Path.Combine(Env.TempPath, "InexistingFile");
			QuantumMawbCollection collection = new QuantumMawbCollection(Factory, sampleInvalidExit2FilePath);
		}

		public void TestInvalidFileNameDoesNotThrowExceptionOnLoadFromFile()
		{
			var collection = new QuantumMawbCollection(Factory, TestUtils.InvalidFileNameFullName);
			AssertEquals("Count before load", 0, collection.Count);
			collection.LoadFromFile();
			AssertEquals("Count after load", 0, collection.Count);
		}

		public void TestInvalidFormatFileDoesNotThrowExceptionOnLoadFromFile()
		{
			var collection = new QuantumMawbCollection(Factory, TestUtils.InvalidFormatFileFullName);
			AssertEquals("Count before load", 0, collection.Count);
			collection.LoadFromFile();
			AssertEquals("Count after load", 0, collection.Count);
		}

		public void TestLoadThrowsException()
		{
			try
			{
				QuantumMawbCollection collection = GetTestQuantumMawbCollection();
				AssertEquals("Should not have reported any errors", 0, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
				collection.Load();
				AssertEquals("Should have reported 1 error", 1, ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Count);
				AssertEquals("Should report error with appropriate description", "Cannot Load() on a NonPersistentBusinessObjectCollection", ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance[0].InnerException.Message.Trim());
			}
			finally
			{
				ZArchitecture.Core.Testing.ExceptionReporterTestListener.Instance.Clear();
			}
		}

		[ExpectException(typeof(NotSupportedException))]
		public void TestAddNewThrowsException()
		{
			string tempSampleX2Path = TestUtils.CopyResourceToFile(TNTTestUtils.SampleX2Name);
			QuantumMawbCollection collection = new QuantumMawbCollection(Factory, tempSampleX2Path);
			collection.AddNew();
		}

		protected TNTTestUtils TestUtils;
		protected override void SetUp()
		{
			base.SetUp();
			TestUtils = new TNTTestUtils();
		}

		protected override void TearDown()
		{
			TestUtils.DeleteTempDirectoryFiles();
			base.TearDown();
		}

		protected override QuantumMawbCollection GetCollectionToTest()
		{
			return GetTestQuantumMawbCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			QuantumMawbCollection testCollection = GetCollectionToTest();
			testCollection.LoadFromFile();
			return testCollection[0];
		}

		protected QuantumMawbCollection GetTestQuantumMawbCollection()
		{
			QuantumMawbCollection result = new QuantumMawbCollection(Factory, TestUtils.TinyFileFullName);
			return result;
		}
	}
}
