using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsDepartureMovementHeaderPhase4ValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckBM_GS_NKCusAgent()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_GS_NKCusAgentInfo);
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			ValidationTestHelper.AssertErrorFieldIsNotMandatory(departureMovement.BM_GS_NKCusAgentInfo);
		}

		public void TestCheckBM_InlandTransportMode()
		{
			var messageErrorText = "This field is not used for Spanish Customs.";

			CombineAssertions(() =>
			{
				departureMovement.Validation.ValidateBM_InlandTransportMode();
				AssertNoMessageErrorContaining(departureMovement.BM_InlandTransportModeInfo, messageErrorText);

				departureMovement.BM_InlandTransportMode = "0";
				AssertHasMessageErrorContaining(departureMovement.BM_InlandTransportModeInfo, messageErrorText);

				departureMovement.BM_InlandTransportMode = ZString.Empty;
				AssertNoMessageErrorContaining(departureMovement.BM_InlandTransportModeInfo, messageErrorText);
			});
		}

		public void TestCheckBM_PlaceOfUnloading_ListValidation()
		{
			departureMovement.BM_PlaceOfUnloading = "XYZ";
			AssertNoMessageErrors(departureMovement.BM_PlaceOfUnloadingInfo);
		}

		public void TestCheckConditionC547()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T2;
			departureMovement.Header.BH_FTZMove = true;
			departureMovement.GoodsItems.AddNew();

			CombineAssertions(() =>
			{
				departureMovement.Validation.ValidateBM_InBondEntryType();
				AssertHasNotifications("Transport document must be present", departureMovement.BM_InBondEntryTypeInfo);
			});
		}

		public void TestCheckBM_LocationOfGoodsCode()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(departureMovement.BM_LocationOfGoodsCodeInfo);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			departureMovement = header.MovementHeader;
		}
		NctsDepartureMovementHeader departureMovement;
	}
}
