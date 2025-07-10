using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common.Testing
{
	sealed class DocumentWrapperTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDocument = declaration.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "tst1";
			supportingDocument.CSI_Description = "tst desc 1";
			supportingDocument.CSI_SubType = "1";
			supportingDocument.CSI_ReferenceNumber = "Reference 1";
			supportingDocument.CSI_DateOfIssue = ZDateTime.BrettsBirthday;

			var wrapper = new DocumentWrapper(supportingDocument);
			AssertEquals("tst1", wrapper.Code);
			AssertEquals("tst desc 1", wrapper.Description);
			AssertEquals("1", wrapper.Type);
			AssertEquals("Reference 1", wrapper.RefNumber);
			AssertEquals(ZDateTime.BrettsBirthday, wrapper.DateIssue);
			AssertEquals(ZString.Empty, wrapper.PFAIdentification);
			AssertEquals(ZString.Empty, wrapper.PFADocument);

			var previousDocument = declaration.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "tst2";
			previousDocument.CSI_Description = "tst desc 2";
			previousDocument.CSI_SubType = "2";
			previousDocument.CSI_ReferenceNumber = "Reference 2";
			previousDocument.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(1);

			wrapper = new DocumentWrapper(previousDocument);
			AssertEquals("tst2", wrapper.Code);
			AssertEquals("tst desc 2", wrapper.Description);
			AssertEquals("2", wrapper.Type);
			AssertEquals("Reference 2", wrapper.RefNumber);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(1), wrapper.DateIssue);
			AssertEquals(ZString.Empty, wrapper.PFAIdentification);
			AssertEquals(ZString.Empty, wrapper.PFADocument);

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			var goodItem = nctsHeader.UnloadingMovementHeader.GoodsItems.AddNew();
			var nctsSupportingDocument = goodItem.SupportingDocuments.AddNew();
			nctsSupportingDocument.CSI_Code = "tst3";
			nctsSupportingDocument.CSI_Description = "tst desc 3";
			nctsSupportingDocument.CSI_SubType = "3";
			nctsSupportingDocument.CSI_ReferenceNumber = "Reference 3";
			nctsSupportingDocument.CSI_DateOfIssue = ZDateTime.BrettsBirthday.AddDays(2);

			wrapper = new DocumentWrapper(nctsSupportingDocument);
			AssertEquals("tst3", wrapper.Code);
			AssertEquals("tst desc 3", wrapper.Description);
			AssertEquals("3", wrapper.Type);
			AssertEquals("Reference 3", wrapper.RefNumber);
			AssertEquals(ZDateTime.BrettsBirthday.AddDays(2), wrapper.DateIssue);
			AssertEquals(ZString.Empty, wrapper.PFAIdentification);
			AssertEquals(ZString.Empty, wrapper.PFADocument);
		}
	}
}
