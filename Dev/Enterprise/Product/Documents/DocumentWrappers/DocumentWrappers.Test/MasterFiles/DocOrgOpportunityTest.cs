using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgOpportunity))]
	public class DocOrgOpportunityTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var opportunity = Factory.New<OrgOpportunity>();

			return new DocumentWrapper[]
			{
				DocOrgOpportunity.New(opportunity, Factory)
			};
		}
	}
}
