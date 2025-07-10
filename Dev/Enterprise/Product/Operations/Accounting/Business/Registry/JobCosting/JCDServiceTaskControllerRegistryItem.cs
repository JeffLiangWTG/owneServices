using System;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JCDServiceTaskControllerRegistryItem : CodePairRegistryItem
	{
		public JCDServiceTaskControllerRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new JCDServiceTaskControllerRegistryItemDataType(), storage, options, JCDActionList.Codes.NotInitialized))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);
			OnUpdateAction?.Invoke(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}
	}
}