using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocProfitShareAgreement))]
	sealed class DocProfitShareAgreementTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var agentRelationship = Factory.New<OrgAgentRelationship>();
			Agreement = agentRelationship.ProfitShareDetails.AddNew();
			return new DocumentWrapper[] { DocProfitShareAgreement.New(Agreement, Factory) };
		}

		OrgProfitShareDetails Agreement;
	}
}
