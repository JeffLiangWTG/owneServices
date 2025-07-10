using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(UserDefinedData))]
	class UserDefinedDataTest : DataObjectTestCase<UserDefinedData>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			var maxLenghtValues = base.ExpectedMaxLengthValues();
			maxLenghtValues.Add(nameof(UserDefinedData.Name), 250);
			maxLenghtValues.Add(nameof(UserDefinedData.Value), UniversalXmlInfo.MaxStringLength);
			return maxLenghtValues;
		}

		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(UserDefinedData.Value)
		};
	}
}
