using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckRevokeReason()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var messageSendingObject = new G3MessageSendingObject(bill);

			messageSendingObject.RevokeReason = string.Empty;
			messageSendingObject.Validation.ValidateRevokeReason();
			AssertHasMessageError(messageSendingObject.RevokeReasonInfo, "You have not entered a Revoke Reason.");
		}
	}
}
