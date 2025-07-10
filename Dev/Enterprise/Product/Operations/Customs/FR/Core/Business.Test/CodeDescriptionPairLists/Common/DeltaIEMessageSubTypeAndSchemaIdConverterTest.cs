using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.FR.Business.CodeDescriptionPairLists.Testing
{
	sealed class DeltaIEMessageSubTypeAndSchemaIdConverterTest : TestCaseWithFactory
	{
		public void TestConvertMessageSubTypeToSchemaId_WhenMessageIsFRAType()
		{
			var messageSubTypes = new List<string>() { "101", "102", "103" };
			var convertedSchemaIds = messageSubTypes.Select(x => DeltaIEMessageSubTypeAndSchemaIdConverter.ConvertMessageSubTypeToSchemaId(x));
			var expectedConvertedSchemaIds = new List<string>() { "FRA101", "FRA102", "FRA103" };
			AssertContainsExactElementsInExactOrder("MessageSubType being 101/102/103 should be added with a FRA prefix to form the SchemaId.", expectedConvertedSchemaIds, convertedSchemaIds);
		}

		public void TestFromMessageSubTypeToSchemaId_WhenMessageIsIEType()
		{
			var messageSubTypes = new List<string>() { "404", "410", "413", "414", "415", "431", "432", "433", "456", "457", "460", "462", "917" };
			var convertedSchemaIds = messageSubTypes.Select(x => DeltaIEMessageSubTypeAndSchemaIdConverter.ConvertMessageSubTypeToSchemaId(x));
			var expectedConvertedSchemaIds = new List<string>() { "IE404", "IE410", "IE413", "IE414", "IE415", "IE431", "IE432", "IE433", "IE456", "IE457", "IE460", "IE462", "IE917" };
			AssertContainsExactElementsInExactOrder("MessageSubType being 404/410/413/414/415/431/432/433/456/457/460/462/917 should be added with a IE prefix to form the SchemaId.", expectedConvertedSchemaIds, convertedSchemaIds);
		}

		public void TestConvertSchemaIdToMessageSubType()
		{
			var schemaIds = new List<string>() { "FRA101", "FRA102", "FRA103", "IE404", "IE410", "IE413", "IE414", "IE415", "IE431", "IE432", "IE433", "IE456", "IE457", "IE460", "IE462", "IE917" };
			var convertedMessageSubTypes = schemaIds.Select(x => DeltaIEMessageSubTypeAndSchemaIdConverter.ConvertSchemaIdToMessageSubType(x));
			var expectedConvertedMessageSubTypes = new List<string>() { "101", "102", "103", "404", "410", "413", "414", "415", "431", "432", "433", "456", "457", "460", "462", "917" };
			AssertContainsExactElementsInExactOrder("MessageSubType is the numeric characters of schemaIds", expectedConvertedMessageSubTypes, convertedMessageSubTypes);
		}
	}
}
