using CargoWise.Common;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class WineCodeDataCollection : CusCodeDataCollection<WineCodeData>
	{
		public WineCodeDataCollection(EMCSJobComInvoiceLine invoiceLine)
			: base(invoiceLine, CusCodeDataTypeList.Codes.WineCode)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly EMCSJobComInvoiceLine invoiceLine;

		protected override bool AllowNewCore => base.AllowNewCore && !(invoiceLine.Declaration?.IsMessageStatusSentOrAcknowledged ?? false);
	}
}
