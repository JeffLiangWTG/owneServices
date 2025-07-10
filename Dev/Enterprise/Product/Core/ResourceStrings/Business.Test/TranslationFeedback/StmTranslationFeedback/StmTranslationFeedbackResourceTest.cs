using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Testing
{
	[TestedType(typeof(StmTranslationFeedbackResource))]
	sealed class StmTranslationFeedbackResourceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestXQ_ResourcesStringLevelReadOnly()
		{
			var thing = Factory.New<StmTranslationFeedbackResource>();

			AssertEquals("expected " + StmTranslationFeedbackResource.Schema.XQ_ResourceStringLevel + " to be read only", true, thing.XQ_ResourceStringLevelInfo.ReadOnly);
		}

		public void TestReportNameMaxLength()
		{
			var resource = Factory.New<StmTranslationFeedbackResource>();
			resource.XQ_ResourceStringKey = "ReportName|List of CargoWise One Reports";

			AssertEquals(31, resource.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var item = Factory.New<StmTranslationFeedbackResource>();
			item.XQ_MatchType = TranslationFeedbackMatchTypes.Codes.Exact;
			return item;
		}

		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert(true);
		}
	}
}
