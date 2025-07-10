using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.DataExport.Testing
{
	[TestedType(typeof(DataExportPaymentFooter))]
	internal class DataExportPaymentFooterTest : DataExportPaymentTest
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DataExportPaymentFooter(Factory, Payment);
		}

		protected override BusinessObject GetNewDirectPayment()
		{
			return new DataExportPaymentFooter(Factory, DirectPayment);
		}

		#endregion
	}
}
