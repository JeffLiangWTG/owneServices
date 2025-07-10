using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IE.Messaging.Testing
{
	class IEOutgoingMessageProcessorTest : TestCaseWithFactory
	{
		public void TestSendNCTSMessage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefSysConfigType("IEIENSUBMT", "IEN SUBMISSION DESCRIPTION", "IEN SUBMISSION LONG DESCRIPTION");
			helper.CreateRefSysConfig("IEIENSUBMT", "https://www.ieetest.ie", ZDateTime.BrettsBirthday, ZDateTime.Empty);
			Factory.Save();

			var registration = ObjectFactory.Get<IProductRegistration>();
			registration.KeyForTest.DatabaseTypeForTest = DatabaseTypes.Codes.Test;

			(var company, var branch) = InterchangeProcessorTestHelper.CreateCompanyAndBranch(Factory, Core.Constants.CountryCodes.Ireland, "T1");
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(company);
			var companyPK = company.PK;
			var branchPK = branch.PK;

			var outgoingMessage = Factory.New<Enterprise.Messaging.Business.EDIMessage>();
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.IECustomsNCTS;
			outgoingMessage.EM_GB = branchPK;
			outgoingMessage.EM_GP = companyCredential.PK;
			outgoingMessage.EM_MessageType = "015";
			outgoingMessage.EM_MessageText = "<Greeting>HELLO</Greeting>";
			outgoingMessage.EM_Status = EDIMessage.Status.Pending;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive; // to allow saving of message number
			outgoingMessage.EM_MessageNum = "TEST1234567890";
			outgoingMessage.EM_ApplicationReference = ZGuid.NewZGuid().ToString();
			Factory.Save();
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			Factory.Save();

			new IEOutgoingMessageProcessor(new BatchProcessor.LoggingInformation()).ProcessMessage(System.Threading.CancellationToken.None);

			outgoingMessage.Reload();
			AssertEquals("EM_Status should be sent", EDIMessage.Status.Sent, outgoingMessage.EM_Status);
			AssertEquals("Interchange should be IEN", Enterprise.Messaging.Business.EDIInterchange.ApplicationCodes.IECustomsNCTS, outgoingMessage.Interchange.EI_ApplicationCode);
			AssertContains("Interchange should get correct endpoint", "https://www.ieetest.ie", outgoingMessage.Interchange.EI_HeaderText);
		}
	}
}
