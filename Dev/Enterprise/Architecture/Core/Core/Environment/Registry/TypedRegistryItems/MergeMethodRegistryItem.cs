using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Integration.Customs;
using ResString = Enterprise.ZArchitecture.Core.SourceGenerated.ResString;

namespace Enterprise.ZArchitecture.Environment
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	class MergeMethodRegistryItem : CodePairRegistryItem
	{
		public MergeMethodRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, ICodeDescriptionPairListProvider lookUpList, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new MergeMethodRegistryItemImpl(name, category, caption, hint, storage, options, defaultValue, lookUpList))
		{
		}
	}

	class CountrySepecificMergeMethodListProvider : ICodeDescriptionPairListProvider
	{
		public CountrySepecificMergeMethodListProvider(OLookUpEditType lookupEditType)
		{
			LookupEditType = lookupEditType;
			Factory = new BusinessObjectFactory();
		}

		readonly OLookUpEditType LookupEditType;
		readonly BusinessObjectFactory Factory;
		Guid CompanyOrOwnerPK;

		public CodeDescriptionPairList CodeDescriptionPairList
		{
			get
			{
				var company = (ICompany)Factory.Load(ObjectFactory.GetType("IGlbCompany"), CompanyOrOwnerPK);
				var currentCountryCode = company?.Country?.Code ?? string.Empty;
				var currentCompanyCode = company?.Code ?? string.Empty;
				var cacheKey = ZString.Format("MergeMethodCodeDescriptionPairList_{0}", currentCompanyCode);// Cached Key
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var lookUpList = GetMergeByListLookup(currentCompanyCode);
					if (currentCountryCode == Constants.CountryCodes.Canada)
					{
						if (!lookUpList.ContainsCode(MasterFiles.Business.OrgConstants.MergeInvoiceLines.TariffAndMultiInvoices))
						{
							lookUpList.AddPair(MasterFiles.Business.OrgConstants.MergeInvoiceLines.TariffAndMultiInvoices, ResString.GetMultilingualString("6DC2A6FD-814A-4E24-B2FF-9B8C1F6F50CE", "(CA ONLY) Classification/Tariff over multiple invoices"));
						}
					}
					return lookUpList;
				});
			}
		}

		CodeDescriptionPairList GetMergeByListLookup(string companyCode)
		{
			var retriever = (IMergeByListReflectionRetriever)ObjectFactory.Get("IMergeByListReflectionRetriever");
			var list = (CodeDescriptionPairList)retriever.GetMergeByListByCompanyCode(companyCode);
			if (list == null || list.Count <= 0)
			{
				list = new CodeDescriptionPairList(LookupEditType);
			}
			return list;
		}

		public void SetContext(Guid companyOrOwnerPK)
		{
			CompanyOrOwnerPK = companyOrOwnerPK;
		}
	}

	class MergeMethodRegistryItemImpl : RegistryItemImpl
	{
		public MergeMethodRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, object defaultValue, ICodeDescriptionPairListProvider lookUpList)
			: base(name, category, caption, hint, new CodePairRegistryDataType(lookUpList, false, true), storage, options, defaultValue)
		{
			this.lookUpList = lookUpList as CountrySepecificMergeMethodListProvider;
		}

		protected CountrySepecificMergeMethodListProvider lookUpList;

		protected override Guid GetRegistryItemPKCore(Guid companyPk, Guid branchPk, Guid departmentPk)
		{
			lookUpList.SetContext(companyPk);
			return base.GetRegistryItemPKCore(companyPk, branchPk, departmentPk);
		}
	}
}
