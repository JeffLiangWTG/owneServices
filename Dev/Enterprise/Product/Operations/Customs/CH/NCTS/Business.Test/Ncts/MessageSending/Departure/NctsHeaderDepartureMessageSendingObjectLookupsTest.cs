using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CoreRefCusCodeListTypeCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes;
using DepartureCustomsStatusList = Enterprise.Customs.CH.NCTS.Business.NCTS5DepartureCustomsStatusList.Codes;
using NctsTypeOfDeclaration = Enterprise.Customs.EU.NCTS.Business.NctsConstants.NctsTypeOfDeclaration.Codes;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(NctsHeaderDepartureMessageSendingObjectLookups))]
sealed class NctsHeaderDepartureMessageSendingObjectLookupsTest : TestCaseWithFactory
{
	public void TestMessageTypeList_T_CH_Empty_MRN() => AssertMessageTypeList(NctsTypeOfDeclaration.NationalTransitSwitzerland, ZString.Empty, true, "NT515, NC016");

	public void TestMessageTypeList_T_CH_Empty_NoMRN() => AssertMessageTypeList(NctsTypeOfDeclaration.NationalTransitSwitzerland, ZString.Empty, false, "NT515");

	public void TestMessageTypeList_T_CH_Allocated_MRN() => AssertMessageTypeList(NctsTypeOfDeclaration.NationalTransitSwitzerland, DepartureCustomsStatusList.MrnAllocated, true, "NT513, NT014, NC123, NC016");

	public void TestMessageTypeList_T_CH_Allocated_NoMRN() => AssertMessageTypeList(NctsTypeOfDeclaration.NationalTransitSwitzerland, DepartureCustomsStatusList.MrnAllocated, false, "NT513, NT014, NC123");

	public void TestMessageTypeList_T_CH_Other_MRN() => AssertMessageTypeList(NctsTypeOfDeclaration.NationalTransitSwitzerland, DepartureCustomsStatusList.Acknowledged, true, "NT513, NT014, NC016");

	public void TestMessageTypeList_T_CH_Other_NoMRN() => AssertMessageTypeList(NctsTypeOfDeclaration.NationalTransitSwitzerland, DepartureCustomsStatusList.Acknowledged, false, "NT513, NT014");

	public void TestMessageTypeList_T1_Empty_MRN() => AssertMessageTypeList(NctsTypeOfDeclaration.GoodsMovingUnderExternalCommunityTransitProcedure, ZString.Empty, true, "NT015, NC016");

	public void TestMessageTypeList_T1_Empty_NoMRN() => AssertMessageTypeList(NctsTypeOfDeclaration.GoodsMovingUnderExternalCommunityTransitProcedure, ZString.Empty, false, "NT015");

	public void TestMessageTypeList_T1_UnderEnquiry_MRN() => AssertMessageTypeList(NctsTypeOfDeclaration.GoodsMovingUnderExternalCommunityTransitProcedure, DepartureCustomsStatusList.UnderEnquiry, true, "NT141, NC016");

	public void TestMessageTypeList_T1_UnderEnquiry_NoMRN() => AssertMessageTypeList(NctsTypeOfDeclaration.GoodsMovingUnderExternalCommunityTransitProcedure, DepartureCustomsStatusList.UnderEnquiry, false, "NT141");

	public void TestMessageTypeList_T1_Allocated_MRN() => AssertMessageTypeList(NctsTypeOfDeclaration.GoodsMovingUnderExternalCommunityTransitProcedure, DepartureCustomsStatusList.MrnAllocated, true, "NT013, NT014, NC123, NC016");

	public void TestMessageTypeList_T1_Allocated_NoMRN() => AssertMessageTypeList(NctsTypeOfDeclaration.GoodsMovingUnderExternalCommunityTransitProcedure, DepartureCustomsStatusList.MrnAllocated, false, "NT013, NT014, NC123");

	public void TestMessageTypeList_T1_Other_MRN() => AssertMessageTypeList(NctsTypeOfDeclaration.GoodsMovingUnderExternalCommunityTransitProcedure, DepartureCustomsStatusList.Acknowledged, true, "NT013, NT014, NC016");

	public void TestMessageTypeList_T1_Other_NoMRN() => AssertMessageTypeList(NctsTypeOfDeclaration.GoodsMovingUnderExternalCommunityTransitProcedure, DepartureCustomsStatusList.Acknowledged, false, "NT013, NT014");

