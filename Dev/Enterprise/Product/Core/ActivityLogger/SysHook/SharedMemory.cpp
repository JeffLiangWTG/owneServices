#include "stdafx.h"
#include "Syshook.h"

const uint32_t g_nBlocks = 0x2000;
const uint32_t g_dwBufferSize = (sizeof(char) + sizeof(HookMessage)) * g_nBlocks + 0x100;

LPVOID g_pMapView = nullptr;
HANDLE g_hFileMapping = nullptr;
char volatile* g_pBlockUsed = nullptr;
HookMessage volatile* g_pMessages = nullptr;
uint32_t volatile* g_pNextFreeBlock = nullptr;
uint32_t* g_pNextUnreadBlock = nullptr;

//
// Memory layout
//
// This shared memory uses two index sections at the beginning to indicate free and unread
// blocks of memory.
//
// g_pMapView/g_pBlockUsed		--> +===========+ 0x0000_0000
//									|           |
//									|           |
//									|           |
// g_pMessages					--> +-----------+ 0x0000_2000
//									|           |
//									|           |
//									|           |
// g_pNextFreeBlock				--> +-----------+ 0x0000_4000
// g_pNextUnreadBlock			--> +--         | 0x0000_4008
//									+--         | 0x0000_401C
//									+--         | 0x0000_4030
//									+--         | 0x000._....
// g_pMapView + g_dwBufferSize	--> +===========+ 0x0002_A100
//

//Writes out the hook details into the memory provided by pMessage
Linkage StorageClass int32_t CallingConvention WaitForBlock(int32_t dwMilliseconds, HookMessage* pMessage)
{
	MSG msg;
	int32_t dwWaitMilliseconds = 0;

	do
	{
		if (PeekMessage(&msg, nullptr, 0, 0, PM_REMOVE))
		{
			DispatchMessage(&msg);
		}
		else
		{
			if (g_pBlockUsed[*g_pNextUnreadBlock])
			{
				pMessage->dwProcessId = g_pMessages[*g_pNextUnreadBlock].dwProcessId;
				pMessage->hookId = g_pMessages[*g_pNextUnreadBlock].hookId;
				pMessage->nCode = g_pMessages[*g_pNextUnreadBlock].nCode;
				pMessage->wParam = g_pMessages[*g_pNextUnreadBlock].wParam;
				pMessage->lParam = g_pMessages[*g_pNextUnreadBlock].lParam;

				// mark this block as "we've finished reading from it
				g_pBlockUsed[*g_pNextUnreadBlock] = 0;

				(*g_pNextUnreadBlock)++;
				if (*g_pNextUnreadBlock >= g_nBlocks)
				{
					*g_pNextUnreadBlock = 0;
				}

				return WAIT_OBJECT_0;
			}

			Sleep(100);
			if ((dwWaitMilliseconds += 100) >= dwMilliseconds)
			{
				break;
			}
		}

	}
	while (msg.message != WM_QUIT);

	return WAIT_TIMEOUT;
}

// Writes a hook message to shared memory
void WriteToSharedMemory(int32_t hookId, int32_t nCode, WPARAM wParam, LPARAM lParam)
{
	static auto dwProcessId = GetCurrentProcessId();
	auto nextFreeBlock = static_cast<uint32_t>((InterlockedIncrement(g_pNextFreeBlock) - 1) % g_nBlocks); //circular buffer

	g_pMessages[nextFreeBlock].dwProcessId = dwProcessId;
	g_pMessages[nextFreeBlock].hookId = hookId;
	g_pMessages[nextFreeBlock].nCode = nCode;
	g_pMessages[nextFreeBlock].wParam = wParam;
	g_pMessages[nextFreeBlock].lParam = lParam;

	// mark this block as "free to read from"
	g_pBlockUsed[nextFreeBlock] = 1;
}

inline bool IsMappingOpen()
{
	return (g_hFileMapping != nullptr && g_pMapView != nullptr);
}

inline void InitPointers()
{
	g_pBlockUsed = reinterpret_cast<char*>(g_pMapView);
	g_pMessages = reinterpret_cast<HookMessage*>((char*)g_pMapView + g_nBlocks);
	g_pNextFreeBlock = reinterpret_cast<uint32_t volatile*>(g_pMessages + g_nBlocks);
	g_pNextUnreadBlock = const_cast<uint32_t*>(g_pNextFreeBlock + 1);
}

bool NewMapping()
{
	g_hFileMapping = CreateFileMapping(INVALID_HANDLE_VALUE, nullptr, PAGE_READWRITE, 0, g_dwBufferSize, g_SharedMemoryMappingName);

	if (g_hFileMapping != nullptr)
	{
		g_pMapView = MapViewOfFile(g_hFileMapping, FILE_MAP_WRITE, 0, 0, g_dwBufferSize);
		if (g_pMapView != nullptr)
		{
			SecureZeroMemory(g_pMapView, g_dwBufferSize);
			InitPointers();
		}
	}

	return IsMappingOpen();
}

bool OpenMapping()
{
	g_hFileMapping = OpenFileMapping(FILE_MAP_WRITE, FALSE, g_SharedMemoryMappingName);

	if (g_hFileMapping != nullptr)
	{
		g_pMapView = MapViewOfFile(g_hFileMapping, FILE_MAP_WRITE, 0, 0, g_dwBufferSize);
		InitPointers();
	}

	return IsMappingOpen();
}

bool CloseMapping()
{
	if (g_pMapView != nullptr)
	{
		UnmapViewOfFile(g_pMapView);
		g_pMapView = nullptr;
	}

	if (g_hFileMapping != nullptr)
	{
		CloseHandle(g_hFileMapping);
		g_hFileMapping = nullptr;
	}

	return !IsMappingOpen();
}
