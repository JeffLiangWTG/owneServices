using System;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Windows.UI;
using Enterprise.Dat.Implementation.Preconditions;
using NUnit.Framework;

namespace Enterprise.Dat.Implementation
{
	static class InstalledSoftwareDetection
	{
		public static RequiredSoftware GetInstalledSoftware()
		{
			var result = new RequiredSoftware();
			if (IsResolutionHighEnoughForGuiTests && IsStandardDpi)
			{
				result |= RequiredSoftware.CanRunGUITests;
			}
			if (IsXpsPrinterInstalled)
			{
				result |= RequiredSoftware.XpsPrinter;
			}
			if (IsPowerBi)
			{
				result |= RequiredSoftware.PowerBi;
			}
			if (OlapServerChecker.IsRunning())
			{
				result |= RequiredSoftware.OlapServer;
			}
			if (IsSqlServerSpatial110)
			{
				result |= RequiredSoftware.SqlServerSpatial110;
			}
			if (IsSsasTabular2016OrLater)
			{
				result |= RequiredSoftware.SsasTabular2016OrLater;
			}
			if (IsMsOledbSqlProviderInstalled)
			{
				result |= RequiredSoftware.MsOledbSqlProvider;
			}

			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1063:DoNotUseSystemWindowsFormsScreen", Justification = "Baseline")]
		static bool IsResolutionHighEnoughForGuiTests
		{
			get { return Screen.AllScreens.All(screen => screen.WorkingArea.Height >= 768 && screen.WorkingArea.Width >= 1366); }
		}

		static bool IsStandardDpi
		{
			get { return (int)ControlDpiScalingHelper.DpiX == ControlDpiScalingHelper.BaseDpiX && (int)ControlDpiScalingHelper.DpiY == ControlDpiScalingHelper.BaseDpiY; }
		}

		static bool IsXpsPrinterInstalled
		{
			get
			{
				bool result = false;
				try
				{
					PrinterSettings settings = new PrinterSettings();
					settings.PrinterName = "Microsoft XPS Document Writer";
					result = settings.IsValid;
				}
				catch (InvalidPrinterException)
				{
				}
				return result;
			}
		}

		internal static bool IsVm
		{
			get
			{
				return new Regex(@"-DAT\d").IsMatch(System.Environment.MachineName);
			}
		}

		static bool IsPowerBi
		{
			get
			{
				try
				{
					if (!PowerBiCheck.IsPowerBiInstalledLocally())
					{
						return false;
					}
					if (!PowerBiCheck.IsPowerBiWorking())
					{
						return false;
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return false; //Just to avoid any crash
				}
				return true;
			}
		}

		static bool IsSqlServerSpatial110
		{
			get
			{
				return (!System.Environment.Is64BitOperatingSystem || File.Exists(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.SystemX86), "SqlServerSpatial110.dll"))
					&& File.Exists(Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.System), "SqlServerSpatial110.dll")));
			}
		}

		static bool IsSsasTabular2016OrLater
		{
			get
			{
				return SsasCheck.IsSsasMeetsRequirement();
			}
		}

		static bool IsMsOledbSqlProviderInstalled
		{
			get
			{
				return MsOledbSqlProviderChecker.IsMsOledbSqlProviderInstalled();
			}
		}
	}
}
