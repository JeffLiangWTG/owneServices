using System;
using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class SubscriptionListNodeCollection : NonPersistentBusinessObjectCollection<SubscriptionProperties>
	{
		#region Implemention
		public SubscriptionListNodeCollection(bool isDescriptionRequired)
			: this()
		{
			IsDescriptionRequired = isDescriptionRequired;
			IsHRCampaign = false;
		}

		SubscriptionListNodeCollection()
			: base()
		{
		}
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			var obj = new SubscriptionProperties
			{
				IsDescritipionRequired = IsDescriptionRequired,
				IsHRCampaign = IsHRCampaign
			};
			OnNewBusinessObjectAdded?.Invoke(obj, null);
			obj.OnValidateDuplicated += new EventHandler(ValidateNoDuplicated);
			return obj;
		}

		readonly bool IsDescriptionRequired;

		public event EventHandler OnNewBusinessObjectAdded;

		public void ValidateNoDuplicated(object sender, EventArgs e)
		{
			var newSub = sender as SubscriptionProperties;
			bool bFoundSame = false;
			foreach (SubscriptionProperties sub in this)
			{
				if (sub != newSub && sub.MediaCategoryWithAll.Equals(newSub.MediaCategoryWithAll) && sub.MediaTypeWithAll.Equals(newSub.MediaTypeWithAll))
				{
					bFoundSame = true;
					break;
				}
			}
			if (bFoundSame)
			{
				newSub.MediaCategoryWithAllInfo.AddError(ResString.GetMultilingualString("1785f2a9-252c-406e-92d7-263fb77af35d", "There are duplicated items."));
				newSub.MediaTypeWithAllInfo.AddError(ResString.GetMultilingualString("c615b5f7-020d-4f49-ab45-6099cdf5222e", "There are duplicated items."));
			}
		}

		internal bool IsHRCampaign
		{
			get { return isHRCampaign; }
			set
			{
				isHRCampaign = value;
				foreach (SubscriptionProperties node in this)
				{
					node.IsHRCampaign = value;
				}
			}
		}

		bool isHRCampaign;

		#endregion
	}
}
