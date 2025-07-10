using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(TaxGroupCodeType))]
	class TaxGroupCodeTypeTest : DataObjectTestCase<TaxGroupCodeType>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(TaxGroupCodeType.Code), 3 },
			{ nameof(TaxGroupCodeType.Description), 256 },
			{ nameof(TaxGroupCodeType.GovernmentCode), 10 }
		};
	}
}
