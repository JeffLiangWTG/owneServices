using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.Testing
{
	class NctsSupportingDocumentValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCSI_ReferenceNumber()
		{
			const string errorMessage = "Invalid TIR Carnet Number. A TIR declaration requires a Supporting Document of type 952 (TIR Carnet) on the first goods item with the same code declared in the TIR declaration(C902)";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			nctsHeader.MovementHeader.TirCarnetNumber = "CARNET 123";
			var sd = nctsHeader.MovementHeader.GoodsItems[0].SupportingDocuments[0];
			sd.CSI_Code = NctsHeaderValidationHelper.TirCarnetDocumentCode;

			CombineAssertions(() =>
			{
				sd.CSI_ReferenceNumber = "CARNET 123";
				AssertNoMessageError("CSI_ReferenceNumber = TirCarnetNumber", sd.CSI_ReferenceNumberInfo, errorMessage);

				sd.CSI_ReferenceNumber = "REF 123";
				AssertHasMessageError("CSI_ReferenceNumber <> TirCarnetNumber", sd.CSI_ReferenceNumberInfo, errorMessage);

				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Arrival;
				sd.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("Not Departure Movement", sd.CSI_ReferenceNumberInfo, errorMessage);

				nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
				nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;
				sd.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("Not TIR Declaration", sd.CSI_ReferenceNumberInfo, errorMessage);

				nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
				sd.CSI_Code = "900";
				sd.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("SupportingDocumentType <> 952", sd.CSI_ReferenceNumberInfo, errorMessage);
			});
		}
	}
}
