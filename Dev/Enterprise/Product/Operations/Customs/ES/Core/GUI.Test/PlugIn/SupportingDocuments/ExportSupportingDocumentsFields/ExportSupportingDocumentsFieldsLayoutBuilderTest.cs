using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.GUI.Testing
{
	[TestedType(typeof(ExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>))]
	public class ExportSupportingDocumentsFieldsLayoutBuilderTest : ColumnLayoutBuilderAbstractTest<ExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>, JobDeclaration, ExportSupportingDocumentsFieldsControlBag>
	{
		protected override ExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration> GetColumnLayoutBuilderForTesting() => new ExportSupportingDocumentsFieldsLayoutBuilder<JobDeclaration>();

		protected override int ExpectedMaxColumns => 1;
	}
}
