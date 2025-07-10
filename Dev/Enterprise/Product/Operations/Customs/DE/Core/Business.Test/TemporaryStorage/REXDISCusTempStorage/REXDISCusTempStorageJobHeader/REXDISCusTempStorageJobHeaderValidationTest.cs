using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.DE.Business.CusTempStorage.Testing
{
	sealed class REXDISCusTempStorageJobHeaderValidationTest : CusTempStorageJobHeaderValidationAbstractTest<CusTempStorageJobHeader>
	{
		public void TestCheckSJH_DepartureDate()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			header.Validation.ValidateSJH_DepartureDate();
			AssertNoMessageErrors(header.SJH_DepartureDateInfo);
			header.SJH_TransportMode = TransportTypeList.Codes.Air;
			header.Validation.ValidateSJH_DepartureDate();
			AssertHasMessageError(header.SJH_DepartureDateInfo, MandatoryValidation.YouHaveNotEnteredMessage(header.SJH_DepartureDateInfo.HumanReadableName));
			header.SJH_DepartureDate = ZDate.Today;
			AssertNoMessageErrors(header.SJH_DepartureDateInfo);
		}

		public void TestCheckSJH_TransportMeansCode_Mandatory()
		{
			var header = GetCusTempStorageJobHeaderToTest();
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.SJH_TransportMeansCodeInfo);
		}

		protected override CusTempStorageJobHeader GetCusTempStorageJobHeaderToTest()
		{
			var header = Factory.NewWithValidTestData<CusTempStorageJobHeader>();
			header.SJH_AppCode = TemporaryStorageApplicationCodeList.Codes.REX;
			return header;
		}
	}
}
