using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocOrgARTermsCollection))]
	public class DocOrgARTermsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocOrgARTermsCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			OrgARTerms arTerms = Factory.NewWithValidTestData<OrgARTerms>();
			return DocOrgARTerms.New(arTerms, Factory);
		}

		protected override DocOrgARTermsCollection GetCollectionToTest()
		{
			return new DocOrgARTermsCollection(Factory);
		}
	}
}
