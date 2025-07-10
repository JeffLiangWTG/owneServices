using System;
using CargoWise.Common.Testing;

namespace Enterprise.ZArchitecture.Environment
{
	public static class Excel
	{
#if DEBUG
		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		public static bool IsExcelInstalled
		{
			get { return (Globals.IsTest) ? IsExcelInstalledDuringTest : Type.GetTypeFromProgID("Excel.Application") != null; }
		}

		[SuppressThreadStaticFieldMessage]
		public static bool IsExcelInstalledDuringTest = true;

		[SuppressThreadStaticFieldMessage]
		public static int MaxRowCountSupported97_2003 = 65536;
		[SuppressThreadStaticFieldMessage]
		public static int MaxColCountSupported97_2003 = 256;

		[SuppressThreadStaticFieldMessage]
		public static int MaxRowCountSupported2007 = 1048576;
		[SuppressThreadStaticFieldMessage]
		public static int MaxColCountSupported2007 = 16384;

		public static void Reset()
		{
			IsExcelInstalledDuringTest = true;
			MaxRowCountSupported97_2003 = 65536;
			MaxColCountSupported97_2003 = 256;
			MaxRowCountSupported2007 = 1048576;
			MaxColCountSupported2007 = 16384;
		}
#else
		[System.Diagnostics.CodeAnalysis.SuppressMessage("ApiDesign", "RS0030:Do not used banned APIs", Justification = "Baseline")]
		public static bool IsExcelInstalled 
		{ 
			get { return Type.GetTypeFromProgID("Excel.Application") != null; } 
		}

		public const int MaxRowCountSupported97_2003 = 65536;
		public const int MaxColCountSupported97_2003 = 256;
		public const int MaxRowCountSupported2007 = 1048576;
		public const int MaxColCountSupported2007 = 16384;
#endif

	}
}
