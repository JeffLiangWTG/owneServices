using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DataTransfer.MessageDelivery.Testing
{
	sealed class BusinessObjectSerializerFactoryTest : TransactionedTestCase
	{
		public void TestGetSerializer_ItShouldReturnXmlBusinessObjectSerializerWhenActionIsNull()
		{
			var serializer = BusinessObjectSerializerFactory.GetSerializer(null, null, null, null, null, null);
			AssertEquals(true, serializer is XmlBusinessObjectSerializer);
		}

		public void TestGetSerializer_ItShouldReturnDxlBusinessObjectSerializerWhenActionTriggerTypeIsSendDescartesXml()
		{
			action.PQ_TriggerType = "DXL";
			var serializer = BusinessObjectSerializerFactory.GetSerializer(null, null, null, null, null, null);
			AssertEquals(true, serializer is XmlBusinessObjectSerializer);
		}

		public void TestGetSerializer_ItShouldReturnNativeXmlBusinessObjectSerializerWhenActionTriggerTypeIsSendNativeXml()
		{
			action.PQ_TriggerType = "XMN";
			var serializer = BusinessObjectSerializerFactory.GetSerializer(null, null, null, null, null, null);
			AssertNotNull(serializer);
			AssertEquals(true, serializer is IBusinessObjectSerializer);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			BusinessObjectFactory factory = new BusinessObjectFactory();
			action = factory.New<ProcessTaskNotification>();
		}
		ProcessTaskNotification action;

		#endregion
	}
}
