using System;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class CASSFileImportDefaultTaxIDRegistryItem : StronglyTypedRegistryItem<CASSFileImportDefaultTaxID>
	{
		public CASSFileImportDefaultTaxIDRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new CASSFileImportDefaultTaxIDRegistryItemRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		public class CASSFileImportDefaultTaxIDRegistryItemRegistryItemImpl : RegistryItemImpl
		{
			public CASSFileImportDefaultTaxIDRegistryItemRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new CASSFileImportDefaultTaxIDRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				CASSFileImportDefaultTaxID result = new CASSFileImportDefaultTaxID();
				result.StandardRatedTaxID = AccTaxRate.Helper.FindTaxRatePK(Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, companyPK);
				result.ZeroRatedTaxID = AccTaxRate.Helper.FindTaxRatePK(Factory, AccTaxRate.Helper.MainFreeGSTTaxRegistryID, companyPK);

				return result;
			}

			BusinessObjectFactory fFactory;
			BusinessObjectFactory Factory
			{
				get { return fFactory ?? (fFactory = new BusinessObjectFactory()); }
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CASSFileImportDefaultTaxIDRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class CASSFileImportDefaultTaxIDRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CASSFileImportDefaultTaxID>
	{
	}
}
