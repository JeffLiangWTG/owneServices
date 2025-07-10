using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(CalculateDeliveryDueDateOptionsRegistryItemDataType))]
	sealed class CalculateDeliveryDueDateOptionsRegistryItemDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CalculateDeliveryDueDateOptionsRegistryItemDataType>
	{
		protected override CalculateDeliveryDueDateOptionsRegistryItemDataType GetNewDataType()
		{
			return new CalculateDeliveryDueDateOptionsRegistryItemDataType(new CalculateDeliveryDueDateOptions { IsActive = false });
		}

		protected override string ExpectedEditorName => "CalculateDeliveryDueDateOptionsRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var option1 = new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = true };
			var option2 = new CalculateDeliveryDueDateOptions(new CalculateDeliveryDueDateTransportModeList(RegistryFactory.Instance).GetDefaultCalculateDeliveryDueDateOptions()) { IsActive = false };
			var option3 = new CalculateDeliveryDueDateOptions(ActiveTransportModesForCalculateDeliveryDateOption()) { IsActive = true };

			var xml1 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CalculateDeliveryDueDateOptions><IsActive>Y</IsActive><ArrayOfCalculateDeliveryDueDateTransportMode xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>AIR</Code><Description>Air Freight</Description><Enabled>Y</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>SEA</Code><Description>Sea Freight</Description><Enabled>N</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>ROA</Code><Description>Road Freight</Description><Enabled>N</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>RAI</Code><Description>Rail Freight</Description><Enabled>N</Enabled></CalculateDeliveryDueDateTransportMode></ArrayOfCalculateDeliveryDueDateTransportMode></CalculateDeliveryDueDateOptions>";
			var xml2 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CalculateDeliveryDueDateOptions><IsActive>N</IsActive><ArrayOfCalculateDeliveryDueDateTransportMode xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>AIR</Code><Description>Air Freight</Description><Enabled>Y</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>SEA</Code><Description>Sea Freight</Description><Enabled>N</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>ROA</Code><Description>Road Freight</Description><Enabled>N</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>RAI</Code><Description>Rail Freight</Description><Enabled>N</Enabled></CalculateDeliveryDueDateTransportMode></ArrayOfCalculateDeliveryDueDateTransportMode></CalculateDeliveryDueDateOptions>";
			var xml3 = "<?xml version=\"1.0\" encoding=\"utf-16\"?><CalculateDeliveryDueDateOptions><IsActive>Y</IsActive><ArrayOfCalculateDeliveryDueDateTransportMode xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>AIR</Code><Description>Air Freight</Description><Enabled>Y</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>SEA</Code><Description>Sea Freight</Description><Enabled>Y</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>ROA</Code><Description>Road Freight</Description><Enabled>N</Enabled></CalculateDeliveryDueDateTransportMode><CalculateDeliveryDueDateTransportMode><CodeMaxLength>3</CodeMaxLength><Code>RAI</Code><Description>Rail Freight</Description><Enabled>N</Enabled></CalculateDeliveryDueDateTransportMode></ArrayOfCalculateDeliveryDueDateTransportMode></CalculateDeliveryDueDateOptions>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(option1, xml1),
				new ValidSampleAndBinaryValueInDB(option2, xml2),
				new ValidSampleAndBinaryValueInDB(option3, xml3)
			};
		}

		CalculateDeliveryDueDateTransportModeCollection ActiveTransportModesForCalculateDeliveryDateOption()
		{
			var activeTransportModes = new CalculateDeliveryDueDateTransportModeCollection();
			activeTransportModes.Add(Core.Constants.TransportModes.Air, Core.Constants.TransportModeDescriptions.Air, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Sea, Core.Constants.TransportModeDescriptions.Sea, true);
			activeTransportModes.Add(Core.Constants.TransportModes.Road, Core.Constants.TransportModeDescriptions.Road, false);
			activeTransportModes.Add(Core.Constants.TransportModes.Rail, Core.Constants.TransportModeDescriptions.Rail, false);
			return activeTransportModes;
		}
	}
}
