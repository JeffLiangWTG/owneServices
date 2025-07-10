using CargoWise.EntityFramework;
using Enterprise.ResourceStrings.Business;
using Enterprise.ResourceStrings.Business.Testing;

namespace Enterprise.ResourceStrings.Module.TranslationFeedback.Testing
{
	class TranslationFeedbackControllerTest : TranslationFeedbackTestCase
	{
		public void TestDeleteMultiple()
		{
			AddMockTranslation("kdg", "Dog", "狗");
			AddMockTranslation("kdrgn", "Dragon", "龍");

			var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks[0].Factory.Save();
			feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdrgn", "Dragon"));
			feedbacks[0].XT_SuggestedTranslation = "虯";
			feedbacks[0].Factory.Save();
			AssertEquals(2, ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true)).Length);
			new TranslationFeedbackController().DeleteMultiple(new BusinessObjectFactory().Load<StmTranslationFeedback>(new ZQuery()));
			AssertEquals(0, ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true)).Length);
			var allFeedbacks = new BusinessObjectFactory().Load<StmTranslationFeedback>(new ZQuery());
			AssertEquals(TranslationFeedbackStatusList.Codes.Canceled, allFeedbacks[0].XT_Status);
			AssertEquals(TranslationFeedbackStatusList.Codes.Canceled, allFeedbacks[1].XT_Status);
		}

		public void TestDelete()
		{
			AddMockTranslation("kdg1", "Dog", "狗");
			var feedbacks = TranslationFeedbackFactory.Get(new BusinessObjectFactory(), Core.SharedConstants.Languages.ChineseTraditional, Res.GetData("kdg1", "Dog"));
			feedbacks[0].XT_SuggestedTranslation = "犬";
			feedbacks[0].Factory.Save();
			AssertEquals(1, ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true)).Length);
			new TranslationFeedbackController().ShowDeleteForm(feedbacks[0]);
			AssertEquals(TranslationFeedbackStatusList.Codes.Canceled, feedbacks[0].XT_Status);
			AssertEquals(0, ResourceStringsFactory.Load(new ZQuery(HelpDataStringSchema.HD_IsCheckedOut, true)).Length);
		}
	}
}
