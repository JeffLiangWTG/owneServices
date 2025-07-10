using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(MSXMessageSendingObject))]
	sealed class MSXMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<CusEntryHeader>();
			return new MSXMessageSendingObject(header);
		}
	}
}
