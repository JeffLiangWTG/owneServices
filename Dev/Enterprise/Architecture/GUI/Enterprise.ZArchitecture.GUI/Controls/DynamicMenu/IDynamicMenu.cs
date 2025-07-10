using System;
using CargoWise.ComponentModel;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IDynamicMenu : IMenuItem
	{
		event EventHandler Opening;
	}
}
