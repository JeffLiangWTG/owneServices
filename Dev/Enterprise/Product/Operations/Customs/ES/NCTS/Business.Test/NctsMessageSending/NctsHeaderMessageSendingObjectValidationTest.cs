using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderMessageSendingObjectValidation))]
sealed class NctsHeaderMessageSendingObjectValidationTest : TestCaseWithFactory
{
	public void TestValidateReasonForCancellation() => CombineAssertions(() =>
	{
		var message = "Please enter a cancellation reason.";

		var sendingObj = new NctsHeaderMessageSendingObject(header, SendingType.None) { ReasonForCancellation = string.Empty };
		AssertNoErrorContaining(sendingObj.ReasonForCancellationInfo, message);

		header.MovementHeader.BM_CustomsStatus = "DGP";
		sendingObj = new NctsHeaderMessageSendingObject(header, SendingType.None) { ReasonForCancellation = string.Empty };
		AssertHasErrorContaining(sendingObj.ReasonForCancellationInfo, message);

		sendingObj.ReasonForCancellation = "Some reason";
		AssertNoErrorContaining(sendingObj.ReasonForCancellationInfo, message);
	});

	protected override void SetUp()
	{
		base.SetUp();
		header = Factory.NewWithValidTestData<NctsHeader>();
		header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Departure;
		header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		Factory.Save();
	}
	NctsHeader header;
}
