using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPreviousDocument))]
	public class NctsPreviousDocumentTest : CusSupportingInfoTest<NctsPreviousDocument>
	{
		public void TestCSI_LineNo_MaxLength()
		{
			AssertEquals(5, previousDocument.CSI_LineNoInfo.MaxLength);
		}

		public void TestLookupsTypePhase4()
		{
			AssertType<NctsPreviousDocumentPhase4Lookups>(previousDocument.Lookups);
		}

		public void TestLookupsTypePhase5()
		{
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			AssertType<NctsPreviousDocumentPhase5Lookups>(previousDocument.Lookups);
		}

		public void TestValidationType()
		{
			AssertType<NctsPreviousDocumentValidation>(previousDocument.Validation);
		}

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			previousDocument = goodsItem.PreviousDocuments.AddNew();
		}
		NctsPreviousDocument previousDocument;
		NctsHeader nctsHeader;

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => previousDocument;

		protected override IEnumerable<NctsPreviousDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var previousDocument = goodsItem.PreviousDocuments.AddNew();
			previousDocument.CSI_Code = "A";
			yield return previousDocument;
		}

		protected override BusinessObject GetNewBusinessObject() => previousDocument;
	}
}
