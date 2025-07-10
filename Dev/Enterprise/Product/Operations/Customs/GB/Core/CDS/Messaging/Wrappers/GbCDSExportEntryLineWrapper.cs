using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers
{
	public class GbCDSExportEntryLineWrapper : GbCDSImportEntryLineWrapper
	{
		readonly JobComInvoiceLine randomLine;

		public GbCDSExportEntryLineWrapper(CusEntryLine entryLine) : base(entryLine)
		{
			randomLine = entryLine.RandomLine;
		}

		protected override ZShort DescriptionMaxLength => 280;

		protected override IEnumerable<ICountry> GetOrigins()
		{
			var countryOfOrigin = ImportLine.CountryOfOriginCode;
			if (!countryOfOrigin.IsEmpty)
			{
				yield return CountryWrapper.New(countryOfOrigin, Constants.OriginTypeCodes.NonPreferential);
			}
		}

		protected override IEnumerable<IClassification> GetClassifications()
		{
			var commodityAndTaric = randomLine.JI_FormattedTariff.KeepAlphanumericCharacters();
			if (!commodityAndTaric.IsEmpty)
			{
				var tsp = commodityAndTaric.SubstringSafe(0, 8);
				if (!tsp.IsEmpty)
				{
					yield return ClassificationWrapper.New(tsp, Constants.Classification.IdentificationTypeCodes.TSP);
				}
			}

			var supplementaryCodes = GetSupplementaryCodes();
			foreach (var supplementaryCode in supplementaryCodes)
			{
				yield return supplementaryCode;
			}
		}
	}
}
