using System;
using System.Runtime.InteropServices;
using CargoWise.Common;
using CargoWise.Loader.Common.Native;

namespace CargoWise.Loader.Common
{
	public sealed class UserAccountControl
	{
		public const string RegistryKeyName = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System";
		public const string RegistryValueName = "EnableLUA";
		IServiceContainer services;

		public UserAccountControl()
			: this(new VersionHelper())
		{
		}

		public UserAccountControl(VersionHelper versionHelper)
		{
			Argument.NotNull(versionHelper, nameof(versionHelper));
			this.versionHelper = versionHelper;
		}

		public bool CanElevate
		{
			get
			{
				bool result = IsVistaOrLater;
				if (result)
				{
					TOKEN_ELEVATION_TYPE? elevationType = GetElevationType();
					result = elevationType.HasValue ? (elevationType.Value == TOKEN_ELEVATION_TYPE.TokenElevationTypeLimited) : IsUacEnabledInRegistry;
				}
				return result;
			}
		}

		public bool ElevationRequired
		{
			get { return !AdministratorChecker.UserIsAdministrator() && CanElevate; }
		}

		public IServiceContainer Services
		{
			get
			{
				return services ?? ServiceContainer.Instance;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException(nameof(value));
				}
				services = value;
			}
		}

		bool IsUacEnabledInRegistry
		{
			get
			{
				object value = Services.Registry.GetValue(RegistryKeyName, RegistryValueName, null);
				return (value is int) && ((int)value == 1);
			}
		}

		bool IsVistaOrLater
		{
			get { return versionHelper.IsWindowsVistaOrGreater(); }
		}

		TOKEN_ELEVATION_TYPE? GetElevationType()
		{
			IntPtr process = Services.NativeMethods.GetCurrentProcess();
			if (process != IntPtr.Zero)
			{
				int processTokenValue;
				if (Services.NativeMethods.OpenProcessToken(process, NativeMethods.TOKEN_QUERY, out processTokenValue))
				{
					IntPtr processToken = new IntPtr(processTokenValue);
					try
					{
						uint returnLength;
						uint elevationTypeSize = (uint)Marshal.SizeOf(Enum.GetUnderlyingType(typeof(TOKEN_ELEVATION_TYPE)));
						IntPtr elevationTypeToken = Marshal.AllocHGlobal((int)elevationTypeSize);
						try
						{
							if (Services.NativeMethods.GetTokenInformation(
								processToken,
								TOKEN_INFORMATION_CLASS.TokenElevationType,
								elevationTypeToken,
								elevationTypeSize,
								out returnLength))
							{
								if (elevationTypeSize == returnLength)
								{
									return (TOKEN_ELEVATION_TYPE)Marshal.ReadInt32(elevationTypeToken);
								}
							}
						}
						finally
						{
							Marshal.FreeHGlobal(elevationTypeToken);
						}
					}
					finally
					{
						Services.NativeMethods.CloseHandle(processToken);
					}
				}
			}
			return null;
		}

		readonly VersionHelper versionHelper;
	}
}
