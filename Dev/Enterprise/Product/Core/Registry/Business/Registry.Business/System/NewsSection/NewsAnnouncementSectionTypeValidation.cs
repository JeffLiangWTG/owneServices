using CargoWise.EntityFramework;

namespace Enterprise.Registry.Business
{
	public class NewsAnnouncementSectionTypeValidation : ZValidation
	{
		public NewsAnnouncementSectionTypeValidation(NewsAnnouncementSectionType parent)
			: base(parent)
		{
			this.Parent = parent;
		}

		readonly NewsAnnouncementSectionType Parent;

		public void ValidateCode()
		{
			ValidateCalculatedProperty(Parent.CodeInfo);
		}

		protected virtual void CheckCode()
		{
			MandatoryValidation.CheckEntered(Parent.CodeInfo);
			if (!Parent.SystemDefined)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.CodeInfo);
			}
		}

		public void ValidateOrderBy()
		{
			ValidateCalculatedProperty(Parent.OrderItemsByInfo);
		}

		protected virtual void CheckOrderItemsBy()
		{
			MandatoryValidation.CheckEntered(Parent.OrderItemsByInfo);
			ListValidation.ErrorIfInvalidCode(Parent.OrderItemsByInfo);
		}
		public void ValidateEnglishDescriptionToShow()
		{
			ValidateCalculatedProperty(Parent.EnglishDescriptionToShowInfo);
		}

		protected virtual void CheckEnglishDescriptionToShow()
		{
			MandatoryValidation.CheckEntered(Parent.EnglishDescriptionToShowInfo);
		}

		public override System.Type AutoValidationType
		{
			get { return typeof(NewsAnnouncementSectionTypeValidation); }
		}

		public override void ValidateAll()
		{
			ValidateCode();
			ValidateOrderBy();
			ValidateEnglishDescriptionToShow();
		}
	}
}

