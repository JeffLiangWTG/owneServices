using CargoWise.Common;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class SplitProvider
	{
		public SplitProvider(EMCSJobComInvoiceHeader emcsJobComInvoiceHeader)
		{
			this.emcsJobComInvoiceHeader = Argument.NotNull(emcsJobComInvoiceHeader, nameof(emcsJobComInvoiceHeader));
		}

		public EMCSJobDeclaration EMCSJobDeclaration => (EMCSJobDeclaration)emcsJobComInvoiceHeader.JobDeclaration;

		protected readonly EMCSJobComInvoiceHeader emcsJobComInvoiceHeader;
	}
}
