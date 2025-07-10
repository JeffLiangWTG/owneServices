//--------------------------------------------------------------------------------------------------

// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoJPAFRHeaderValidation
//
//    This class should be used for overriding validation in AutoJPAFRHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.JP.AFR.Business
{
	using CargoWise.EntityFramework;
	using Helper = VesselDetailValidationHelper;

	public class JPAFRHeaderValidation : AutoJPAFRHeaderValidation
	{
		public JPAFRHeaderValidation(AutoJPAFRHeader parent) : base(parent) { }

		protected new JPAFRHeader Parent => (JPAFRHeader)base.Parent;

		#region CheckMethods

		protected override void CheckJPH_Voyage()
		{
			base.CheckJPH_Voyage();
			var targetInfo = Parent.JPH_VoyageInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckVoyageNumber(targetInfo);
			}
		}

		protected override void CheckJPH_VesselName()
		{
			base.CheckJPH_VesselName();
			var targetInfo = Parent.JPH_VesselNameInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckVesselName(targetInfo, Parent.Vessel);
			}
		}

		protected override void CheckJPH_RadioCallSign()
		{
			base.CheckJPH_RadioCallSign();
			var targetInfo = Parent.JPH_RadioCallSignInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckCallSign(targetInfo);
			}
		}

		protected override void CheckJPH_RN_NKCountryOfReg()
		{
			base.CheckJPH_RN_NKCountryOfReg();
			var targetInfo = Parent.JPH_RN_NKCountryOfRegInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckCountryOfReg(targetInfo);
			}
		}

		protected override void CheckJPH_CarrierCode()
		{
			base.CheckJPH_CarrierCode();
			var targetInfo = Parent.JPH_CarrierCodeInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckCarrierCode(targetInfo, Parent.Carrier);
			}
		}

		protected override void CheckJPH_RL_NKLoading()
		{
			base.CheckJPH_RL_NKLoading();
			var targetInfo = Parent.JPH_RL_NKLoadingInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckLoadingPortCode(targetInfo, Parent.Loading);
			}
		}

		protected override void CheckJPH_LoadingPortSuffix()
		{
			base.CheckJPH_LoadingPortSuffix();
			var targetInfo = Parent.JPH_LoadingPortSuffixInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckLoadingPortSuffix(targetInfo);
			}
		}

		protected override void CheckJPH_DischargePortSuffix()
		{
			if (Parent.JPH_IsShippingLineEntry)
			{
				base.CheckJPH_DischargePortSuffix();
				var targetInfo = Parent.JPH_DischargePortSuffixInfo;
				if (PassCheckWithBillStatus(targetInfo))
				{
					Helper.CheckDischargePortSuffix(targetInfo);
				}
			}
		}

		protected override void CheckJPH_RelaxedAppId()
		{
			base.CheckJPH_RelaxedAppId();
			PassCheckWithBillStatus(Parent.JPH_RelaxedAppIdInfo);
		}

		protected override void CheckJPH_RL_NKDischarge()
		{
			base.CheckJPH_RL_NKDischarge();
			var targetInfo = Parent.JPH_RL_NKDischargeInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckDischargePortCode(targetInfo);
			}
		}

		protected override void CheckJPH_ETD()
		{
			base.CheckJPH_ETD();
			var targetInfo = Parent.JPH_ETDInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckETD(targetInfo);
			}
			ValidateJPH_ETA();
		}

		protected override void CheckJPH_ETA()
		{
			base.CheckJPH_ETA();
			var eTA = Parent.JPH_ETA;
			var targetInfo = Parent.JPH_ETAInfo;
			if (PassCheckWithBillStatus(targetInfo))
			{
				Helper.CheckETA(targetInfo, Parent.JPH_ETD);
				if (eTA.IsValid && !Parent.IsInDatabase && eTA.Date.ToDateTime() < ValidationUtils.GetCurrentJPDate)
				{
					targetInfo.AddMessageError(ValidationConstants.Header.PastDateNotAllowedForETA);
				}
			}
		}

		protected override void CheckJPH_MasterBillNumber()
		{
			base.CheckJPH_MasterBillNumber();

			var mBOLNum = Parent.JPH_MasterBillNumber;
			var mBOLInfo = Parent.JPH_MasterBillNumberInfo;
			if (!Parent.JPH_IsShippingLineEntry && PassCheckWithBillStatus(mBOLInfo))
			{
				MandatoryValidation.MessageErrorIfNotEntered(mBOLInfo);
				if (mBOLNum.Length < 6)
				{
					mBOLInfo.AddMessageError(ValidationConstants.Shared.BOLNumMissingCarrierCode);
				}
				else
				{
					var carrierCodeInMBOL = mBOLNum.Left(4);
					var carrierCode = Parent.JPH_CarrierCode;

					if (!carrierCodeInMBOL.IsValidBillPrefix())
					{
						mBOLInfo.AddMessageError(ValidationConstants.Header.MBOLStartWithInvalidCarrierCode + " " + ValidationConstants.Header.CarrierCodeInvalid);
					}
					else if (!string.IsNullOrEmpty(carrierCode) && carrierCodeInMBOL != carrierCode.PadRight(4, '-'))
					{
						mBOLInfo.AddWarning(ValidationConstants.Header.MBOLNumDoesntMatchCarrierCode);
					}
				}

				if (mBOLNum.ExcludeChars(ValidationConstants.Constants.ValidNACCSCharactersForBillNumber).Length != 0)
				{
					mBOLInfo.AddMessageError(ValidationConstants.Shared.InvalidNACCSCharInBOLNumber);
				}

				if (Parent.IsMBOLNumDuplicate())
				{
					mBOLInfo.AddWarning(ValidationConstants.Header.MBOLDuplicated);
				}

				if (!mBOLNum.IsEmpty && mBOLNum.StartsWith(" ", System.StringComparison.CurrentCultureIgnoreCase))
				{
					mBOLInfo.AddMessageError(ValidationConstants.Header.BOLNumberCannotStartWithSpace);
				}
			}
		}

		protected override void CheckJPH_VesselDetailsChanged()
		{
			base.CheckJPH_VesselDetailsChanged();
			if (Business.ValidationUtils.GetCurrentJPDate < JPAFRRegistry.Instance.AFR2017EffectiveLiveDate.Value && !Parent.JPH_IsShippingLineEntry)
			{
				var vesselDetailsChanged = Parent.JPH_VesselDetailsChanged;
				if (vesselDetailsChanged)
				{
					Parent.JPH_VesselDetailsChangedInfo.AddMessageError(ValidationConstants.Header.VesselDetailsChangedIsNotActive);
				}
			}
		}

		#endregion

		#region Implementation

		bool PassCheckWithBillStatus(ZPropertyInfo targetInfo)
		{
			var passResult = true;
			if (Parent.IsInDatabase)
			{
				var oldValue = targetInfo.OriginalValue;
				var newValue = targetInfo.Value;
				var shouldValidateAgainstReleaseStatus = Parent.ShouldValidateVesselInformationChange == null || Parent.ShouldValidateVesselInformationChange();
				if (!oldValue.Equals(newValue))
				{
					var fieldName = targetInfo.HumanReadableName;
					if (Parent.IsAnyBillAlreadyRegistered && shouldValidateAgainstReleaseStatus)
					{
						targetInfo.AddError(ValidationConstants.Shared.ChangingFieldWhenBillIsRegistered(fieldName, oldValue.ToString(), newValue.ToString()));
						passResult = false;
					}
					else if (Parent.IsAnyBillHaveMessagingInProgress)
					{
						targetInfo.AddError(ValidationConstants.Shared.ChangingFieldWhenMessagingIsInProgress(fieldName, oldValue.ToString(), newValue.ToString()));
						passResult = false;
					}
				}
			}
			return passResult;
		}

		#endregion
	}
}
