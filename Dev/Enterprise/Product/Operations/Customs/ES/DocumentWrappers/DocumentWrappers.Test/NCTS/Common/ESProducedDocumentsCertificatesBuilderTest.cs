using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS.Testing
{
	sealed class ESProducedDocumentsCertificatesBuilderTest : TestCaseWithFactory
	{
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

			var builder = new ESProducedDocumentsCertificatesBuilder();

			AssertEquals("GetProducedDocumentsCertificatesFormatted", "9100: 3278923; 9120: 45982309", builder.GetProducedDocumentsCertificatesFormatted(supportingDocuments));
		}

		public void TestGetProducedDocumentsCertificatesFormattedWithEmptyFields()
		{
			List<SupportingDocument> supportingDocuments = new List<SupportingDocument>();

			var document1 = Factory.New<SupportingDocument>();
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";
			document1.CSI_DateOfIssue = new ZDate(2021, 12, 10);
			supportingDocuments.Add(document1);

			var document2 = Factory.New<SupportingDocument>();
			document2.CSI_Code = "9120";
			document2.CSI_Description = "9120 Test";
			supportingDocuments.Add(document2);

			var document3 = Factory.New<SupportingDocument>();
			document3.CSI_Code = "9130";
			document3.CSI_ReferenceNumber = "reference";
			document3.CSI_Description = "9120 Test";
			supportingDocuments.Add(document3);

			var builder = new ESProducedDocumentsCertificatesBuilder();

			AssertEquals("GetProducedDocumentsCertificatesFormatted", ": 3278923; 9120:; 9130: reference", builder.GetProducedDocumentsCertificatesFormatted(supportingDocuments));
		}
	}
}
