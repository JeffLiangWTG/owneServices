using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.MailManager.Business
{
	[TestedType(typeof(MailRecipient))]
	sealed class MailRecipientBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			DummyMailItem item = factory.New<DummyMailItem>();
			item.MI_Direction = DirectionList.Codes.Receive;
			item.MI_LastAttemptDateTime = ZDateTime.UtcNow;
			item.MI_ReceivedDateTime = ZDateTime.UtcNow;
			item.MI_SendDateTime = ZDateTime.UtcNow;
			return item.MailRecipientsCore_Exposed.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}
	}
}
