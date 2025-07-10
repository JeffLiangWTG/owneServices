using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class AESCommonOriginWrapper : IAESCommonOrigin
	{
		public AESCommonOriginWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		public ZString CountryOfOrigin => invoiceLine.JI_CountryOfOrigin;

		public ZString StateOfOrigin => invoiceLine.JI_StateOrRegionOfOrigin;
	}
}
