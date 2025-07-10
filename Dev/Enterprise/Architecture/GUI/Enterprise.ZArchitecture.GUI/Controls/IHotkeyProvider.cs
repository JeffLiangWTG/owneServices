using System.Collections.Generic;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public interface IHotkeyProvider
	{
		HotkeyRegister Hotkeys { get; }
		string TypeNameForDisplay { get; }
	}

	public interface IAdditionalHotkeyProviders : IHotkeyProvider
	{
		IEnumerable<IHotkeyProvider> OtherProviders { get; }
	}
}
