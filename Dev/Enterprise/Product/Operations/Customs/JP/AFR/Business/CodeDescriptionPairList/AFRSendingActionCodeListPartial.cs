using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.AFR.Business
{
	partial class AFRSendingActionCodeList
	{
		public static CodeDescriptionPairList GetActionCodeList(BusinessObjectFactory factory, ActionCode actionCode)
		{
			var key = GetActionKey(actionCode);
			return factory.GetCachedValue("AFRSendingActionCodeList:" + key, () =>
				{
					if (key == AmendingKey)
					{
						return GetAmendingList();
					}
					else if (key == CorrectVesselInformationWithRegistrationKey)
					{
						return GetRegistrationList();
					}
					else if (key == CorrectVesselInformationWithAmendmentKey)
					{
						return GetAddOnlyList();
					}
					else if (key == CorrectMasterInformation)
					{
						return GetUpdateDeleteList();
					}
					else if (key == NewBill)
					{
						return GetRegisterAddList();
					}
					return new AFRSendingActionCodeList();
				});
		}

		static string GetActionKey(ActionCode actionCode)
		{
			var result = DefaultKey;
			switch (actionCode)
			{
				case ActionCode.AmendingAdd:
				case ActionCode.AmendingDelete:
				case ActionCode.AmendingUpdate:
					result = AmendingKey;
					break;
				case ActionCode.CorrectVesselInformationByRegistration:
					result = CorrectVesselInformationWithRegistrationKey;
					break;
				case ActionCode.CorrectVesselInformationByAmendment:
					result = CorrectVesselInformationWithAmendmentKey;
					break;
				case ActionCode.CorrectMasterInformation:
				case ActionCode.ChangeDepartureTimeAfterATD:
					result = CorrectMasterInformation;
					break;
				case ActionCode.ReRegisterMasterAfterATD:
				case ActionCode.ReRegisterMasterBeforeATD:
				case ActionCode.NewBill:
					result = NewBill;
					break;
			}
			return result;
		}

		const string AmendingKey = "Amending";
		const string CorrectVesselInformationWithRegistrationKey = "Registration";
		const string CorrectVesselInformationWithAmendmentKey = "CorrectVesselAmendment";
		const string CorrectMasterInformation = "CorrectMasterInformation";
		const string DefaultKey = "Default";
		const string NewBill = "NewBill";

		static CodeDescriptionPairList GetAmendingList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.Add, Descriptions.Add);
			result.AddPair(Codes.Delete, Descriptions.Delete);
			result.AddPair(Codes.Update, Descriptions.Update);
			return result;
		}

		static CodeDescriptionPairList GetRegistrationList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.Register, Descriptions.Register);
			return result;
		}

		static CodeDescriptionPairList GetAddOnlyList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.Add, Descriptions.Add);
			return result;
		}

		static CodeDescriptionPairList GetUpdateDeleteList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.Update, Descriptions.Update);
			result.AddPair(Codes.Delete, Descriptions.Delete);
			return result;
		}

		static CodeDescriptionPairList GetRegisterAddList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.Register, Descriptions.Register);
			result.AddPair(Codes.Add, Descriptions.Add);
			return result;
		}
	}
}
