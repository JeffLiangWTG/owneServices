using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IIsVisibleForBindingControl
	{
		ZBool IsVisibleForBinding { get; set; }
		event EventHandler IsVisibleForBindingChanged;
	}
}
