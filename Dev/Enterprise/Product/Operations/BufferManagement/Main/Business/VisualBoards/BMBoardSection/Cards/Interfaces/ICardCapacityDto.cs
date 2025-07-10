using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public interface ICardCapacityDto
	{
		int Sequence { get; }
		IEnumerable<ZString> AssignedStaff { get; }
		ZString AssignedResourceCode { get; }
		bool IsAssignedToCCR { get; }
		bool IsInCCRWorkflow { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		decimal RelevantEstimateHours { get; }
		ZGuid ComponentPK { get; }
		IEnumerable<ZGuid> PenetratedComponentsPK { get; }
	}
}
