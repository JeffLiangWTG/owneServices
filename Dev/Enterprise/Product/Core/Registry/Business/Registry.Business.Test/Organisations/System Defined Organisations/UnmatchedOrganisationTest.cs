using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(UnmatchedOrganisation))]
	sealed class UnmatchedOrganisationTest : SystemDefinedOrganisationTestCase
	{
		protected override string OrgCodeForTesting
		{
			get { return "UNMATCHED"; }
		}

		public void TestOrganisationCreateUnmatched()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, "UNMATCHED");
			BusinessObject unmatched = (BusinessObject)Factory.LoadTop1<IOrgHeader>(filter);
			if (unmatched != null)
			{
				unmatched.Delete();
				Factory.Save();
			}
			unmatched = (BusinessObject)Factory.LoadTop1<IOrgHeader>(filter);

			AssertEquals(null, unmatched);
			AssertNotEquals(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation, ZGuid.Empty);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			unmatched = (BusinessObject)factory.LoadTop1<IOrgHeader>(filter);

			AssertNotEquals(null, unmatched);

			Type orgHeaderType = ObjectFactory.GetType<IOrgHeader>();

			BusinessObject org = factory.LoadTop1(orgHeaderType, new ZQuery(OrgHeaderSchema.OH_Code, "UNMATCHED"));

			AssertNotNull(org);

			AssertEquals(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation, org.PK);

			AssertEquals(orgHeaderType.GetProperty("OH_IsActive").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_FullName").GetValue(org, null), "UNMATCHED ORGANISATION");
			AssertEquals(orgHeaderType.GetProperty("OH_RL_NKClosestPort").GetValue(org, null), "AUSYD");
			AssertEquals(orgHeaderType.GetProperty("OH_Language").GetValue(org, null), Core.SharedConstants.Languages.English);
			AssertEquals(orgHeaderType.GetProperty("OH_IsConsignee").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsConsignor").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsTransportClient").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsWarehouseClient").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsForwarder").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsShippingProvider").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsBroker").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsMiscFreightServices").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsCompetitor").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_IsSalesLead").GetValue(org, null), true);
			AssertEquals(orgHeaderType.GetProperty("OH_Code").GetValue(org, null), "UNMATCHED");

			Type orgAddressType = ObjectFactory.GetType<IOrgAddress>();
			object adr = orgHeaderType.GetProperty("MainAddress").GetValue(org, null);
			AssertNotNull(adr);

			AssertEquals(orgAddressType.GetProperty("OA_IsActive").GetValue(adr, null), true);
			AssertEquals(orgAddressType.GetProperty("OA_Code").GetValue(adr, null), "OFC: NO ADDRESS SPECIFIED");
			AssertEquals(orgAddressType.GetProperty("OA_Language").GetValue(adr, null), Core.SharedConstants.Languages.English);
			AssertEquals(orgAddressType.GetProperty("OA_Address1").GetValue(adr, null), "NO ADDRESS SPECIFIED");
			AssertEquals(orgAddressType.GetProperty("OA_Address2").GetValue(adr, null), "PLEASE SEE ATTACHED NOTE");
			AssertEquals(orgAddressType.GetProperty("OA_City").GetValue(adr, null), "NA");
			AssertEquals(orgAddressType.GetProperty("OA_State").GetValue(adr, null), "NSW");

			Type orgAddressCapabilityType = ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IOrgAddressCapability>();
			BusinessObject adrCap = Factory.LoadTop1(orgAddressCapabilityType, new ZQuery(OrgAddressCapabilitySchema.PZ_OA, orgAddressType.GetProperty("PK").GetValue(adr, null)));

			AssertNotNull(adrCap);
			AssertEquals(orgAddressCapabilityType.GetProperty("PZ_AddressType").GetValue(adrCap, null), "OFC");
			AssertEquals(orgAddressCapabilityType.GetProperty("PZ_IsMainAddress").GetValue(adrCap, null), true);

			StmNote orgNote = Factory.LoadTop1<StmNote>(new ZQuery(StmNoteSchema.ST_ParentID, org.PK).AddToFilter(StmNoteSchema.ST_Table, org.TableName));

			AssertEquals(orgNote.ST_Table, "OrgHeader");
			AssertEquals(orgNote.ST_Description, "Unmatched Org Details");
			AssertEquals(orgNote.ST_NoteText, "This organization is a system-generated default organization, used for the purposes of matching when an organization cannot be found in the system during a data import. This organization will be used as a place holder for a consignee, consignor or broker, instead of creating temporary organizations, when an organization match is not available. This feature may be disabled in the system registry by your administrator.");
			AssertEquals(orgNote.ST_NoteType, "PRV");

			Type orgPatternMatchType = ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IOrgPatternMatch>();
			ZQuery query = new ZQuery(OrgPatternMatchSchema.OS_OH, org.PK);
			query.AddToFilter(JoinCondition.And, OrgPatternMatchSchema.OS_OA, orgAddressType.GetProperty("PK").GetValue(adr, null));
			BusinessObject orgPatternMatch = Factory.LoadTop1(orgPatternMatchType, query);

			AssertNotNull(orgPatternMatch);
			AssertEquals(orgPatternMatchType.GetProperty("OS_FullCompanyName").GetValue(orgPatternMatch, null), "UNMATCHED ORGANISATION");
			AssertEquals(orgPatternMatchType.GetProperty("OS_CompanyName1").GetValue(orgPatternMatch, null), "U532");
			AssertEquals(orgPatternMatchType.GetProperty("OS_CompanyName2").GetValue(orgPatternMatch, null), "O625");
			AssertEquals(orgPatternMatchType.GetProperty("OS_IsCorporation").GetValue(orgPatternMatch, null), false);
			AssertEquals(orgPatternMatchType.GetProperty("OS_IsPOBox").GetValue(orgPatternMatch, null), false);
			AssertEquals(orgPatternMatchType.GetProperty("OS_Address1").GetValue(orgPatternMatch, null), "A362");
			AssertEquals(orgPatternMatchType.GetProperty("OS_Address2").GetValue(orgPatternMatch, null), "S121");
			AssertEquals(orgPatternMatchType.GetProperty("OS_City").GetValue(orgPatternMatch, null), "N000");
			AssertEquals(orgPatternMatchType.GetProperty("OS_State").GetValue(orgPatternMatch, null), "N200");
			AssertEquals(orgPatternMatchType.GetProperty("OS_UNLOCO").GetValue(orgPatternMatch, null), "AUSYD");
		}
	}
}
