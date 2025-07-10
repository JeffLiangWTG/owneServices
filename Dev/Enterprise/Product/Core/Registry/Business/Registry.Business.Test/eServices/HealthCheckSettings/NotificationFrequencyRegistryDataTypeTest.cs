using Enterprise.Registry.Business.eServices.HealthCheckSettings;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NotificationFrequencyRegistryDataType))]
	sealed class NotificationFrequencyRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NotificationFrequencyRegistryDataType>
	{
		protected override NotificationFrequencyRegistryDataType GetNewDataType()
		{
			return new NotificationFrequencyRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "NotificationFrequencyRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var notificationFrequency = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Periodically,
				TimeInterval = 15
			};

			var byteValue = new byte[]
			{
			60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
			0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,78,0,111,0,116,0,105,0,102,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,70,0,114,0,101,0,113,0,117,0,101,0,110,0,99,0,121,0,62,0,60,0,83,0,101,0,116,
			0,116,0,105,0,110,0,103,0,115,0,62,0,80,0,69,0,82,0,73,0,79,0,68,0,73,0,67,0,65,0,76,0,76,0,89,0,60,0,47,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,84,0,105,0,109,
			0,101,0,73,0,110,0,116,0,101,0,114,0,118,0,97,0,108,0,62,0,49,0,53,0,60,0,47,0,84,0,105,0,109,0,101,0,73,0,110,0,116,0,101,0,114,0,118,0,97,0,108,0,62,0,60,0,47,0,78,0,111,0,116,0,105,
			0,102,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,70,0,114,0,101,0,113,0,117,0,101,0,110,0,99,0,121,0,62,0
			};

			var notificationDisable = new NotificationFrequency
			{
				Settings = HealthCheckConstants.NotificationFrequencyConstants.Disable,
				TimeInterval = 15 // still can set value but will display 0
			};

			var byteValueDisable = new byte[]
			{
			60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
			0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,78,0,111,0,116,0,105,0,102,0,105,0,99,0,97,0,116,0,105,0,111,0,110,0,70,0,114,0,101,0,113,0,117,0,101,0,110,0,99,0,121,0,62,0,60,0,83,0,101,0,116,
			0,116,0,105,0,110,0,103,0,115,0,62,0,68,0,73,0,83,0,65,0,66,0,76,0,69,0,60,0,47,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,115,0,62,0,60,0,84,0,105,0,109,0,101,0,73,0,110,0,116,0,101,
			0,114,0,118,0,97,0,108,0,62,0,48,0,60,0,47,0,84,0,105,0,109,0,101,0,73,0,110,0,116,0,101,0,114,0,118,0,97,0,108,0,62,0,60,0,47,0,78,0,111,0,116,0,105,0,102,0,105,0,99,0,97,0,116,0,105,
			0,111,0,110,0,70,0,114,0,101,0,113,0,117,0,101,0,110,0,99,0,121,0,62,0
			};

			return new[] { new ValidSampleAndBinaryValueInDB(notificationFrequency, byteValue), new ValidSampleAndBinaryValueInDB(notificationDisable, byteValueDisable) };
		}
	}
}
