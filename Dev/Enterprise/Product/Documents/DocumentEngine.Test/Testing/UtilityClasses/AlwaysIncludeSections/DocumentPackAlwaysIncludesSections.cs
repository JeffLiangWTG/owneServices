using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	/// <summary>
	/// Please DO NOT USE this class outside of the DocumentEngine solution, it's going to be made internal.
	/// </summary>
	public class DocumentPackAlwaysIncludesSections : DocumentPack //TODO: Make this internal, make DocumentWrappers.BaseRunDocumentsTest descend from DocumentTemplateTestCase.
	{
		public DocumentPackAlwaysIncludesSections(DocumentCommand documentCommand, IDocumentSupportable parentBusinessObject, UserControlProviderList userFieldList)
			: base(documentCommand, parentBusinessObject, userFieldList, null)
		{
		}

		protected override Report GetNewReport(Enterprise.ExcelTemplates.ExcelTemplate template, DataProviderList docDataProvider, string reportName, Enterprise.DocumentEngine.RuntimeOptions.UserControlProviderList userDefinedFieldValueList, DocumentDirection direction, bool isPasswordProtectedForModifying, bool isPasswordProtectedForOpening, ZGuid menuTemplatePivotPK)
		{
			return new ReportAlwaysIncludesSections(this, template, docDataProvider, reportName, userDefinedFieldValueList, direction, isPasswordProtectedForModifying, isPasswordProtectedForOpening);
		}
	}

	[TestedType(typeof(DocumentPackAlwaysIncludesSections))]
	sealed class DocumentPackAlwaysIncludesSectionsTest : NonPersistentBusinessObjectCollectionTestCase<DocumentPackAlwaysIncludesSections>
	{
		protected override DocumentPackAlwaysIncludesSections GetCollectionToTest()
		{
			return new DocumentPackAlwaysIncludesSections(Factory.New<DocumentCommand>(), null, new UserControlProviderList());
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new Report(new DocumentPack(), null);
		}
	}
}
