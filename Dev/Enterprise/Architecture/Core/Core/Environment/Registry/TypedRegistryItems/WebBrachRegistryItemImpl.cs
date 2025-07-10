using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class WebBrachRegistryItemImpl : RegistryItemImpl
	{
		public WebBrachRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, object defaultValue)
			: base(name, category, caption, hint, new WebBranchCodeGuidRegistryDataType(), editorInfo, storage, options, defaultValue)
		{
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return EnvProxy.GetAnyBranchWithWebAddress(new BusinessObjectFactory());
		}
	}
}
