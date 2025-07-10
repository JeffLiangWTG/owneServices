using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.Testing
{
	[TestedType(typeof(PreviousDocumentCollection))]
	public class PreviousDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<PreviousDocument>
	{
		protected override Customs.Business.CusSupportingInfoCollection<PreviousDocument> GetCusSupportingInfoCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			return new PreviousDocumentCollection(declaration);
		}
	}
}
