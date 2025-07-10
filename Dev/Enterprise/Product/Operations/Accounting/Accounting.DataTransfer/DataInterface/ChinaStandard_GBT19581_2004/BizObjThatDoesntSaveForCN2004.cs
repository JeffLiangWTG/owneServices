using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT19581_2004
{
	public class BizObjThatDoesntSaveForCN2004 : BusinessObjectThatDoesntSaveForCN
	{
		public BizObjThatDoesntSaveForCN2004(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public enum FilesType
		{
			TXT = 0,
			XML = 1
		}

		public FilesType ExportFilesType { get; set; }
		List<ZString> fExportFiles;

		public List<ZString> ExportFiles
		{
			get { return fExportFiles ?? (fExportFiles = new List<ZString>()); }
			set { fExportFiles = value; }
		}
	}
}
