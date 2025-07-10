using System;
using System.Runtime.InteropServices;
using CargoWise.Common;

namespace CargoWise.Loader.Common
{
	public static class AdministratorChecker
	{
#if DEBUG
		public static IDisposable OverrideForTest(bool mockResult)
		{
			MockResult = mockResult;
			Mocking = true;
			return new MockResetter();
		}

		class MockResetter : IDisposable
		{
			public void Dispose()
			{
				Mocking = false;
			}
		}

		[ThreadStatic]
		static bool Mocking;
		[ThreadStatic]
		static bool MockResult;
#endif

		public static bool UserIsAdministrator()
		{
#if DEBUG
			if (Mocking)
			{
				return MockResult;
			}
#endif
			bool result = false;
			SID_IDENTIFIER_AUTHORITY ntAuthority = new SID_IDENTIFIER_AUTHORITY(new byte[] { 0, 0, 0, 0, 0, 5 }); // SECURITY_NT_AUTHORITY
			IntPtr administratorGroupSid;
			if (AllocateAndInitializeSid(ref ntAuthority, 2, SECURITY_BUILTIN_DOMAIN_RID, DOMAIN_ALIAS_RID_ADMINS, 0, 0, 0, 0, 0, 0, out administratorGroupSid))
			{
				if (!CheckTokenMembership(IntPtr.Zero, administratorGroupSid, out result))
				{
					result = false;
				}
				FreeSid(administratorGroupSid);
			}
			return result;
		}

		[StructLayout(LayoutKind.Sequential)]
		struct SID_IDENTIFIER_AUTHORITY
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			readonly byte[] value;

			public SID_IDENTIFIER_AUTHORITY(byte[] value)
			{
				Argument.NotNull(value, nameof(value));
				if (value.Length != 6)
				{
					throw new ArgumentException("SID_IDENTIFIER_AUTHORITY must be 6 bytes", nameof(value));
				}

				this.value = new byte[6];
				Array.Copy(value, this.value, 6);
			}
		}

		const int SECURITY_BUILTIN_DOMAIN_RID = 0x00000020;
		const int DOMAIN_ALIAS_RID_ADMINS = 0x00000220;

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern bool AllocateAndInitializeSid(
			ref SID_IDENTIFIER_AUTHORITY pIdentifierAuthority,
			byte nSubAuthorityCount,
			int nSubAuthority0,
			int nSubAuthority1,
			int nSubAuthority2,
			int nSubAuthority3,
			int nSubAuthority4,
			int nSubAuthority5,
			int nSubAuthority6,
			int nSubAuthority7,
			out IntPtr pSid
			);

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern IntPtr FreeSid(IntPtr pSid);

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern bool CheckTokenMembership(
			IntPtr tokenHandle,
			IntPtr sidToCheck,
			out bool isMember
			);
	}
}
