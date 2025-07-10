using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.CN.Business
{
	class CNSWClientSettingRegistryItemImpl : RegistryItemImpl
	{
		public CNSWClientSettingRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, object defaultValue)
			: base(name, category, caption, hint, new CNSWClientSettingRegistryDataType(), storage, defaultValue)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var defaultValue = (CNSWClientSetting)base.GetDefaultValueCore(companyPK, branchPK, departmentPK);
			defaultValue.RegistryItemInternals = this;
			return defaultValue;
		}
	}
}
