using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public interface IReportsGridUserControlMenuProvider
	{
		IReadOnlyList<ZMenuItem> AdditionalReportsGridMenuItems { get; }

		IReadOnlyList<ZMenuItem> AdditionalMenuItemsForMainForm { get; }
	}
}
