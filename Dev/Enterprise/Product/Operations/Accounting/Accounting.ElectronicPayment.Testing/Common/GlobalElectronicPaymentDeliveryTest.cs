using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.ElectronicPayment.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Accounting.ElectronicPayment.Testing.Common
{
	public class GlobalElectronicPaymentDeliveryTest : TestCaseWithFactory
	{
		public void TestQueuedStatus()
		{
			var delivery = new GlobalElectronicPaymentDeliveryForTest();
			AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, delivery.GetQueuedStatus());
		}

		public void TestTransportType()
		{
			var delivery = new GlobalElectronicPaymentDeliveryForTest();
			AssertEquals(EDIInterchangeTransportTypeList.Codes.eHub, delivery.GetTransportType());
		}

		public void TestRecipientID()
		{
			var communicationMode = new NonPersistentEDICommunicationMode();
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationMode.EK_Destination = "OFX_XHUB_EPAYMENT";
			communicationMode.EK_MessagePurpose = "Test";

			var delivery = new GlobalElectronicPaymentDeliveryForTest();
			AssertEquals("OFX_XHUB_EPAYMENT", delivery.GetDestinationRecipientID(communicationMode));
		}

		public void TestGetInterestedLogParents()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var testHelper = new EPaymentTestHelper(objectCreator);

			var company1 = objectCreator.CreateCompanyAndBranch("AUMEL");
			Factory.Save();

			AccEPaymentQuote quote;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, company1.FirstActiveBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				quote = testHelper.CreateQuote(company1);
				Factory.Save();
			}

			var delivery = new GlobalElectronicPaymentDeliveryForTest();
			var expectedLogParents = new List<IStmALogParent>() { quote, quote.PaymentApproval };
			AssertContainsExactElementsInAnyOrder(expectedLogParents, delivery.GetLogParents(quote));
		}

		class GlobalElectronicPaymentDeliveryForTest : GlobalElectronicPaymentDelivery
		{
			public string GetQueuedStatus() => base.InterchangeQueuedStatus;

			public string GetTransportType() => base.TransportType;

			public string GetDestinationRecipientID(IEDICommunicationsMode mode) => base.GetRecipientID(mode);

			public IEnumerable<IStmALogParent> GetLogParents(IStmALogParent triggerBOWithLogs) => base.GetInterestedLogParents(triggerBOWithLogs);
		}
	}
}