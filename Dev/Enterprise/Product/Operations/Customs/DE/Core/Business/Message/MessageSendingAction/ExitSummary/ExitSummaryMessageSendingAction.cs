using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.DE.Business
{
	public class ExitSummaryMessageSendingAction : MessageSendingAction, IObsoleteValidation
	{
		public ExitSummaryMessageSendingAction(CusExitDetail exitDetail) : base(exitDetail, (x) => ((CusExitDetail)x).CED_MovementReferenceNumber)
		{
		}

		public new CusExitDetail MessagingObject => (CusExitDetail)base.MessagingObject;

		public ZString ReferenceNumber => MessagingObject.Header.CEH_ReferenceNumber;

		public ZString Status => MessagingObject.CED_Status;

		public ZString StatusDescription => ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, Status, Core.Constants.CountryCodes.Germany, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportCustomsStatus, ZDate.Today)?.ZZD_Description ?? ZString.Empty;

		[List(nameof(Lookups) + "." + nameof(ExitSummaryMessageSendingActionLookups.MessageTypeList))]
		[MaxLength(3)]
		public ZString MessageType
		{
			get { return messageType; }
			set
			{
				SetNonPersistentPropertyValue(MessageTypeInfo, ref messageType, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateMessageType();
				}
			}
		}
		ZString messageType;

		public ZPropertyInfo MessageTypeInfo => GetZPropertyInfo(nameof(MessageType));

		#region Lookups

		public ExitSummaryMessageSendingActionLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new ExitSummaryMessageSendingActionLookups(this);
				}

				return fLookups;
			}
		}
		ExitSummaryMessageSendingActionLookups fLookups;

		#endregion

		#region Validation

		public ExitSummaryMessageSendingActionValidation Validation => new ExitSummaryMessageSendingActionValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
	}
}
