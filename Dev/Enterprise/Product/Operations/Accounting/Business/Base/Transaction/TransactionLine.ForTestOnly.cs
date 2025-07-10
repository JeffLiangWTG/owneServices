#if DEBUG

using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public partial class TransactionLine
	{
		public ZString LineType_ForTestOnly => LineType;

		public ZInt Multiplier_ForTestOnly => Multiplier;

		public ILocation PlaceOfSupplyLocation_ForTestOnly => PlaceOfSupplyLocation;

		public SecurityCheckpoint OverrideInputVatRecoverableSecurityCheckPoint_ForTestOnly => OverrideInputVatRecoverableSecurityCheckPoint;

		public ZDecimal RoundAmountToCurrencyDecimals_ForTestOnly(ZDecimal value) => RoundAmountToCurrencyDecimals(value);
	}
}

#endif
