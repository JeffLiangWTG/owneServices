using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IL.Business
{
	public class DCAParametersRegistryItem : StronglyTypedRegistryItem<DCAParameters>
	{
		public DCAParametersRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new DCAParametersRegistryItemImpl(name, category, caption, hint, new DCAParametersRegistryDataType(), storage, options))
		{
		}
	}

	class DCAParametersRegistryItemImpl : RegistryItemImpl
	{
		public DCAParametersRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options) : base(name, category, caption, hint, dataType, storage, options)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var defaultValue = (DCAParameters)base.GetDefaultValueCore(companyPK, branchPK, departmentPK);
			if (!defaultValue.AllServices)
			{
				defaultValue.AddDefaultServices();
			}
			return defaultValue;
		}
	}
}
