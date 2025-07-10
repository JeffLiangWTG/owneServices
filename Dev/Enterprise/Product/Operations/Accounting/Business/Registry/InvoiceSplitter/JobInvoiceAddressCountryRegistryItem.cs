using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using ResString = Enterprise.Accounting.Business.ResString;

namespace Enterprise.Accounting.Registry.Business
{
	public class JobInvoiceAddressCountryRegistryItem : CodePairRegistryItem
	{
		public JobInvoiceAddressCountryRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new JobInvoiceAddressCountryRegistryItemImpl(name, category, caption, hint, storage, options))
		{
		}

		public bool CanSplitRuleBeAppliedOnDebtor(OrgAddress debtorAddress)
		{
			bool result = false;
			if (Value == ALL)
			{
				result = true;
			}
			else
			{
				if (debtorAddress == null)
				{
					result = false;
				}
				else
				{
					bool isSameCountry = debtorAddress.OA_RN_NKCountryCode == GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					if (Value == SAM)
					{
						result = isSameCountry;
					}
					else if (Value == DIF)
					{
						result = !isSameCountry;
					}
				}
			}
			return result;
		}

		static ICodeDescriptionPairListProvider GetLookupListProvider()
		{
			return new CodeDescriptionPairListProvider(() =>
			{
				var lookUpList = new CodeDescriptionPairList();
				lookUpList.AddPair(ALL, ResString.GetMultilingualString("b9cf3736-adab-4980-9a52-e23fd0748c4c", "Apply rule regardless of country/region of the invoice address"));
				lookUpList.AddPair(SAM, ResString.GetMultilingualString("f81dcb34-076e-4640-af4b-87476b03b4d3", "Apply rule if invoice address is in the same country/region as current company"));
				lookUpList.AddPair(DIF, ResString.GetMultilingualString("e9c02d17-32ce-46e4-8a28-0af698bff36a", "Apply rule if invoice address is in a different country/region from the current company"));
				return lookUpList;
			});
		}

		const string ALL = "ALL";
		const string SAM = "SAM";
		const string DIF = "DIF";

		class JobInvoiceAddressCountryRegistryItemImpl : RegistryItemImpl
		{
			public JobInvoiceAddressCountryRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
				: base(name, category, caption, hint, new CodePairRegistryDataType(GetLookupListProvider(), false, true), storage, options)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				var currentCompany = Factory.Load<GlbCompany>(new ZGuid(companyPK));
				var defaultValue = (currentCompany?.Country?.Code ?? ZString.Empty) == Core.Constants.CountryCodes.China ? SAM : ALL;
				return defaultValue;
			}

			ReadOnlyBusinessObjectFactory factory;
			ReadOnlyBusinessObjectFactory Factory
			{
				get { return factory ?? (factory = new ReadOnlyBusinessObjectFactory()); }
			}
		}
	}
}
