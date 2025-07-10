using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.Customs.Base.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.General.Testing
{
	[TestedType(typeof(DocJobComInvoiceLine))]
	sealed class DocJobComInvoiceLineTest : DocBaseJobComInvoiceLineAbstractTest<BaseJobComInvoiceLine, DocJobComInvoiceLine>
	{
		#region Implementation

		protected override string TestingCountry
		{
			get { return Core.Constants.CountryCodes.Fiji; }
		}

		protected override DocJobComInvoiceLine CreateInvoiceLineWrapper(BaseJobComInvoiceLine invoiceLineInternal)
		{
			return DocJobComInvoiceLine.New(invoiceLineInternal, Factory);
		}

		#endregion
	}
}
