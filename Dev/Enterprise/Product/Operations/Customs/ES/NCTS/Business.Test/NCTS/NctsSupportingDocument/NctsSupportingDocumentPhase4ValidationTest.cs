using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NctsSupportingDocumentPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			nctsHeader.MovementHeader.TirCarnetNumber = "CARNET 123";
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			var supportingDoc = goodsItem.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = NctsHeaderValidationHelper.TirCarnetDocumentCode;
			supportingDoc.CSI_ReferenceNumber = "CARNET 456";
			AssertNoMessageErrors("No message error even if CSI_ReferenceNumber != TirCarnetNumber.", supportingDoc.CSI_ReferenceNumberInfo);
		}
	}
}
