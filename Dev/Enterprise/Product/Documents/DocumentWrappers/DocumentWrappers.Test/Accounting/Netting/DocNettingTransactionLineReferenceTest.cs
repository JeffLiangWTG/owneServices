using Enterprise.Accounting.Netting;

namespace Enterprise.DocumentWrappers.Testing
{
	sealed class DocNettingTransactionLineReferenceTest : DocBaseWrapperTest
	{
		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return DocNettingTransactionLineReference.New(new NettingTransactionLineReference(), Factory);
		}
	}
}
