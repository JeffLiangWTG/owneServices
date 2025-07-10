using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class OriginWrapper : IOrigin
	{
		OriginWrapper(JobComInvoiceLine line)
		{
			this.line = Argument.NotNull(line, nameof(line));
		}

		public static OriginWrapper New(JobComInvoiceLine line) => line == null ? null : new OriginWrapper(line);

		public string CountryOfOrigin => countryOfOrigin ?? (countryOfOrigin = line.JI_CountryOfOrigin);
		string countryOfOrigin;

		public string CountryOfPreferentialOrigin => countryOfPreferentialOrigin ?? (countryOfPreferentialOrigin = line.ZG_CountryOfSupply);
		string countryOfPreferentialOrigin;

		readonly JobComInvoiceLine line;
	}
}
