using System.Text.RegularExpressions;
using CargoWise.Customs.GB.MessageContracts.SafetyAndSecurity;
using CargoWise.Types;

namespace Enterprise.Customs.GB.SafetyAndSecurity.Messaging
{
	class CommodityWrapper : ICommodity
	{
		readonly ZString combinedNomenclature;

		public CommodityWrapper(ZString combinedNomenclature)
		{
			this.combinedNomenclature = NormalizeCommodityCode(combinedNomenclature);
		}

		public string CombinedNomenclature => combinedNomenclature;

		static ZString NormalizeCommodityCode(ZString value) => ((ZString)Regex.Replace(value, @"[^\d]", string.Empty)).SubstringSafe(0, 8);
	}
}
