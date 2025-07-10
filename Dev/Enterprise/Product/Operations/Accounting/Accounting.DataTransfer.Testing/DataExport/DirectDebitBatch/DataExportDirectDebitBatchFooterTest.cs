using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportDirectDebitBatchFooter))]
	internal class DataExportDirectDebitBatchFooterTest : DataExportDirectDebitBatchTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportDirectDebitBatchFooter(Factory, DirectDebitBatchHeader);
		}

		#endregion
	}
}
