using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.ComplianceRisk.Integration
{
	public class ComplianceAssessmentPointPairInfo
	{
		public ComplianceAssessmentPointPairInfo()
		{
		}

		public ComplianceAssessmentPointPairInfo(IEnumerable<ComplianceCheckRequestPointPair> pointPairs)
		{
			Argument.NotNull(pointPairs, nameof(pointPairs));
			PointPairs = pointPairs;
		}

		public bool SupportAssessmentByBorderWise => PointPairs != null;
		public IEnumerable<ComplianceCheckRequestPointPair> PointPairs { get; }
	}

	public class ComplianceCheckRequestPointPair
	{
		public ComplianceCheckRequestPointPairLocation OriginPoint { get; set; }
		public ComplianceCheckRequestPointPairLocation DestinationPoint { get; set; }
		public string Mode { get; set; }
		public ZDateTime EstimatedTimeOfDeparture { get; set; }
		public ZDateTime EstimatedTimeOfArrival { get; set; }
	}

	public class ComplianceCheckRequestPointPairLocation
	{
		public string Country { get; set; }
		public string UNLOCO { get; set; }
		public string MovementDescription { get; set; }
	}
}
