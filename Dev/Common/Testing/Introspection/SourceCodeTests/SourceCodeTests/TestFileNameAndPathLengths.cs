using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace CargoWise.Common.SourceCode.Testing;

sealed class TestFileNameAndPathLengths : TestCase
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	public void TestFileNameAndPathLengthsAreWithinWindowsLimitations()
	{
		var filesThatAreTooLong = "";
		var directoriesThatAreTooLong = "";

		var basePathLength = BaseSourcePath.Length;
		var root = BaseSourcePath;
		if (root.EndsWith("\\"))
		{
			root = root.Remove(root.Length - 1);
		}

		var dirs = new List<string>();
		var files = new List<string>();
		Win32API.EnumerateDirectory(root, dirs, files);

		AssertEquals(true, dirs.Any());
		AssertEquals(true, files.Any());

		directoriesThatAreTooLong = string.Join("\r\n", dirs.Where(x => x.Length - basePathLength > MaxDirectoryLength));
		filesThatAreTooLong = string.Join("\r\n", files.Where(x => x.Length - basePathLength > MaxFileNameLength));

		CombineAssertions(() =>
		{
			AssertEquals("There should be no files too long", string.Empty, filesThatAreTooLong);
			AssertEquals("There should be no directories too long", string.Empty, directoriesThatAreTooLong);
		});
	}

	static class Win32API
	{
		enum FINDEX_INFO_LEVELS
		{
			FindExInfoStandard = 0,
			FindExInfoBasic = 1
		}
		enum FINDEX_SEARCH_OPS
		{
			FindExSearchNameMatch = 0,
			FindExSearchLimitToDirectories = 1,
			FindExSearchLimitToDevices = 2
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		struct WIN32_FIND_DATA
		{
			public uint dwFileAttributes;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
			public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
			public uint nFileSizeHigh;
			public uint nFileSizeLow;
			public uint dwReserved0;
			public uint dwReserved1;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
			public string cFileName;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
			public string cAlternateFileName;
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		static extern IntPtr FindFirstFileEx(
			string lpFileName,
			FINDEX_INFO_LEVELS fInfoLevelId,
			out WIN32_FIND_DATA lpFindFileData,
			FINDEX_SEARCH_OPS fSearchOp,
			IntPtr lpSearchFilter,
			int dwAdditionalFlags);

		[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
		static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("Kernel32.dll", EntryPoint = "FindClose", SetLastError = true)]
		static extern bool FindClose(IntPtr hFindFile);

		const uint FILE_ATTRIBUTE_DIRECTORY = 16;
		const uint FILE_ATTRIBUTE_REPARSE_POINT = 1024;
		const int FIND_FIRST_EX_LARGE_FETCH = 2;
		static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
		const int ERROR_NO_MORE_FILES = 18;

		public static void EnumerateDirectory(string root, List<string> dirs, List<string> files)
		{
			var fn = root + "\\*";
			var hFile = FindFirstFileEx(
					fn,
					FINDEX_INFO_LEVELS.FindExInfoBasic,
					out var findData,
					FINDEX_SEARCH_OPS.FindExSearchNameMatch,
					IntPtr.Zero,
					FIND_FIRST_EX_LARGE_FETCH);

			if (hFile != INVALID_HANDLE_VALUE)
			{
				for (; ; )
				{
					if (findData.cFileName != "." && findData.cFileName != ".." && (findData.dwFileAttributes & FILE_ATTRIBUTE_REPARSE_POINT) == 0)
					{
						var fName = $@"{root}\{findData.cFileName}";

						if ((findData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY) != 0)
						{
							dirs.Add(fName);
							EnumerateDirectory(fName, dirs, files);
						}
						else
						{
							files.Add(fName);
						}
					}

					if (!FindNextFile(hFile, out findData))
					{
						var lastErr = Marshal.GetLastWin32Error();
						if (lastErr == ERROR_NO_MORE_FILES)
						{
							break;
						}
						else
						{
							throw new Win32Exception(lastErr, fn);
						}
					}
				}

				if (!FindClose(hFile))
				{
					throw new Win32Exception(Marshal.GetLastWin32Error(), fn);
				}
			}
			else
			{
				throw new Win32Exception(Marshal.GetLastWin32Error(), fn);
			}
		}
	}

	const int BufferForOurLocalWorkspacePath = 20;
	const int WindowsLimitForDirectoryLength = 248;
	const int WindowsLimitForFileNameLength = 260;

	const int MaxDirectoryLength = WindowsLimitForDirectoryLength - BufferForOurLocalWorkspacePath;
	const int MaxFileNameLength = WindowsLimitForFileNameLength - BufferForOurLocalWorkspacePath;
}
