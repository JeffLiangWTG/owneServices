using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	[TestedType(typeof(NctsHeaderMessageSendingObjectLookups))]
	sealed class NctsHeaderMessageSendingObjectLookupsTest : TestCaseWithFactory
	{
		public void TestArrivalLookups()
		{
			AssertEquals(string.Empty, GetNewLookups(NctsMovementType.Codes.Arrival).MessageTypeList.CodesAsString);
		}

		public void TestDepartureLookups()
		{
			AssertEquals(string.Empty, GetNewLookups(NctsMovementType.Codes.Departure).MessageTypeList.CodesAsString);
		}

		public void TestReleaseRequestedFlags()
		{
			AssertEquals("Y, N", GetNewLookups(NctsMovementType.Codes.Departure).ReleaseRequestedFlags.CodesAsString);
		}

		public void TestDestinationCustomsOfficeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEANR100", "ANTWERP PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEZEE100", "ZEEBRUGGE PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEGEN100", "GENT PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE002325", "BREMEN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_SubApplicationCode = NctsTypeOfAdditionalDeclarationList.Codes.D;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			var messageSendingAction = new NctsHeaderMessageSendingObject(header);
			var lookups = messageSendingAction.Lookups;

			var list = lookups.DestinationCustomsOfficeCodeList;
			list.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "BEANR100", "BEZEE100", "BEGEN100", "DE002325" }, list.Select(x => x.ZZD_Code));
		}

		public void TestDepartureOfficeOfEnquiryCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEANR100", "ANTWERP PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEZEE100", "ZEEBRUGGE PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);
			helper.CreateCusCodeListWithAttribute(GlbCompany.CurrentCompany.Country.Code, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "BEGEN100", "GENT PORT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "DE002325", "BREMEN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, RefCusCodeListAttributeTypes.Codes.ROLE, OfficeCodes_NCTS.Codes.NCTSOfficeOfEnquiry);
			Factory.Save();

			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType(NctsMovementType.Codes.Departure);
			var movementHeader = header.MovementHeader;
			movementHeader.BM_SubApplicationCode = NctsTypeOfAdditionalDeclarationList.Codes.D;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.A;
			var messageSendingAction = new NctsHeaderMessageSendingObject(header);
			var lookups = messageSendingAction.Lookups;

			var list = lookups.DestinationCustomsOfficeCodeList;
			list.Load();
			AssertContainsExactElementsInAnyOrder(new ZString[] { "BEANR100", "BEZEE100", "BEGEN100", "DE002325" }, list.Select(x => x.ZZD_Code));
		}

		public void TestConsignees()
		{
			var header = Factory.New<NctsHeader>();
			header.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			header.SetMovementType("D");
			var movementHeader = header.MovementHeader;
			movementHeader.BM_SubApplicationCode = "D";
			movementHeader.BM_AdditionalDeclarationType = "A";
			var messageSendingAction = new NctsHeaderMessageSendingObject(header);
			var lookups = messageSendingAction.Lookups;

			AssertType<ConsigneeCollection>(lookups.Consignees);
		}

		NctsHeaderMessageSendingObjectLookups GetNewLookups(string headerType)
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.BH_HeaderType = headerType;
			var sendingObject = new NctsHeaderMessageSendingObject(nctsHeader);
			return sendingObject.Lookups;
		}
	}
}
