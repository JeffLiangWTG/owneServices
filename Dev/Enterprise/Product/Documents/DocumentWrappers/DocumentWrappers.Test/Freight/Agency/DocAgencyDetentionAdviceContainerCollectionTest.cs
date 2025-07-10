using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocAgencyDetentionAdviceContainerCollection))]
	sealed class DocAgencyDetentionAdviceContainerCollectionTest : DocBaseWrapperCollectionTest<DocAgencyDetentionAdviceContainerCollection>
	{
		#region Implementation

		protected override DocAgencyDetentionAdviceContainerCollection GetNewDocumentWrapperCollection()
		{
			return new DocAgencyDetentionAdviceContainerCollection(Factory);
		}

		protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			BillOfLadingContainer container = Factory.New<BillOfLading>().RealContainers.AddNew();
			DocAgencyDetentionAdviceLine wrapper = DocAgencyDetentionAdviceLine.New(container, Factory);
			collection.Add(wrapper);
			return wrapper;
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}

		#endregion
	}
}
