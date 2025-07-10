using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business
{
	public class AFRMMFeeApportionManager : BaseFeeApportionManager, IFeeApportionManager
	{
		public AFRMMFeeApportionManager()
		{
		}

		protected override ZDecimal GetLineValue(CusEntryLine entryLine) => entryLine.EffectiveNetWeight.Amount;

		protected override IEnumerable<CusEntryLine> GetCandidateEntryLines(Customs.Business.ICusEntryLineCollection<CusEntryLine> entryLines)
		{
			return entryLines.Where(x => x.RandomLine.FMMBenefit != FMMBenefitTypeList.Codes.Exemption);
		}
	}
}
