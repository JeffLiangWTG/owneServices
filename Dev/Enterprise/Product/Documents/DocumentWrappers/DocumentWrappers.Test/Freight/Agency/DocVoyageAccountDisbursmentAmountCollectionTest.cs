using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing
{
	[TestedType(typeof(DocVoyageAccountDisbursementAmountCollection))]
	sealed class DocVoyageAccountDisbursmentAmountCollectionTest : DocBaseWrapperCollectionTest<DocVoyageAccountDisbursementAmountCollection>
	{
		#region Implementation

		protected override DocVoyageAccountDisbursementAmountCollection GetNewDocumentWrapperCollection()
		{
			return new DocVoyageAccountDisbursementAmountCollection(Factory);
		}

		protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			DocVoyageAccountDisbursementAmount amount = DocVoyageAccountDisbursementAmount.New("Title", Money.Empty, Factory);
			collection.Add(amount);
			return amount;
		}

		protected override object GetNewObjectToWrap()
		{
			return null;
		}

		#endregion
	}
}
