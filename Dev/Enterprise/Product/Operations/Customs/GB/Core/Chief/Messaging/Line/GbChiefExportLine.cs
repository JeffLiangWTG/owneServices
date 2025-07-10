using CargoWise.Types;
using Enterprise.Customs.EU.Integration.SadH;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.Chief.Messaging
{
	class GbChiefExportLine : GbChiefLine, IExportLine
	{
		public GbChiefExportLine(GbChiefExportHeader header, CusEntryLine actualEntryLine)
			: base(header, actualEntryLine)
		{
		}

		public ZString CountryOfDestinationCode
		{
			get { return header.ConvertFromUnToChiefCountry(randomLine.ZG_CountryOfDestination); }
		}

		public ZBool FECCountryOfDestination
		{
			get { return randomLine.ZG_FecDST; }
		}

		public ZString TransportChargesMethodOfPayment => actualEntryLine.TransportChargesMethodOfPayment;
	}
}
