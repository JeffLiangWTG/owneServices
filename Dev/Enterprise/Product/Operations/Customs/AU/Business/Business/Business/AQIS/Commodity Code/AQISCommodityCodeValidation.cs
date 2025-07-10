
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISCommodityCodeValidation : AQISSingleValueValidation
	{
		public AQISCommodityCodeValidation(AQISCommodityCode parent)
			: base(parent)
		{
			this.aQISCommodityCode = parent;
		}

		public override ZInt MaximumNumberOfRecordsAllowed
		{
			get { return 10; }
		}

		public override void ValidateCodeAgainstLookupList()
		{
			ListValidation.MessageErrorIfInvalidCode(aQISCommodityCode.CodeInfo, aQISCommodityCode.Lookups.AQISCommodityCodeList);
		}

		readonly AQISCommodityCode aQISCommodityCode;
	}
}
