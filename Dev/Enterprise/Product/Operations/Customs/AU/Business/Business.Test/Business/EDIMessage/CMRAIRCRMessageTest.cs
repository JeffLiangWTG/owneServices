using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRAIRCRMessage))]
	public class CMRAIRCRMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAssumeMessageClearIfAcknowledgedAndNoResponse()
		{
			EDIMessage message = (EDIMessage)GetNewBusinessObject();
			Assert(message.AssumeMessageClearIfAcknowledgedAndNoResponse);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIMessage result = (EDIMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
