using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(DeliveryModeRegistryDataType))]
	sealed class DeliveryModeRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<DeliveryModeRegistryDataType>
	{
		protected override DeliveryModeRegistryDataType GetNewDataType()
		{
			return new DeliveryModeRegistryDataType();
		}

		protected override string ExpectedEditorName => "DeliveryModeRegistryItemEditor";

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = DeliveryModeCollection.GetDefault();

			var xml = @"<?xml version=""1.0"" encoding=""utf-16""?><DeliveryModeCollection DefaultCode=""CFS/CFS""><DeliveryMode><CodeMaxLength>7</CodeMaxLength><Code>CFS/CFS</Code><Description>CFS/CFS</Description><UserDefinedCode>CFS/CFS</UserDefinedCode><UserDefinedDescription>CFS/CFS</UserDefinedDescription><IsSystemDefined>Y</IsSystemDefined></DeliveryMode><DeliveryMode><CodeMaxLength>7</CodeMaxLength><Code>CY/CY</Code><Description>CY/CY</Description><UserDefinedCode>CY/CY</UserDefinedCode><UserDefinedDescription>CY/CY</UserDefinedDescription><IsSystemDefined>Y</IsSystemDefined></DeliveryMode><DeliveryMode><CodeMaxLength>7</CodeMaxLength><Code>CY/CFS</Code><Description>CY/CFS</Description><UserDefinedCode>CY/CFS</UserDefinedCode><UserDefinedDescription>CY/CFS</UserDefinedDescription><IsSystemDefined>Y</IsSystemDefined></DeliveryMode><DeliveryMode><CodeMaxLength>7</CodeMaxLength><Code>CFS/CY</Code><Description>CFS/CY</Description><UserDefinedCode>CFS/CY</UserDefinedCode><UserDefinedDescription>CFS/CY</UserDefinedDescription><IsSystemDefined>Y</IsSystemDefined></DeliveryMode></DeliveryModeCollection>";

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection, xml)
			};
		}
	}
}
