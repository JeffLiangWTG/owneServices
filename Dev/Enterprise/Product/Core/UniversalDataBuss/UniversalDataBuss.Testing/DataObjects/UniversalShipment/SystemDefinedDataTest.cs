using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(SystemDefinedData))]
	class SystemDefinedDataTest : DataObjectTestCase<SystemDefinedData>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			var maxLenghtValues = base.ExpectedMaxLengthValues();
			maxLenghtValues.Add(nameof(SystemDefinedData.Name), 35);
			maxLenghtValues.Add(nameof(SystemDefinedData.Category), 64);
			maxLenghtValues.Add(nameof(SystemDefinedData.Value), UniversalXmlInfo.MaxStringLength);
			return maxLenghtValues;
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(SystemDefinedData.Value)
		};
	}
}
