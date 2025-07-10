using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(SettlementMethod))]
	class SettlementMethodTest : DataObjectTestCase<SettlementMethod>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues()
		{
			int settlementMethodMaxLength = new[] { JobRequiredDocumentSchema.EQ_DocNumber.MaxLength, GetDefaultFieldLength() }.Max();

			return new Dictionary<string, int>()
			{
				{ nameof(SettlementMethod.AuthorizationReference), settlementMethodMaxLength }
			};
		}
	}
}

