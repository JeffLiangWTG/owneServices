using CargoWise.EntityFramework;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CMRCARLSTMessage))]
	public class CMRCARLSTMessageTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEM_MessageTypeSetOnSetDefaultValues()
		{
			AssertEquals("EM_MessageType", CMRMessage.CMRMessageTypes.CARLST, ((EDIMessage)GetNewBusinessObject()).EM_MessageType);
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
