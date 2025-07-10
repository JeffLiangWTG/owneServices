using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSDocumentCollection<EMCSDocument>))]
	sealed public class EMCSDocumentCollectionTest : EMCSDocumentCollectionAbstractTest<EMCSDocument>
	{
		protected override CusSupportingInfoCollection<EMCSDocument> GetCusSupportingInfoCollection() => new EMCSDocumentCollection<EMCSDocument>(Factory.New<EMCSJobDeclaration>());
	}
}