	void AssertMessageTypeList(string entryType, ZString customsStatus, bool addMrn, string expectedMessageTypeList)
	{
		MessageSendingObject.NctsHeader.MovementHeader.BM_InBondEntryType = entryType;
		MessageSendingObject.NctsHeader.MovementHeader.BM_CustomsStatus = customsStatus;
		if (addMrn)
		{
			MessageSendingObject.NctsHeader.MovementReferenceNumberSetter("1");
		}
		AssertEquals($"BM_InBondEntryType='{entryType}'; BM_CustomsStatus='{customsStatus}'; MRN='{addMrn}'", expectedMessageTypeList, Lookups.MessageTypeList.CodesAsString);
	}

	public void TestReasonCodeList() => CombineAssertions(() =>
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1053).CreateCode("53A").CreateCode("53B");
		testHelper.CreateCodeList(CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1054).CreateCode("54A").CreateCode("54B");
		testHelper.CreateCodeList(CH.Business.UniversalReferenceConstants.RefCusCodeList.PassarTypes.N1141).CreateCode("41A").CreateCode("41B");
		Factory.Save();

		AssertEquals("no MessageType", 0, Lookups.ReasonCodeList.Count);

		MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT013;
		AssertEquals(MessageSendingObject.MessageType, "53A, 53B", Lookups.ReasonCodeList.CodesAsString);

		MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT014;
		AssertEquals(MessageSendingObject.MessageType, "54A, 54B", Lookups.ReasonCodeList.CodesAsString);

		MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT015;
		AssertEquals(MessageSendingObject.MessageType, 0, Lookups.ReasonCodeList.Count);

		MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT141;
		AssertEquals(MessageSendingObject.MessageType, "41A, 41B", Lookups.ReasonCodeList.CodesAsString);

		MessageSendingObject.MessageType = PassarMessageTypeList.Codes.NT513;
		AssertEquals(MessageSendingObject.MessageType, "53A, 53B", Lookups.ReasonCodeList.CodesAsString);
	});

	public void TestActualDestinationCustomsOfficeList()
	{
		var testHelper = new RefDataTestHelper(Factory);
		testHelper.CreateCodeList(CoreRefCusCodeListTypeCodes.CustomsOffice, Core.Constants.CountryCodes.NorthernIreland_ForUseOnlyByEuInCertainScopes)
			.CreateCode("DES01").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DES")
			.CreateCode("DEP01").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
		testHelper.CreateCodeList(CoreRefCusCodeListTypeCodes.CustomsOffice, Core.Constants.CountryCodes.Serbia_ForEUTrading)
			.CreateCode("DES02").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DES")
			.CreateCode("DEP02").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
		testHelper.CreateCodeList(CoreRefCusCodeListTypeCodes.CustomsOffice, Core.Constants.CountryCodes.IsleOfMan)
			.CreateCode("DES03").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DES")
			.CreateCode("DEP03").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DEP");
		testHelper.CreateCodeList(CoreRefCusCodeListTypeCodes.CustomsOffice, Core.Constants.CountryCodes.SanMarino)
			.CreateCode("DES04").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DES").WithValidity(new ZDateTime(2023, 5, 25), new ZDateTime(2023, 5, 26));
		testHelper.CreateCodeList(CoreRefCusCodeListTypeCodes.CustomsOffice, Core.Constants.CountryCodes.Albania)
			.CreateCode("DES05").WithAttribute(Universal.RefCusCodeListAttributeTypes.Codes.ROLE, "DES");
		Factory.Save();

		var list = Lookups.ActualDestinationCustomsOfficeList;
		list.Load();
		var test = list.Select(x => x.ZZD_Code).ToArray();
		AssertContainsExactElementsInAnyOrder(new ZString[] { "DES01", "DES02", "DES03" }, list.Select(x => x.ZZD_Code));
	}

	public void TestConsignees()
	{
		MessageSendingObject.NctsHeader.MovementHeader.BM_SubApplicationCode = "D";
		MessageSendingObject.NctsHeader.MovementHeader.BM_AdditionalDeclarationType = "A";
		AssertType<ConsigneeCollection>(Lookups.Consignees);
	}

	NctsHeaderDepartureMessageSendingObjectLookups Lookups => MessageSendingObject.Lookups;

	NctsHeaderDepartureMessageSendingObject MessageSendingObject => messageSendingObject ?? (messageSendingObject = CreateMessageSendingObject());
	NctsHeaderDepartureMessageSendingObject messageSendingObject;

	NctsHeaderDepartureMessageSendingObject CreateMessageSendingObject()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return new NctsHeaderDepartureMessageSendingObject(nctsHeader);
	}
}
