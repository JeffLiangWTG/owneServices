using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(StmTranslationFeedbackResourceCollection))]
	sealed class StmTranslationFeedbackResourceCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Factory.New<StmTranslationFeedback>().SavedContexts;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<StmTranslationFeedbackResource>();
		}
	}
}
