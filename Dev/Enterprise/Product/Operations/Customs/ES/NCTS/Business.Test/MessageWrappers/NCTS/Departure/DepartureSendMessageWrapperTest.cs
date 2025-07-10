using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class DepartureSendMessageWrapperTest : WrapperHelperTest<DepartureSendMessageWrapper>
	{
		public void TestCustomsProcedureCategory3()
		{
			nctsHeader.MovementHeader.BM_InBondEntryType = HeaderDataNCTS.DeclarationType;
			AssertEquals("Expected filled CustomsProcedureCategory3", HeaderDataNCTS.DeclarationType, wrapper.CustomsProcedureCategory3);
		}

		public void TestCountryOfDeparture()
		{
			nctsHeader.BH_RL_NKImportLoadPort = HeaderDataNCTS.DepartureCountry;
			AssertEquals("Expected filled CountryOfDeparture", HeaderDataNCTS.DepartureCountry, wrapper.CountryOfDeparture);
		}

		public void TestCustomsOfficesOfTransit()
		{
			CombineAssertions(() =>
			{
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "NN123456", new ZDateTime(2012, 10, 12, 6, 6, 0));
				CreateCustomsOfficeForTest(nctsHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfTransit, "TT123456", new ZDateTime(2012, 10, 13, 7, 7, 0));

				var customsOfficeOfTransit = wrapper.CustomsOfficesOfTransit;

				AssertEquals("Expected 2 CustomsOfficesOfTransit", 2, customsOfficeOfTransit.Count);
				AssertSame("Cached CustomsOfficesOfTransit", customsOfficeOfTransit, wrapper.CustomsOfficesOfTransit);
			});
		}

		public void TestCustomsOfficeOfDestination()
		{
			CombineAssertions(() =>
			{
				nctsHeader.DestinationCustomsOfficeCodeForDeparture = HeaderDataNCTS.DestinationCustomsOffice;
				var customsOfficeOfDestination = wrapper.CustomsOfficeOfDestination;

				AssertNotNull("Expected not null CustomsOfficeOfDestination", customsOfficeOfDestination);
				AssertSame("Cached CustomsOfficeOfDestination", customsOfficeOfDestination, wrapper.CustomsOfficeOfDestination);
			});
		}

		public void TestGuaranteeNumbers()
		{
			CombineAssertions(() =>
			{
				nctsHeader.GetEffectiveGuarantees().AddNew();
				nctsHeader.GetEffectiveGuarantees().AddNew();
				nctsHeader.GetEffectiveGuarantees().AddNew();

				var guaranteeNumbers = wrapper.GuaranteeNumbers;

				AssertEquals("Expected 3 GuaranteeNumbers", 3, guaranteeNumbers.Count);
				AssertSame("Cached GuaranteeNumbers", wrapper.GuaranteeNumbers, guaranteeNumbers);
			});
		}

		public void TestTransitTransportMedium()
		{
			nctsHeader.MovementHeader.BM_TransportAtDeparture = NctsTransportData1.Id;
			nctsHeader.MovementHeader.BM_RN_NKTransportAtDepartureCountry = NctsTransportData1.Nationality;
			var transitTransportMedium = wrapper.TransitTransportMedium;

			CombineAssertions(() =>
			{
				AssertNotNull("TransitTransportMedium wrapped", transitTransportMedium);
				AssertSame("Cached TransitTransportMedium", wrapper.TransitTransportMedium, transitTransportMedium);
			});
		}

		public void TestNullPrincipal()
		{
			AssertExceptionThrown<NullReferenceException>(() => wrapper.Principal.ToString());
		}

		public void TestPrincipal()
		{
			CombineAssertions(() =>
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				nctsHeader.Principal.OrganisationPK = orgHeader.PK;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				var principal = wrapper.Principal;

				AssertNotNull("Expected filled Principal", principal);
				AssertSame("Cached Principal", wrapper.Principal, principal);
			});
		}

		public void TestRepresentative()
		{
			var orgHeaderRepresentative = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.MovementHeader.Representative.OrganisationPK = orgHeaderRepresentative.PK;
			orgHeaderRepresentative.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeaderRepresentative.OH_Category = OrgConstants.Category.NaturalPersonIndividual;

			wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
			var representative = wrapper.Representative;
			CombineAssertions(() =>
			{
				AssertNotNull("Expected filled Representative", wrapper.Representative);
				AssertSame("Cached Representative", wrapper.Representative, representative);
			});
		}

		public void TestLines()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected 1 Line (mandatory at least one)", 1, wrapper.Lines.Count);

				nctsHeader.MovementHeader.GoodsItems.AddNew();
				nctsHeader.MovementHeader.GoodsItems.AddNew();

				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);

				var lines = wrapper.Lines;

				AssertEquals("Expected 3 Lines", 3, lines.Count);
				AssertSame("Cached Lines", wrapper.Lines, lines);
			});
		}

		public void TestNationalSimplificationIndicator()
		{
			nctsHeader.NationalSimplificatorInd = HeaderDataNCTS.NationalSimplificatorInd;
			AssertEquals("Expected filled NationalSimplificationIndicator", HeaderDataNCTS.NationalSimplificatorInd, wrapper.NationalSimplificationIndicator);
		}

		public void TestDeclarantIdForUNBSegment()
		{
			var orgHeaderDeclarant = Factory.New<OrgHeader>();
			orgHeaderDeclarant.OH_Code = HeaderDataNCTS.DeclarantCode;
			orgHeaderDeclarant.Addresses.AddNew();

			CombineAssertions("Only representative declared", () =>
			{
				nctsHeader.DeclarantOrgPK = ZGuid.Empty;
				AssertEquals("Expected empty Id when declarant is not declared", ZString.Empty, wrapper.DeclarantIdForUNBSegment);

				nctsHeader.DeclarantOrgPK = orgHeaderDeclarant.PK;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected empty Id when declarant declared but has no id", ZString.Empty, wrapper.DeclarantIdForUNBSegment);

				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapper.DeclarantIdForUNBSegment);

				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderDeclarant.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected NIF Id when category is not NAT and NIF is declared", "NIF22222222", wrapper.DeclarantIdForUNBSegment);

				orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderDeclarant.OH_Category = OrgConstants.Category.Government;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected Country+NIF Id when category is not NAT and there is no EORI declared", "ESNIF22222222", wrapper.DeclarantIdForUNBSegment);

				OrgCusCode eoriCusCode = orgHeaderDeclarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected EORI Id with country code when category is NAT and EORI is declared", "FR22222222", wrapper.DeclarantIdForUNBSegment);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.DeclarantIdForUNBSegment);
			});

			var orgHeaderPrincipal = Factory.New<OrgHeader>();
			orgHeaderPrincipal.OH_Code = HeaderDataNCTS.PrincipalCode;
			orgHeaderPrincipal.Addresses.AddNew();

			CombineAssertions("Only principal declared", () =>
			{
				nctsHeader.DeclarantOrgPK = ZGuid.Empty;

				nctsHeader.Principal.OrganisationPK = ZGuid.Empty;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected empty Id when principal is not declared", ZString.Empty, wrapper.DeclarantIdForUNBSegment);

				nctsHeader.Principal.OrganisationPK = orgHeaderPrincipal.PK;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected empty Id when principal declared but has no id", ZString.Empty, wrapper.DeclarantIdForUNBSegment);

				orgHeaderPrincipal.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapper.DeclarantIdForUNBSegment);

				orgHeaderPrincipal.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderPrincipal.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected NIF Id when category is NAT and NIF is declared", "NIF22222222", wrapper.DeclarantIdForUNBSegment);

				orgHeaderPrincipal.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
				orgHeaderPrincipal.OH_Category = OrgConstants.Category.Government;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected Country+NIF Id when category is not NAT and there is no EORI declared", "ESNIF22222222", wrapper.DeclarantIdForUNBSegment);

				OrgCusCode eoriCusCode = orgHeaderPrincipal.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected EORI Id with country code when category is not NAT and EORI is declared", "FR22222222", wrapper.DeclarantIdForUNBSegment);

				eoriCusCode.OK_CustomsRegNo = "ES22222222";
				eoriCusCode.OK_RN_NKCodeCountry = "ES";
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.DeclarantIdForUNBSegment);
			});

			CombineAssertions("When representative and principal are declared", () =>
			{
				nctsHeader.DeclarantOrgPK = orgHeaderDeclarant.PK;
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected EORI Id from Principal when representative and principal are the same", "ES22222222", wrapper.DeclarantIdForUNBSegment);

				orgHeaderPrincipal.CustomsCodes.RemoveAndDeleteAll();
				OrgCusCode eoriCusCode = orgHeaderPrincipal.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
				wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
				AssertEquals("Expected EORI Id from Representative when representative and principal are not the same", "ES22222222", wrapper.DeclarantIdForUNBSegment);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			nctsHeader.MovementHeader.GoodsItems.AddNew();
			wrapper = new DepartureSendMessageWrapper(nctsHeader, Certificate);
		}

		NctsHeader nctsHeader;
		DepartureSendMessageWrapper wrapper;

		protected override DepartureSendMessageWrapper GetProvider() => wrapper;

		public static EuOfficeCode CreateCustomsOfficeForTest(NctsHeader nctsHeader, string officePurpose, string officeCode, ZDateTime arrivalTime, bool clearOffices = false)
		{
			NctsEuOfficeCode office = null;
			if (clearOffices)
			{
				nctsHeader.CustomsOffices.RemoveAndDeleteAll();
			}
			else
			{
				if (officePurpose == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination && nctsHeader.IsDepartureMovement)
				{
					office = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
				}
				else if (officePurpose == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture && nctsHeader.IsDepartureMovement)
				{
					office = nctsHeader.CustomsOffices.Cast<NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture);
				}
			}

			if (office == null)
			{
				office = nctsHeader.CustomsOffices.AddNew();
				office.CY_Code = officePurpose;
			}

			office.CY_Data = officeCode;
			office.CY_Date = arrivalTime;

			var dataGroupingCode = officeCode.Substring(0, 2);
			var factory = nctsHeader.Factory;
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(dataGroupingCode, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office Code");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, dataGroupingCode);
			helper.CreateNewOrGetExistingCusCodeList(officeCode, dataGroupingCode, "Office Description" + officeCode, new ZString[] { officePurpose });
			nctsHeader.Factory.Save();
			return office;
		}
	}
}

