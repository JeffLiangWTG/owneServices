using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestedType(typeof(RequestedDocumentCollection))]
	class RequestedDocumentCollectionTest : Customs.Business.Testing.CusSupportingInfoCollectionTest<RequestedDocument>
	{
		protected override CusSupportingInfoCollection<RequestedDocument> GetCusSupportingInfoCollection()
		{
			var instruction = Factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew();
			return new RequestedDocumentCollection(instruction);
		}
	}
}
