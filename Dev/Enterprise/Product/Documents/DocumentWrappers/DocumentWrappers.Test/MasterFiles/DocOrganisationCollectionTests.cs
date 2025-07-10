using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrganisationCollection))]
	sealed class DocOrganisationCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocOrganisationCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgHeader = Factory.New<OrgHeader>();
			return DocOrganisation.New(orgHeader, Factory);
		}

		protected override DocOrganisationCollection GetCollectionToTest()
		{
			return new DocOrganisationCollection(Factory);
		}
	}
}
