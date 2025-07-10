using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class SubscriptionPropertiesLookups : ZLookups
	{
		public SubscriptionPropertiesLookups(SubscriptionProperties prop)
			: base(prop)
		{
		}

		public new SubscriptionProperties Parent => (SubscriptionProperties)base.Parent;

		public ReadOnlyCodeDescriptionPairList MediaCategoryWithAllList
		{
			get
			{
				return Factory.GetCachedValue<ReadOnlyCodeDescriptionPairList>("SubscriptionPropertiesLookups.MediaCategoryWithAllList" + Parent.IsHRCampaign + Parent.ShouldHaveAllCampaignCategoryAndType, () => GetMediaCategoryList(Parent.ShouldHaveAllCampaignCategoryAndType));
			}
		}
		public ReadOnlyCodeDescriptionPairList MediaTypeWithAllList
		{
			get
			{
				return Factory.GetCachedValue<ReadOnlyCodeDescriptionPairList>("SubscriptionPropertiesLookups.MediaTypeWithAllList" + Parent.IsHRCampaign + Parent.ShouldHaveAllCampaignCategoryAndType, () => GetMediaTypeList(Parent.ShouldHaveAllCampaignCategoryAndType));
			}
		}

		ReadOnlyCodeDescriptionPairList GetMediaTypeList(bool withAllValue = false)
		{
			return GetListWithAll(withAllValue, Parent.IsHRCampaign ? OrganisationsDataRegistry.Instance.HRCampaignCategory2List.Value.GetCodeDescriptionPairList() : OrganisationsDataRegistry.Instance.CampaignCategory2List.Value.GetCodeDescriptionPairList(), ConstListValues.AllCampaignTypeDescription);
		}

		CodeDescriptionPairList GetMediaCategoryList(bool withAllValue = false)
		{
			return GetListWithAll(withAllValue, Parent.IsHRCampaign ? OrganisationsDataRegistry.Instance.HRCampaignCategory1List.Value.GetCodeDescriptionPairList() : OrganisationsDataRegistry.Instance.CampaignCategory1List.Value.GetCodeDescriptionPairList(), ConstListValues.AllCampaignCategoryDescription);
		}

		CodeDescriptionPairList GetListWithAll(bool withAllValue, CodeDescriptionPairList initialList, MultilingualString allValueDescription)
		{
			if (withAllValue)
			{
				initialList.AddPairIfNotExist(ConstListValues.AllCampaignCategoryAndTypeCode, allValueDescription);
			}
			initialList.Sort();
			return initialList;
		}

		public static class ConstListValues
		{
			public static MultilingualString AllCampaignCategoryDescription => ResString.GetMultilingualString("9499cb36-8582-4ae9-ba9a-2e2d2747dfce", "All Categories");
			public static MultilingualString AllCampaignTypeDescription => ResString.GetMultilingualString("e9a5f2bd-6922-4343-b473-5e1ad6775da5", "All Types");
			public static ZString AllCampaignCategoryAndTypeCode { get; } = "ALL";
		}

		protected override BusinessObjectFactory Factory => factory ?? (factory = new BusinessObjectFactory());

		BusinessObjectFactory factory;
	}
}
