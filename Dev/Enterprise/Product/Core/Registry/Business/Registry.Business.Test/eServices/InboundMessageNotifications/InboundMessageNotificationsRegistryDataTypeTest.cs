using CargoWise.Types;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InboundMessageNotificationsRegistryDataType))]
	sealed class InboundMessageNotificationsRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<InboundMessageNotificationsRegistryDataType>
	{
		protected override InboundMessageNotificationsRegistryDataType GetNewDataType()
		{
			return new InboundMessageNotificationsRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "InboundMessageNotificationsRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var inboundMessageNotificationsRule = new InboundMessageNotificationsRule();

			inboundMessageNotificationsRule.NotifyGroup = ZGuid.Empty;
			inboundMessageNotificationsRule.NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.None;
			inboundMessageNotificationsRule.NotifyGroupWhenUserFound = true;

			byte[] byteValue = System.Array.Empty<byte>();

			return new[] { new ValidSampleAndBinaryValueInDB(inboundMessageNotificationsRule, byteValue) };
		}
	}
}
