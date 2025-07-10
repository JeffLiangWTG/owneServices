using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;

namespace Enterprise.ZArchitecture.GUI
{
	public class StmALogAsAddedByUserTest : TestCaseWithFactory
	{
		public void TestCustomEventsRemoveNotAllowAddedByUserEvents()
		{
			var log = Factory.New<StmALogAsAddedByUser>();

			var notAllowed = new List<Event>()
			{
				Events.ComplianceRiskInteraction,
				Events.ApprovedAllocationWithContainerWeightLimitExceeded
			};

			Assert("Not Allow Added By User Events are removed", (log.CustomEvents as List<Event>).Intersect(notAllowed).IsNullOrEmpty());
		}
	}
}
