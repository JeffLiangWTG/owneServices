using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing.ImportSupportingDocumentsFields
{
	[TestedType(typeof(ImportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>))]
	sealed class ImportSupportingDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ImportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>, JobDeclaration, ImportSupportingDocumentsFieldsControlBag>
	{
		protected override ImportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new ImportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();

		protected override int ExpectedMaxColumns => 1;
	}
}
