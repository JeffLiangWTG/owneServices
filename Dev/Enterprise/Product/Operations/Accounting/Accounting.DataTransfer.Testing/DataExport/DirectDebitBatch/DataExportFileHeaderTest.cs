using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportFileHeader))]
	internal class DataExportFileHeaderTest : DataExportDirectDebitBatchTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportFileHeader(Factory, DirectDebitBatchHeader);
		}

		#endregion
	}
}
