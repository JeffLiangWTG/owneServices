using Enterprise.Accounting.Netting;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocNettingTransactionReferenceTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocNettingTransactionReference.New(new NettingTransactionReference(), Factory);
		}
	}
}
