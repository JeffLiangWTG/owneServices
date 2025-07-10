using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.MasterFiles.Business.Test
{
	internal class EdiOrgMembershipValidationTest : BusinessObjectValidationTestCase
	{
		public void TestMembershipType()
		{
			var allowed = new CodeDescriptionBoolCollection(10);
			allowed.Add("MEM1", (NoResString)"desc1", true);
			allowed.Add("MEM2", (NoResString)"desc2", true);
			EDIDataRegistry.Instance.OrgMembershipTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, allowed);
			var org = Factory.New<EDIOrgHeader>();
			var memberships = org.Memberships;
			var item1 = memberships.AddNew();
			item1.EOR_MembershipType = "MEM1";
			AssertNoNotifications(item1.EOR_MembershipTypeInfo);
			item1.EOR_MembershipType = "MEM2";
			AssertNoNotifications(item1.EOR_MembershipTypeInfo);
			item1.EOR_MembershipType = "bad";
			AssertHasErrors(item1.EOR_MembershipTypeInfo);
			item1.EOR_MembershipType = "";
			AssertHasErrors(item1.EOR_MembershipTypeInfo);
		}

		public void TestValidTo()
		{
			var org = Factory.New<EDIOrgHeader>();
			var memberships = org.Memberships;
			var item1 = memberships.AddNew();
			item1.EOR_MembershipType = "FTA";
			item1.EOR_ValidFrom = new ZDate(2018, 4, 1);

			var item2 = memberships.AddNew();
			item2.EOR_MembershipType = "FOO";
			item2.EOR_ValidFrom = new ZDate(2018, 5, 1);

			item1.Validation.ValidateEOR_ValidTo();
			item2.Validation.ValidateEOR_ValidTo();
			AssertNoErrors(item1.EOR_ValidToInfo);
			AssertNoErrors(item2.EOR_ValidToInfo);

			item2.EOR_MembershipType = "FTA";
			item1.Validation.ValidateEOR_ValidTo();
			item2.Validation.ValidateEOR_ValidTo();
			AssertHasErrors(item1.EOR_ValidToInfo);
			AssertHasErrors(item2.EOR_ValidToInfo);

			item1.EOR_ValidTo = new ZDate(2018, 4, 30);
			item2.Validation.ValidateEOR_ValidTo();
			AssertNoErrors(item1.EOR_ValidToInfo);
			AssertNoErrors(item2.EOR_ValidToInfo);
		}
	}
}
