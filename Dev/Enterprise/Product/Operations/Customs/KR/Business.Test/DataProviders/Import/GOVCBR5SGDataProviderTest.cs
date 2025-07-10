using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	sealed class GOVCBR5SGDataProvidersTest : XMLMessageTestHelper<GOVCBR5SGDataProvidersTest>
	{
		public void TestHeader()
		{
			var finalPriceReport = new FinalPriceReportByDateExtensionHeader(Factory);
			finalPriceReport.CustomsOffice = "030";
			finalPriceReport.GB_Branch = GlbBranch.CurrentBranch.PK;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(finalPriceReport.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "5SG41");

			var result = new Import5SGHeaderCreator().Create(finalPriceReport);
			AssertEquals("030", result.DeclarationCustomsOffice);
			AssertEquals("5SG41", result.UnipassDeclarantID);
		}

		public void TestEntry()
		{
			var finalPriceReport = new FinalPriceReportByDateExtensionHeader(Factory);
			var finalPriceReportLine1 = finalPriceReport.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportLine1.ImportDeclarationNumber = "4163720000123M";
			finalPriceReportLine1.ExtensionDate = new ZDate(2021, 10, 01);
			finalPriceReportLine1.ApplicationReason = "Test1";

			var finalPriceReportLine2 = finalPriceReport.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportLine2.ImportDeclarationNumber = "4163719112456M";
			finalPriceReportLine2.ExtensionDate = new ZDate(2021, 10, 02);
			finalPriceReportLine2.ApplicationReason = "Test2";

			var finalPriceReportLine3 = finalPriceReport.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportLine3.ImportDeclarationNumber = "4163719112789M";
			finalPriceReportLine3.ExtensionDate = ZDate.Empty;
			finalPriceReportLine3.ApplicationReason = "Test3";

			var result = new Import5SGHeaderCreator().Create(finalPriceReport);
			AssertEquals(3, result.Declarations.Length);

			AssertEquals("4163720000123M", result.Declarations[0].ImportDeclarationNumber);
			AssertEquals(new ZDate(2021, 10, 01), result.Declarations[0].ExtensionDate);
			AssertEquals("Test1", result.Declarations[0].ApplicationReason);

			AssertEquals("4163719112456M", result.Declarations[1].ImportDeclarationNumber);
			AssertEquals(new ZDate(2021, 10, 02), result.Declarations[1].ExtensionDate);
			AssertEquals("Test2", result.Declarations[1].ApplicationReason);

			AssertEquals("4163719112789M", result.Declarations[2].ImportDeclarationNumber);
			AssertEquals(ZDate.Empty, result.Declarations[2].ExtensionDate);
			AssertEquals("Test3", result.Declarations[2].ApplicationReason);
		}

		[TestDate(2021, 09, 01)]
		public void TestXML()
		{
			var finalPriceReport = new FinalPriceReportByDateExtensionHeader(Factory);
			finalPriceReport.CustomsOffice = "030";
			finalPriceReport.GB_Branch = GlbBranch.CurrentBranch.PK;

			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(finalPriceReport.Branch.GB_GC.ToGuid(), Guid.Empty, Guid.Empty, "5SG41");

			var finalPriceReportLine1 = finalPriceReport.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportLine1.ImportDeclarationNumber = "4163720000123M";
			finalPriceReportLine1.ExtensionDate = new ZDate(2021, 10, 01);
			finalPriceReportLine1.ApplicationReason = "Test1";

			var finalPriceReportLine2 = finalPriceReport.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportLine2.ImportDeclarationNumber = "4163719112456M";
			finalPriceReportLine2.ExtensionDate = new ZDate(2021, 10, 02);
			finalPriceReportLine2.ApplicationReason = "Test2";

			var finalPriceReportLine3 = finalPriceReport.FinalPriceReportByDateExtensionLines.AddNew();
			finalPriceReportLine3.ImportDeclarationNumber = "4163719112789M";
			finalPriceReportLine3.ExtensionDate = new ZDate(2021, 10, 03);
			finalPriceReportLine3.ApplicationReason = "Test3";

			var import5SG = new Import5SGHeaderCreator().Create(finalPriceReport);
			var result = new GOVCBR5SGMessageBuilder(import5SG).GenerateMessage();
			var fileReader = new TestFileReader(typeof(GOVCBR5SGDataProvidersTest));
			var testFile = fileReader.GetEmbeddedFileText(TestFilesPath, "GOVCBR5SG_D1.xml");
			using (var makeStream = KRXmlObjectSerializer.Serialize(result))
			{
				var readerSource = new TextReaderSource(makeStream);
				var serialisedXml = readerSource.GetReader().ReadToEnd();

				AssertXMLEquals(testFile, serialisedXml);
			}
		}

		public override string TestFilesPath => "Enterprise.Customs.KR.Business.Testing.TestFiles.Import.Outgoing";
	}
}
