using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(BillOfLadingClause))]
	class BillOfLadingClauseTest : DataObjectTestCase<BillOfLadingClause>
	{
		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(BillOfLadingClause.Detail)
		};
	}
}
