using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class GoodsShipmentItemDestinationWrapper : IDestination
	{
		GoodsShipmentItemDestinationWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		readonly JobComInvoiceLine invoiceLine;

		public static GoodsShipmentItemDestinationWrapper New(JobComInvoiceLine invoiceLine) => invoiceLine == null || invoiceLine.ZG_CountryOfDestination.IsEmpty ? null : new GoodsShipmentItemDestinationWrapper(invoiceLine);

		public string CcQualifier => invoiceLine.Declaration.JE_CustomsOffice.StartsWith(Core.Constants.CountryCodes.France) ? ZString.Empty : Core.Constants.CountryCodes.France;

		public string CountryOfDestination => countryOfDestination ?? (countryOfDestination = invoiceLine.ZG_CountryOfDestination);
		string countryOfDestination;

		public string RegionOfDestination => regionOfDestination ?? (regionOfDestination = CountryOfDestination == Core.Constants.CountryCodes.France ? (invoiceLine.Declaration.ImporterDocumentaryAddress?.Address?.OA_PostCode.Left(2) ?? ZString.Empty) : ZString.Empty);
		string regionOfDestination;
	}
}
