using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	public abstract class InvoiceHeaderCollectionTest<TCollection, TJobComInvoiceHeader> : Customs.Business.Testing.BaseInvoiceHeaderCollectionTest<TCollection, TJobComInvoiceHeader>
		where TCollection : InvoiceHeaderActiveCollection
		where TJobComInvoiceHeader : JobComInvoiceHeader
	{
		protected override BaseJobDeclaration GetNewJobDeclaration()
		{
			var dec = JobDeclaration.New(Factory);
			dec.JE_MessageType = MessageTypeList.Codes.Import;
			return dec;
		}
	}
}
