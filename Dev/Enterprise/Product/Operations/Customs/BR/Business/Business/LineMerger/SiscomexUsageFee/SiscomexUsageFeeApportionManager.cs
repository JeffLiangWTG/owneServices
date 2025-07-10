using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.BR.Business
{
	public class SiscomexUsageFeeApportionManager : BaseFeeApportionManager, IFeeApportionManager
	{
		public SiscomexUsageFeeApportionManager()
		{
		}

		protected override ZDecimal GetLineValue(CusEntryLine entryLine) => new EntryLineUniversalRate(entryLine).CustomsValue;
	}
}
