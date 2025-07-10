using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(StmTranslationFeedbackCollection))]
	public class StmTranslationFeedbackCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmTranslationFeedbackCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			elementNumber++;
			return StmTranslationFeedback.New(
				Factory,
				new HelpDataString() { HD_Language = Res.DefaultLanguage, HD_Code = "x" + elementNumber, HD_Caption = "Test " + elementNumber },
				new HelpDataString() { HD_Language = Core.SharedConstants.Languages.French, HD_Code = "x" + elementNumber, HD_Caption = "Essai " + elementNumber },
				TranslationFeedbackMatchTypes.Codes.Exact);
		}

		int elementNumber;
	}
}
