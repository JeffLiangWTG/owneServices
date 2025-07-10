using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.Client.EDI.MasterFiles.GUI.Testing
{
	[TestedType(typeof(EDIOrgRelatedPartiesModuleFilter))]
	public class EDIOrgRelatedPartiesModuleFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestFilter_RelatedParty()
		{
			RelatedPartyFilter.RelatedParty = party1.PK;
			filteredOrgs.Load(RelatedPartyFilter.Query);

			AssertCollectionContains("should return correct org", org1, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org2, filteredOrgs);

			RelatedPartyFilter.RelatedParty = party2.PK;
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertCollectionContains("should return correct org", org2, filteredOrgs);
			AssertCollectionNotContains("should not return incorrect org", org1, filteredOrgs);
		}

		public void TestFilter_PartyType()
		{
			RelatedPartyFilter.Direction = "";
			RelatedPartyFilter.RelatedParty = party1.PK;
			RelatedPartyFilter.TransportMode = "";
			RelatedPartyFilter.ContainerMode = "";
			RelatedPartyFilter.PartyType = EDIOrgRelatedPartyLookups.WARPConstant;
			Assert("Party Type WRP Valid", !RelatedPartyFilter.HasErrors);
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertContainsExactElementsInAnyOrder("Related Party Type WRP", new[] { org1.PK }, filteredOrgs.Select(x => x.PK));

			RelatedPartyFilter.RelatedParty = party2.PK;
			RelatedPartyFilter.PartyType = EDIOrgRelatedPartyLookups.ContractingPartyCode;
			Assert("Party Type COP Valid", !RelatedPartyFilter.HasErrors);
			filteredOrgs.Load(RelatedPartyFilter.Query);
			AssertContainsExactElementsInAnyOrder("Related Party Type WRP", new[] { org2.PK }, filteredOrgs.Select(x => x.PK));
		}

		public void TestClearAndIsEmpty()
		{
			RelatedPartyFilter.RelatedParty = party1.PK;
			RelatedPartyFilter.PartyType = EDIOrgRelatedPartyLookups.WARPConstant;
			AssertEquals(false, RelatedPartyFilter.IsEmpty);
			AssertEquals(false, RelatedPartyFilter.Query.IsEmpty);

			RelatedPartyFilter.Clear();
			AssertEquals(true, RelatedPartyFilter.IsEmpty);
			AssertEquals(true, RelatedPartyFilter.Query.IsEmpty);

			AssertEquals("", RelatedPartyFilter.PartyType);
			AssertEquals("", RelatedPartyFilter.TransportMode);
			AssertEquals("", RelatedPartyFilter.ContainerMode);
			AssertEquals("", RelatedPartyFilter.Direction);
			AssertEquals(ZGuid.Empty, RelatedPartyFilter.RelatedParty);
		}

		public void TestLists()
		{
			string expected =
				"DLV - Delivery\r\n" +
				"PIC - Pickup\r\n" +
				"PAD - Pickup and Delivery";
			AssertEquals(expected, RelatedPartyFilter.DirectionList.ElementsAsString);
		}

		public void TestShouldCalculateDirection()
		{
			RelatedPartyFilter.PartyType = "MMM";
			AssertEquals(true, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.LocalTransport;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.LocalTransportBillTo;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.CustomsAgentBroker;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.InvoiceCustomsJobsTo;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.InvoiceFreightJobsTo;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ReportRevenueTo;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ControllingCustomer;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = RelatedPartyTypeList.Codes.ClientCFS;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = EDIOrgRelatedPartyLookups.ContractingPartyCode;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);

			RelatedPartyFilter.PartyType = EDIOrgRelatedPartyLookups.WARPConstant;
			AssertEquals(false, RelatedPartyFilter.ShouldCalculateDirection);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EDIOrgRelatedPartiesModuleFilter("Test");
		}

		EDIOrgRelatedPartiesModuleFilter RelatedPartyFilter
		{
			get
			{
				if (fRelatedPartyFilter == null)
				{
					fRelatedPartyFilter = new EDIOrgRelatedPartiesModuleFilter("Test");
				}
				return fRelatedPartyFilter;
			}
		}

		EDIOrgRelatedPartiesModuleFilter fRelatedPartyFilter;

		protected override void SetUp()
		{
			base.SetUp();

			filteredOrgs = new OrgHeaderCollection(Factory);

			org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "AAA";

			org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "BBB";

			party1 = Factory.New<OrgHeader>();
			party1.OH_Code = "Party1";

			party2 = Factory.New<OrgHeader>();
			party2.OH_Code = "Party2";

			relatedParty1 = Factory.New<OrgRelatedParty>();
			relatedParty1.PR_OH_Parent = org1.PK;
			relatedParty1.PR_OH_RelatedParty = party1.PK;
			relatedParty1.PR_PartyType = EDIOrgRelatedPartyLookups.WARPConstant;

			relatedParty2 = Factory.New<OrgRelatedParty>();
			relatedParty2.PR_OH_Parent = org2.PK;
			relatedParty2.PR_OH_RelatedParty = party2.PK;
			relatedParty2.PR_PartyType = EDIOrgRelatedPartyLookups.ContractingPartyCode;

			Factory.Save();
			ResetFilter();
		}

		void ResetFilter()
		{
			RelatedPartyFilter.RelatedParty = ZGuid.Empty;
			RelatedPartyFilter.TransportMode = "";
			RelatedPartyFilter.ContainerMode = "";
			RelatedPartyFilter.Direction = "";
			RelatedPartyFilter.PartyType = "";
		}

		OrgHeaderCollection filteredOrgs;
		OrgHeader org1;
		OrgHeader org2;
		OrgHeader party1;
		OrgHeader party2;
		OrgRelatedParty relatedParty1;
		OrgRelatedParty relatedParty2;

		#endregion
	}
}
