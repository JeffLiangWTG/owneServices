using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgPartyScreeningStatus))]
	public class DocOrgPartyScreeningStatusTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrgPartyScreeningStatus.New(ScreeningStatus(),Factory)
			};
		}

		StmEntityScreeningLog ScreeningStatus()
		{
			return Factory.NewWithValidTestData<StmEntityScreeningLog>();
		}
	}
}
