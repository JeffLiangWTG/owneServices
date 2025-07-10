using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class ArrivalNctsHeaderValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckArrivalMrnFromUser_Mandatory()
		{
			CombineAssertions(() =>
			{
				header.Validation.ValidateArrivalMrnFromUser();
				AssertHasMessageErrorContaining("Not entered", header.ArrivalMrnFromUserInfo, MandatoryValidation.YouHaveNotEntered);

				header.ArrivalMrnFromUser = "17DE123456789123E3";
				AssertNoMessageErrorContaining("Entered", header.ArrivalMrnFromUserInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckEventCancellationReason()
		{
			header.BH_ExportFlag = EventFlagList.Codes.Cancelled;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(header.EventCancellationReasonInfo);
		}

		public void TestCheckBH_ExportFlag()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(header.BH_ExportFlagInfo, "~", EventFlagList.Codes.Yes);
		}

		public void TestValidateAll()
		{
			header.BH_ExportFlag = EventFlagList.Codes.Cancelled;
			header.Validation.ValidateAll();
			CombineAssertions(() =>
			{
				AssertHasMessageErrorContaining("Event Cancellation Reason", header.EventCancellationReasonInfo, MandatoryValidation.YouHaveNotEntered);
				AssertHasMessageErrorContaining("Arrival Mrn", header.ArrivalMrnFromUserInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
		}
		NctsHeader header;
	}
}
