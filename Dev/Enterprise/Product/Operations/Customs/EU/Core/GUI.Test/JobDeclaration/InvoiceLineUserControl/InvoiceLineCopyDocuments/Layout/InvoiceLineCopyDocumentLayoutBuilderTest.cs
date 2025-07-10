using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineCopyDocumentLayoutBuilder))]
	sealed class InvoiceLineCopyDocumentLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceLineCopyDocumentLayoutBuilder, CopyDocumentsSelectionHeader, InvoiceLineCopyDocumentControlBag>
	{
		protected override InvoiceLineCopyDocumentLayoutBuilder GetColumnLayoutBuilderForTesting()
		{
			return new InvoiceLineCopyDocumentLayoutBuilder();
		}
	}
}
