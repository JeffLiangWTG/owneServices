using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	sealed class ProducedDocumentsCertificatesBuilderTest : TestCaseWithFactory
	{
		public void TestAppendSupportingDocument()
		{
			var document = Factory.New<SupportingDocument>();
			document.CSI_Code = "9100";
			document.CSI_ReferenceNumber = "3278923";
			document.CSI_SubType = "1";
			document.CSI_Quantity = 12;
			document.CSI_Description = "SIC TRANSIT GLORIA MUNDI";
			document.CSI_DateOfIssue = new ZDate(2021, 12, 10);

			var result = new ZStringBuilder();
			var builder = new ProducedDocumentsCertificatesBuilderForTest();
			builder.AppendSupportingDocument_Exposed(result, document);

			AssertEquals("TestAppendSupportingDocument", "9100 3278923 P=1 Q=12 \"SIC TRANSIT GLORIA MUNDI\"", result.ToString());
		}

		public void TestAppendSupportingDocumentWithNullBuilder()
		{
			var builder = new ProducedDocumentsCertificatesBuilderForTest();
			var document = Factory.NewWithValidTestData<SupportingDocument>();

			AssertExceptionThrown<ArgumentNullException>("Null builder", () => builder.AppendSupportingDocument_Exposed(null, document));
		}

		public void TestAppendSupportingDocumentWithNullDocument()
		{
			var builder = new ProducedDocumentsCertificatesBuilderForTest();
			var result = new ZStringBuilder();

			AssertExceptionThrown<ArgumentNullException>("Null document", () => builder.AppendSupportingDocument_Exposed(result, null));
		}

		public void TestAppendSupportingDocumentInfo()
		{
			List<SupportingDocument> supportingDocuments = new List<SupportingDocument>();

			var document1 = Factory.New<SupportingDocument>();
			document1.CSI_Code = "9100";
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";
			document1.CSI_DateOfIssue = new ZDate(2021, 12, 10);
			supportingDocuments.Add(document1);

			var document2 = Factory.New<SupportingDocument>();
			document2.CSI_Code = "9120";
			document2.CSI_ReferenceNumber = "45982309";
			document2.CSI_Description = "9120 Test";
			supportingDocuments.Add(document2);

			var builder = new ProducedDocumentsCertificatesBuilder();
			ZStringBuilder result = new ZStringBuilder();
			builder.AppendSupportingDocumentInfo(result, supportingDocuments);

			AssertEquals("AppendSupportingDocumentInfo", "9100 3278923 P=1 Q=12 \"I HAVE NO CLUE\"9120 45982309 \"9120 Test\"", result.ToString());
		}

		public void TestAppendSupportingDocumentInfoWithNullBuilder()
		{
			var builder = new ProducedDocumentsCertificatesBuilder();
			var document1 = Factory.NewWithValidTestData<SupportingDocument>();
			List<SupportingDocument> supportingDocuments = new List<SupportingDocument>();
			supportingDocuments.Add(document1);

			AssertExceptionThrown<ArgumentNullException>("Null builder", () => builder.AppendSupportingDocumentInfo(null, supportingDocuments));
		}

		public void TestAppendSupportingDocumentInfoWithNullDocumentList()
		{
			var builder = new ProducedDocumentsCertificatesBuilder();
			var result = new ZStringBuilder();

			AssertExceptionThrown<ArgumentNullException>("Null document list", () => builder.AppendSupportingDocumentInfo(result, null));
		}

		public void TestGetProducedDocumentsCertificatesFormatted()
		{
			List<SupportingDocument> supportingDocuments = new List<SupportingDocument>();

			var document1 = Factory.New<SupportingDocument>();
			document1.CSI_Code = "9100";
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";
			document1.CSI_DateOfIssue = new ZDate(2021, 12, 10);
			supportingDocuments.Add(document1);

			var document2 = Factory.New<SupportingDocument>();
			document2.CSI_Code = "9120";
			document2.CSI_ReferenceNumber = "45982309";
			document2.CSI_Description = "9120 Test";
			supportingDocuments.Add(document2);

			var builder = new ProducedDocumentsCertificatesBuilder();

			AssertEquals("GetProducedDocumentsCertificatesFormatted", "9100 3278923 P=1 Q=12 \"I HAVE NO CLUE\", 9120 45982309 \"9120 Test\"", builder.GetProducedDocumentsCertificatesFormatted(supportingDocuments));
		}

		public void TestGetProducedDocumentsCertificatesFormattedWithNullDocumentList()
		{
			var builder = new ProducedDocumentsCertificatesBuilder();

			AssertExceptionThrown<ArgumentNullException>("Documents formatted with Null document list", () => builder.GetProducedDocumentsCertificatesFormatted(null));
		}

		class ProducedDocumentsCertificatesBuilderForTest : ProducedDocumentsCertificatesBuilder
		{
			public void AppendSupportingDocument_Exposed(ZStringBuilder result, SupportingDocument supportingDocument) => base.AppendSupportingDocument(result, supportingDocument);
		}
	}
}
