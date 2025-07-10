using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class GuaranteeVoucherSoldSendingActionValidation : ZValidation
	{
		public GuaranteeVoucherSoldSendingActionValidation(GuaranteeVoucherSoldSendingAction parent) : base(parent)
		{
			Parent = parent;
		}
		public GuaranteeVoucherSoldSendingAction Parent { get; }

		public override Type AutoValidationType => typeof(GuaranteeVoucherSoldSendingActionValidation);

		public override void ValidateAll()
		{
			ValidateHolderOfTransitProcedure();
			ValidateVoucherAmount();
			ValidateCustomsOfficeOfGuarantee();
			CheckExpiryDate();
		}

		public void ValidateHolderOfTransitProcedure()
		{
			ValidateCalculatedProperty(Parent.HolderOfTransitProcedureInfo);
		}

		public void ValidateVoucherAmount()
		{
			ValidateCalculatedProperty(Parent.VoucherAmountInfo);
		}

		public void ValidateCustomsOfficeOfGuarantee()
		{
			ValidateCalculatedProperty(Parent.CustomsOfficeOfGuaranteeInfo);
		}

		void CheckExpiryDate()
		{
			if (Parent.Header.CPH_EndDate.IsEmpty)
			{
				Parent.AddRowMessageError(Res.GetString("F80532C9-038A-484B-849E-DCFB3A31ED0E", "Please enter the end date of the guarantee."));
			}
		}

		protected void CheckHolderOfTransitProcedure()
		{
			var parent = Parent;
			if (!parent.IsValidationSuspended)
			{
				var info = parent.HolderOfTransitProcedureInfo;
				if (info.Value.IsEmpty)
				{
					info.AddError(MandatoryValidation.MustBeEnteredMessage(info.Description));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(info);
				}
			}
		}

		protected void CheckVoucherAmount()
		{
			var parent = Parent;
			if (!parent.IsValidationSuspended)
			{
				if (parent.TIRCarnet)
				{
					var info = parent.VoucherAmountInfo;
					if (info.Value.IsEmpty)
					{
						info.AddError(Res.GetString("E8A80C37-C18A-4211-944D-B5D8C8D9AA08", "If TIR Carnet is YES, then {0} is required.", info.HumanReadableName));
					}
				}
			}
		}

		protected void CheckCustomsOfficeOfGuarantee()
		{
			var parent = Parent;
			if (!parent.IsValidationSuspended)
			{
				var info = parent.CustomsOfficeOfGuaranteeInfo;
				if (info.Value.IsEmpty)
				{
					info.AddError(MandatoryValidation.MustBeEnteredMessage(info.Description));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(info);
				}
			}
		}
	}
}
