using System;
using System.Linq;
using Enterprise.Customs.IT.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business;

public class ITCustomsNumberViewStmNumsWrapperLookups : CustomsNumberViewStmNumsWrapperLookups
{
	public ITCustomsNumberViewStmNumsWrapperLookups(ITCustomsNumberViewStmNumsWrapper parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList TypeList => Factory.GetCachedValue<NumberRangeTypeList>();

	public CodeDescriptionPairList AppliesToList
	{
		get
		{
			var ownerCompany = Parent.SN_Owner.ToGuid();
			return Factory.GetCachedValue(FormattableString.Invariant($"ITAccountsManagementList_{ownerCompany}"), () => GetAppliesToList(ownerCompany));
		}
	}

	#region Implementation

	protected new ITCustomsNumberViewStmNumsWrapper Parent => (ITCustomsNumberViewStmNumsWrapper)base.Parent;

	CodeDescriptionPairList GetAppliesToList(Guid ownerCompany)
	{
		var accounts = ITAccountsManagementRegistry
			.Instance
			.AccountsManagement
			.GetFallBackValueAtAllLevels(ownerCompany, branchPK: Guid.Empty, departmentPK: Guid.Empty)
			.OfType<Account>();

		var taxNumbers = accounts.Select(x => x.DeclarantTaxNumber);

		var result = new CodeDescriptionPairList();
		foreach (var declarantTaxNumber in taxNumbers.Distinct())
		{
			result.AddPair(declarantTaxNumber);
		}
		result.Sort();

		return result;
	}

	#endregion
}
