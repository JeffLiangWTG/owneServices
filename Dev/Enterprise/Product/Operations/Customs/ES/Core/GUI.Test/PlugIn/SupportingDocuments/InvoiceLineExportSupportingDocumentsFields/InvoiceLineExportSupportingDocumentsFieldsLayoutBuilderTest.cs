using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(InvoiceLineExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>))]
	public class InvoiceLineExportSupportingDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<InvoiceLineExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>, JobDeclaration, InvoiceLineExportSupportingDocumentsFieldsControlBag>
	{
		protected override InvoiceLineExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new InvoiceLineExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();

		protected override int ExpectedMaxColumns => 1;
	}
}
