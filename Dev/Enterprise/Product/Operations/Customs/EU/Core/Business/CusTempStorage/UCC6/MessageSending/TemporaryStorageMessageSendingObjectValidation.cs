using CargoWise.EntityFramework;
namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageMessageSendingObjectValidation : AutoTemporaryStorageMessageSendingObjectValidation
	{
		public TemporaryStorageMessageSendingObjectValidation(AutoTemporaryStorageMessageSendingObject parent) : base(parent)
		{
		}

		public new TemporaryStorageMessageSendingObject Parent
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return (TemporaryStorageMessageSendingObject)base.Parent; }
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			CheckFallbackProcedure();
		}

		protected override void CheckMessageType()
		{
			base.CheckMessageType();
			MandatoryValidation.CheckEntered(Parent.MessageTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.MessageTypeInfo);
		}

		protected override void CheckVOCReason()
		{
			base.CheckVOCReason();

			RuleForInvalidationRequestTSD();
		}

		void RuleForInvalidationRequestTSD()
		{
			var parent = Parent;

			if (parent.MessageType == TemporaryStorageMessageTypeList.Codes.InvalidationRequestTSD)
			{
				MandatoryValidation.CheckEntered(parent.VOCReasonInfo);
			}
		}

		protected virtual bool ShouldCheckFallbackProcedure => true;

		void CheckFallbackProcedure()
		{
			var parent = Parent;
			if (ShouldCheckFallbackProcedure && !(parent.AlternativeDateOfAcceptance.IsEmpty &&
				parent.CustomsReference.IsEmpty &&
				parent.CustomsJustification.IsEmpty))
			{
				if (parent.AlternativeDateOfAcceptance.IsEmpty)
				{
					FallbackProcedureRequires(parent.AlternativeDateOfAcceptanceInfo);
				}
				if (parent.CustomsReference.IsEmpty)
				{
					FallbackProcedureRequires(parent.CustomsReferenceInfo);
				}
				if (parent.CustomsJustification.IsEmpty)
				{
					FallbackProcedureRequires(parent.CustomsJustificationInfo);
				}
			}

			void FallbackProcedureRequires(ZPropertyInfo propertyInfo)
			{
				Parent.AddRowError(Res.GetString("F32500E7-29D2-446B-9E09-78BEB5523C81", "{0} is required for Fallback Procedure", propertyInfo.HumanReadableName));
			}
		}
	}
}
