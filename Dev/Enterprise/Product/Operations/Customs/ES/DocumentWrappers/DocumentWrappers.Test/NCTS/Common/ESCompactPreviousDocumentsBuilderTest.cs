using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;

namespace Enterprise.Customs.ES.DocumentWrappers.NCTS.Testing
{
	sealed class ESCompactPreviousDocumentsBuilderTest : TestCaseWithFactory
	{
		public void TestGetCountrySpecificFields()
		{
			var previousDocuments = new List<PreviousDocument>();

			var document1 = Factory.New<PreviousDocument>();
			document1.CSI_Procedure = "7";
			document1.CSI_SubType = "Z";
			document1.CSI_Code = "380";
			document1.CSI_ReferenceNumber = "34217890";
			document1.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
			document1.CSI_Status = "G";
			document1.CSI_CustomsOffice = "IT279100";
			document1.CSI_LineNo = 1;
			previousDocuments.Add(document1);

			var document2 = Factory.New<PreviousDocument>();
			document2.CSI_Procedure = "7";
			document2.CSI_SubType = "Y";
			document2.CSI_Code = "CLE";
			document2.CSI_ReferenceNumber = "20070701";
			document2.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
			document2.CSI_Status = "G";
			document2.CSI_CustomsOffice = "IT279100";
			document2.CSI_LineNo = 2;
			previousDocuments.Add(document2);

			var builder = new ESCompactPreviousDocumentsBuilder();

			AssertEquals("GetPreviousDocumentsFormatted", "Z 380 34217890; Y CLE 20070701", builder.GetPreviousDocumentsFormatted(previousDocuments));
		}

		public void TestGetCountrySpecificFields_PreviousDocumentIsSummaryDeclaration()
		{
			var previousDocuments = new List<PreviousDocument>();

			var document1 = Factory.New<PreviousDocument>();
			document1.CSI_Procedure = "7";
			document1.CSI_SubType = "X";
			document1.CSI_Code = "380";
			document1.CSI_ReferenceNumber = "34217890";
			document1.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
			document1.CSI_Status = "G";
			document1.CSI_CustomsOffice = "IT279100";
			document1.CSI_LineNo = 1;
			previousDocuments.Add(document1);

			var document2 = Factory.New<PreviousDocument>();
			document2.CSI_Procedure = "7";
			document2.CSI_SubType = "Y";
			document2.CSI_ReferenceNumber = "20070701";
			document2.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
			document2.CSI_Status = "G";
			document2.CSI_CustomsOffice = "IT279100";
			previousDocuments.Add(document2);

			var builder = new ESCompactPreviousDocumentsBuilder();

			AssertEquals("GetPreviousDocumentsFormatted", "X 380 34217890 1; Y 20070701", builder.GetPreviousDocumentsFormatted(previousDocuments));
		}
	}
}
