using System.Runtime.InteropServices;

namespace Enterprise.ZArchitecture.GUI
{
	internal static class InputSimulatorUnsafeNativeMethods
	{
		[DllImport("user32.dll")]
		internal static extern uint MapVirtualKey(uint uCode, uint uMapType);

#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs")]
		[DllImport("user32.dll")]
		internal static extern bool SetKeyboardState(byte[] lpKeyState);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1414:MarkBooleanPInvokeArgumentsWithMarshalAs")]
		[DllImport("user32.dll")]
		internal static extern bool GetKeyboardState(byte[] lpKeyState);
#endif
	}
}