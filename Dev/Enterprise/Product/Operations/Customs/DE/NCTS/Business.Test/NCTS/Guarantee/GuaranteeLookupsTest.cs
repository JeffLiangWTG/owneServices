using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using EUGuaranteeTypeList = Enterprise.Customs.EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	sealed class GuaranteeLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestBondTypeList()
		{
			header.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.TIR;
			CombineAssertions(() =>
			{
				var list = lookups.BondTypeList;
				AssertSame("Cached", new GuaranteeLookups(guarantee).BondTypeList, list);
				AssertEquals("BM_InBondEntryType is 'TIR'", "B", list.CodesAsString);

				header.MovementHeader.BM_InBondEntryType = NctsDeclarationTypeList.Codes.T1;
				header.MovementHeader.IsSimplifiedNctsProcedure = true;
				AssertEquals("BM_InBondEntryType isn't 'TIR' and IsSimplifiedNctsProcedure is true", "0, 1", lookups.BondTypeList.CodesAsString);

				header.MovementHeader.IsSimplifiedNctsProcedure = false;
				AssertEquals("BM_InBondEntryType isn't 'TIR' and IsSimplifiedNctsProcedure is false", "0, 1, 2, 3, 4, 8, B, R", lookups.BondTypeList.CodesAsString);
			});
		}

		public void TestReferenceNumbers()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			CreateGuaranteeHeader(EUGuaranteeTypeList.Codes.TRA, "DETRA01", GuaranteeSubTypeList.Codes._1, Core.Constants.CountryCodes.Germany);
			CreateGuaranteeHeader(EUGuaranteeTypeList.Codes.TRA, "DETRA02", GuaranteeSubTypeList.Codes._2, Core.Constants.CountryCodes.Germany);
			CreateGuaranteeHeader(EUGuaranteeTypeList.Codes.TRA, "LVTRA01", GuaranteeSubTypeList.Codes._1, Core.Constants.CountryCodes.Latvia);
			CreateGuaranteeHeader("COM", "DECOM01", GuaranteeSubTypeList.Codes._1, Core.Constants.CountryCodes.Germany);
			Factory.Save();

			guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._1;
			var list = lookups.ReferenceNumbers;

			CombineAssertions(() =>
			{
				AssertSame("Cached", list, lookups.ReferenceNumbers);
				AssertArrayEqualsByElements("PW_BondType 1", new ZString[] { "DETRA01" }, list.Select(x => x.CPH_Number).ToArray());

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._2;
				AssertArrayEqualsByElements("PW_BondType 2", new ZString[] { "DETRA02" }, lookups.ReferenceNumbers.Select(x => x.CPH_Number).ToArray());
			});

			CusGuaranteeHeader CreateGuaranteeHeader(ZString type, ZString number, ZString subType, ZString countryCode)
			{
				var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
				result.CPH_Type = type;
				result.CPH_Number = number;
				result.CPH_OH_PermitHolder = orgHeader.PK;
				result.CPH_RN_NKCountryCode = countryCode;
				result.CPH_SubType = subType;
				return result;
			}
		}

		public void TestReferenceNumbers_FilterBusinessObjectDefaults()
		{
			ZString bondType = NctsGuaranteeTypeList.Codes._1;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			guarantee.PW_BondType = bondType;
			guarantee.NctsHeader.Principal.OrganisationPK = orgHeader.PK;

			CombineAssertions(() =>
			{
				var filters = lookups.ReferenceNumbers.FilterBusinessObjectDefaults.Cast<FilterBusinessObjectDefault>().ToArray();
				AssertCollectionContains("GuaranteeSubType", filters, f => f.FilterName == CusGuaranteeHeaderCollection.FilterConstants.GuaranteeSubType && f.Value.Equals(bondType));
				AssertCollectionContains("GuaranteeHolders", filters, f => f.FilterName == CusGuaranteeHeaderCollection.FilterConstants.GuaranteeHolders && f.Value.Equals(orgHeader.PK));
			});
		}

		public void TestSingleReferenceNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			CreateGuaranteeHeader(orgHeader, "DETRA01", GuaranteeSubTypeList.Codes._1);
			CreateGuaranteeHeader(orgHeader, "DETRA02", GuaranteeSubTypeList.Codes._1);
			CreateGuaranteeHeader(orgHeader, "DETRA03", GuaranteeSubTypeList.Codes._2);
			Factory.Save();

			CombineAssertions(() =>
			{
				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._1;
				AssertEquals("Multiple reference numbers", null, lookups.SingleReferenceNumber);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._2;
				AssertEquals("Single reference numbers", "DETRA03", lookups.SingleReferenceNumber.CPH_Number);

				guarantee.PW_BondType = NctsGuaranteeTypeList.Codes._3;
				AssertEquals("No reference numbers", null, lookups.SingleReferenceNumber);
			});
		}

		public void TestAccessCodeList()
		{
			CombineAssertions(() =>
			{
				var list = lookups.AccessCodeList;
				AssertSame("Cached", new GuaranteeLookups(guarantee).AccessCodeList, list);
				AssertEquals("Values", ZString.Empty, list.CodesAsString);
			});
		}

		public void TestAccessCodeList_RuleCodePIN()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			header.Principal.E2_OA_Address = orgHeader.MainAddress.PK;
			header.Principal.Address.OA_OH = orgHeader.PK;
			var guaranteeHeader1 = CreateGuaranteeHeader(orgHeader, "DETRA01", GuaranteeSubTypeList.Codes._0);
			var guaranteeHeader2 = CreateGuaranteeHeader(orgHeader, "DETRA01", GuaranteeSubTypeList.Codes._1);

			var guaranteeRule1 = guaranteeHeader2.AdditionalAccessCodes.AddNew();
			guaranteeRule1.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule1.CPR_ValueFrom = "123";
			guaranteeRule1.CPR_Description = "abc";

			var guaranteeRule2 = guaranteeHeader2.AdditionalAccessCodes.AddNew();
			guaranteeRule2.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule2.CPR_ValueFrom = "456";
			guaranteeRule2.CPR_Description = "def";

			var guaranteeRule3 = guaranteeHeader2.AdditionalAccessCodes.AddNew();
			guaranteeRule3.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.AccessCodePinPersonalIdentificationNumber;
			guaranteeRule3.CPR_ValueFrom = "789";
			guaranteeRule3.CPR_Description = "xyz";

			var guaranteeRule4 = guaranteeHeader2.AdditionalAccessCodes.AddNew();
			guaranteeRule4.CPR_RuleCode = PermitRuleCodeListForAccessCodes.Codes.DefaultAccessCodeDefaultPin;
			guaranteeRule4.CPR_ValueFrom = "555";
			guaranteeRule4.CPR_Description = "xxx";

			Factory.Save();

			guarantee.PW_BondType = GuaranteeSubTypeList.Codes._1;
			guarantee.PW_BondNumber = guaranteeHeader2.CPH_Number;
			guarantee.PW_CPH_Guarantee = guaranteeHeader2.PK;
			var lookup = new GuaranteeLookups(guarantee);
			CombineAssertions(() =>
			{
				var list = lookup.AccessCodeList;
				AssertSame("Cached", new GuaranteeLookups(guarantee).AccessCodeList, list);
				AssertEquals("Values", "123 - abc\r\n456 - def\r\n789 - xyz", list.ElementsAsString);
			});
		}

		CusGuaranteeHeader CreateGuaranteeHeader(OrgHeader orgHeader, ZString number, ZString subType)
		{
			var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			result.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			result.CPH_Number = number;
			result.CPH_OH_PermitHolder = orgHeader.PK;
			result.CPH_SubType = subType;
			return result;
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			guarantee = header.MovementHeader.Guarantees.AddNew();
			lookups = new GuaranteeLookups(guarantee);
		}
		NctsHeader header;
		Guarantee guarantee;
		GuaranteeLookups lookups;
	}
}
