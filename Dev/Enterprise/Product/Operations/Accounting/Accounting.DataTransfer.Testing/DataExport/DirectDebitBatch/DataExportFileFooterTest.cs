using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportFileFooter))]
	internal class DataExportFileFooterTest : DataExportDirectDebitBatchTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportFileFooter(Factory, DirectDebitBatchHeader);
		}

		#endregion
	}
}
