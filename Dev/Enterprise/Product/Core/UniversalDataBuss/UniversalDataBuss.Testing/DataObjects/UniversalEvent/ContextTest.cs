using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(Context))]
	class ContextTest : DataObjectTestCase<Context>
	{
		protected override List<string> ExpectedAllowLineControlWhiteSpaceAttributePropertiesCore() => new List<string>()
		{
			nameof(Context.Value)
		};

		protected override List<string> ExpectedTrimWhiteSpacePropertiesCore() => new List<string>()
		{
			nameof(Context.Value)
		};
	}
}

