using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.ExitControl.GUI
{
	public interface IExitControlMainMenuProvider
	{
		IReadOnlyList<ZMenuItem> AdditionalMainMenuItems { get; }
	}
}
