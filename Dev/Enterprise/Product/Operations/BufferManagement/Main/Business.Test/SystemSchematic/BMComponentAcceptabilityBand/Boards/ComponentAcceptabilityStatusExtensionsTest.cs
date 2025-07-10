using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Business.Test
{
	class ComponentAcceptabilityStatusExtensionsTest : BMSTestCaseWithFactory
	{
		public void TestGetStatusTextHasSameValuesAsComponentAcceptabilityBandEnum()
		{
			var riskStatues = ((IEnumerable<ComponentAcceptabilityStatus>)Enum.GetValues(typeof(ComponentAcceptabilityStatus)))
				.Select(status => status.GetStatusText());
			var enumValues = BMSTestHelper.GetEnumMemberAttributeValues<ComponentAcceptabilityStatus>();

			AssertContainsExactElementsInAnyOrder(riskStatues, riskStatues);
		}
	}
}
