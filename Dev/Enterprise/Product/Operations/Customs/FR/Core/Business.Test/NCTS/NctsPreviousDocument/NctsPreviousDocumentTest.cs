using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsPreviousDocument))]
	sealed class NctsPreviousDocumentTest : CusSupportingInfoTest<NctsPreviousDocument>
	{
		public void TestValidation()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsPreviousDocumentValidation>(previousDocument.Validation);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
			AssertType<NctsPreviousDocumentPhase5Validation>(previousDocument.Validation);
		}

		public void TestCSI_DescriptionMaxLength()
		{
			AssertEquals(26, previousDocument.CSI_DescriptionInfo.MaxLength);
		}

		public void TestReferenceNumberFieldType()
		{
			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes._337;
			AssertEquals("Reference Number field type should be equal FieldType.TextCodeFindBox.ToString() as CSI_code is 337.", nameof(ZArchitecture.FieldType.TextCodeFindBox), previousDocument.ReferenceNumberFieldType);

			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes._270;
			AssertEquals("Reference Number field type should be equal FieldType.Te.ToString() as CSI_code is not 337.", nameof(ZArchitecture.FieldType.Text), previousDocument.ReferenceNumberFieldType);
		}

		public void TestShowCodeFindBoxForReferenceNumber()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			previousDocument.CSI_Code = PreviousDocumentCodeList.Codes._337;
			AssertEquals("Phase 4: Should return true when CSI_Code is 337.", true, previousDocument.ShowCodeFindBoxForReferenceNumber);

			previousDocument.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			AssertEquals("Phase 4: Should return false when CSI_Code is N337.", false, previousDocument.ShowCodeFindBoxForReferenceNumber);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItemPhase5 = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var previousDocumentPhase5 = goodsItemPhase5.PreviousDocuments.AddNew();

			previousDocumentPhase5.CSI_Code = UniversalReferenceConstants.RefCusCodeList.PreviousDocumentsCodes.TemporayStorage;
			AssertEquals("Phase 5: Should return true when CSI_Code is N337.", true, previousDocumentPhase5.ShowCodeFindBoxForReferenceNumber);

			previousDocumentPhase5.CSI_Code = PreviousDocumentCodeList.Codes._337;
			AssertEquals("Phase 5: Should return false when CSI_Code is 337.", false, previousDocumentPhase5.ShowCodeFindBoxForReferenceNumber);
		}

		public void TestLookups() => CombineAssertions(() =>
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			AssertType<NctsPreviousDocumentPhase4Lookups>(previousDocument.Lookups);

			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var previousDocumentPhase5 = nctsHeader.Bills.AddNew().GoodsItems.AddNew().PreviousDocuments.AddNew();
			AssertType<NctsPreviousDocumentPhase5Lookups>(previousDocumentPhase5.Lookups);
		});

		protected override BusinessObject GetNewBusinessObject() => previousDocument;

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}

		protected override IEnumerable<NctsPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "A";
			yield return previousDocument;
		}

		NctsPreviousDocument previousDocument;
		NctsHeader nctsHeader;
	}
}
