using System;
using System.Linq;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Customs
{
	sealed class BooleanCountryDependingRegistryItemImpl : RegistryItemImpl
	{
		public BooleanCountryDependingRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options, object defaultValue, string[] explicitCountries, object explicitCountryValue)
			: base(name, category, caption, hint, dataType, null, storage, options, defaultValue, useDefaultDefaultValue: false)
		{
			this.explicitCountries = explicitCountries;
			this.explicitCountryValue = explicitCountryValue;
		}
		readonly string[] explicitCountries;
		readonly object explicitCountryValue;

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			var countryCode = companyPK != Guid.Empty ? RegistryFactory.Instance.Load<IGlbCompany>(companyPK)?.Country?.RN_Code.ToString() : string.Empty;

			return !string.IsNullOrEmpty(countryCode) && explicitCountries.Contains(countryCode) ? explicitCountryValue : base.GetDefaultValueCore(companyPK, branchPK, departmentPK);
		}
	}
}
