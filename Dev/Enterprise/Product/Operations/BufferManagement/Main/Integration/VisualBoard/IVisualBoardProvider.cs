using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Integration
{
	public interface IVisualBoardProvider : IBusiness
	{
		IVisualBoardProvider ReloadInFactory(BusinessObjectFactory factory);
		Guid PK { get; }
		IEnumerable<ISlideShowFrame> Boards { get; }

		string Name { get; }
		string Description { get; }
		string HumanReadableShortcutName { get; }
		int StartingRefreshIntervalInMinutes { get; }

		string OwnerStaffCode { get; }
		ZGuid OwnerGroupPK { get; }
		SecurityCheckpoint EditCheckpoint { get; }

		ControllerID ControllerID { get; }
		ZBool IsVisibleToCurrentCompany { get; }
	}
}
