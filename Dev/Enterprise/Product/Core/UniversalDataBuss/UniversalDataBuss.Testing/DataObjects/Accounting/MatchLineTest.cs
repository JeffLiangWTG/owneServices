using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(MatchLine))]
	class MatchLineTest : DataObjectTestCase<MatchLine>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			return new Dictionary<string, int>()
			{
				{ nameof(MatchLine.MatchGroupNumber), AccTransactionMatchLinkSchema.AP_MatchGroupNum.MaxLength },
			};
		}
	}
}
