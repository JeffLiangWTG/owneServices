using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BordereauRequestSendingObject))]
sealed class BordereauRequestSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestProperties() => CombineAssertions(() =>
	{
		var credential = CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();
		var sendingObject = new BordereauRequestSendingObject();
		AssertEquals("ApplicationCode", ApplicationCodeList.Codes.CHCustomsEdec, sendingObject.ApplicationCode);
		AssertEquals("MessageTypeForEDIMessage", MessageTypeCodeList.Codes.BOR, sendingObject.MessageTypeForEDIMessage);
		AssertEquals("MessageSubTypeForEDIMessage", MessageSubTypeCodeList.Codes.BordereauResponse, sendingObject.MessageSubTypeForEDIMessage);
		AssertEquals("GetApplicationReference", ZString.Empty, sendingObject.GetApplicationReference());
		AssertEquals("GetCredentialPK", credential.PK, sendingObject.GetCredentialPK());
	});
}
