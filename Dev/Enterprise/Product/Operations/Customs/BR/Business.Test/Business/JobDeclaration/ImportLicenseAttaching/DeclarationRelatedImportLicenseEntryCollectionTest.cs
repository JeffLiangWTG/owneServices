using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(DeclarationRelatedImportLicenseEntryCollection))]
	class DeclarationRelatedImportLicenseEntryCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new DeclarationRelatedImportLicenseEntryCollection(Factory.New<JobDeclaration>());
	}
}
