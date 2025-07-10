using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(OrganisationWrapperCollection))]
	sealed class OrganisationWrapperCollectionTest : GenericWrapperCollectionTest<OrganisationWrapperCollection>
	{
		#region Implementation

		protected override GenericWrapper GetNewWrapperToAddToTheCollection()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			return new OrganisationWrapper(OrganisationUsageType.Consignee, header, ContactType.LocalClient, Factory);
		}

		protected override OrganisationWrapperCollection GetNewDocumentWrapperCollection()
		{
			return new OrganisationWrapperCollection(Factory);
		}

		#endregion
	}
}
