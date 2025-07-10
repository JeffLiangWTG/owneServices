using CargoWise.EntityFramework.Testing;

namespace Enterprise.Registry.Business.Testing
{
	sealed class NewsAnnouncementSectionTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidOrderItemsBy()
		{
			var sectionType = new NewsAnnouncementSectionType();

			sectionType.OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime;
			sectionType.Validation.ValidateOrderBy();
			AssertNoErrors(sectionType.OrderItemsByInfo);

			sectionType.OrderItemsBy = "Something invalid";
			sectionType.Validation.ValidateOrderBy();
			AssertHasError("Orderby must be valid.", sectionType.OrderItemsByInfo, "Enter a valid Sort Type.");
		}

		public void TestUniqueCode()
		{
			var sectionTypeCollection = new NewsAnnouncementSectionTypeCollection();
			var sectionType = sectionTypeCollection.AddNew();
			sectionType.Code = "DUP";
			sectionType.SystemDefined = true;
			var sectionType2 = sectionTypeCollection.AddNew();
			sectionType2.Code = "DUP";
			sectionType2.SystemDefined = false;

			sectionType.Validation.ValidateCode();

			AssertNoError("No unique error for SystemDefined Code.", sectionType.CodeInfo, "The Code has been duplicated and must be unique.");
			AssertHasError("Code must be uniqe for manual value.", sectionType2.CodeInfo, "The Code has been duplicated and must be unique.");
		}

		public void TestValidateDescription_Mandatory()
		{
			var sectionType = new NewsAnnouncementSectionType();

			sectionType.EnglishDescriptionToShow = string.Empty;
			AssertHasError("Description must be entered.", sectionType.EnglishDescriptionToShowInfo, "Please enter a Description.");

			sectionType.EnglishDescriptionToShow = "Some Description";
			AssertNoErrors(sectionType.EnglishDescriptionToShowInfo);
		}
	}
}
