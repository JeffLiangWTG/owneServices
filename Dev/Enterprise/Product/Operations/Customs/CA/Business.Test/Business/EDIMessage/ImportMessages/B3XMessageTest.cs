using CargoWise.EntityFramework;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.Common.Shared;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(B3XMessage))]
	public class B3XMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTransactionNumber()
		{
			message.EM_MessageText = "UNH+2287+CUSDEC:S:99B:UN'BGM+:::AB+454+9'CST++I'LOC+41+497'LOC+11+497'RFF+TN:0001831'RFF+ARA:105759013RM0001'TDT+11++2++9165'DOC+785+803609238364B'DTM+204:20160120:102'MOA+43:4000'UNS+D'DMS+1'NAD+SE++GHJ LTD. INT?'L  ?+?:??@'DOC+935'DTM+129:20160119:102'LOC+27+AU+AU'PAT+1+CONSIGN:::02'MOA+6::CAD'CST+1+POS+1+8544700090+23'MOA+40:400000'MOA+43:400000'MOA+125:400000'RFF+LI:1:0'MOA+38:400000'TAX+7+VAT++5.0'MOA+1:20000'GIR+1+1'MEA+AAR++MTR:2134'TAX+5+++0.00'MOA+155:000'UNS+S'TAX+7+:::K90'MOA+1:20000'TAX+4+:::K90'MOA+176:20000'UNT+37+2287'";
			AssertEquals("TransactionNumber", "000001831", message.TransactionNumber);
		}

		public void TestMessageSubTypeDescription()
		{
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageSubType = B3EntryStatusList.Codes.Accepted;
			AssertEquals("EM_MessageSubTypeDescription", B3EntryStatusList.Descriptions.Accepted, message.EM_MessageSubTypeDescription);

			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageSubType = MessageSubTypeCodes.Codes.Original;
			AssertEquals("EM_MessageSubTypeDescription", MessageSubTypeCodes.Descriptions.Original, message.EM_MessageSubTypeDescription);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var result = (B3XMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<B3XMessage>();
		}

		public virtual void TestDefaultValues()
		{
			AssertEquals("EM_ApplicationCode", EDIMessage.ApplicationCodes.CAIMP, message.EM_ApplicationCode);
			AssertEquals("EM_MessageType", MessageTypeList.Codes.XTypeEntry, message.EM_MessageType);
			AssertEquals("ShouldShowInterpretation", true, message.ShouldShowInterpretation);
		}

		public void TestMessageNumberFilledIn()
		{
			var number = Env.NumberFountains.EDIFACTNumberFountain("M", "IMP", EDIMessage.ApplicationCodes.CAIMP).PeekPreliminaryFormatted(Factory);
			Factory.Save();
			AssertEquals("MessageNumberFilledIn", "Message Number = " + number, message.EM_MessageText);
		}

		public void TestBatchNumberInCUSDEC()
		{
			message.EM_MessageText = "UNH+63+CUSDEC:S:99B:UN'BGM++033+9'UNT+3+63'";
			AssertEquals("BatchNumber", "033", message.BatchNumber);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			message = (EDIMessage)GetNewBusinessObject();
			message.EM_MessageText = "Message Number = " + EDIMessage.MessageNumberPlaceHolder;
		}

		EDIMessage message;

		#endregion
	}
}
