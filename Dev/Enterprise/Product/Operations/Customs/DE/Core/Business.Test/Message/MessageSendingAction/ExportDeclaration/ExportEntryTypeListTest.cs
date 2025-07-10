using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	class ExportEntryTypeListTest : TestCase
	{
		public void TestIsCancellation()
		{
			CombineAssertions(() =>
			{
				foreach (var code in new ExportEntryTypeList().GetAllCodes())
				{
					AssertEquals($"IsCancellation({code})", code == ExportEntryTypeList.Codes.CancellationRequest, ExportEntryTypeList.IsCancellation(code));
				}
			});
		}

		public void TestIsExitToExport()
		{
			CombineAssertions(() =>
			{
				foreach (var code in new ExportEntryTypeList().GetAllCodes())
				{
					AssertEquals($"IsExitToExport({code})", code == ExportEntryTypeList.Codes.ExitToExport, ExportEntryTypeList.IsExitToExport(code));
				}
			});
		}

		public void TestIsCancellationOrExitToExport()
		{
			CombineAssertions(() =>
			{
				foreach (var code in new ExportEntryTypeList().GetAllCodes())
				{
					AssertEquals($"IsCancellationOrExitToExport({code})", code == ExportEntryTypeList.Codes.CancellationRequest || code == ExportEntryTypeList.Codes.ExitToExport, ExportEntryTypeList.IsCancellationOrExitToExport(code));
				}
			});
		}

		public void TestIsExportDeclaration()
		{
			CombineAssertions(() =>
			{
				foreach (var code in new ExportEntryTypeList().GetAllCodes())
				{
					AssertEquals($"IsExportDeclaration({code})", code == ExportEntryTypeList.Codes.ExportDeclaration, ExportEntryTypeList.IsExportDeclaration(code));
				}
			});
		}

		public void TestIsExportAmendment()
		{
			CombineAssertions(() =>
			{
				foreach (var code in new ExportEntryTypeList().GetAllCodes())
				{
					AssertEquals($"IsExportAmendment({code})", code == ExportEntryTypeList.Codes.ExportAmendment, ExportEntryTypeList.IsExportAmendment(code));
				}
			});
		}

		public void TestIsSupplementaryExportDeclaration()
		{
			CombineAssertions(() =>
			{
				foreach (var code in new ExportEntryTypeList().GetAllCodes())
				{
					AssertEquals($"IsSupplementaryExportDeclaration({code})", code == ExportEntryTypeList.Codes.SupplementaryExportDeclaration, ExportEntryTypeList.IsSupplementaryExportDeclaration(code));
				}
			});
		}

		public void TestIsExportAmendmentOrSupplementaryExportDeclaration()
		{
			CombineAssertions(() =>
			{
				foreach (var code in new ExportEntryTypeList().GetAllCodes())
				{
					AssertEquals($"IsExportAmendmentOrSupplementaryExportDeclaration({code})", code == ExportEntryTypeList.Codes.ExportAmendment || code == ExportEntryTypeList.Codes.SupplementaryExportDeclaration, ExportEntryTypeList.IsExportAmendmentOrSupplementaryExportDeclaration(code));
				}
			});
		}
	}
}
