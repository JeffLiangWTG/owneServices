using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Customs.EU.EMCS.Testing
{
	[TestedType(typeof(EMCSInvoiceLineCollection))]
	sealed class EMCSInvoiceLineCollectionTest : DocumentWrappers.Testing.DocBaseWrapperCollectionTest<EMCSInvoiceLineCollection>
	{
		protected override DocumentWrapper AddNewDocumentWrapperToCollection(DocumentWrapperCollection collection)
		{
			var wrapper = EMCSInvoiceLine.New(declaration.InvoiceHeader.InvoiceLines.AddNew(), Factory);
			collection.Add(wrapper);
			return wrapper;
		}

		protected override EMCSInvoiceLineCollection GetNewDocumentWrapperCollection()
		{
			declaration = Factory.New<EMCSJobDeclaration>();
			var wrapper = EMCSInvoiceLine.New(declaration.InvoiceHeader.InvoiceLines.AddNew(), Factory);
			return new EMCSInvoiceLineCollection(declaration.InvoiceLines, Factory);
		}

		protected override object GetNewObjectToWrap() => null;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
		}
		EMCSJobDeclaration declaration;
	}
}
