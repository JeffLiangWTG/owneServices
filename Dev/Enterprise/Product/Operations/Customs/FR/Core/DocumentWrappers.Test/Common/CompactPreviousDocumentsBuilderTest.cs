using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.DocumentWrappers.Common.Testing;

sealed class CompactPreviousDocumentsBuilderTest : TestCaseWithFactory
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
		document2.CSI_LineNo = 1;
		previousDocuments.Add(document2);

		var builder = new CompactPreviousDocumentsBuilder();

		AssertEquals("GetPreviousDocumentsFormatted", "Z-380-7-34217890 G-02/01/2000-IT279100-1; Y-CLE-7-20070701 G-03/01/2000-IT279100-1", builder.GetPreviousDocumentsFormatted(previousDocuments));
	}
}
