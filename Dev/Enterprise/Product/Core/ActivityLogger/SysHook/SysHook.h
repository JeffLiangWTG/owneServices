#pragma once

#include "stdafx.h"
#include <cstdint>

struct HookMessage
{
	int32_t dwProcessId;
	int32_t hookId;
	int32_t nCode;
	WPARAM wParam; // 8 byte on x64, 4 byte on x86
	LPARAM lParam; // 8 byte on x64, 4 byte on x86
};

#define Linkage extern "C"
#define StorageClass __declspec(dllexport)
#define CallingConvention __cdecl

// interop
Linkage StorageClass int32_t CallingConvention AttachHooks();
Linkage StorageClass int32_t CallingConvention DetachHooks();
Linkage StorageClass int32_t CallingConvention WaitForBlock(int32_t dwMilliseconds, HookMessage* pMessage);
Linkage StorageClass int32_t CallingConvention GetSyshookPath(WCHAR* buf, int32_t length);
Linkage inline StorageClass int32_t CallingConvention GetMaxPath() { return MAX_PATH; }

// HookProc.cpp
LRESULT CALLBACK MouseHookProcedure(int32_t nCode, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK KeyboardHookProcedure(int32_t nCode, WPARAM wParam, LPARAM lParam);
LRESULT CALLBACK CBTHookProcedure(int32_t nCode, WPARAM wParam, LPARAM lParam);

// SharedMemory.cpp
void WriteToSharedMemory(int32_t hookId, int32_t nCode, WPARAM wParam, LPARAM lParam);
inline bool IsMappingOpen();
bool OpenMapping();
bool NewMapping();
bool CloseMapping();

// Variables
extern HINSTANCE g_dllHandle;
extern WCHAR g_SharedMemoryMappingName[];
