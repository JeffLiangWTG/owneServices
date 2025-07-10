using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsSupportingDocumentPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheck_CSI_ReferenceNumber()
		{
			const string messageError = "Invalid TIR Carnet Number. A TIR declaration requires a Supporting Document of type 952 (TIR Carnet) on the first goods item with the same code declared in the TIR declaration(C902)";
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;
			var supportingDocument = nctsHeader.MovementHeader.SupportingDocuments.AddNew();
			supportingDocument.CSI_Code = "952";

			CombineAssertions(() =>
			{
				var targetInfo = supportingDocument.CSI_ReferenceNumberInfo;
				supportingDocument.CSI_ReferenceNumber = "12345";
				AssertHasMessageError("IsDepartureMovement; IsTIRDeclaration; IsTirCarnetDocumentCode; Different TirCarnetNumber", targetInfo, messageError);

				nctsHeader.MovementHeader.TirCarnetNumber = "12345";
				supportingDocument.Validation.ValidateCSI_ReferenceNumber();
				AssertNoMessageError("IsDepartureMovement; IsTIRDeclaration; IsTirCarnetDocumentCode; Same TirCarnetNumber", targetInfo, messageError);
			});
		}
	}
}
