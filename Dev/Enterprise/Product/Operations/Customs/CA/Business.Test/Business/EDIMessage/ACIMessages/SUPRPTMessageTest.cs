using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(SUPRPTMessage))]
	sealed class SUPRPTMessageTest : ACIEDIMessageTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			SUPRPTMessage result = (SUPRPTMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<SUPRPTMessage>();
		}

		public override void TestDefaultValues()
		{
			base.TestDefaultValues();
			AssertEquals("", MessageTypeList.Codes.SupplementaryCargoReport, message.EM_MessageType);
		}
	}
}
