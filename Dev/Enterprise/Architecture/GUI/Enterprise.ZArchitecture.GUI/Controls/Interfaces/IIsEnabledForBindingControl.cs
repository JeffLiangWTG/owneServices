using System;
using CargoWise.Types;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IIsEnabledForBindingControl
	{
		ZBool IsEnabledForBinding { get; set; }
		event EventHandler IsEnabledForBindingChanged;
	}
}
