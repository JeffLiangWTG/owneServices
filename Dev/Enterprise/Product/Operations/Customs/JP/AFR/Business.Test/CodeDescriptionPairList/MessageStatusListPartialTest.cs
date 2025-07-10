using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.JP.AFR.Business.Testing
{
	class MessageStatusListPartialTest : TestCaseWithFactory
	{
		public void TestGetHeaderLevelStatusList()
		{
			var list1 = MessageStatusList.GetHeaderLevelStatusList(Factory, true);
			var list2 = MessageStatusList.GetHeaderLevelStatusList(Factory, true);
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals(6, list1.Count);
			AssertEquals(MessageStatusList.Descriptions.AwaitingDepartureTimeRegistration, list1.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingDepartureTimeRegistration));
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillRegistrationCompletion, list1.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion));
			AssertEquals(MessageStatusList.Descriptions.ClearDepartureTimeRegistration, list1.GetDescriptionFromCode(MessageStatusList.Codes.ClearDepartureTimeRegistration));
			AssertEquals(MessageStatusList.Descriptions.ClearHouseBillRegistrationCompletion, list1.GetDescriptionFromCode(MessageStatusList.Codes.ClearHouseBillRegistrationCompletion));
			AssertEquals(MessageStatusList.Descriptions.ErrorDepartureTimeRegistration, list1.GetDescriptionFromCode(MessageStatusList.Codes.ErrorDepartureTimeRegistration));
			AssertEquals(MessageStatusList.Descriptions.ErrorHouseBillRegistrationCompletion, list1.GetDescriptionFromCode(MessageStatusList.Codes.ErrorHouseBillRegistrationCompletion));
			var list3 = MessageStatusList.GetHeaderLevelStatusList(Factory);
			var list4 = MessageStatusList.GetHeaderLevelStatusList(Factory);
			AssertEquals(true, object.ReferenceEquals(list3, list4));
			AssertEquals(false, object.ReferenceEquals(list3, list1));
			AssertEquals(3, list3.Count);
			AssertEquals(MessageStatusList.Descriptions.AwaitingHouseBillRegistrationCompletion, list3.GetDescriptionFromCode(MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion));
			AssertEquals(MessageStatusList.Descriptions.ClearHouseBillRegistrationCompletion, list3.GetDescriptionFromCode(MessageStatusList.Codes.ClearHouseBillRegistrationCompletion));
			AssertEquals(MessageStatusList.Descriptions.ErrorHouseBillRegistrationCompletion, list3.GetDescriptionFromCode(MessageStatusList.Codes.ErrorHouseBillRegistrationCompletion));
		}

		public void TestGetBillLevelStatusList()
		{
			var list = new MessageStatusList();
			var headerList = MessageStatusList.GetHeaderLevelStatusList(Factory, true);
			var list1 = MessageStatusList.GetBillLevelStatusList(Factory);
			var list2 = MessageStatusList.GetBillLevelStatusList(Factory);
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals(list.Count - headerList.Count, list1.Count);
			foreach (ICodeDescription pair in list)
			{
				if (!headerList.ContainsCode(pair.Code))
				{
					AssertEquals(pair.Code, pair.Description, list.GetDescriptionFromCode(pair.Code));
				}
			}
		}

		public void TestIsMessagingInProgressType()
		{
			var pendingCodes = new[] {
				MessageStatusList.Codes.AwaitingDepartureTimeRegistration,
				MessageStatusList.Codes.AwaitingHouseBillAdd,
				MessageStatusList.Codes.AwaitingHouseBillDelete,
				MessageStatusList.Codes.AwaitingHouseBillRegistration,
				MessageStatusList.Codes.AwaitingHouseBillRegistrationCompletion,
				MessageStatusList.Codes.AwaitingHouseBillUpdate,
				MessageStatusList.Codes.AwaitingMasterBillAddAfterATD,
				MessageStatusList.Codes.AwaitingMasterBillDelete,
				MessageStatusList.Codes.AwaitingMasterBillRegistration,
				MessageStatusList.Codes.AwaitingMasterBillUpdate
			};

			foreach (ICodeDescription pair in new MessageStatusList())
			{
				AssertEquals(pair.Code, pendingCodes.Contains(pair.Code), MessageStatusList.IsMessagingInProgressType(pair.Code));
			}
		}
	}
}
