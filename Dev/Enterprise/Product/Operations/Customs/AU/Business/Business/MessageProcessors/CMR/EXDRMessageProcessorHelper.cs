using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public static class EXDRMessageProcessorHelper
	{
		public static void ClearDeclaration(EDIMessage incomingMessage, EDIMessage outgoingMessage, BaseJobDeclaration declaration)
		{
			if (incomingMessage != null)
			{
				incomingMessage.EM_MessageSubType = nameof(Core.Constants.EXDRMessageSubType.CLR);

				if (outgoingMessage != null && declaration != null)
				{
					var status = declaration.JE_EntryStatus = CustomsEntryStatus.CargoCleared.Code;      // give some value to status in case MessageSubType is not in case statement so waiting for response is replaced. 
					declaration.LogCustomsCleared();
					switch (outgoingMessage.EM_MessageSubType)
					{
						case "ORG":
							status = CustomsEntryStatus.ClearOriginal.Code;
							break;
						case "REP":
						case "AMD":
							status = CustomsEntryStatus.ClearReplacement.Code;
							break;
						case "WDW":
							status = CustomsEntryStatus.ClearWithdrawal.Code;
							break;
					}
					declaration.JE_EntryStatus = status;
					UpdateEntryHeaderStatus(declaration, status);
				}
			}
		}

		public static void ErrorDeclaration(EDIMessage incomingMessage, EDIMessage outgoingMessage, BaseJobDeclaration declaration)
		{
			if (incomingMessage != null)
			{
				incomingMessage.EM_MessageSubType = nameof(Core.Constants.EXDRMessageSubType.ERR);

				if (outgoingMessage != null && declaration != null)
				{
					var status = ZString.Empty;
					declaration.LogCustomsImpediment();
					switch (outgoingMessage.EM_MessageSubType)
					{
						case "ORG":
							status = CustomsEntryStatus.ErrorOriginal.Code;
							break;
						case "REP":
						case "AMD":
							status = CustomsEntryStatus.ErrorReplacement.Code;
							break;
						case "WDW":
							status = CustomsEntryStatus.ErrorWithdrawal.Code;
							break;
					}
					declaration.JE_EntryStatus = status;
					UpdateEntryHeaderStatus(declaration, status);
				}
			}
		}

		public static void RejectDeclaration(EDIMessage incomingMessage, EDIMessage outgoingMessage, BaseJobDeclaration declaration)
		{
			if (incomingMessage != null)
			{
				incomingMessage.EM_MessageSubType = nameof(Core.Constants.EXDRMessageSubType.REJ);

				if (outgoingMessage != null && declaration != null)
				{
					var status = ZString.Empty;
					switch (outgoingMessage.EM_MessageSubType)
					{
						case "ORG":
							status = CustomsEntryStatus.FailOriginal.Code;
							break;
						case "REP":
						case "AMD":
							status = CustomsEntryStatus.FailReplacement.Code;
							break;
						case "WDW":
							status = CustomsEntryStatus.FailWithdrawal.Code;
							break;
					}
					declaration.JE_EntryStatus = status;
					UpdateEntryHeaderStatus(declaration, status);
				}
			}
		}

		public static void RevokeDeclaration(EDIMessage incomingMessage, EDIMessage outgoingMessage, BaseJobDeclaration declaration)
		{
			RejectDeclaration(incomingMessage, outgoingMessage, declaration);

			if (incomingMessage != null)
			{
				incomingMessage.EM_MessageSubType = nameof(Core.Constants.EXDRMessageSubType.REV);
			}
		}

		public static void UpdateEntryHeaderStatus(BaseJobDeclaration declaration, ZString status)
		{
			var entryHeader = (declaration as JobDeclaration)?.EntryHeader;
			if (entryHeader != null)
			{
				entryHeader.CH_Status = status;
			}
		}
	}
}
