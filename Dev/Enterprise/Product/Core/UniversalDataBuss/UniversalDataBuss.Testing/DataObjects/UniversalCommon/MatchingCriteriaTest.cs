using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Testing
{
	[TestedType(typeof(MatchingCriteria))]
	class MatchingCriteriaTest : DataObjectTestCase<MatchingCriteria>
	{
		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(MatchingCriteria.Value)
		};
	}
}
