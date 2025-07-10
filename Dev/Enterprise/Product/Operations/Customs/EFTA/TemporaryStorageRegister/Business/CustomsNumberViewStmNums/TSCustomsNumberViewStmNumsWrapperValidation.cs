using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;

public class TSCustomsNumberViewStmNumsWrapperValidation : CustomsNumberViewStmNumsWrapperValidation
{
	public TSCustomsNumberViewStmNumsWrapperValidation(TSCustomsNumberViewStmNumsWrapper parent)
		: base(parent)
	{
	}

	protected new TSCustomsNumberViewStmNumsWrapper Parent => (TSCustomsNumberViewStmNumsWrapper)base.Parent;

	public void ValidateIsActive()
	{
		ValidateCalculatedProperty(Parent.IsActiveInfo);
	}

	protected virtual void CheckIsActive()
	{
		var parent = Parent;
		if (parent.IsActive && parent.StmNums.Provider.CustomsNumberWrappers.Cast<TSCustomsNumberViewStmNumsWrapper>().Any(x => x.IsActive && x.PK != parent.PK))
		{
			parent.IsActiveInfo.AddError(Res.GetString("EF0C5681-C7F8-45CA-B6CF-059CB4BF868D", "There is already one configuration active. Only one configuration active is allowed per premises."));
		}
	}

	public void ValidateNumberPrefix() => ValidateCalculatedProperty(Parent.NumberPrefixInfo);

	protected void CheckNumberPrefix() => CheckNumbers(Parent.NumberPrefixInfo);

	public void ValidateNumberSuffix() => ValidateCalculatedProperty(Parent.NumberSuffixInfo);

	protected void CheckNumberSuffix() => CheckNumbers(Parent.NumberSuffixInfo);

	void CheckNumbers(ZPropertyInfo propertyInfo)
	{
		var parent = Parent;

		if (parent.NumberPrefix.Contains(TSCustomsNumberSeparator) || parent.NumberSuffix.Contains(TSCustomsNumberSeparator))
		{
			propertyInfo.AddError(Res.GetString("492E59DB-BDCC-475E-A3F1-716B2CA247AE", "Character '@' is not allowed for Prefix or Suffix."));
		}
	}

	const string TSCustomsNumberSeparator = "@";
}
