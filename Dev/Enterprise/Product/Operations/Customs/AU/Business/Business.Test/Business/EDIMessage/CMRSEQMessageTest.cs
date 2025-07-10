using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRSEQMessage))]
	public class CMRSEQMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEM_MessageTypeSet()
		{
			CMRSEQMessage message = Factory.New<CMRSEQMessage>();
			AssertEquals("EM_MessageType", CMRMessage.CMRMessageTypes.SEQ, message.EM_MessageType);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			EDIMessage result = (EDIMessage)GetNewBusinessObject();
			result.EM_MessageText = EDIMessage.MessageNumberPlaceHolder;
			return result;
		}

		protected override bool CanPersistedObjectBeDeleted => false;

		#endregion
	}
}
