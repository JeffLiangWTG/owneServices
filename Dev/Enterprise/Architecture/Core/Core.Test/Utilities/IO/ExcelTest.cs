using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class ExcelTest : TestCase
	{
		public void TestIsExcelInstalledInDebugMode()
		{
			AssertEquals("Default should be true.", true, Excel.IsExcelInstalledDuringTest);
			AssertEquals("Default should be true.", true, Excel.IsExcelInstalled);
		}

		public void TestMaxRowCountInDebugMode()
		{
			AssertEquals("Default should be 65536.", 65536, Excel.MaxRowCountSupported97_2003);
			AssertEquals("Default should be 1048576.", 1048576, Excel.MaxRowCountSupported2007);
		}

		public void TestMaxColCountInDebugMode()
		{
			AssertEquals("Default should be 256.", 256, Excel.MaxColCountSupported97_2003);
			AssertEquals("Default should be 16384.", 16384, Excel.MaxColCountSupported2007);
		}

		public void TestReset()
		{
			Excel.IsExcelInstalledDuringTest = false;
			Excel.MaxRowCountSupported97_2003 = 7;
			Excel.MaxColCountSupported97_2003 = 5;
			Excel.MaxRowCountSupported2007 = 9;
			Excel.MaxColCountSupported2007 = 6;
			AssertEquals("Precondition - ensure we have changed IsExcelInstalledDuringTest.", false, Excel.IsExcelInstalledDuringTest);
			AssertEquals("Precondition - ensure we have changed the MaxRowCountSupported97_2003.", 7, Excel.MaxRowCountSupported97_2003);
			AssertEquals("Precondition - ensure we have changed the MaxColCountSupported97_2003.", 5, Excel.MaxColCountSupported97_2003);
			AssertEquals("Precondition - ensure we have changed the MaxRowCountSupported2007.", 9, Excel.MaxRowCountSupported2007);
			AssertEquals("Precondition - ensure we have changed the MaxColCountSupported2007.", 6, Excel.MaxColCountSupported2007);

			Excel.Reset();
			AssertEquals("Default should be true.", true, Excel.IsExcelInstalledDuringTest);
			AssertEquals("Default should be 65536.", 65536, Excel.MaxRowCountSupported97_2003);
			AssertEquals("Default should be 256.", 256, Excel.MaxColCountSupported97_2003);
			AssertEquals("Default should be 1048576.", 1048576, Excel.MaxRowCountSupported2007);
			AssertEquals("Default should be 16384.", 16384, Excel.MaxColCountSupported2007);
		}
	}
}
