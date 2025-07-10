using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public interface IConsignmentsGridUserControlMenuProvider
	{
		IReadOnlyList<ZMenuItem> AdditionalConsignmentsGridMenuItems { get; }

		IReadOnlyList<ZMenuItem> AdditionalMenuItemsForMainForm { get; }
	}
}
