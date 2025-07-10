namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	// This is outside the DEBUG bit to make sure the namespace is always available even if the classes are not.
	public enum TestFilesSubFolder { ReportTestFiles = 1, DocumentTestFiles = 2, AreaTestFiles = 3, CustomizableDocumentTemplates = 4, ExpectedDocuments = 5 }
	// That makes sure that using statements outside DEBUG directives don't explode on a release build.
}
