using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public static class KeyToAscii
{
	[DllImport("User32.dll")]
	public static extern int ToAscii(int uVirtKey, int uScanCode, byte[] lpbKeyState, byte[] lpChar, int uFlags);

	public static char ToAscii(int keyCode, bool shift = false, bool ctrl = false, bool alt = false)
	{
		char result;

		var keyState = new byte[256];
		if (shift)
		{
			keyState[(int)Keys.ShiftKey] = 0x80;
		}
		if (ctrl)
		{
			keyState[(int)Keys.ControlKey] = 0x80;
		}
		if (alt)
		{
			keyState[(int)Keys.Menu] = 0x80;
		}

		// Allocate 2 bytes for lpChar because ToAscii may return up to 2 characters 
		// in some rare cases (e.g., using dead keys on international keyboards).
		var lpChar = new byte[2];

		if (ToAscii(keyCode, 0, keyState, lpChar, 0) == 1)
		{
			result = Convert.ToChar(lpChar[0]);
		}
		else
		{
			result = new char();
		}

		return result;
	}
}
