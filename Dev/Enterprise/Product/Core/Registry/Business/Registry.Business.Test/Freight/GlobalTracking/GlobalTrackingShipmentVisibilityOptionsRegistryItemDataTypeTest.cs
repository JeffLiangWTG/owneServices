using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType))]
	sealed class GlobalTrackingShipmentVisibilityRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType>
	{
		GlobalTrackingShipmentVisibilityServiceEhubIDCollection DefaultServiceEhubIDs => new GlobalTrackingShipmentVisibilityServiceEhubIDList().GetDefaultGlobalTrackingShipmentVisibilityServiceEhubIDs();
		protected override GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType GetNewDataType()
		{
			return new GlobalTrackingShipmentVisibilityOptionsRegistryItemDataType(new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation	= false, ServiceEhubIDs = DefaultServiceEhubIDs });
		}

		protected override string ExpectedEditorName => "GlobalTrackingShipmentVisibilityOptionsRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var option1 = new GlobalTrackingShipmentVisibilityOptions { IsActive = false, IsDefaultContainerAutomation = false, ServiceEhubIDs = DefaultServiceEhubIDs };
			var option2 = new GlobalTrackingShipmentVisibilityOptions { IsActive = true, IsDefaultContainerAutomation = false, ServiceEhubIDs = DefaultServiceEhubIDs };
			var option3 = new GlobalTrackingShipmentVisibilityOptions { IsActive = true, IsDefaultContainerAutomation = true, ServiceEhubIDs = DefaultServiceEhubIDs };

			var xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><GlobalTrackingShipmentVisibilityOptions><IsActive>N</IsActive><IsDefaultContainerAutomation>N</IsDefaultContainerAutomation><ArrayOfGlobalTrackingShipmentVisibilityServiceEhubID xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlobalTrackingShipmentVisibilityServiceEhubID><CodeMaxLength>4</CodeMaxLength><Code>CA</Code><Description /><EhubID>CONTAINER_TRACKING</EhubID><Service>CA</Service></GlobalTrackingShipmentVisibilityServiceEhubID><GlobalTrackingShipmentVisibilityServiceEhubID><CodeMaxLength>4</CodeMaxLength><Code>AWBA</Code><Description /><EhubID>FLIGHT_MONITORING_SYSTEM</EhubID><Service>AWBA</Service></GlobalTrackingShipmentVisibilityServiceEhubID></ArrayOfGlobalTrackingShipmentVisibilityServiceEhubID></GlobalTrackingShipmentVisibilityOptions>";
			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><GlobalTrackingShipmentVisibilityOptions><IsActive>Y</IsActive><IsDefaultContainerAutomation>N</IsDefaultContainerAutomation><ArrayOfGlobalTrackingShipmentVisibilityServiceEhubID xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlobalTrackingShipmentVisibilityServiceEhubID><CodeMaxLength>4</CodeMaxLength><Code>CA</Code><Description /><EhubID>CONTAINER_TRACKING</EhubID><Service>CA</Service></GlobalTrackingShipmentVisibilityServiceEhubID><GlobalTrackingShipmentVisibilityServiceEhubID><CodeMaxLength>4</CodeMaxLength><Code>AWBA</Code><Description /><EhubID>FLIGHT_MONITORING_SYSTEM</EhubID><Service>AWBA</Service></GlobalTrackingShipmentVisibilityServiceEhubID></ArrayOfGlobalTrackingShipmentVisibilityServiceEhubID></GlobalTrackingShipmentVisibilityOptions>";
			var xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><GlobalTrackingShipmentVisibilityOptions><IsActive>Y</IsActive><IsDefaultContainerAutomation>Y</IsDefaultContainerAutomation><ArrayOfGlobalTrackingShipmentVisibilityServiceEhubID xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><GlobalTrackingShipmentVisibilityServiceEhubID><CodeMaxLength>4</CodeMaxLength><Code>CA</Code><Description /><EhubID>CONTAINER_TRACKING</EhubID><Service>CA</Service></GlobalTrackingShipmentVisibilityServiceEhubID><GlobalTrackingShipmentVisibilityServiceEhubID><CodeMaxLength>4</CodeMaxLength><Code>AWBA</Code><Description /><EhubID>FLIGHT_MONITORING_SYSTEM</EhubID><Service>AWBA</Service></GlobalTrackingShipmentVisibilityServiceEhubID></ArrayOfGlobalTrackingShipmentVisibilityServiceEhubID></GlobalTrackingShipmentVisibilityOptions>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(option1, xml1),
				new ValidSampleAndBinaryValueInDB(option2, xml2),
				new ValidSampleAndBinaryValueInDB(option3, xml3),
			};
		}
	}
}
