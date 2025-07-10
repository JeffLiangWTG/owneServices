using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(EntryNumber))]
	sealed class EntryNumberTest : DataObjectTestCase<EntryNumber>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>
			{
				{ nameof(EntryNumber.Category), 3 },
				{ nameof(EntryNumber.Number), 35 },
				{ nameof(EntryNumber.EntryLineReference), 50 }
			};
		}
	}
}

