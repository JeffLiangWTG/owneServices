using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(NctsSupportingDocument))]
	class NctsSupportingDocumentTest : CusSupportingInfoTest<NctsSupportingDocument>
	{
		public void TestValidation()
		{
			AssertType<NctsSupportingDocumentValidation>(supportingDocument.Validation);
		}

		protected override IEnumerable<NctsSupportingDocument> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var nctsHeader = factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			yield return goodItem.SupportingDocuments.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject() => supportingDocument;

		protected override void SetUp()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			supportingDocument = goodItem.SupportingDocuments.AddNew();
		}
		NctsSupportingDocument supportingDocument;
	}
}
