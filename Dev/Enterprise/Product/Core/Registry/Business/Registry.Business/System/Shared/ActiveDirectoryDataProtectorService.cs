using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace Enterprise.Registry.Business
{
	internal class ActiveDirectoryDataProtectorService : IDataProtectorService
	{
		internal string Descriptor { get; }

		public ActiveDirectoryDataProtectorService(SecurityIdentifier sid)
		{
			Descriptor = $"SID={sid?.Value ?? throw new ArgumentNullException(nameof(sid))}";
		}

		public string Encrypt(string textToEncrypt)
		{
			if (string.IsNullOrEmpty(textToEncrypt))
			{
				throw new ArgumentException("Text to encrypt cannot be null or empty.", nameof(textToEncrypt));
			}

			var result = NativeMethods.NCryptCreateProtectionDescriptor(Descriptor, 0, out var hDescriptor);
			using (hDescriptor)
			{
				if (result != 0)
				{
					var inner = new Win32Exception(result);
					throw new CryptographicException($"Failed to create protection descriptor '{Descriptor}'. HResult: {result:X}", inner);
				}

				var data = Encoding.UTF8.GetBytes(textToEncrypt);
				result = NativeMethods.NCryptProtectSecret(hDescriptor.DangerousGetHandle(), NcryptSilentFlag, data, (uint)data.Length, IntPtr.Zero, IntPtr.Zero, out var encryptedPtr, out var encryptedSize);
				using (encryptedPtr)
				{
					if (result != 0)
					{
						var inner = new Win32Exception(result);
						throw new CryptographicException($"Failed to protect secret for '{Descriptor}'. HResult: {result:X}", inner);
					}

					if (encryptedSize > int.MaxValue)
					{
						throw new CryptographicException($"Encrypted data size {encryptedSize} is too large for '{Descriptor}'.");
					}

					var encryptedData = new byte[encryptedSize];
					Marshal.Copy(encryptedPtr.DangerousGetHandle(), encryptedData, 0, (int)encryptedSize);

					return Convert.ToBase64String(encryptedData);
				}
			}
		}

		public string Decrypt(string encryptedString)
		{
			if (string.IsNullOrEmpty(encryptedString))
			{
				throw new ArgumentException("Text to decrypt cannot be null or empty.", nameof(encryptedString));
			}

			try
			{
				var encryptedData = Convert.FromBase64String(encryptedString);
				var result = NativeMethods.NCryptUnprotectSecret(IntPtr.Zero, NcryptSilentFlag, encryptedData, (uint)encryptedData.Length, IntPtr.Zero, IntPtr.Zero, out var decryptedPtr, out var decryptedSize);
				using (decryptedPtr)
				{
					if (result != 0)
					{
						var inner = new Win32Exception(result);
						throw new CryptographicException($"Failed to unprotect secret for '{Descriptor}'. HResult: {result:X}", inner);
					}

					if (decryptedSize > int.MaxValue)
					{
						throw new CryptographicException($"Decrypted data size {decryptedSize} is too large for '{Descriptor}'.");
					}

					var decryptedData = new byte[decryptedSize];
					Marshal.Copy(decryptedPtr.DangerousGetHandle(), decryptedData, 0, (int)decryptedSize);

					return Encoding.UTF8.GetString(decryptedData);
				}
			}
			catch (FormatException e)
			{
				throw new CryptographicException($"Failed to decode Base64 string for '{Descriptor}'.", e);
			}
		}

		internal class LocalAllocHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			// Called by P/Invoke when returning SafeHandles
			public LocalAllocHandle()
				: base(ownsHandle: true) { }

			// Do not provide a finalizer - SafeHandle's critical finalizer will call ReleaseHandle for you.
			protected override bool ReleaseHandle()
			{
				Marshal.FreeHGlobal(handle); // actually calls LocalFree
				return true;
			}
		}

		internal sealed class NCryptDescriptorHandle : SafeHandleZeroOrMinusOneIsInvalid
		{
			// Called by P/Invoke when returning SafeHandles
			public NCryptDescriptorHandle()
				: base(ownsHandle: true)
			{
			}

			// Do not provide a finalizer - SafeHandle's critical finalizer will call ReleaseHandle for you.
			protected override bool ReleaseHandle()
			{
				return (NativeMethods.NCryptCloseProtectionDescriptor(handle) == 0);
			}
		}

		internal class NativeMethods
		{
			[DllImport("ncrypt.dll", SetLastError = true, CharSet = CharSet.Unicode)]
			[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
			internal static extern int NCryptCreateProtectionDescriptor(
				string pwszDescriptorString,
				uint dwFlags,
				out NCryptDescriptorHandle phDescriptor);

			[DllImport("ncrypt.dll", SetLastError = true)]
			[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
			internal static extern int NCryptProtectSecret(
				IntPtr hDescriptor,
				uint dwFlags,
				byte[] pbData,
				uint cbData,
				IntPtr pMemPara,
				IntPtr hWnd,
				out LocalAllocHandle ppbProtectedBlob,
				out uint pcbResult);

			[DllImport("ncrypt.dll", SetLastError = true)]
			[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
			internal static extern int NCryptUnprotectSecret(
				IntPtr hDescriptor,
				uint dwFlags,
				byte[] pbProtectedBlob,
				uint cbProtectedBlob,
				IntPtr pMemPara,
				IntPtr hWnd,
				out LocalAllocHandle ppbData,
				out uint pcbData);

			[DllImport("ncrypt.dll", SetLastError = true)]
			[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
			internal static extern int NCryptCloseProtectionDescriptor(IntPtr hDescriptor);
		}

		const uint NcryptSilentFlag = 0x00000040;
	}
}
