using System;
using System.Collections.Generic;
using CargoWise.PAVE.Common.DTO;

namespace Enterprise.BufferManagement.Service.Shared
{
	public interface ISectionService
	{
		ISection Get(Guid sectionPK);
		IEnumerable<AcceptabilityBandResultDTO> GetTimeRecording(Guid sectionPK);
		IEnumerable<FilterStripDTO> GetFilterStrips(Guid sectionPK);
	}
}
