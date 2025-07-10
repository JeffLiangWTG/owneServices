using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.Common
{
	public class PreCalcEntryLineWrapper : IPreCalcEntryLine
	{
		public PreCalcEntryLineWrapper(CusEntryLineFee fee)
		{
			this.fee = Argument.NotNull(fee, " cannot be null");
		}
		public ISupplementaryUnit SuppUnit => fee.CF_MethodOfCalculation != "%" ? new SupplementaryUnitWrapper(fee) : null;
		public ITax Tax => new TaxWrapper(fee);

		public ZBool IsVAT => fee.CF_ChargeType == FeeTypeCodeConverter.EUFeeCodeForVAT;

		readonly CusEntryLineFee fee;
	}
}
