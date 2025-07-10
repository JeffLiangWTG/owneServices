using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportPaymentHeader))]
	internal class DataExportPaymentHeaderTest : DataExportPaymentTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportPaymentHeader(Factory, Payment);
		}

		protected override BusinessObject GetNewDirectPayment()
		{
			return new DataExportPaymentHeader(Factory, DirectPayment);
		}

		#endregion
	}
}
