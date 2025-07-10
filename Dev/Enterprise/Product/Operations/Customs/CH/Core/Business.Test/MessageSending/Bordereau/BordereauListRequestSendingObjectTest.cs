using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BordereauListRequestSendingObject))]
sealed class BordereauListRequestSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestConfiguration() => CombineAssertions(() =>
	{
		AssertEquals("ApplicationCode", ApplicationCodeList.Codes.CHCustomsEdec, SendingObject.ApplicationCode);
		AssertEquals("MessageTypeForEDIMessage", MessageTypeCodeList.Codes.BOR, SendingObject.MessageTypeForEDIMessage);
		AssertEquals("MessageSubTypeForEDIMessage", MessageSubTypeCodeList.Codes.BordereauList, SendingObject.MessageSubTypeForEDIMessage);
		AssertEquals("GetApplicationReference", ZString.Empty, SendingObject.GetApplicationReference());
	});

	public void TestGetCredentialPK()
	{
		var credential = CredentialsTestHelper.CreateCurrentCompanyCertificateCredential();
		AssertEquals(credential.PK, SendingObject.GetCredentialPK());
	}

	public void TestToMessageString() => CombineAssertions(() =>
	{
		SendingObject.StartDate = new ZDate("2024, 2, 29");
		SendingObject.EndDate = new ZDate("2025, 8, 31");

		var xml = XDocument.Parse(SendingObject.ToMessageString());
		var nsm = new XmlNamespaceManager(new NameTable());
		nsm.AddNamespace("b", xml.Root.Name.NamespaceName);
		AssertEquals("startDate", "2024-02-29", xml.XPathSelectElement("/b:bordereauRequest/b:bordereauList/b:dateRange/b:startDate", nsm)?.Value);
		AssertEquals("endDate", "2025-08-31", xml.XPathSelectElement("/b:bordereauRequest/b:bordereauList/b:dateRange/b:endDate", nsm)?.Value);
	});

	BordereauListRequestSendingObject SendingObject => sendingObject ??= new BordereauListRequestSendingObject();
	BordereauListRequestSendingObject sendingObject;
}
