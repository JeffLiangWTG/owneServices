using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	class ProducedDocumentsCertificatesBuilderTest : TestCaseWithFactory
	{
		public void TestAppendSupportingDocumentInfo()
		{
			var supportingDocuments = new List<SupportingDocument>();

			var document1 = Factory.New<SupportingDocument>();
			document1.CSI_Code = "9100";
			document1.CSI_ReferenceNumber = "3278923";
			document1.CSI_SubType = "1";
			document1.CSI_Quantity = 12;
			document1.CSI_Description = "I HAVE NO CLUE";
			document1.CSI_DateOfIssue = new ZDate(2021, 12, 10);
			document1.CSI_Availability = "A";
			document1.CSI_Actions = "B";
			supportingDocuments.Add(document1);

			var document2 = Factory.New<SupportingDocument>();
			document2.CSI_Code = "9120";
			document2.CSI_ReferenceNumber = "45982309";
			document2.CSI_Description = "9120 Test";
			document2.CSI_Availability = "C";
			document2.CSI_Actions = "D";
			supportingDocuments.Add(document2);

			var builder = new ProducedDocumentsCertificatesBuilder();
			var result = new ZStringBuilder();
			builder.AppendSupportingDocumentInfo(result, supportingDocuments);

			AssertEquals("AppendSupportingDocumentInfo", "9100-[AB] 3278923 P=1 Q=12 \"I HAVE NO CLUE\"9120-[CD] 45982309 \"9120 Test\"", result.ToString());
		}
	}
}
