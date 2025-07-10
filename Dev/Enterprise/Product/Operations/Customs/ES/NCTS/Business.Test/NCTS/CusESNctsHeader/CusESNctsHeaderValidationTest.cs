using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class CusESNctsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCEN_NationalSimplificatorInd()
		{
			ValidationTestHelper.AssertInvalidCodeMessageError(nctsHeader.NationalSimplificatorIndInfo, "A", NationalSimplificationIndicatorList.Codes.Code1);
		}

		public void TestCheckTNNDocumentType_Mandatory()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.DeclarationSent;
				nctsHeader.TNNDocumentType = ZString.Empty;
				AssertNoMessageErrorContaining(nctsHeader.TNNDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.MovementHeader.BM_Phase = ESNctsMovementHeaderTransactionStatusList.Codes.OriginalDepartureData;
				nctsHeader.TNNDocumentType = ZString.Empty;
				AssertHasMessageErrorContaining(nctsHeader.TNNDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);

				nctsHeader.TNNDocumentType = "T";
				AssertNoMessageErrorContaining(nctsHeader.TNNDocumentTypeInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckCEN_SummaryType()
		{
			var departure = nctsHeader.ESNctsHeader;
			departure.CEN_SummaryType = ZString.Empty;
			var messageError = $"[TR0070] You have not entered a {departure.CEN_SummaryTypeInfo.HumanReadableName}.";
			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var arrival = arrivalNctsHeader.ESNctsHeader;
			arrival.CEN_SummaryType = ZString.Empty;
			CombineAssertions(() =>
			{
				AssertNoMessageError("Departure Rule TR0070 expected no message error", departure.CEN_SummaryTypeInfo, messageError);

				AssertNoMessageError("Arrival not Phase5 Rule TR0070 expected no message error", arrival.CEN_SummaryTypeInfo, messageError);

				arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				arrival.Validation.ValidateCEN_SummaryType();
				AssertHasMessageError("Arrival Phase5 Rule TR0070 expected message error", arrival.CEN_SummaryTypeInfo, messageError);

				arrival.CEN_SummaryType = "A";
				AssertNoMessageError("Arrival Phase5 Rule TR0070 expected no message error when not empty", arrival.CEN_SummaryTypeInfo, messageError);
			});
		}

		public void TestCheckCEN_AutomaticCompletion()
		{
			var departure = nctsHeader.ESNctsHeader;
			departure.CEN_AutomaticCompletion = ZBool.False;
			var messageWarning = $"[NR0049] If Authorization Nº is empty (i.e. public location), Automatic Completion should be ticked.";
			var arrivalNctsHeader = Factory.New<NctsHeader>();
			arrivalNctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var arrival = arrivalNctsHeader.ESNctsHeader;
			arrivalNctsHeader.ArrivalMovementHeader.AuthorizationNumber = ZString.Empty;
			arrival.CEN_AutomaticCompletion = ZBool.False;
			CombineAssertions(() =>
			{
				AssertNoWarningContaining("Departure Rule NR0049 expected no message warning", departure.CEN_AutomaticCompletionInfo, messageWarning);

				AssertNoWarningContaining("Arrival not Phase5 Rule NR0049 expected no message warning", arrival.CEN_AutomaticCompletionInfo, messageWarning);

				arrivalNctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
				arrival.Validation.ValidateCEN_AutomaticCompletion();
				AssertHasWarningContaining("Arrival Phase5 Rule NR0049, AuthorizationNumber empty and AutomaticCompletion false expected message warning", arrival.CEN_AutomaticCompletionInfo, messageWarning);

				arrivalNctsHeader.ArrivalMovementHeader.AuthorizationNumber = "A";
				arrival.Validation.ValidateCEN_AutomaticCompletion();
				AssertNoWarningContaining("Arrival Phase5 Rule NR0049, AuthorizationNumber not empty and AutomaticCompletion false expected no message warning", arrival.CEN_AutomaticCompletionInfo, messageWarning);

				arrivalNctsHeader.ArrivalMovementHeader.AuthorizationNumber = ZString.Empty;
				arrival.CEN_AutomaticCompletion = ZBool.True;
				AssertNoWarningContaining("Arrival Phase5 Rule NR0049, AuthorizationNumber empty and AutomaticCompletion true expected no message warning", arrival.CEN_AutomaticCompletionInfo, messageWarning);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		}
		NctsHeader nctsHeader;
	}
}
