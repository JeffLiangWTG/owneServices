using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Currency))]
	class CurrencyTest : DataObjectTestCase<Currency>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(Currency.Code), RefCurrencySchema.RX_Code.MaxLength },
			{ nameof(Currency.Description), RefCurrencySchema.RX_Desc.MaxLength }
		};
	}
}

