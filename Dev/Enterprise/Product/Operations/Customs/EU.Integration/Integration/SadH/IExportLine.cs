using CargoWise.Types;

namespace Enterprise.Customs.EU.Integration.SadH
{
	public interface IExportLine : ILine
	{
		ZString CountryOfDestinationCode { get; } // ITEM-DEST-CNTRY
		ZBool FECCountryOfDestination { get; } // ITEM-PROC-INST "DST"
		ZString TransportChargesMethodOfPayment { get; } // I-TRPT-CHGE-MOP
	}
}
