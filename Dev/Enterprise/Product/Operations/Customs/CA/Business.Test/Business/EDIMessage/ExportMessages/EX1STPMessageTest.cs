using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(EX1STPMessage))]
	sealed class EX1STPMessageTest : EXPEDIMessageTest
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EX1STPMessage result = (EX1STPMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New<EX1STPMessage>();
		}

		public override void TestDefaultValues()
		{
			base.TestDefaultValues();
			AssertEquals("", MessageTypeList.Codes.G7Export, message.EM_MessageType);
		}
	}
}
