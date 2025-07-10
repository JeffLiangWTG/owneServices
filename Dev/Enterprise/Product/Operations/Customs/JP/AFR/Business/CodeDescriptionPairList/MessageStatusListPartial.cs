using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.AFR.Business
{
	partial class MessageStatusList
	{
		public static ICodeDescriptionPairList GetHeaderLevelStatusList(BusinessObjectFactory factory, bool isForVOCC = false)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("JPAFRMessageStatusList_Header" + (isForVOCC ? "V" : ""), () =>
				{
					var result = new CodeDescriptionPairList();
					if (isForVOCC)
					{
						result.AddPair(Codes.AwaitingDepartureTimeRegistration, Descriptions.AwaitingDepartureTimeRegistration);
					}
					result.AddPair(Codes.AwaitingHouseBillRegistrationCompletion, Descriptions.AwaitingHouseBillRegistrationCompletion);
					if (isForVOCC)
					{
						result.AddPair(Codes.ClearDepartureTimeRegistration, Descriptions.ClearDepartureTimeRegistration);
					}
					result.AddPair(Codes.ClearHouseBillRegistrationCompletion, Descriptions.ClearHouseBillRegistrationCompletion);
					if (isForVOCC)
					{
						result.AddPair(Codes.ErrorDepartureTimeRegistration, Descriptions.ErrorDepartureTimeRegistration);
					}
					result.AddPair(Codes.ErrorHouseBillRegistrationCompletion, Descriptions.ErrorHouseBillRegistrationCompletion);
					return result;
				});
		}

		public static ICodeDescriptionPairList GetBillLevelStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue<ICodeDescriptionPairList>("JPAFRMessageStatusList_Bill", () =>
			{
				var result = new MessageStatusList();
				foreach (ICodeDescription pair in GetHeaderLevelStatusList(factory, true))
				{
					result.RemoveCode(pair.Code);
				}
				return result;
			});
		}

		public static bool IsMessagingInProgressType(ZString code)
		{
			return code == Codes.AwaitingDepartureTimeRegistration ||
				code == Codes.AwaitingHouseBillAdd ||
				code == Codes.AwaitingHouseBillDelete ||
				code == Codes.AwaitingHouseBillRegistration ||
				code == Codes.AwaitingHouseBillRegistrationCompletion ||
				code == Codes.AwaitingHouseBillUpdate ||
				code == Codes.AwaitingMasterBillAddAfterATD ||
				code == Codes.AwaitingMasterBillDelete ||
				code == Codes.AwaitingMasterBillRegistration ||
				code == Codes.AwaitingMasterBillUpdate;
		}
	}
}
