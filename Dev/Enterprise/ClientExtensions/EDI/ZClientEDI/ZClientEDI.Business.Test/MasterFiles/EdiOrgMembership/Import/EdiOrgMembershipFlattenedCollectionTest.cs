using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiOrgMembershipFlattenedCollection))]
	public class EdiOrgMembershipFlattenedCollectionTest : NonPersistentBusinessObjectCollectionTestCase<EdiOrgMembershipFlattenedCollection>
	{
		protected override EdiOrgMembershipFlattenedCollection GetCollectionToTest()
		{
			return new EdiOrgMembershipFlattenedCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EdiOrgMembershipFlattened();
		}
	}
}
