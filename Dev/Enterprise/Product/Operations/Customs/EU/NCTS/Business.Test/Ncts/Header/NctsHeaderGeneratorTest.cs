using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	public class NctsHeaderGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2023, 04, 13, 17, 07, 32)]
		public void TestGenerateArrivalFromDeparture()
		{
			var departureHeader = getDepartureForFullTest();
			var generator = new NctsHeaderGenerator();
			var arrivalHeader = generator.GenerateArrivalFromDeparture(departureHeader);

			CombineAssertions(() =>
			{
				AssertEquals("BH_ApplicationCode should always be NC5", CusInBondApplicationCodeList.Codes.NCTS5, arrivalHeader.BH_ApplicationCode);
				AssertEquals("BH_HeaderType should always be A", NctsMovementType.Codes.Arrival, arrivalHeader.BH_HeaderType);
				AssertEquals("BM_MessageStatus should always be empty", ZString.Empty, arrivalHeader.ArrivalMovementHeader.BM_MessageStatus);
				AssertEquals("BH_ExportFlag should always be N", EventFlagList.Codes.No, arrivalHeader.BH_ExportFlag);

				AssertNotNull("A DestinationTrader should be created", arrivalHeader.DestinationTrader);
				AssertEquals("The DestinationTrader should be the same as the consingee of the departure", departureHeader.Consignee.OrganisationPK, arrivalHeader.DestinationTrader.OrganisationPK);

				AssertEquals("BM_SubApplicationCode should always be A", Common.EU.NctsMoveHeaderType.Codes.Arrival, arrivalHeader.ArrivalMovementHeader.BM_SubApplicationCode);
				AssertEquals("BM_UnloadingDate should always be the current time", new ZDateTimeOffset(2023, 04, 13, 17, 07, 32), arrivalHeader.ArrivalMovementHeader.BM_UnloadingDate);

				AssertEquals("BM_DischargeType should always be FD for TIR Declaration", NctsConstants.DischargeTypes.FullDischarge, arrivalHeader.ArrivalMovementHeader.BM_DischargeType);
				AssertEquals("BM_CarnetTotalPages should always be 0 for TIR Declaration", ZShort.Zero, arrivalHeader.ArrivalMovementHeader.BM_CarnetTotalPages);

				var query = new ZQuery(CusCodeDataSchema.CY_ParentID, arrivalHeader.ArrivalMovementHeader.PK);
				query.AddToFilter(CusCodeDataSchema.CY_ParentTableCode, CusInBondMoveHeaderSchema.Constants.Prefix);
				var queryResult = Factory.Load<NctsEuOfficeCode>(query);

				AssertEquals("There should be an ArrivalOffice", 1, queryResult.Length);
				AssertEquals("The office type should be EUO", "EUO", queryResult[0].CY_Type);
				AssertEquals("The office code should be DSA", OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, queryResult[0].CY_Code);
				AssertEquals("The office data should come from departure", "ABC001", queryResult[0].CY_Data);

				AssertEquals("DestinationCustomsOfficeCodeForArrival has the correct value", "ABC001", arrivalHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival);

				var customsOfficesDSA = arrivalHeader.ArrivalMovementHeader.CustomsOffices.Cast<NctsEuOfficeCode>().Where(x => x.CY_Code == OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
				AssertEquals("CustomsOffices has only one DSA office", 1, customsOfficesDSA.Count());
				AssertEquals("CustomsOffices DSA office has the correct value", "ABC001", customsOfficesDSA.First().CY_Data);

				AssertEquals("There should be an authorization usage", 1, arrivalHeader.CusAuthorizationUsages.Count);
				AssertEquals("AGC_Code should be ACT", "ACT", arrivalHeader.CusAuthorizationUsages[0].AGC_Code);
				AssertEquals("AGC_Number should be the number of the permit", "Permit the vrog", arrivalHeader.CusAuthorizationUsages[0].AGC_Number);
				AssertEquals("AGC_OH_Owner should be the holder of the permit", arrivalHeader.DestinationTrader.OrganisationPK, arrivalHeader.CusAuthorizationUsages[0].AGC_OH_Owner);

				AssertEquals("The MRN should be equal to the one in departure", "123", arrivalHeader.MovementReferenceNumber);
			});
		}

		public void TestGenerateArrivalFromDepartureOverriddenMessageStatus()
		{
			var departureHeader = getDepartureForFullTest();
			var generator = new NctsHeaderGeneratorForTest();
			var arrivalHeader = generator.GenerateArrivalFromDeparture(departureHeader);

			AssertEquals("BM_MessageStatus should be filled in with the overridden value", "MAN", arrivalHeader.ArrivalMovementHeader.BM_MessageStatus);
		}

		public void TestGenerateArrivalFromDepartureNoTir()
		{
			var departureHeader = getDepartureForNoTIRTest();
			var generator = new NctsHeaderGenerator();
			var arrivalHeader = generator.GenerateArrivalFromDeparture(departureHeader);

			AssertEquals("BM_DischargeType should not be entered for other than TIR Declaration", ZString.Empty, arrivalHeader.ArrivalMovementHeader.BM_DischargeType);
		}

		public void TestGenerateArrivalFromDepartureNotTIRAuthorizedTest()
		{
			var departureHeader = getDepartureForNotTIRAuthorizedTest();
			var generator = new NctsHeaderGenerator();
			var arrivalHeader = generator.GenerateArrivalFromDeparture(departureHeader);

			AssertEquals("There should be no authorization usage created", 0, arrivalHeader.CusAuthorizationUsages.Count);

			departureHeader = getDepartureForAuthorization_ACETest();
			arrivalHeader = generator.GenerateArrivalFromDeparture(departureHeader);

			AssertEquals("There should be an authorization usage", 1, arrivalHeader.CusAuthorizationUsages.Count);
			AssertEquals("AGC_Code should be ACE", "ACE", arrivalHeader.CusAuthorizationUsages[0].AGC_Code);
			AssertEquals("AGC_Number should be the number of the permit", "ACE Auth 123", arrivalHeader.CusAuthorizationUsages[0].AGC_Number);
			AssertEquals("AGC_OH_Owner should be the holder of the permit", arrivalHeader.DestinationTrader.OrganisationPK, arrivalHeader.CusAuthorizationUsages[0].AGC_OH_Owner);
		}

		public void TestIsGenerationAllowed()
		{
			CombineAssertions(() =>
			{
				var departureHeader = getDepartureForFullTest();
				var generator = new NctsHeaderGenerator();
				AssertEquals("If there is no arrival notification with this MRN then generation should be allowed", true, generator.IsGenerationAllowed(departureHeader));

				var arrivalHeader = generator.GenerateArrivalFromDeparture(departureHeader);

				AssertEquals("We've just created the arrival notification, we should not be able to generate another one with the same MRN", false, generator.IsGenerationAllowed(departureHeader));
			});
		}

		NctsHeader getDepartureForFullTest()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			departureHeader.MovementReferenceEntryNumber.CE_EntryNum = "123";
			departureHeader.MovementHeader.DestinationCustomsOfficeCodeForDeparture = "ABC001";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "DE";

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "Address1";
			address1.AddressCode = "add1";
			address1.OA_OH = org.PK;
			var cusCode1 = org.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = "LV";
			cusCode1.OK_CodeType = "CTR";
			cusCode1.OK_CustomsRegNo = "LV001";
			cusCode1.OK_OA_PremisesAddress = address1.PK;

			departureHeader.Consignee.E2_OA_Address = org.MainAddress.PK;

			var departureMovement = departureHeader.MovementHeader;
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;

			var cusPermitHeader = Factory.New<CusAuthorisationHeader>();
			cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			cusPermitHeader.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForTirProcedure;
			cusPermitHeader.CPH_OH_PermitHolder = org.PK;
			cusPermitHeader.CPH_Number = "Permit the vrog";
			cusPermitHeader.CPH_StartDate = ZDate.Today;

			return departureHeader;
		}

		NctsHeader getDepartureForAuthorization_ACETest()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			departureHeader.MovementReferenceEntryNumber.CE_EntryNum = "123";
			departureHeader.DestinationCustomsOfficeCodeForDeparture = "ABC001";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_RN_NKCountryCode = "DE";

			var address1 = Factory.NewWithValidTestData<OrgAddress>();
			address1.OA_Address1 = "Address1";
			address1.AddressCode = "add1";
			address1.OA_OH = org.PK;
			var cusCode1 = org.CustomsCodes.AddNew();
			cusCode1.OK_RN_NKCodeCountry = "LV";
			cusCode1.OK_CodeType = "CTR";
			cusCode1.OK_CustomsRegNo = "LV001";
			cusCode1.OK_OA_PremisesAddress = address1.PK;

			departureHeader.Consignee.E2_OA_Address = org.MainAddress.PK;

			var departureMovement = departureHeader.MovementHeader;
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.MixedConsignmentOfT1AndT2GoodsPhase5;

			var cusPermitHeader = Factory.New<CusAuthorisationHeader>();
			cusPermitHeader.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Authorisation;
			cusPermitHeader.CPH_Type = NctsArrivalAuthorizationCodeList.Codes.AuthorizationForTheStatusOfAuthorizedConsigneeForUnionTransit;
			cusPermitHeader.CPH_OH_PermitHolder = org.PK;
			cusPermitHeader.CPH_Number = "ACE Auth 123";
			cusPermitHeader.CPH_StartDate = ZDate.Today;

			return departureHeader;
		}

		NctsHeader getDepartureForNoTIRTest()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			departureHeader.MovementReferenceEntryNumber.CE_EntryNum = "123";
			departureHeader.DestinationCustomsOfficeCodeForDeparture = "ABC001";

			var departureMovement = departureHeader.MovementHeader;
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.GoodsMovingUnderExternalCommunityTransitProcedure;

			return departureHeader;
		}

		NctsHeader getDepartureForNotTIRAuthorizedTest()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureHeader.MovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			departureHeader.MovementReferenceEntryNumber.CE_EntryNum = "123";
			departureHeader.DestinationCustomsOfficeCodeForDeparture = "ABC001";

			return departureHeader;
		}

		class NctsHeaderGeneratorForTest : NctsHeaderGenerator
		{
			protected override string NewMessageStatus => "MAN";
		}
	}
}
