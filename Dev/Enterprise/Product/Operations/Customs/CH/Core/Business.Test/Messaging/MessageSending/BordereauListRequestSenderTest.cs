using System;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(BordereauListRequestSender))]
sealed class BordereauListRequestSenderTest : TestCaseWithFactory
{
	[TestDate(2024, 12, 15, 4, 0, 0)]
	public void TestSendBordereauListRequest() => CombineAssertions(() =>
	{
		AssertEquals("Pre-condition: No EDIMessage", 0, LoadEDIMessages().Length);

		var sender = new BordereauListRequestSender(new LoggingInformationForTesting());
		using (DisposableEnvironment.ForCompany(company.GC_Code))
		using (CHCustomsDataRegistry.Instance.EdecBordereauConfig.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new EdecBordereauConfig() { NumberOfDays = 10 }))
		{
			sender.Send();
		}

		var ediMessage = LoadEDIMessages().SingleOrDefault();
		AssertNotNull("EDIMessage created", ediMessage);
		AssertEquals("EM_ApplicationCode", ApplicationCodeList.Codes.CHCustomsEdec, ediMessage?.EM_ApplicationCode);
		AssertEquals("EM_MessageType", MessageTypeCodeList.Codes.BOR, ediMessage?.EM_MessageType);
		AssertEquals("EM_MessageSubType", MessageSubTypeCodeList.Codes.BordereauList, ediMessage?.EM_MessageSubType);
		AssertEquals("EM_ReceiveTransmit", ReceiveTransmitList.Codes.Transmit, ediMessage?.EM_ReceiveTransmit);
		AssertNotEquals("EM_MessageNum", ZString.Empty, ediMessage?.EM_MessageNum);
		AssertEquals("EM_Status", EDIMessage.Status.Queued, ediMessage?.EM_Status);
		AssertEquals("EM_LinkTable", GlbCompanySchema.Constants.TableName, ediMessage?.EM_LinkTable);
		AssertEquals("EM_LinkUniqueID", company.PK, ediMessage?.EM_LinkUniqueID);
		AssertEquals("EM_GP", CompanyCredentialPK, ediMessage?.EM_GP);

		var xml = XDocument.Parse(ediMessage?.EM_MessageText.ToString());
		var nsm = new XmlNamespaceManager(new NameTable());
		nsm.AddNamespace("b", xml.Root.Name.NamespaceName);
		AssertEquals("startDate", "2024-12-05", xml.XPathSelectElement("/b:bordereauRequest/b:bordereauList/b:dateRange/b:startDate", nsm)?.Value);
		AssertEquals("endDate", "2024-12-15", xml.XPathSelectElement("/b:bordereauRequest/b:bordereauList/b:dateRange/b:endDate", nsm)?.Value);
		AssertEquals("documentStatus", "read", xml.XPathSelectElement("/b:bordereauRequest/b:bordereauList/b:documentStatus", nsm)?.Value);
		AssertEquals("accountNumber", CustomsAccountNo, xml.XPathSelectElement("/b:bordereauRequest/b:bordereauList/b:accountNumber", nsm)?.Value);
		AssertEquals("processingCenterNumber", null, xml.XPathSelectElement("/b:bordereauRequest/b:bordereauList/b:processingCenterNumber", nsm)?.Value);
		AssertEquals("requestorTraderIdentificationNumber", CustomsRegistrationNo, xml.XPathSelectElement("/b:bordereauRequest/b:requestorTraderIdentificationNumber", nsm)?.Value);
		AssertEquals("requestorCorrelationID", null, xml.XPathSelectElement("/b:bordereauRequest/b:requestorCorrelationID", nsm)?.Value);

		EDIMessage[] LoadEDIMessages() => Factory.Load<EDIMessage>(new ZQuery());
	});

	protected override void SetUp()
	{
		base.SetUp();
		company = MessageProcessorTestHelper.CreateCompany(Factory, cad: CustomsAccountNo, customsRegNo: CustomsRegistrationNo, certificateCredential: true);
	}

	GlbCompany company;
	ZGuid CompanyCredentialPK => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword.PK;

	const string CustomsRegistrationNo = "CRN01";
	const string CustomsAccountNo = "12345678";
}
