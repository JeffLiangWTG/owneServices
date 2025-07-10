using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Startup.Testing
{
	sealed class HotkeyControl : ZUserControl, IHotkeyProvider
	{
		public override string TypeNameForDisplay => typeNameForDisplay;
		public string typeNameForDisplay;
	}
}
