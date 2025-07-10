using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.Module.Testing
{
	[TestedType(typeof(MembershipFilter))]
	public class MembershipFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestMembershipTypeList()
		{
			Assert(new MembershipFilter().MembershipTypeList.Count > 0);
		}

		public void TestDescription()
		{
			AssertEquals("Has Membership", new MembershipFilter().Description);
		}

		public void TestFilter()
		{
			var types = new CodeDescriptionBoolCollection(10);
			types.Add("MEM1", (NoResString)"desc1", true);
			types.Add("MEM2", (NoResString)"desc2", false);
			types.Add("MEM3", (NoResString)"desc3", false);

			EDIDataRegistry.Instance.OrgMembershipTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, types);
			var filter = new MembershipFilter();

			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			var org3 = Factory.NewWithValidTestData<EDIOrgHeader>();

			Factory.Save();

			var mem1 = org1.Memberships.AddNew();
			mem1.EOR_MembershipType = "MEM1";
			mem1.EOR_ValidFrom = new ZDate(2022, 1, 1);
			mem1.EOR_ValidTo = new ZDate(2022, 2, 23);
			mem1.EOR_OH_Organisation = org2.PK;

			var mem2 = org1.Memberships.AddNew();
			mem2.EOR_MembershipType = "MEM2";
			mem2.EOR_ValidFrom = new ZDate(2022, 1, 1);
			mem2.EOR_ValidTo = new ZDate(2022, 2, 23);

			var mem3 = org1.Memberships.AddNew();
			mem3.EOR_MembershipType = "MEM3";
			mem3.EOR_ValidFrom = new ZDate(2023, 1, 1);
			mem3.EOR_ValidTo = new ZDate(2023, 2, 23);

			var mem4 = org3.Memberships.AddNew();
			mem4.EOR_MembershipType = "MEM1";
			mem4.EOR_ValidFrom = new ZDate(2022, 1, 1);
			mem4.EOR_ValidTo = new ZDate(2022, 2, 23);
			mem4.EOR_OH_Organisation = org3.PK;

			var mem5 = org2.Memberships.AddNew();
			mem5.EOR_MembershipType = "MEM1";
			mem5.EOR_ValidFrom = new ZDate(2023, 1, 1);
			mem5.EOR_ValidTo = new ZDate(2023, 2, 23);
			mem5.EOR_OH_Organisation = org3.PK;

			var mem6 = org3.Memberships.AddNew();
			mem6.EOR_MembershipType = "MEM2";
			mem6.EOR_ValidFrom = new ZDate(2020, 1, 1);
			mem6.EOR_ValidTo = new ZDate(2020, 2, 23);

			Factory.Save();

			filter.MembershipType = "MEM1";
			filter.ValidFromFrom = new ZDate(2020, 1, 1);
			filter.ValidFromTo = new ZDate(2020, 2, 2);
			filter.Organisation = org2.PK;

			var result = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertEquals(0, result.Length);

			filter.ValidFromFrom = new ZDate(2022, 1, 1);
			filter.ValidFromTo = new ZDate(2022, 2, 2);

			result = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(org1.PK, result[0].PK);

			filter.ValidToFrom = new ZDate(2022, 1, 1);
			filter.ValidToTo = new ZDate(2022, 2, 2);

			result = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertEquals(0, result.Length);

			filter.ValidToTo = new ZDate(2022, 3, 3);

			result = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(org1.PK, result[0].PK);

			filter.Organisation = org3.PK;
			result = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(org3.PK, result[0].PK);

			filter.Organisation = ZGuid.Empty;
			filter.MembershipType = "MEM2";
			filter.ValidFromFrom = ZDate.Empty;
			filter.ValidFromTo = ZDate.Empty;
			filter.ValidToFrom = ZDate.Empty;
			filter.ValidToTo = ZDate.Empty;

			result = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertEquals(2, result.Length);
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK, org3.PK }, result.Select(o => o.PK));

			filter.ValidFromFrom = new ZDate(2020, 1, 1);
			result = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertEquals(2, result.Length);
			AssertContainsExactElementsInAnyOrder(new[] { org1.PK, org3.PK }, result.Select(o => o.PK));
		}

		public void TestOrganisation_ReadOnly()
		{
			var types = new CodeDescriptionBoolCollection(10);
			types.Add("MEM1", (NoResString)"desc1", true);
			types.Add("MEM2", (NoResString)"desc2", false);

			EDIDataRegistry.Instance.OrgMembershipTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, types);
			var filter = new MembershipFilter();
			AssertEquals(true, filter.Organisation_ReadOnly);

			filter.MembershipType = "MEM2";
			AssertEquals(true, filter.Organisation_ReadOnly);

			filter.MembershipType = "MEM1";
			AssertEquals(false, filter.Organisation_ReadOnly);
		}

		public void TestValidateOrganisation()
		{
			var types = new CodeDescriptionBoolCollection(10);
			types.Add("MEM1", (NoResString)"desc1", true);
			types.Add("MEM2", (NoResString)"desc2", false);

			EDIDataRegistry.Instance.OrgMembershipTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, types);

			var filter = new MembershipFilter();
			filter.MembershipType = "MEM1";
			filter.Organisation = ZGuid.NewZGuid();
			AssertHasErrors(filter.OrganisationInfo);

			filter.Organisation = Factory.LoadTop1<EDIOrgHeader>(new ZQuery()).PK;
			AssertNoErrors(filter.OrganisationInfo);
		}

		public void TestValidateValidFromFrom()
		{
			var filter = new MembershipFilter();
			AssertNoErrors(filter.ValidFromFromInfo);

			filter.ValidFromTo = new ZDate(2025, 1, 1);
			AssertNoErrors(filter.ValidFromFromInfo);

			filter.ValidFromFrom = new ZDate(2024, 1, 1);
			AssertNoErrors(filter.ValidFromFromInfo);

			filter.ValidFromFrom = new ZDate(2026, 1, 1);
			AssertHasErrors(filter.ValidFromFromInfo);
		}

		public void TestValidateValidFromTo()
		{
			var filter = new MembershipFilter();
			AssertNoErrors(filter.ValidFromToInfo);

			filter.ValidFromTo = new ZDate(2025, 1, 1);
			AssertNoErrors(filter.ValidFromToInfo);

			filter.ValidFromFrom = new ZDate(2024, 1, 1);
			AssertNoErrors(filter.ValidFromToInfo);

			filter.ValidFromTo = new ZDate(2023, 1, 1);
			AssertHasErrors(filter.ValidFromToInfo);
		}

		public void TestValidateValidToFrom()
		{
			var filter = new MembershipFilter();
			AssertNoErrors(filter.ValidToFromInfo);

			filter.ValidToTo = new ZDate(2025, 1, 1);
			AssertNoErrors(filter.ValidToToInfo);

			filter.ValidToFrom = new ZDate(2024, 1, 1);
			AssertNoErrors(filter.ValidToFromInfo);

			filter.ValidToFrom = new ZDate(2026, 1, 1);
			AssertHasErrors(filter.ValidToFromInfo);
		}

		public void TestValidateValidToTo()
		{
			var filter = new MembershipFilter();
			AssertNoErrors(filter.ValidToToInfo);

			filter.ValidToTo = new ZDate(2025, 1, 1);
			AssertNoErrors(filter.ValidToToInfo);

			filter.ValidFromFrom = new ZDate(2024, 1, 1);
			AssertNoErrors(filter.ValidToToInfo);

			filter.ValidToTo = new ZDate(2023, 1, 1);
			AssertHasErrors(filter.ValidToToInfo);
		}

		public void TestNoOverlap()
		{
			var filter = new MembershipFilter();

			filter.ValidToFrom = new ZDate(2020, 1, 1);
			filter.ValidToTo = new ZDate(2021, 1, 1);
			filter.ValidFromFrom = new ZDate(2022, 1, 1);
			filter.ValidFromTo = new ZDate(2022, 5, 5);
			AssertHasErrors(filter.ValidFromFromInfo);

			filter.ValidFromFrom = new ZDate(2021, 1, 1);
			AssertNoErrors(filter.ValidFromFromInfo);

			filter.ValidFromTo = ZDate.Empty;
			AssertNoErrors(filter.ValidFromFromInfo);
		}

		public void TestMembershipType()
		{
			var types = new CodeDescriptionBoolCollection(10);
			types.Add("MEM1", (NoResString)"desc1", true);
			types.Add("MEM2", (NoResString)"desc2", false);
			EDIDataRegistry.Instance.OrgMembershipTypes.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, types);

			var filter = new MembershipFilter();
			filter.MembershipType = "MEM1";
			AssertNoErrors(filter.MembershipTypeInfo);

			filter.MembershipType = "MEM3";
			AssertHasErrors(filter.MembershipTypeInfo);

			filter.MembershipType = "MEM2";
			AssertNoErrors(filter.MembershipTypeInfo);
		}

		public void TestAgreementVersionField()
		{
			var org1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org1.Memberships.Add(Factory.NewWithValidTestData<EdiOrgMembership>());
			org1.Memberships[0].SetPropertyValue("EOR_AgreementVersion", ZDate.Today);

			var org2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			org2.Memberships.Add(Factory.NewWithValidTestData<EdiOrgMembership>());
			org2.Memberships[0].SetPropertyValue("EOR_AgreementVersion", ZDate.Today.AddDays(10));
			Factory.Save();

			var filter = new MembershipFilter();
			filter.AgreementVersionFrom = ZDate.Today.AddDays(-3);
			filter.AgreementVersionTo = ZDate.Today.AddDays(3);

			var results = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertCollectionContains(org1, results);
			AssertCollectionNotContains(org2, results);

			filter.AgreementVersionFrom = ZDate.Today.AddDays(8);
			filter.AgreementVersionTo = ZDate.Today.AddDays(12);

			results = Factory.Load<EDIOrgHeader>(filter.Query);
			AssertCollectionNotContains(org1, results);
			AssertCollectionContains(org2, results);
		}
	}
}
