using System.Linq;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	class ComplianceAssessmentPointPairInfoTest : TestCase
	{
		public void TestConstructor()
		{
			var assessmentPointPairInfo = new ComplianceAssessmentPointPairInfo();
			AssertEquals(false, assessmentPointPairInfo.SupportAssessmentByBorderWise);
			AssertNull(assessmentPointPairInfo.PointPairs);

			assessmentPointPairInfo = new ComplianceAssessmentPointPairInfo(new[]
			{
				new ComplianceCheckRequestPointPair
				{
					OriginPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "AU",
						MovementDescription = "Origin",
						UNLOCO = "AUSYD"
					},
					DestinationPoint = new ComplianceCheckRequestPointPairLocation
					{
						Country = "US",
						MovementDescription = "Destination",
						UNLOCO = "USORD"
					},
					EstimatedTimeOfArrival = new ZDateTime(2024, 1, 12),
					EstimatedTimeOfDeparture = new ZDateTime(2024, 1, 2),
					Mode = "SEA"
				}
			});

			AssertEquals(true, assessmentPointPairInfo.SupportAssessmentByBorderWise);
			AssertContainsExactElementsInAnyOrder(new[] { ("AU", "AUSYD", "Origin", "US", "USORD", "Destination", new ZDateTime(2024, 1, 12), new ZDateTime(2024, 1, 2), "SEA") },
			assessmentPointPairInfo.PointPairs.Select(u => (u.OriginPoint.Country, u.OriginPoint.UNLOCO, u.OriginPoint.MovementDescription, u.DestinationPoint.Country, u.DestinationPoint.UNLOCO, u.DestinationPoint.MovementDescription, u.EstimatedTimeOfArrival, u.EstimatedTimeOfDeparture, u.Mode)));
		}
	}
}
