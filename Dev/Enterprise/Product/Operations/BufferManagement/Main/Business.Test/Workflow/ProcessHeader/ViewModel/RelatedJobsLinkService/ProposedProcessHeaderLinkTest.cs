using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	[TestedType(typeof(ProposedProcessHeaderLink))]
	class ProposedProcessHeaderLinkTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ProposedProcessHeaderLink(fromJobHeader, toJobHeader);
		}

		protected override void SetUp()
		{
			base.SetUp();

			fromJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			toJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
		}

		ProcessJobHeader fromJobHeader;
		ProcessJobHeader toJobHeader;

		#endregion
	}
}
