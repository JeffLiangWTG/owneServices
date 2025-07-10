using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.DataTransfer.DataInterface.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004.Testing
{
	[TestedType(typeof(BizObjThatDoesntSaveForCN2004))]
	class BizObjThatDoesntSaveForCN2004Test : BusinessObjectThatDoesntSaveForCNTest
	{
		public void TestExportFilesType()
		{
			var bizo = new BizObjThatDoesntSaveForCN2004(Factory) { ExportFilesType = BizObjThatDoesntSaveForCN2004.FilesType.TXT };
			AssertEquals("ExportFilesType", BizObjThatDoesntSaveForCN2004.FilesType.TXT, bizo.ExportFilesType);
		}

		[TestDate(2013, 03, 17)]
		public void TestExportExportFiles()
		{
			var exportFiles = new List<ZString> { "a", "b" };

			var bizo = new BizObjThatDoesntSaveForCN2004(Factory) { ExportFiles = exportFiles };
			AssertEquals("ExportFiles", "a", bizo.ExportFiles[0]);

			bizo.Period = 200909;
			AssertEquals("ExportFiles", "b", bizo.ExportFiles[1]);
		}
	}
}
