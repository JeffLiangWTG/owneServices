using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.Accounting
{
	[TestedType(typeof(DocProfitShareDetail))]
	sealed class DocProfitShareDetailTest : DocumentWrapperTestCase
	{
		public void TestDocTypeCode()
		{
			var consol = Factory.New<ForwardingConsol>();
			ProfitShareDetail profitShareDetail = new ProfitShareDetail(OrgHeader.New(Factory), Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			profitShareDetail.Consol = consol;
			var wrapper = (IDocTypeCode)DocProfitShareDetail.New(profitShareDetail, Factory);

			AssertEquals("Doc Wrapper has Profit Share doc type code", "PRS", wrapper.DocTypeCode);
		}

		public override DocumentWrapper[] GetDocumentWrappers()
		{
			var consol = Factory.New<ForwardingConsol>();
			ProfitShareDetail profitShareDetail = new ProfitShareDetail(OrgHeader.New(Factory), Factory, OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent);
			profitShareDetail.Consol = consol;
			return new DocumentWrapper[] { DocProfitShareDetail.New(profitShareDetail, Factory) };
		}
	}
}
