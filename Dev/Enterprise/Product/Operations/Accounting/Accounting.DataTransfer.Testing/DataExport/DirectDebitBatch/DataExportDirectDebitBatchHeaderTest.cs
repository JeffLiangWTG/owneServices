using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportDirectDebitBatchHeader))]
	internal class DataExportDirectDebitBatchHeaderTest : DataExportDirectDebitBatchTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportDirectDebitBatchHeader(Factory, DirectDebitBatchHeader);
		}

		#endregion
	}
}
