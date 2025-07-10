using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using MailManager;
using NUnit.Framework;

namespace Enterprise.MailManager.Business.Testing
{
	[TestedType(typeof(MailItem))]
	sealed class MailItemBusinessObjectTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			MailItem result = (MailItem)base.GetNewBusinessObjectForDeleteTest(factory);
			result.MI_Direction = DirectionList.Codes.Receive;
			result.MI_LastAttemptDateTime = ZDateTime.UtcNow;
			result.MI_ReceivedDateTime = ZDateTime.UtcNow;
			result.MI_SendDateTime = ZDateTime.UtcNow;
			return result;
		}
	}
}
