using System;
using System.Collections.Generic;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(HolderOfTheTransitProcedureProvider))]
	sealed class HolderOfTheTransitProcedureProviderTest : PartyProviderAbstractTest<HolderOfTheTransitProcedureProvider>
	{
		public void TestTirHolderIdentificationNumber()
		{
			nctsHeader.MovementHeader.BM_InBondEntryType = Constants.OrgCusCodeTypes.TransitOperationHolder;

			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "TIRNumber", Core.Constants.CountryCodes.Belgium)
			};
			address.Organisation.CustomsCodes.AddRange(orgCusCodes);

			nctsHeader.DocAddresses.Add(address);

			AssertEquals("TIRNumber", Provider.TirHolderIdentificationNumber);
		}

		public void TestTirHolderIdentificationNumberNoTIRInBondEntryType()
		{
			nctsHeader.MovementHeader.BM_InBondEntryType = "OTH";

			var orgCusCodes = new List<OrgCusCode>
			{
				Factory.CreateOrgCusCode(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "TIRNumber", Core.Constants.CountryCodes.Belgium)
			};
			address.Organisation.CustomsCodes.AddRange(orgCusCodes);

			nctsHeader.DocAddresses.Add(address);

			AssertNull(Provider.TirHolderIdentificationNumber);
		}

		protected override string ExpectedName => "Oscorp Industries1";

		protected override bool ExpectContactToBeNullWithoutEmail => false;

		protected override bool ExpectContactToBeNullWithoutName => false;

		protected override bool ExpectContactToBeNullWithoutPhone => false;

		protected override Type ExpectedContactPersonProviderType => typeof(HolderOfTransitProcedureContactPersonProvider);

		protected override void SetupAddress()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);
			var principal = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PRC", nctsHeader.Principal, "1", phoneNumber: "120120", contactPhone: "110110", contactName: "LiuHuaQiang", contactEmail: "EMailAddress");
			nctsHeader.Principal.Address.OA_Email = "1099176692@qq.com";
			var guaranteeHeader = NCTSTestHelper.SetupGuarantee(principal);
			guaranteeHeader.MainAccessCode = "AAAA";
			guaranteeHeader.MainAccessPersonName = "LiuHuaQiang";
			guaranteeHeader.CPH_SubType = "0";
			var rule = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule.CPR_Description = "XiangHuaQiang";
			var guarantee = (NctsGuarantee)NCTSTestHelper.CreateGuaranteeForTest(nctsHeader, "0", "1234", "REF", "AAAA", "LO");
			address = nctsHeader.Principal;
		}
		NctsHeader nctsHeader;
	}
}
