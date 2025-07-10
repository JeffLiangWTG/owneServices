using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class MessageSendingActionValidation : AutoMessageSendingActionValidation
	{
		public MessageSendingActionValidation(AutoMessageSendingAction parent)
			: base(parent)
		{
		}

		public new MessageSendingAction Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (MessageSendingAction)base.Parent; }
		}

		protected override void CheckJPM_MasterBillOfLadingNumber()
		{
			base.CheckJPM_MasterBillOfLadingNumber();
			var targetInfo = Parent.JPM_MasterBillOfLadingNumberInfo;
			var parentHeader = Parent.Header;
			if (!parentHeader.JPH_IsShippingLineEntry)
			{
				var mbolNumber = Parent.JPM_MasterBillOfLadingNumber;
				if (mbolNumber.IsEmpty)
				{
					targetInfo.AddError(ValidationConstants.MessageSending.BOLIsEmpty(targetInfo.HumanReadableName));
				}
				else if (mbolNumber.ExcludeChars(ValidationConstants.Constants.ValidNACCSCharactersForBillNumber).Length != 0)
				{
					targetInfo.AddError(ValidationConstants.Shared.InvalidNACCSCharMessageForSending(targetInfo.HumanReadableName));
				}
			}

			CheckHeaderForInvalidCharacters(parentHeader, targetInfo);

			var messageSendingObjects = Parent.MessageSendingObjects;
			if (messageSendingObjects != null)
			{
				var distinctHouseBillCount = messageSendingObjects.OfType<MessageSendingObject>().Select(sendingObject => sendingObject.JPM_BillOfLadingNumber).Distinct().Count();
				if (distinctHouseBillCount != messageSendingObjects.Count)
				{
					targetInfo.AddError(ValidationConstants.MessageSending.DuplicationBillNumber);
				}
			}
		}

		void CheckHeaderForInvalidCharacters(JPAFRHeader header, CargoWise.EntityFramework.ZPropertyInfo targetInfo)
		{
			targetInfo.AddErrorIfContainsInvalidNACCSCharacter(header.JPH_CarrierCodeInfo, header.JPH_CarrierCode);
			targetInfo.AddErrorIfContainsInvalidNACCSCharacter(header.JPH_VoyageInfo, header.JPH_Voyage);
			targetInfo.AddErrorIfContainsInvalidNACCSCharacter(header.JPH_RadioCallSignInfo, header.JPH_RadioCallSign);
			targetInfo.AddErrorIfContainsInvalidNACCSCharacter(header.JPH_RL_NKLoadingInfo, header.JPH_RL_NKLoading);
			targetInfo.AddErrorIfContainsInvalidNACCSCharacter(header.JPH_LoadingPortSuffixInfo, header.JPH_LoadingPortSuffix);
		}

		protected override void CheckJPM_ETA()
		{
			base.CheckJPM_ETA();
			var targetValue = Parent.JPM_ETA;
			if (targetValue.IsValid && targetValue < ValidationUtils.GetCurrentJPDate)
			{
				Parent.JPM_ETAInfo.AddMessageError(ValidationConstants.Header.PastDateNotAllowedForETA);
			}
		}
	}
}
