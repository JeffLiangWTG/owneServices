using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class AccountFeeDefaultTaxIDRegistryItem : GuidRegistryItem
	{
		public AccountFeeDefaultTaxIDRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new AccountFeeDefaultTaxIDRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		public class AccountFeeDefaultTaxIDRegistryItemImpl : RegistryItemImpl
		{
			public AccountFeeDefaultTaxIDRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new GuidRegistryDataType(), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var currentCompany = Factory.Load<GlbCompany>(new ZGuid(companyPK));

				var taxType = (currentCompany?.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.China ? AccTaxRate.Types.Rated : AccTaxRate.Types.Exempt;
				var taxCode = (currentCompany?.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.China ? "VAT6" : null;
				var taxRate = AccTaxRate.FindExistingTaxRate(factory, taxCode, taxType, currentCompany?.Country?.Code ?? ZString.Empty);

				var taxID = taxRate != null ? taxRate.PK.ToGuid() : Guid.Empty;
				return taxID;
			}

			ReadOnlyBusinessObjectFactory factory;
			ReadOnlyBusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new ReadOnlyBusinessObjectFactory()); }
			}
		}
	}
}
