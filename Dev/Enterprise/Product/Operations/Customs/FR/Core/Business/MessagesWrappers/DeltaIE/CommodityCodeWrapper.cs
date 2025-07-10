using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class CommodityCodeWrapper : ICommodityCode
	{
		CommodityCodeWrapper(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		public static CommodityCodeWrapper New(JobComInvoiceLine jobComInvoiceLine) => jobComInvoiceLine == null ? null : new CommodityCodeWrapper(jobComInvoiceLine);

		public string CombinedNomenclatureCode => combinedNomenclatureCode ?? (combinedNomenclatureCode = invoiceLine.JI_Tariff.SubstringSafe(6, 2));
		string combinedNomenclatureCode;

		public string HarmonizedSystemSubheadingCode => harmonizedSystemSubheadingCode ?? (harmonizedSystemSubheadingCode = invoiceLine.JI_Tariff.Left(6));
		string harmonizedSystemSubheadingCode;

		public ICollection<INationalAdditionalCode> NationalAdditionalCode => nationalAdditionalCode ?? (nationalAdditionalCode = GetNationallAdditionalCodes());
		ICollection<INationalAdditionalCode> nationalAdditionalCode;

		ICollection<INationalAdditionalCode> GetNationallAdditionalCodes()
		{
			var result = new List<INationalAdditionalCode>();
			var customsOffice = invoiceLine.Declaration?.JE_CustomsOffice ?? string.Empty;
			invoiceLine.FRAdditionalCodes.ForEach(x => result.Add(NationalAdditionalCodeWrapper.New(x, customsOffice)));
			return result.Any() ? result : null;
		}

		public ICollection<ITaricAdditionalCode> TaricAdditionalCode => taricAdditionalCode ?? (taricAdditionalCode = GetTaricAdditionalCodes());
		ICollection<ITaricAdditionalCode> taricAdditionalCode;

		ICollection<ITaricAdditionalCode> GetTaricAdditionalCodes()
		{
			var result = new List<ITaricAdditionalCode>();
			invoiceLine.CEAdditionalCodes.ForEach(x => result.Add(TaricAdditionalCodeWrapper.New(x)));
			return result.Any() ? result : null;
		}

		public string TaricCode => taricCode ?? (taricCode = invoiceLine.JI_Tariff.SubstringSafe(8, 2));
		string taricCode;

		readonly JobComInvoiceLine invoiceLine;
	}
}
