using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MiscOrganisation))]
	sealed class MiscOrganisationTest : SystemDefinedOrganisationTestCase
	{
		protected override string OrgCodeForTesting
		{
			get { return "MISC"; }
		}

		public void TestMiscOrgIsCreatedIfNotExists()
		{
			ZQuery filter = new ZQuery(OrgHeaderSchema.OH_Code, "MISC");
			BusinessObject misc = (BusinessObject)Factory.LoadTop1<IOrgHeader>(filter);
			if (misc != null)
			{
				misc.Delete();
				Factory.Save();
			}
			misc = (BusinessObject)Factory.LoadTop1<IOrgHeader>(filter);

			AssertEquals(null, misc);

			SystemDefinedOrganisation.ClearOrgPKCacheForTest();

			MiscOrganisation org = new MiscOrganisation(Factory);
			AssertNotEquals(ZGuid.Empty, org.Organisation);

			BusinessObjectFactory factory = new BusinessObjectFactory();
			misc = (BusinessObject)factory.LoadTop1<IOrgHeader>(filter);
			AssertNotEquals(null, misc);

			AssertEquals(misc["OH_IsActive"], true);
			AssertEquals(misc["OH_FullName"], "MISCELLANEOUS ORGANISATION (SYSTEM DEFINED)");
			AssertEquals(misc["OH_RL_NKClosestPort"], "AUSYD");
			AssertEquals(misc["OH_Language"], Core.SharedConstants.Languages.English);
			AssertEquals(misc["OH_IsConsignee"], true);
			AssertEquals(misc["OH_IsConsignor"], true);
			AssertEquals(misc["OH_IsTransportClient"], true);
			AssertEquals(misc["OH_IsWarehouseClient"], true);
			AssertEquals(misc["OH_IsForwarder"], true);
			AssertEquals(misc["OH_IsShippingProvider"], true);
			AssertEquals(misc["OH_IsBroker"], true);
			AssertEquals(misc["OH_IsMiscFreightServices"], true);
			AssertEquals(misc["OH_IsCompetitor"], true);
			AssertEquals(misc["OH_IsSalesLead"], true);
			AssertEquals(misc["OH_Code"], "MISC");

			BusinessObject adr = misc["MainAddress"] as BusinessObject;
			AssertNotNull(adr);

			AssertEquals(adr["OA_IsActive"], true);
			AssertEquals(adr["OA_Code"], "OFC: NO ADDRESS SPECIFIED");
			AssertEquals(adr["OA_Language"], Core.SharedConstants.Languages.English);
			AssertEquals(adr["OA_Address1"], "NO ADDRESS SPECIFIED");
			AssertEquals(adr["OA_Address2"], "SYSTEM DEFINED ORGANISATION");
			AssertEquals(adr["OA_City"], "NA");
			AssertEquals(adr["OA_State"], "NSW");

			Type orgPatternMatchType = ObjectFactory.GetType<Enterprise.MasterFiles.Integration.IOrgPatternMatch>();
			ZQuery query = new ZQuery(OrgPatternMatchSchema.OS_OH, misc.PK);
			query.AddToFilter(JoinCondition.And, OrgPatternMatchSchema.OS_OA, adr.PK);
			BusinessObject orgPatternMatch = factory.LoadTop1(orgPatternMatchType, query);

			AssertNotNull(orgPatternMatch);
			AssertEquals(orgPatternMatch["OS_FullCompanyName"], "MISCELLANEOUS ORGANISATION SYSTEM DEFINED");
			AssertEquals(orgPatternMatch["OS_CompanyName1"], "M245");
			AssertEquals(orgPatternMatch["OS_CompanyName2"], "O625");
			AssertEquals(orgPatternMatch["OS_CompanyName3"], "S235");
			AssertEquals(orgPatternMatch["OS_CompanyName4"], "D153");
			AssertEquals(orgPatternMatch["OS_Address1"], "A362");
			AssertEquals(orgPatternMatch["OS_Address2"], "S121");
			AssertEquals(orgPatternMatch["OS_Address3"], "S235");
			AssertEquals(orgPatternMatch["OS_Address4"], "D153");
			AssertEquals(orgPatternMatch["OS_City"], "N000");
			AssertEquals(orgPatternMatch["OS_State"], "N200");
			AssertEquals(orgPatternMatch["OS_UNLOCO"], "AUSYD");
		}
	}
}
