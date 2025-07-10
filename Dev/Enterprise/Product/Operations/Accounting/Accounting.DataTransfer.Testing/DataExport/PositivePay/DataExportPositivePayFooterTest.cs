using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportPositivePayFooter))]
	internal class DataExportPositivePayFooterTest : DataExportPositivePayTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportPositivePayFooter(Factory, TransactionHeaders, BankAccount);
		}

		#endregion
	}
}
