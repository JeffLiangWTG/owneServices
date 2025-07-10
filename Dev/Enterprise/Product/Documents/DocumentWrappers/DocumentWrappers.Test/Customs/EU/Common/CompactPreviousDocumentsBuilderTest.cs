using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.DocumentWrappers.Customs.EU.Testing
{
	[Enterprise.MasterFiles.Business.Testing.CountrySpecificTest(Core.Constants.CountryCodes.Latvia)]
	sealed class CompactPreviousDocumentsBuilderTest : TestCaseWithFactory
	{
		public void TestGetPreviousDocumentsFormatted_Phase4()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.Bills.AddNew().GoodsItems.AddNew();

			var documentOnFirstLine = item.PreviousDocuments.AddNew();
			documentOnFirstLine.CSI_Procedure = "7";
			documentOnFirstLine.CSI_SubType = Enterprise.Customs.EU.Business.PreviousDocumentClassList.Codes.PreviousDocument;
			documentOnFirstLine.CSI_Code = "380";
			documentOnFirstLine.CSI_ReferenceNumber = "34217890";
			documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
			documentOnFirstLine.CSI_Status = "G";
			documentOnFirstLine.CSI_CustomsOffice = "IT279100";
			documentOnFirstLine.CSI_LineNo = 1;
			documentOnFirstLine.CSI_Description = "ITDESC";

			var documentOnSecondLine = item.PreviousDocuments.AddNew();
			documentOnSecondLine.CSI_Procedure = "7";
			documentOnSecondLine.CSI_SubType = "Y";
			documentOnSecondLine.CSI_Code = "CLE";
			documentOnSecondLine.CSI_ReferenceNumber2 = "20070701";
			documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
			documentOnSecondLine.CSI_Status = "G";
			documentOnSecondLine.CSI_CustomsOffice = "IT279100";
			documentOnSecondLine.CSI_LineNo = 1;

			var builder = new CompactPreviousDocumentsBuilder();

			AssertEquals("GetPreviousDocumentsFormatted", "380 - 34217890 - ITDESC; CLE", builder.GetPreviousDocumentsFormatted(item.PreviousDocuments.Cast<NctsPreviousDocument>(), false));
		}

		public void TestGetPreviousDocumentsFormatted_Phase5()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.Bills.AddNew().GoodsItems.AddNew();

			var documentOnFirstLine = item.PreviousDocuments.AddNew();
			documentOnFirstLine.CSI_Procedure = "7";
			documentOnFirstLine.CSI_SubType = ZString.Empty;
			documentOnFirstLine.CSI_Code = "380";
			documentOnFirstLine.CSI_ReferenceNumber = "34217890";
			documentOnFirstLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 2);
			documentOnFirstLine.CSI_Status = "G";
			documentOnFirstLine.CSI_CustomsOffice = "IT279100";
			documentOnFirstLine.CSI_LineNo = 1;

			var documentOnSecondLine = item.PreviousDocuments.AddNew();
			documentOnSecondLine.CSI_Procedure = "7";
			documentOnSecondLine.CSI_SubType = "Y";
			documentOnSecondLine.CSI_Code = "CLE";
			documentOnSecondLine.CSI_ReferenceNumber2 = "20070701";
			documentOnSecondLine.CSI_DateOfIssue = new ZDateTime(2000, 1, 3);
			documentOnSecondLine.CSI_Status = "G";
			documentOnSecondLine.CSI_CustomsOffice = "IT279100";
			documentOnSecondLine.CSI_LineNo = 1;

			var builder = new CompactPreviousDocumentsBuilder();

			AssertEquals("GetPreviousDocumentsFormatted", "380-34217890; CLE", builder.GetPreviousDocumentsFormatted(item.PreviousDocuments.Cast<NctsPreviousDocument>(), true));
		}

		public void TestGetPreviousDocumentsFormattedWhenListIsEmpty()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var item = header.Bills.AddNew().GoodsItems.AddNew();

			var builder = new CompactPreviousDocumentsBuilder();
			AssertEquals("GetPreviousDocumentsFormatted on empty list", ZString.Empty, builder.GetPreviousDocumentsFormatted(item.PreviousDocuments.Cast<NctsPreviousDocument>()));
		}

		public void TestGetPreviousDocumentsFormattedWhenListIsNull()
		{
			var builder = new CompactPreviousDocumentsBuilder();
			AssertEquals("GetPreviousDocumentsFormatted on null list", ZString.Empty, builder.GetPreviousDocumentsFormatted(null));
		}
	}
}
