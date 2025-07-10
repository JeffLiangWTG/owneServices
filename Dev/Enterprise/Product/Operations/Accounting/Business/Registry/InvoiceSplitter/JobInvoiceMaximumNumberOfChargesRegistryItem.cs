using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobInvoiceMaximumNumberOfChargesRegistryItem : IntRegistryItem
	{
		public JobInvoiceMaximumNumberOfChargesRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, NumericRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, int defaultValue, int minValue, int maxValue)
			: base(new JobInvoiceMaximumNumberOfChargesRegistryItemItemImpl(name, category, caption, hint, editorInfo, storage, options, defaultValue, minValue, maxValue))
		{
		}

		class JobInvoiceMaximumNumberOfChargesRegistryItemItemImpl : RegistryItemImpl
		{
			public JobInvoiceMaximumNumberOfChargesRegistryItemItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, NumericRegistryEditorInfo editorInfo, RegistryStorageFlags storage, RegistryOptions options, int defaultValue, int minValue, int maxValue)
				: base(name, category, caption, hint, new IntRegistryDataType(minValue, maxValue), editorInfo, storage, options, defaultValue)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var currentCompany = Factory.Load<GlbCompany>(new ZGuid(companyPK));
				var defaultMaxChargeNumber = (currentCompany?.GC_RN_NKCountryCode ?? string.Empty) == Core.Constants.CountryCodes.China ? 8 : 0;
				return defaultMaxChargeNumber;
			}

			ReadOnlyBusinessObjectFactory factory;
			ReadOnlyBusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = "JobInvoiceMaximumNumberOfChargesRegistryItemItemImpl_GetFactory" }); }
			}
		}
	}
}
