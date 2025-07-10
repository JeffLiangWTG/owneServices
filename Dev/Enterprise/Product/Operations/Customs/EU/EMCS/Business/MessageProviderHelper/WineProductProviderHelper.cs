using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class WineProductProviderHelper
	{
		public WineProductProviderHelper(EMCSJobComInvoiceLine emcsInvoiceLine)
		{
			this.emcsInvoiceLine = emcsInvoiceLine;
		}
		readonly EMCSJobComInvoiceLine emcsInvoiceLine;

		public string GrowingZoneCode => emcsInvoiceLine.ZG_GrowingZone;

		public string ProductCategory => emcsInvoiceLine.ZG_WineCategory;

		public string ThirdCountryOfOrigin => emcsInvoiceLine.ZG_WineCountryOrigin;

		public IReadOnlyCollection<string> OperationCodes => operationCodes ?? (operationCodes = emcsInvoiceLine.OperationCodeDataCollection.Cast<WineCodeData>().Select(x => x.CY_Code.ToString()).ToArray());
		IReadOnlyCollection<string> operationCodes;
	}
}
