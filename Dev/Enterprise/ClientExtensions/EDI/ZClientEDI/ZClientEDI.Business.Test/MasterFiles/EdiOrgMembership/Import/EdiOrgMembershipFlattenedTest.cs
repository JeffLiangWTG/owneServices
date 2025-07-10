using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	[TestedType(typeof(EdiOrgMembershipFlattened))]
	public class EdiOrgMembershipFlattenedTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSchema()
		{
			AssertEquals(AutoEdiOrgMembershipFlattened.Schema.MembershipTypeMaxLength, EdiOrgMembership.Schema.EOR_MembershipTypeMaxLength);
			AssertEquals(AutoEdiOrgMembershipFlattened.Schema.OrgCodeMaxLength, OrgHeader.Schema.OH_CodeMaxLength);
		}
	}
}
