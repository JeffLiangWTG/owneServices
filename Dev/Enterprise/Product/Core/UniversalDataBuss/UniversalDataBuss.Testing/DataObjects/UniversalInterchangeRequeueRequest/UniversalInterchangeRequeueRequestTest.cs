using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(InterchangeRequeueRequest))]
	class InterchangeRequeueRequestTest : TopLevelDataObjectTestCase<InterchangeRequeueRequest>
	{
	}

	[TestedType(typeof(InterchangeRequeueRequestFilter))]
	class FilterTest : DataObjectTestCase<InterchangeRequeueRequestFilter>
	{
	}
}
