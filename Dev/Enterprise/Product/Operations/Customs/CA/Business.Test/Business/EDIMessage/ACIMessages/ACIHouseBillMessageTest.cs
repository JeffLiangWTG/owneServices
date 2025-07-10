using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(ACIHouseBillMessage))]
	sealed class ACIHouseBillMessageTest : ACIForwarderMessageTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (ACIHouseBillMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<ACIHouseBillMessage>();
		}

		public override void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.CAACI, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.ACIHouseBill, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestTransactionNumber()
		{
			var message = (ACIHouseBillMessage)GetNewBusinessObject();
			AssertEquals("no linked object", ZString.Empty, message.TransactionNumber);
			var shipment = Factory.New<Freight.Forwarding.Business.ForwardingShipment>();
			message.EM_LinkedObject = shipment;
			AssertEquals("shipment linked object", ZString.Empty, message.TransactionNumber);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.TransactionNumber.AccountSecurityCode = "40000";
			declaration.TransactionNumber.SequentialNumber = "4228";
			var entryHeader = declaration.ActiveEntryHeaders.AddNew();
			message.EM_LinkedObject = entryHeader;
			AssertEquals("entry header linked object", "40000000042286", message.TransactionNumber);
		}

		public void TestSystemDefinedValues()
		{
			var testMessage = Factory.New<ACIHouseBillMessage>();
			testMessage.EM_ApplicationCode = "ACI";
			testMessage.EM_MessageType = "AHB";
			testMessage.EM_MessageSubType = "MFH";
			testMessage.PrimaryCCN = "CCN1";

			testMessage.SetSystemDefinedValue(EDIMessage.Schema.SNPType, new ZString("CA"));
			testMessage.SetSystemDefinedValue(EDIMessage.Schema.SubLocation, new ZString("3380"));
			testMessage.SetSystemDefinedValue(EDIMessage.Schema.CBSAOffice, new ZString("0809"));
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var messageInNewFactory = factory2.Load<ACIHouseBillMessage>(testMessage.PK);
			AssertEquals("Sub-location", "3380", messageInNewFactory.SubLocation);
			AssertEquals("CBSA Office", "0809", messageInNewFactory.CBSAOffice);
			AssertEquals("Primary CCN", "CCN1", messageInNewFactory.PrimaryCCN);
			AssertEquals("Primary SNPType", "CA", messageInNewFactory.SNPType);
		}
	}
}
