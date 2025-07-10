using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing
{
	[TestedType(typeof(ReportCollection))]
	sealed class ReportCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReportCollection>
	{
		[ExpectNoExceptions]
		public void TestEmptyConstructor()
		{
			new ReportCollection();
		}

		[ExpectExceptionMessage(typeof(ApplicationException), "Any object that implements IDeliverable must be a BusinessObject")]
		public void TestAddNonBusinessObject()
		{
			var reports = new ReportCollection();
			reports.Add(new Mock<IDeliverable>().Object);
		}

		public void TestIndexerGet()
		{
			testReportCollection.Add(GetNewReport());
			AssertNotNull(testReportCollection[0]);
		}

		public void TestAddRangeReportCollection()
		{
			AssertEquals("Precondition - TestReportCollection.Count is 0", 0, testReportCollection.Count);
			testReportCollection.Add(GetNewReport());
			testReportCollection.Add(GetNewReport());
			testReportCollection.Add(GetNewReport());

			var testReportCollection2 = new ReportCollection();
			AssertEquals("Precondition - TestReportCollection2.Count is 0", 0, testReportCollection2.Count);
			testReportCollection2.AddRange(testReportCollection);
			AssertEquals(3, testReportCollection2.Count);
		}

		public void TestAddRangeArray()
		{
			AssertEquals(0, testReportCollection.Count);
			testReportCollection.AddRange(new Report[] { GetNewReport(), GetNewReport(), GetNewReport(), GetNewReport() });
			AssertEquals(4, testReportCollection.Count);
		}

		public void TestContains()
		{
			var testReport = GetNewReport();
			testReportCollection.Add(testReport);
			Assert(testReportCollection.Contains(testReport));
		}

		public void TestIndexOf()
		{
			testReportCollection.Add(GetNewReport());
			testReportCollection.Add(GetNewReport());
			var testReport = GetNewReport();
			testReportCollection.Add(testReport);
			AssertEquals(2, testReportCollection.IndexOf(testReport));
		}

		public void TestInsert()
		{
			testReportCollection.Add(GetNewReport());
			testReportCollection.Add(GetNewReport());
			var testReport = GetNewReport();
			testReportCollection.Insert(1, testReport);
			AssertEquals(1, testReportCollection.IndexOf(testReport));
		}

		public void TestCount()
		{
			AssertEquals(0, testReportCollection.Count);
			testReportCollection.Add(GetNewReport());
			testReportCollection.Add(GetNewReport());
			testReportCollection.Add(GetNewReport());
			AssertEquals(3, testReportCollection.Count);
		}

		public void TestAllowNew()
		{
			AssertEquals("ReportCollection.AllowNew should be false", false, new ReportCollection().AllowNew);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => GetNewReport();

		protected override ReportCollection GetCollectionToTest() => new ReportCollection();

		protected override void SetUp()
		{
			base.SetUp();
			pack = new DocumentPack();
			testReportCollection = new ReportCollection();
			embeddedResourceRetriever = new EmbeddedResourceRetriever();
			var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
			emptyAndValidTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
		}

		ExcelTemplateForUnitTesting emptyAndValidTemplate;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		protected override void TearDown()
		{
			base.TearDown();
			embeddedResourceRetriever?.Dispose();
		}

		Report GetNewReport() => new Report(pack, emptyAndValidTemplate);

		DocumentPack pack;
		ReportCollection testReportCollection;
	}
}
