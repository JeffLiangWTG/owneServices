using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class H7MessageSendingObjectValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckOperationCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.MovementReferenceNumber = ZString.Empty;
			var messageSendingObject = new H7MessageSendingObject(bill);

			messageSendingObject.Action = DeclarationMessageTypeList.Codes.H7Declaration;
			messageSendingObject.OperationCode = OperationCodeList.Codes.InvalidateH7WithTransit;
			messageSendingObject.Validation.ValidateOperationCode();
			AssertNoMessageErrorContaining(messageSendingObject.OperationCodeInfo, "You have not entered an Operation Code.");

			messageSendingObject.Action = DeclarationMessageTypeList.Codes.H7Declaration;
			messageSendingObject.OperationCode = ZString.Empty;
			messageSendingObject.Validation.ValidateOperationCode();
			AssertNoMessageErrorContaining(messageSendingObject.OperationCodeInfo, "You have not entered an Operation Code.");

			messageSendingObject.Action = DeclarationMessageTypeList.Codes.H7ReExport;
			messageSendingObject.OperationCode = ZString.Empty;
			messageSendingObject.Validation.ValidateOperationCode();
			AssertHasMessageErrorContaining(messageSendingObject.OperationCodeInfo, "You have not entered an Operation Code.");
		}
	}
}
