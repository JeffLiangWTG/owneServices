using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business.Testing
{
	public class StatusCodeDescriptionPairListTest : TestCaseWithFactory
	{
		public void TestGetStatusList()
		{
			foreach (CodeDescriptionPair reason in new ReasonCodeDescriptionPairList())
			{
				CodeDescriptionPairList statusList = StatusCodeDescriptionPairList.GetStatusList(reason.Code);
				Type statusSubListType = typeof(AutoStatusCodeDescriptionPairList).GetNestedType(reason.Code);
				if (statusSubListType != null)
				{
					AssertEquals("Status list specified for reason code '" + reason.Code + "' but no statuses were returned from GetStatusList", true, statusList.Count > 0);
				}

				if (statusList.Count > 1)
				{
					AssertEquals(StatusCodeDescriptionPairList.EmptyStatus, statusList[0].Code);
					AssertEquals("No Status", statusList[0].Description);
				}
			}
		}

		public void TestGetStatusList_WhenReasonCodeDoesntHaveAStatusSubList()
		{
			AssertEquals("When the reason code doesnt have a status sub-list, an empty list should be returned", 0, StatusCodeDescriptionPairList.GetStatusList("XXX").Count);
		}

		public void TestStatusCodeSubListNestedTypeName_HasToBeAValidReasonCode()
		{
			Type[] statusSubListTypes = typeof(AutoStatusCodeDescriptionPairList).GetNestedTypes();
			ReasonCodeDescriptionPairList reasonCodeList = new ReasonCodeDescriptionPairList();
			AssertEquals("There should be at least a few status lists", true, statusSubListTypes.Length > 3);
			foreach (Type statusSubListType in statusSubListTypes)
			{
				if (statusSubListType.Name != "Codes" && statusSubListType.Name != "Descriptions")
				{
					string reasonCode = statusSubListType.Name;
					Assert(reasonCode + " should be part of the Reason Code List", reasonCodeList.ContainsCode(reasonCode));
				}
			}
		}

		public void TestSpecialCaseStatuses()
		{
			StatusCodeDescriptionPairList list = StatusCodeDescriptionPairList.GetStatusList(CommercialQueueCodeDescriptionPairList.Codes.Finance, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment);
			AssertEquals("Should be an empty list", 0, list.Count);
			list = StatusCodeDescriptionPairList.GetStatusList(CommercialQueueCodeDescriptionPairList.Codes.EIR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment);
			AssertEquals("Should be an empty list", 0, list.Count);
			list = StatusCodeDescriptionPairList.GetStatusList(CommercialQueueCodeDescriptionPairList.Codes.AR, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment);
			AssertEquals("Should be an empty list", 0, list.Count);
			list = StatusCodeDescriptionPairList.GetStatusList(CommercialQueueCodeDescriptionPairList.Codes.Hold, ReasonCodeDescriptionPairList.Codes.OQ_HeldForPayment);
			Assert("Should not be an empty list", list.Count > 0);
			list = StatusCodeDescriptionPairList.GetStatusList(DeclarationQueueCodeDescriptionPairList.Codes.Submitted, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration);
			AssertEquals("There should be two items in the list", 2, list.Count);
			AssertEquals(StatusCodeDescriptionPairList.EmptyStatus, list[0].Code);
			AssertEquals("No Status", list[0].Description);
			AssertEquals(StatusCodeDescriptionPairList.Codes.Y1_DocumentsToCustomsAQIS, list[1].Code);
			AssertEquals(StatusCodeDescriptionPairList.Descriptions.Y1_DocumentsToCustomsAQIS, list[1].Description);
			list = StatusCodeDescriptionPairList.GetStatusList(DeclarationQueueCodeDescriptionPairList.Codes.Classification, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration);
			AssertEquals("Should be an empty list", 0, list.Count);
			list = StatusCodeDescriptionPairList.GetStatusList(DeclarationQueueCodeDescriptionPairList.Codes.Lodgement, ReasonCodeDescriptionPairList.Codes.X2_FullDeclaration);
			AssertEquals("Should be an empty list", 0, list.Count);
		}
	}
}
