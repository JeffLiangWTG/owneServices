using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public sealed class AlertOrRejectSendingAction : EMCSMessageSendingAction, IAlertOrReject, IObsoleteValidation
	{
		public AlertOrRejectSendingAction(EMCSJobDeclaration declaration) : base(declaration)
		{
		}

		[ResourceStringData("27E594AD-1E87-47A7-881D-3F920CCADCE3", Caption = "Rejected?")]
		public ZBool RejectedFlag
		{
			get => rejectedFlag;
			set
			{
				SetNonPersistentPropertyValue(RejectedFlagInfo, ref rejectedFlag, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRejectedFlag();
				}
			}
		}
		ZBool rejectedFlag;

		public ZPropertyInfo RejectedFlagInfo => GetZPropertyInfo(nameof(RejectedFlag));

		[ResourceStringData("EE5DD1E1-0129-4C11-9183-D842EF2BD547", Caption = "Date")]
		public ZDateTime DateOfAlertOrRejection
		{
			get => dateOfAlertOrRejection;
			set
			{
				SetNonPersistentPropertyValue(DateOfAlertOrRejectionInfo, ref dateOfAlertOrRejection, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateDateOfAlertOrRejection();
				}
			}
		}
		ZDateTime dateOfAlertOrRejection;

		public ZPropertyInfo DateOfAlertOrRejectionInfo => GetZPropertyInfo(nameof(DateOfAlertOrRejection));

		public AlertOrRejectReasonCollection AlertOrRejectionReasons
		{
			get
			{
				if (fAlertOrRejectionReasons == null)
				{
					fAlertOrRejectionReasons = new AlertOrRejectReasonCollection(Factory);
					RegisterEditableChildObject(fAlertOrRejectionReasons);
				}
				return fAlertOrRejectionReasons;
			}
		}

		IEnumerable<IAlertOrRejectReason> IAlertOrReject.AlertOrRejectionReasons => AlertOrRejectionReasons.Cast<IAlertOrRejectReason>().ToArray();

		AlertOrRejectReasonCollection fAlertOrRejectionReasons;

		#region Validation

		public AlertOrRejectSendingActionValidation Validation => new AlertOrRejectSendingActionValidation(this);

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion
	}
}
