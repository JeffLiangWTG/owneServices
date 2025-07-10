using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.MailManager.Business.Testing
{
	[TestedType(typeof(CustomsMailItem))]
	sealed class CustomsMailItemBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var mailItem = (CustomsMailItem)base.GetNewBusinessObjectForDeleteTest(factory);
			mailItem.MI_Direction = DirectionList.Codes.Receive;
			mailItem.MI_LastAttemptDateTime = ZDateTime.UtcNow;
			mailItem.MI_ReceivedDateTime = ZDateTime.UtcNow;
			mailItem.MI_SendDateTime = ZDateTime.UtcNow;
			return mailItem;
		}
	}
}
