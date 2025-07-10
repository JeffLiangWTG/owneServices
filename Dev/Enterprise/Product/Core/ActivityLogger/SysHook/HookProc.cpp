#include "stdafx.h"
#include "Syshook.h"

void CommonHookProcedure(int32_t hookId, int32_t nCode, WPARAM wParam, LPARAM lParam)
{
	if (nCode >= 0)
	{
		static bool bMappingIsOpened = IsMappingOpen();
		if (!bMappingIsOpened)
		{
			bMappingIsOpened = OpenMapping();
		}

		if (bMappingIsOpened)
		{
			WriteToSharedMemory(hookId, nCode, wParam, lParam);
		}
	}
}

// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nc-winuser-hookproc
// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getwindowtextlengthw
// https://docs.microsoft.com/en-us/windows/desktop/api/winuser/nf-winuser-getwindowtextw
LRESULT CALLBACK CBTHookProcedure(int32_t nCode, WPARAM wParam, LPARAM lParam)
{
	CommonHookProcedure(WH_CBT, nCode, wParam, lParam);
	return CallNextHookEx(nullptr, nCode, wParam, lParam);
}

// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644984(v=vs.85).aspx
LRESULT CALLBACK KeyboardHookProcedure(int32_t nCode, WPARAM wParam, LPARAM lParam)
{
	CommonHookProcedure(WH_KEYBOARD, nCode, wParam, lParam);
	return CallNextHookEx(nullptr, nCode, wParam, lParam);
}

// https://msdn.microsoft.com/en-us/library/windows/desktop/ms644988(v=vs.85).aspx
LRESULT CALLBACK MouseHookProcedure(int32_t nCode, WPARAM wParam, LPARAM lParam)
{
	if (nCode >= 0)
	{
		static LONG prevX = 0;
		static LONG prevY = 0;
		PMOUSEHOOKSTRUCT pMouseHookStruct = (PMOUSEHOOKSTRUCT)lParam;
		if (wParam != WM_MOUSEMOVE || pMouseHookStruct->pt.x != prevX || pMouseHookStruct->pt.y != prevY)
		{
			prevX = pMouseHookStruct->pt.x;
			prevY = pMouseHookStruct->pt.y;
			CommonHookProcedure(WH_MOUSE, nCode, wParam, lParam);
		}
	}

	return CallNextHookEx(nullptr, nCode, wParam, lParam);
}
