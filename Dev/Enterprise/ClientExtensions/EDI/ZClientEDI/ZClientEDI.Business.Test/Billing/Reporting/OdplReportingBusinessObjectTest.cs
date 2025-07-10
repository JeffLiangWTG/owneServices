using System.IO;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(OdplReportingBusinessObject))]
	sealed class OdplReportingBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetUsage()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var reportingBizO = new OdplReportingBusinessObjectForTest(Factory, new ZDateTime(2010, 08, 1), organisation.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			var reportAsBlob = reportingBizO.GetPdfUsageReport();
			Assert("Report should not be empty", !reportAsBlob.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDocTemplate()
		{
			BillingTestHelper.LoadClientSpecificDocuments();
			var reportingBizO = new OdplReportingBusinessObjectForTest(Factory, new ZDateTime(2010, 08, 1), organisation.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

			var expectedCSV = @" XXXSYD - XXXSYD Company Aug 2010 
 Server: Company: 
 Dates and times are in Universal Coordinated Time (UTC) 
 1.TopLevelDescription 
 1.Header1 
 1.Col1 
 1.Col1 
 2.TopLevelDescription 
 2.Header1 2.Header2 
 2.Col1 2.Col2 
 2.Col1 2.Col2 
 3.TopLevelDescription 
 3.Header1 3.Header2 3.Header3 
 3.Col1 3.Col2 3.Col3 
 3.Col1 3.Col2 3.Col3 
 4.TopLevelDescription 
 4.Header1 4.Header2 4.Header3 4.Header4 
 4.Col1 4.Col2 4.Col3 4.Col4 
 4.Col1 4.Col2 4.Col3 4.Col4 
 5.TopLevelDescription 
 5.Header1 5.Header2 5.Header3 5.Header4 5.Header5 
 5.Col1 5.Col2 5.Col3 5.Col4 5.Col5 
 5.Col1 5.Col2 5.Col3 5.Col4 5.Col5 
 6.TopLevelDescription 
 6.Header1 6.Header2 6.Header3 6.Header4 6.Header5 6.Header6 
 6.Col1 6.Col2 6.Col3 6.Col4 6.Col5 6.Col6 
 6.Col1 6.Col2 6.Col3 6.Col4 6.Col5 6.Col6 
 7.TopLevelDescription 
 7.Header1 7.Header2 7.Header3 7.Header4 7.Header5 7.Header6 7.Header7 
 7.Col1 7.Col2 7.Col3 7.Col4 7.Col5 7.Col6 7.Col7 
 7.Col1 7.Col2 7.Col3 7.Col4 7.Col5 7.Col6 7.Col7 
 8.TopLevelDescription 
 8.Header1 8.Header2 8.Header3 8.Header4 8.Header5 8.Header6 8.Header7 8.Header8 
 8.Col1 8.Col2 8.Col3 8.Col4 8.Col5 8.Col6 8.Col7 8.Col8 
 8.Col1 8.Col2 8.Col3 8.Col4 8.Col5 8.Col6 8.Col7 8.Col8 
 9.TopLevelDescription 
 9.Header1 9.Header2 9.Header3 9.Header4 9.Header5 9.Header6 9.Header7 9.Header8 9.Header9 
 9.Col1 9.Col2 9.Col3 9.Col4 9.Col5 9.Col6 9.Col7 9.Col8 9.Col9 
 9.Col1 9.Col2 9.Col3 9.Col4 9.Col5 9.Col6 9.Col7 9.Col8 9.Col9 
 10.TopLevelDescription 
 10.Header1 10.Header2 10.Header3 10.Header4 10.Header5 10.Header6 10.Header7 10.Header8 10.Header9 10.Header10 
 10.Col1 10.Col2 10.Col3 10.Col4 10.Col5 10.Col6 10.Col7 10.Col8 10.Col9 10.Col10 
 10.Col1 10.Col2 10.Col3 10.Col4 10.Col5 10.Col6 10.Col7 10.Col8 10.Col9 10.Col10 
";
			AssertEquals(expectedCSV, reportingBizO.GetUsageReportForDocTemplateTest());
		}

		EDIOrgHeader organisation;

		protected override void SetUp()
		{
			base.SetUp();
			organisation = BillingTestHelper.CreateOrganisation(Factory, "XXX");
			Factory.Save();
		}

		protected override BusinessObject GetNewBusinessObject() => new OdplReportingBusinessObject(Factory, new ZDateTime(2010, 08, 1), "", organisation.PK, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

		sealed class OdplReportingBusinessObjectForTest : OdplReportingBusinessObject
		{
			public OdplReportingBusinessObjectForTest(BusinessObjectFactory factory, ZDateTime periodStart, ZGuid organisationPk, ZGuid clientCompanyPk, ZGuid licenceCompanyPk, ZGuid databasePk)
				: base(factory, periodStart, "", organisationPk, clientCompanyPk, licenceCompanyPk, databasePk)
			{
			}

			protected override BillingSystem BillingSystem
			{
				get
				{
					return new DummyBillingSystem();
				}
			}

			public string GetUsageReportForDocTemplateTest()
			{
				var rawUsage = BillingSystem.LoadOdplRawUsage(Context);
				var docWrapper = DocSystemRawUsage.New(rawUsage, Factory);

				SetSummaryLines(docWrapper.UsageLines1, 1);
				SetSummaryLines(docWrapper.UsageLines2, 2);
				SetSummaryLines(docWrapper.UsageLines3, 3);
				SetSummaryLines(docWrapper.UsageLines4, 4);
				SetSummaryLines(docWrapper.UsageLines5, 5);
				SetSummaryLines(docWrapper.UsageLines6, 6);
				SetSummaryLines(docWrapper.UsageLines7, 7);
				SetSummaryLines(docWrapper.UsageLines8, 8);
				SetSummaryLines(docWrapper.UsageLines9, 9);
				SetSummaryLines(docWrapper.UsageLines10, 10);

				(var rawDocument, _) = BillingInvoicingHelper.GetRawDocumentInExcel(UsageDocTemplate, docWrapper);
				var excelBytes = new ZBlob(rawDocument);
				using (ExcelInterface excelInterface = new ExcelInterface())
				{
					excelInterface.LoadExcelFile(excelBytes);
					using (var tempDir = new TempDirectory())
					{
						var csvFile = Path.Combine(tempDir.DirectoryName, "doc.csv");
						excelInterface.SaveToFile(csvFile);
						var csvContent = File.ReadAllText(csvFile);
						csvContent = csvContent.Replace("\t", " ");
						csvContent = Regex.Replace(csvContent, "[ ]{2,}", " ", RegexOptions.None);
						csvContent = Regex.Replace(csvContent, @"^\s+$[\r\n]*", "", RegexOptions.Multiline);
						csvContent = Regex.Replace(csvContent, @"\r\n|\n\r|\n|\r", "\r\n");
						return csvContent;
					}
				}
			}

			void SetSummaryLines(SummaryLineCollection collection, int colCount)
			{
				collection.RemoveAll();

				var header = new SummaryLine(Factory);
				header.TopLevelDescription = $"{colCount}.TopLevelDescription";
				header.Column9 = $"{colCount}.Header9";

				if (colCount >= 1)
				{ header.Column1 = $"{colCount}.Header1"; }
				if (colCount >= 2)
				{ header.Column2 = $"{colCount}.Header2"; }
				if (colCount >= 3)
				{ header.Column3 = $"{colCount}.Header3"; }
				if (colCount >= 4)
				{ header.Column4 = $"{colCount}.Header4"; }
				if (colCount >= 5)
				{ header.Column5 = $"{colCount}.Header5"; }
				if (colCount >= 6)
				{ header.Column6 = $"{colCount}.Header6"; }
				if (colCount >= 7)
				{ header.Column7 = $"{colCount}.Header7"; }
				if (colCount >= 8)
				{ header.Column8 = $"{colCount}.Header8"; }
				if (colCount >= 9)
				{ header.Column9 = $"{colCount}.Header9"; }
				if (colCount >= 10)
				{ header.Column10 = $"{colCount}.Header10"; }

				for (var idx = 0; idx < 2; idx++)
				{
					var line = collection.AddNew();
					line.Header = header;
					line.Column9 = $"{colCount}.Col9";

					if (colCount >= 1)
					{ line.Column1 = $"{colCount}.Col1"; }
					if (colCount >= 2)
					{ line.Column2 = $"{colCount}.Col2"; }
					if (colCount >= 3)
					{ line.Column3 = $"{colCount}.Col3"; }
					if (colCount >= 4)
					{ line.Column4 = $"{colCount}.Col4"; }
					if (colCount >= 5)
					{ line.Column5 = $"{colCount}.Col5"; }
					if (colCount >= 6)
					{ line.Column6 = $"{colCount}.Col6"; }
					if (colCount >= 7)
					{ line.Column7 = $"{colCount}.Col7"; }
					if (colCount >= 8)
					{ line.Column8 = $"{colCount}.Col8"; }
					if (colCount >= 8)
					{ line.Column9 = $"{colCount}.Col9"; }
					if (colCount >= 10)
					{ line.Column10 = $"{colCount}.Col10"; }
				}
			}
		}
	}
}
