using CargoWise.EntityFramework.Testing;

namespace Enterprise.BufferManagement.Business.Test
{
	internal class BMBoardSlideshowValidationTest : BusinessObjectValidationTestCase
	{
		public void TestAtleastOneBoard()
		{
			var slideshow = Factory.New<BMBoardSlideshow>();
			slideshow.RunPreSaveValidation();
			Assert(slideshow.HasRowErrors);

			slideshow.BoardPivots.AddNew();
			slideshow.RunPreSaveValidation();
			Assert(!slideshow.HasRowErrors);
		}

		public void TestValidationCannotBeCheatedByAddingAndDeletingBoards()
		{
			var slideshow = Factory.New<BMBoardSlideshow>();
			slideshow.RunPreSaveValidation();
			Assert(slideshow.HasRowErrors);

			var pivot = slideshow.BoardPivots.AddNew();
			slideshow.RunPreSaveValidation();
			Assert(!slideshow.HasRowErrors);

			slideshow.BoardPivots.Delete(pivot);
			slideshow.RunPreSaveValidation();
			Assert("Validation should not allow saving slideshow without at least one board", slideshow.HasRowErrors);
		}
	}
}
