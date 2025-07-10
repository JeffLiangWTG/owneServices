using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusGuaranteeHeader = Enterprise.Customs.FR.Business.CusGuaranteeHeader;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	[TestedType(typeof(CC013BWrapper))]
	class CC013BDeclarationWrapperTest : DeclarationWrapperAbstractTest<CC013BWrapper>
	{
		public void TestSecurityCarrierEORI()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			header.BH_OH_Carrier = org.PK;
			AssertEquals("FR12345678900001", wrapper.SecurityCarrierEORI);
		}

		public void TestDeclarantTIN_IsDepartureFallback()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			header.Consignor.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR12345678900001", wrapper.DeclarantTIN);
		}

		public void TestPrincipalTIN()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("FR12345678900001", wrapper.PrincipalTIN);
		}

		public void TestConsignee()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			org.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			org.OH_FullName = "CONSIGNEE NAME";
			header.Consignee.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CONSIGNEE NAME", wrapper.Consignee.Name);
			AssertEquals("FR12345678900001", wrapper.Consignee.TIN);
		}

		public void TestControlResultCode()
		{
			header.MovementHeader.IsSimplifiedNctsProcedure = false;
			header.MovementHeader.BM_InBondEntryType = ZString.Empty;
			AssertEquals(EU.NCTS.Business.NctsControlResult.Codes.ConsideredSatisfactory, wrapper.ControlResultCode);

			header.MovementHeader.IsSimplifiedNctsProcedure = true;
			header.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T1;
			AssertEquals(EU.NCTS.Business.NctsControlResult.Codes.AuthorizedTrader, wrapper.ControlResultCode);

			header.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T2;
			AssertEquals(EU.NCTS.Business.NctsControlResult.Codes.AuthorizedTrader, wrapper.ControlResultCode);

			header.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T2F;
			AssertEquals(EU.NCTS.Business.NctsControlResult.Codes.AuthorizedTrader, wrapper.ControlResultCode);

			header.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T_;
			AssertEquals(EU.NCTS.Business.NctsControlResult.Codes.AuthorizedTrader, wrapper.ControlResultCode);

			header.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.TIR;
			AssertEquals(EU.NCTS.Business.NctsControlResult.Codes.ConsideredSatisfactory, wrapper.ControlResultCode);
		}

		public void TestAgreementNumber()
		{
			var org = DeclarationWrapperHelperTest.CreateOrgHeaderWithDTA(Factory, "CC0002");
			header.Principal.E2_OA_Address = org.MainAddress.PK;
			AssertEquals("CC0002", wrapper.AgreementNumber);
		}

		public void TestPrelodgeDeclarationIndicator()
		{
			header.IsPrelodgedMovement = true;
			AssertEquals(true, wrapper.PrelodgeDeclarationIndicator);

			header.IsPrelodgedMovement = false;
			AssertEquals(false, wrapper.PrelodgeDeclarationIndicator);
		}

		public void TestControlResultCode_Arrival()
		{
			header.BH_HeaderType = EU.NCTS.Business.NctsMovementType.Codes.Arrival;
			AssertEquals(EU.NCTS.Business.NctsControlResult.Codes.ConsideredSatisfactory, wrapper.ControlResultCode);
		}

		public void TestControlResultCode_Departure()
		{
			CombineAssertions(() =>
			{
				header.MovementHeader.IsSimplifiedNctsProcedure = false;
				AssertEquals("Not Simplified", EU.NCTS.Business.NctsControlResult.Codes.ConsideredSatisfactory, wrapper.ControlResultCode);

				header.MovementHeader.IsSimplifiedNctsProcedure = true;
				header.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.T1;
				AssertEquals("Is Simplified and Not TIR", EU.NCTS.Business.NctsControlResult.Codes.AuthorizedTrader, wrapper.ControlResultCode);

				header.MovementHeader.BM_InBondEntryType = EU.NCTS.Business.NctsDeclarationTypeList.Codes.TIR;
				AssertEquals("Is Simplified and TIR", EU.NCTS.Business.NctsControlResult.Codes.ConsideredSatisfactory, wrapper.ControlResultCode);
			});
		}

		public void TestNatureOfSeals()
		{
			header.FRNctsHeader.CFN_NatureOfSeals = NatureOfSealsList.Codes.NS1;
			AssertEquals(NatureOfSealsList.Codes.NS1, wrapper.NatureOfSeals);
		}

		public void TestGuarantees()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var declarant = Factory.NewWithValidTestData<OrgHeader>();
			declarant.OH_Code = "TEST0000";

			var declarantAddress = declarant.Addresses.AddNew();
			declarantAddress.AddressCode = "TestMatchAddress";
			declarantAddress.Address1 = "TestMatchAddress";

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_OH_PermitHolder = declarant.PK;
			guaranteeHeader.CPH_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			guaranteeHeader.CPH_Number = "XZCK";
			guaranteeHeader.CPH_StartDate = ZDate.Today.AddYears(-20);
			guaranteeHeader.CPH_QtyValIndicator = PermitQtyValIndicatorList.Codes.BTH;
			guaranteeHeader.CPH_UnitOfMeasure = "KGM";
			guaranteeHeader.CPH_Type = GuaranteeTypeList.Codes.COD;
			var additionalReference = guaranteeHeader.AdditionalGuaranteeReferences.AddNew();
			additionalReference.CY_Data = "ABC";
			additionalReference.CY_Code = OrgCusAccountDeltaTTypeList.Codes.TR;

			var rule1 = guaranteeHeader.CusGuaranteeRules.AddNew();
			rule1.CPR_RuleCode = EU.Business.PermitRuleCodeList.Codes.ADD;
			rule1.CPR_ValueFrom = "TestMatchAddress";

			header.Principal.E2_OA_Address = declarantAddress.PK;
			header.Declarant.E2_OA_Address = declarantAddress.PK;
			var guarantee1 = header.MovementHeader.Guarantees.AddNew();
			guarantee1.PW_BondAmount = 150m;
			guarantee1.PW_BondNumber = guaranteeHeader.CPH_Number;
			Factory.Save();

			AssertEquals("ABC", wrapper.Guarantees.FirstOrDefault().GuaranteeReferenceNumber);

			var declarant2 = Factory.NewWithValidTestData<OrgHeader>();
			declarant2.OH_Code = "TEST0001";
			var declarant2Address = declarant2.Addresses.AddNew();
			declarant2Address.AddressCode = "AAAA";
			declarant2Address.Address1 = "BBBB";

			guaranteeHeader.CPH_OH_PermitHolder = declarant2.PK;
			Factory.Save();

			AssertEquals("XZCK", wrapper.Guarantees.FirstOrDefault().GuaranteeReferenceNumber);
		}

		public void TestAmendmentDate()
		{
			AssertEquals(EU.NCTS.Business.WrapperHelper.GetLongDate(ZDateTime.Today), wrapper.AmendmentDate);
		}

		protected override void SetUp()
		{
			base.SetUp();
			header = Factory.New<NctsHeader>();
			header.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			wrapper = new CC013BWrapper(header);
		}
		NctsHeader header;
		ICC013BDeclaration wrapper;

		protected override CC013BWrapper GetProvider() => (CC013BWrapper)wrapper;
	}
}
