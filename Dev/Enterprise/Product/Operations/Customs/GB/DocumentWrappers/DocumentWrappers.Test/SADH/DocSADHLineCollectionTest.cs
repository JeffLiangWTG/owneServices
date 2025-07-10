using NUnit.Framework;

namespace Enterprise.Customs.GB.DocumentWrappers.Testing
{
	[TestedType(typeof(DocSADHLineCollection))]
	class DocSADHLineCollectionTest : Enterprise.DocumentWrappers.Customs.EU.Testing.DocSADHLineCollectionTest<DocSADHLineCollection>
	{
		protected override DocSADHLineCollection GetNewDocumentWrapperCollection() => new(EntryLineCollection, Factory);

		protected override string CountryToUseForTesting => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
