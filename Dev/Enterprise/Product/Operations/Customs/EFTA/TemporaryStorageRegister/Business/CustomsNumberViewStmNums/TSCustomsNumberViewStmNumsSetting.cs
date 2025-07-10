using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class TSCustomsNumberViewStmNumsSetting : CustomsNumberViewStmNumsSetting
{
	public TSCustomsNumberViewStmNumsSetting(CusTempStorageRegPremises parent, ZString rangeType) : base(parent, rangeType)
	{
	}

	public CusTempStorageRegPremises Premises => (CusTempStorageRegPremises)Parent;

	protected override int RequiredDigitCore()
	{
		var activeWrapper = Premises.NumberProvider.ActiveWrapper;
		return activeWrapper != null ? Math.Max(activeWrapper.NumberPadding, 1) : 1;
	}

	protected override ZString GenerateCustomsNumberCore(CustomsNumberViewStmNums stmNums, ZString number)
	{
		var activeWrapper = Premises.NumberProvider.ActiveWrapper;
		if (activeWrapper != null)
		{
			return activeWrapper.NumberPrefix + number + activeWrapper.NumberSuffix;
		}

		return base.GenerateCustomsNumberCore(stmNums, number);
	}
}
