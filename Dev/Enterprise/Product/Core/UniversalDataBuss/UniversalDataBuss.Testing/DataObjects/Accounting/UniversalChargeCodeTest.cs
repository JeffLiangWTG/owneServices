using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(UniversalChargeCode))]
	class UniversalChargeCodeTest : DataObjectTestCase<UniversalChargeCode>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(UniversalChargeCode.Code), 10 },
				{ nameof(UniversalChargeCode.Description),80 }
			};
		}
	}
}

