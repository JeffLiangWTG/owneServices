using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business
{
	public class WineProductProvider : IWineProduct
	{
		public WineProductProvider(EMCSJobComInvoiceLine emcsInvoiceLine)
		{
			this.emcsInvoiceLine = Argument.NotNull(emcsInvoiceLine, nameof(emcsInvoiceLine));
		}
		readonly EMCSJobComInvoiceLine emcsInvoiceLine;

		public string GrowingZoneCode => emcsInvoiceLine.ZG_GrowingZone;

		public string ProductCategory => emcsInvoiceLine.ZG_WineCategory;

		public string ThirdCountryOfOrigin => emcsInvoiceLine.ZG_WineCountryOrigin;

		public ITextAndLanguage OtherInformation => otherInformation ?? (otherInformation = new TextAndLanguageProvider(emcsInvoiceLine.JI_WineDetailsComments));
		ITextAndLanguage otherInformation;

		public IReadOnlyCollection<string> OperationCodes => operationCodes ?? (operationCodes = emcsInvoiceLine.OperationCodeDataCollection.Cast<WineCodeData>().Select(x => x.CY_Code.ToString()).ToArray());
		IReadOnlyCollection<string> operationCodes;
	}
}
