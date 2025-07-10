using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Registry.Business.Warehouse
{
	public class TransitReferenceMappingValidation
	{
		public TransitReferenceMappingValidation(TransitReferenceMapping parent)
		{
			Parent = parent;
		}

		readonly TransitReferenceMapping Parent;

		public void ValidateSourceCategory()
		{
			MandatoryValidation.CheckEntered(Parent.SourceCategoryInfo, (IMultilingualString)ResString.GetMultilingualString("4de0dd51-b872-4f0f-bfb4-ace0dc68b473", "Source Category"));
			ListValidation.ErrorIfInvalidCode(Parent.SourceCategoryInfo, Parent.Lookups.SourceReferenceCategoryList);

			ValidateCollisions();
		}

		public void ValidateSourceType()
		{
			MandatoryValidation.CheckEntered(Parent.SourceTypeInfo, (IMultilingualString)ResString.GetMultilingualString("2dc4649c-88ef-4800-b49c-4aaacc95084d", "Source Type"));

			if (Parent.SourceCategory == Parent.TargetCategory && Parent.SourceType == Parent.TargetType)
			{
				Parent.SourceTypeInfo.AddError(ResString.GetMultilingualString("08d163d1-2e6a-47b5-ac9a-9d33c242a5c6", "Source Type cannot equal to Target Type."));
			}

			if (Parent.ParentCollection != null)
			{
				var sourceTypeConflictMessage = ResString.GetMultilingualString("6218bd80-cdee-438a-8ed2-ddc14198458e", "Other conflicting Source Type exist.");
				foreach (TransitReferenceMapping item in Parent.ParentCollection)
				{
					if (item.PK != Parent.PK && item.SourceCategory == Parent.SourceCategory && item.SourceType == Parent.SourceType && item.Direction == Parent.Direction)
					{
						Parent.SourceTypeInfo.AddError(sourceTypeConflictMessage);
					}
				}
			}

			ValidateCollisions();
		}

		public void ValidateTargetCategory()
		{
			MandatoryValidation.CheckEntered(Parent.TargetCategoryInfo, (IMultilingualString)ResString.GetMultilingualString("034c59c5-5002-4672-8d8a-7d6b6ab5552a", "Target Category"));
			ListValidation.ErrorIfInvalidCode(Parent.TargetCategoryInfo, Parent.Lookups.TargetReferenceCategoryList);

			ValidateCollisions();
		}

		public void ValidateTargetType()
		{
			MandatoryValidation.CheckEntered(Parent.TargetTypeInfo, (IMultilingualString)ResString.GetMultilingualString("32ffa86a-0be7-43bd-8ded-455e18b50a30", "Target Type"));
			ListValidation.ErrorIfInvalidCode(Parent.TargetTypeInfo, Parent.Lookups.TargetReferenceTypeList);

			if (Parent.SourceCategory == Parent.TargetCategory && Parent.SourceType == Parent.TargetType)
			{
				Parent.TargetTypeInfo.AddError(ResString.GetMultilingualString("95c74740-c897-4b86-9bc9-4844b7954cbe", "Target Type cannot equal to Source Type."));
			}

			ValidateCollisions();
		}

		public void ValidateDirection()
		{
			ListValidation.ErrorIfInvalidCode(Parent.DirectionInfo, Parent.Lookups.DirectionList);

			if (Parent.ParentCollection != null)
			{
				var sameSourceTypeItems = Parent.ParentCollection.Cast<TransitReferenceMapping>().Where(i => i.SourceCategory == Parent.SourceCategory && i.SourceType == Parent.SourceType).ToList();
				if (sameSourceTypeItems.Any(item => item.PK != Parent.PK && item.Direction == Parent.Direction))
				{
					Parent.DirectionInfo.AddError(ResString.GetMultilingualString("2260d1a1-5727-485f-bac9-e064a783e4a4", "Other conflicting Direction exist."));
				}
			}

			ValidateCollisions();
		}

		void ValidateCollisions()
		{
			if (Parent.ParentCollection == null)
			{
				return;
			}

			var duplicateMessage = ResString.GetMultilingualString("7cd8df1a-26ee-42f8-aee8-6529aeb6d0c9", "Duplicated reference mappings.");
			if (Parent.ParentCollection.Cast<TransitReferenceMapping>().Any(m => m.PK != Parent.PK
					&& m.SourceCategory == Parent.SourceCategory
					&& m.SourceType == Parent.SourceType
					&& m.TargetCategory == Parent.TargetCategory
					&& m.TargetType == Parent.TargetType
					&& m.Direction == Parent.Direction))
			{
				Parent.AddRowError(duplicateMessage);
			}
			else
			{
				Parent.ClearRowNotifications();
			}
		}
	}
}
