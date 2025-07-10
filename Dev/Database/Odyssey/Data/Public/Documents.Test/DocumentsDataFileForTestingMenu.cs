using System.Data;
using System.IO;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Data.Testing
{
	/// <summary>
	/// Allows developers to reload Documents.xml into the database even though it's not embedded into the ExcelTemplates DLL. 
	/// </summary>
	public class DocumentsDataFileForTestingMenu : DocumentsCompleteDataFile
	{
		public DocumentsDataFileForTestingMenu(string absoluteFilePath)
			: base()
		{
			this.absoluteFilePath = absoluteFilePath;
		}

		public static string DocumentsCompleteXmlDefaultFilePath
		{
			get { return Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\ExcelTemplates\DbUpgrader.Data\Documents\DocumentsComplete.xml"); }
		}

		protected override DataSet LoadDataSet()
		{
			return LoadFromXml(absoluteFilePath);
		}

		readonly string absoluteFilePath;
	}
}
