using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportPositivePayHeader))]
	internal class DataExportPositivePayHeaderTest : DataExportPositivePayTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportPositivePayHeader(Factory, TransactionHeaders, BankAccount);
		}

		#endregion
	}
}
