using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocCompetitor))]
	public class DocCompetitorTest : DocumentWrapperTestCase
	{
		public void TestCompetitorNameAndCode()
		{
			var orgCompetitor = Factory.NewWithValidTestData<OrgCompetitor>();
			orgCompetitor.Competitor.OH_FullName = "TSTORG FullName";
			orgCompetitor.Competitor.OH_Code = "TST";
			var docCompetitor = DocCompetitor.New(orgCompetitor, Factory);

			AssertEquals("TSTORG FullName", docCompetitor.CompetitorName);
			AssertEquals("TST", docCompetitor.CompetitorCode);
		}

		#region Implementation

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocCompetitor.New(Factory.NewWithValidTestData<OrgCompetitor>(), Factory)
			};
		}

		protected override DocumentWrapper CreateDocumentWrapperFromStaticNewMethod()
		{
			return DocCompetitor.New(Factory.NewWithValidTestData<OrgCompetitor>(), Factory);
		}

		#endregion
	}
}
