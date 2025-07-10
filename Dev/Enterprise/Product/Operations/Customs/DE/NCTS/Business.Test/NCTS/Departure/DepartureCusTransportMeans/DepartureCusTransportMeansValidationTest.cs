using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class DepartureCusTransportMeansValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTPM_CustomsOfficeAtBorder_MatchCustomsOfficeInDeclaration()
		{
			departureCusTransportMeans.TPM_TypeOfIdentification = NctsTransportTypeOfIdList.Codes._40;
			var office1 = departureMovement.CustomsOfficesForDeparture.AddNew();
			office1.CY_Data = "CUSOFF1";
			office1.CY_Code = "DEP";
			var office2 = departureMovement.CustomsOfficesForDeparture.AddNew();
			office2.CY_Data = "CUSOFF2";
			office2.CY_Code = "TRA";
			const string message = "[TR0052] Customs Office at Border must be equal to at least one Customs Office with Purpose DES/TRA/TXT.";

			CombineAssertions(() =>
			{
				departureCusTransportMeans.TPM_CustomsOffice = "CUSOFF1";
				AssertHasMessageError("DEP", departureCusTransportMeans.TPM_CustomsOfficeInfo, message);
				departureCusTransportMeans.TPM_CustomsOffice = "CUSOFF2";
				AssertNoMessageError("TRA", departureCusTransportMeans.TPM_CustomsOfficeInfo, message);
			});
		}

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = nctsHeader.MovementHeader;
			departureCusTransportMeans = departureMovement.AdditionalTransportAtBorderList.AddNew();
		}

		DepartureCusTransportMeans departureCusTransportMeans;
		NctsHeader nctsHeader;
		NctsDepartureMovementHeader departureMovement;
	}
}
