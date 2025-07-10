using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IBMBoardSection : IBusiness
	{
		ZGuid MS_FC_Component { get; set; }
		ZGuid MS_MB_Board { get; set; }
		ZGuid MS_GG_ReleaseGroup { get; set; }

		ZString MS_SectionType { get; set; }
		ZString MS_LayoutData { get; set; }
		ZDateTime MS_SystemCreateTimeUtc { get; set; }
		ZString MS_SystemCreateUser { get; set; }
		ZDateTime MS_SystemLastEditTimeUtc { get; set; }
		ZString MS_SystemLastEditUser { get; set; }
		ZString SectionName { get; }

		ZInt Row { get; set; }
		ZInt RowSpan { get; set; }
		ZInt RowHeightPercent { get; set; }

		ZInt Column { get; set; }
		ZInt ColSpan { get; set; }
		ZInt ColWidthPercent { get; set; }

		IBoardSectionConfigurationBizo Configuration { get; }

		IBMBoard Board { get; }
		IBMComponent Component { get; }
		IEnumerable<IBMComponent> AllComponents { get; }

		event EventHandler LayoutConfigUpdated;
	}
}
