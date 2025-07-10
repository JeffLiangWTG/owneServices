using CargoWise.EntityFramework;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public partial class BaseAsycudaManifestHeaderValidation
	{
		public void ValidateRegistrationDate()
		{
			ValidateCalculatedProperty(Parent.RegistrationDateInfo);
		}

		protected virtual void CheckRegistrationDate()
		{
			var registrationEntryNumber = Parent.RegistrationEntryNumber;
			if (registrationEntryNumber != null)
			{
				registrationEntryNumber.Validation.ValidateCE_IssueDate();
				Parent.RegistrationDateInfo.AddAllNotificationsFrom(registrationEntryNumber.CE_IssueDateInfo);
			}
		}

		#region SpecificCircumstanceIndicator

		public void ValidateSpecificCircumstanceIndicator()
		{
			ValidateCalculatedProperty(Parent.SpecificCircumstanceIndicatorInfo);
		}

		protected virtual void CheckSpecificCircumstanceIndicator()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.SpecificCircumstanceIndicatorInfo);

			var specificCircumstanceIndicator = Parent.SpecificCircumstanceIndicator;
			if (!specificCircumstanceIndicator.IsEmpty)
			{
				var transportMode = Parent.AMA_TransportMode;
				if (!transportMode.IsEmpty && !SpecificCircumstanceList.IsTransportModeSupported(specificCircumstanceIndicator, transportMode))
				{
					Parent.SpecificCircumstanceIndicatorInfo.AddMessageError(Res.GetString("82e3406e-c7a5-4266-918e-eb410fdb5367", "Specific Circumstance Indicator {0} cannot be used with transport mode {1}.", specificCircumstanceIndicator, transportMode));
				}
			}
		}

		#endregion

		#region MethodOfPayment

		public void ValidateMethodOfPayment()
		{
			ValidateCalculatedProperty(Parent.MethodOfPaymentInfo);
		}

		protected virtual void CheckMethodOfPayment()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.MethodOfPaymentInfo);
		}

		#endregion

		#region ValidateSpecialMentions

		public void ValidateSpecialMentions()
		{
			ValidateCalculatedProperty(Parent.SpecialMentionsInfo);
		}

		protected virtual void CheckSpecialMentions()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.SpecialMentionsInfo);
		}

		#endregion

		#region ValidateETAatFirstCustomsOffice

		public void ValidateETAatFirstCustomsOffice()
		{
			ValidateCalculatedProperty(Parent.ETAatFirstCustomsOfficeInfo);
		}

		protected virtual void CheckETAatFirstCustomsOffice()
		{
		}

		#endregion

		#region ValidateATAatFirstCustomsOffice

		public void ValidateATAatFirstCustomsOffice()
		{
			ValidateCalculatedProperty(Parent.ATAatFirstCustomsOfficeInfo);
		}

		protected virtual void CheckATAatFirstCustomsOffice()
		{
		}

		#endregion

		#region ValidateRegistrationStatus

		public void ValidateRegistrationStatus()
		{
			ValidateCalculatedProperty(Parent.RegistrationStatusInfo);
		}

		protected virtual void CheckRegistrationStatus()
		{
		}

		#endregion
	}
}
