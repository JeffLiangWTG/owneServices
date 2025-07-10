using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public class SubscriptionProperties : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SubscriptionProperties()
			: base()
		{
			IsDescritipionRequired = true;
			isSubscribed = false;
		}

		#region MediaCategory and MediaType
		public ZString MediaCategory
		{
			get { return mediaCategory; }
		}

		[List("Lookups.MediaCategoryWithAllList")]
		[MaxLength(5)]
		public
#if DEBUG
		virtual
#endif
		ZString MediaCategoryWithAll
		{
			get { return mediaCategory.IsEmpty && ShouldHaveAllCampaignCategoryAndType ? SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode : mediaCategory; }
			set
			{
				CheckMaximumLength(MediaCategoryWithAllInfo, value);
				var temp = value == SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode ? ZString.Empty : value;
				SetNonPersistentPropertyValue<ZString>(MediaCategoryWithAllInfo, ref this.mediaCategory, temp);
				if (!IsValidationSuspended)
				{
					ValidateMediaCategory();
				}
				MediaCategoryWithAllInfo.RefreshBinding();
			}
		}
		protected ZString mediaCategory;

		public ZPropertyInfo MediaCategoryWithAllInfo
		{
			get { return GetZPropertyInfo(nameof(MediaCategoryWithAll)); }
		}

		public ZString MediaType
		{
			get { return mediaType; }
		}

		[List("Lookups.MediaTypeWithAllList")]
		[MaxLength(5)]
		public
#if DEBUG
		virtual
#endif
		ZString MediaTypeWithAll
		{
			get { return MediaType.IsEmpty && ShouldHaveAllCampaignCategoryAndType ? SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode : MediaType; }
			set
			{
				CheckMaximumLength(MediaTypeWithAllInfo, value);
				var temp = value == SubscriptionPropertiesLookups.ConstListValues.AllCampaignCategoryAndTypeCode ? ZString.Empty : value;
				SetNonPersistentPropertyValue<ZString>(MediaTypeWithAllInfo, ref this.mediaType, temp);
				if (!IsValidationSuspended)
				{
					ValidateMediaType();
				}
				MediaTypeWithAllInfo.RefreshBinding();
			}
		}
		protected ZString mediaType;

		public ZPropertyInfo MediaTypeWithAllInfo
		{
			get { return GetZPropertyInfo(nameof(MediaTypeWithAll)); }
		}

		public ZString PublishedDescription
		{
			get { return publishedDescription; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(PublishedDescriptionInfo, ref this.publishedDescription, value);
				if (!IsValidationSuspended)
				{
					ValidatePublishedDescription();
				}
			}
		}
		ZString publishedDescription;

		public bool IsDescritipionRequired;

		public ZPropertyInfo PublishedDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(PublishedDescription)); }
		}

		public ZString PublishedSummary
		{
			get { return publishedSummary; }
			set
			{
				SetNonPersistentPropertyValue<ZString>(PublishedSummaryInfo, ref this.publishedSummary, value);
			}
		}
		ZString publishedSummary;

		public ZPropertyInfo PublishedSummaryInfo
		{
			get { return GetZPropertyInfo(nameof(PublishedSummary)); }
		}

		public ZBool IsSubscribed
		{
			get { return isSubscribed; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(IsSubscribedInfo, ref this.isSubscribed, value);
				if (!IsValidationSuspended)
				{
					ValidateIsOrgLevel();
				}
				if (OnIsSubscribedChanged != null)
				{
					OnIsSubscribedChanged.Invoke(this, null);
				}
			}
		}
		ZBool isSubscribed;

		public ZPropertyInfo IsSubscribedInfo
		{
			get { return GetZPropertyInfo(nameof(IsSubscribed)); }
		}

		public ZGuid CampaignPK
		{
			get { return campaignPK; }
			set
			{
				SetNonPersistentPropertyValue<ZGuid>(CampaignPKInfo, ref this.campaignPK, value);
			}
		}
		ZGuid campaignPK;

		public ZPropertyInfo CampaignPKInfo
		{
			get { return GetZPropertyInfo(nameof(CampaignPK)); }
		}

		public bool IsHRCampaign;

		public ZBool IsOrgLevel
		{
			get { return isOrgLevel; }
			set
			{
				SetNonPersistentPropertyValue<ZBool>(IsOrgLevelInfo, ref this.isOrgLevel, value);
				if (!IsValidationSuspended)
				{
					ValidateIsOrgLevel();
				}
			}
		}
		ZBool isOrgLevel;

		public ZPropertyInfo IsOrgLevelInfo
		{
			get { return GetZPropertyInfo(nameof(IsOrgLevel)); }
		}

		void ValidateIsOrgLevel()
		{
			IsOrgLevelInfo.ClearAllNotifications();

			if (IsOrgLevel && !IsSubscribed)
			{
				IsOrgLevelInfo.AddWarning(Res.GetString("e887d9de-8860-4e12-9ce4-228c59120515", "Un-subscribing this contact's Organization from campaign categories/types will overwrite existing related Subscription Preferences for all of the Organization's Contacts."));
			}
		}

		public void ValidateMediaCategory()
		{
			MediaCategoryWithAllInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MediaCategoryWithAllInfo);
			ListValidation.ErrorIfInvalidCode(MediaCategoryWithAllInfo);
			this.OnValidateDuplicated?.Invoke(this, null);
		}

		public void ValidateMediaType()
		{
			MediaTypeWithAllInfo.ClearAllNotifications();
			MandatoryValidation.CheckEntered(MediaTypeWithAllInfo);
			ListValidation.ErrorIfInvalidCode(MediaTypeWithAllInfo);
			this.OnValidateDuplicated?.Invoke(this, null);
		}

		public void ValidatePublishedDescription()
		{
			if (IsDescritipionRequired)
			{
				PublishedDescriptionInfo.ClearAllNotifications();
				MandatoryValidation.CheckEntered(PublishedDescriptionInfo);
			}
		}

		public event EventHandler OnIsSubscribedChanged;

		public event EventHandler OnValidateDuplicated;

		protected override void RunPreSaveValidationCore()
		{
			ValidateMediaCategory();
			ValidateMediaType();
			ValidatePublishedDescription();
			ValidateIsOrgLevel();
		}

		#endregion

		#region TypeDescription

		public ZString MediaCategoryDescription
		{
			get { return Lookups.MediaCategoryWithAllList.GetDescriptionFromCode(MediaCategoryWithAll); }
		}
		public ZPropertyInfo MediaCategoryDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(MediaCategoryDescription)); }
		}

		public ZString MediaTypeDescription
		{
			get { return Lookups.MediaTypeWithAllList.GetDescriptionFromCode(MediaTypeWithAll); }
		}

		public ZPropertyInfo MediaTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(MediaTypeDescription)); }
		}

		#endregion

		#region Lookups

		public bool ShouldHaveAllCampaignCategoryAndType { get; set; } = true;

		public SubscriptionPropertiesLookups Lookups
		{
			get { return lookups ?? (lookups = new SubscriptionPropertiesLookups(this)); }
		}
		SubscriptionPropertiesLookups lookups;
	}

	#endregion
}
