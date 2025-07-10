using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.DE.EMCS.Business
{
	public class WineProductProvider : IWineProduct
	{
		public WineProductProvider(EMCSJobComInvoiceLine emcsInvoiceLine)
		{
			this.emcsInvoiceLine = Argument.NotNull(emcsInvoiceLine, nameof(emcsInvoiceLine));
			helper = new WineProductProviderHelper(emcsInvoiceLine);
		}
		readonly EMCSJobComInvoiceLine emcsInvoiceLine;
		readonly WineProductProviderHelper helper;

		public string GrowingZoneCode => helper.GrowingZoneCode;

		public string ProductCategory => helper.ProductCategory;

		public string ThirdCountryOfOrigin => helper.ThirdCountryOfOrigin;

		public ITextAndLanguage OtherInformation => otherInformation ?? (otherInformation = new TextAndLanguageProvider(emcsInvoiceLine.JI_WineDetailsComments));
		ITextAndLanguage otherInformation;

		public IReadOnlyCollection<string> OperationCodes => helper.OperationCodes;
	}
}
