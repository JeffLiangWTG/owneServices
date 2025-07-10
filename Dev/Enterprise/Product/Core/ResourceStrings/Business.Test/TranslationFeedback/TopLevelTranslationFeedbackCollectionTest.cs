using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(TopLevelTranslationFeedbackCollection))]
	sealed class TopLevelTranslationFeedbackCollectionTest : StmTranslationFeedbackCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new TopLevelTranslationFeedbackCollection(Factory);
		}
	}
}
