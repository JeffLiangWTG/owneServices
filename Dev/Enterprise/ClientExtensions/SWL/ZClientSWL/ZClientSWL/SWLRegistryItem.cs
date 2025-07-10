using System;
using Enterprise.Client.SWL.Business;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.SWL
{
	public class SWLRegistryItem : RegistryItemWrapper
	{
		public SWLRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new ShipnetSetupRegistryDataType(), RegistryStorageFlags.CompanyDepartment, options))
		{
		}

		public void SetValue(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			((IRegistryItem)this).SetValue(companyOrOwnerPK, branchPK, departmentPK, newValue);
		}

		protected override object ValueCore
		{
			get { return null; }
		}
	}

	class ShipnetSetupRegistryDataType : WeaklyTypedNonPersistentBusinessObjectRegistryDataType
	{
		public ShipnetSetupRegistryDataType()
			: base(typeof(ShipnetSetupBusinessObject))
		{
		}
	}
}
