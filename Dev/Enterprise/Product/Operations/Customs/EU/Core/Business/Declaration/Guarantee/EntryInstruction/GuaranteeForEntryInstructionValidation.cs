using CargoWise.EntityFramework;

namespace Enterprise.Customs.EU.Business.Declaration
{
	public class GuaranteeForEntryInstructionValidation : CommonGuaranteeValidation
	{
		public GuaranteeForEntryInstructionValidation(GuaranteeForEntryInstruction parent) : base(parent)
		{
		}

		protected new GuaranteeForEntryInstruction Parent => (GuaranteeForEntryInstruction)base.Parent;

		protected override void CheckPW_BondType()
		{
			base.CheckPW_BondType();
			CheckBondTypeMandatoryValidation();
		}

		protected virtual void CheckBondTypeMandatoryValidation()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondTypeInfo);
		}

		protected override void CheckPW_BondNumber()
		{
			base.CheckPW_BondNumber();
			AddValidationErrorForBondNumberOrBondNumber2IfRequired(Parent.PW_BondNumberInfo);
		}

		protected override void CheckPW_BondNumber2()
		{
			base.CheckPW_BondNumber2();
			AddValidationErrorForBondNumberOrBondNumber2IfRequired(Parent.PW_BondNumber2Info);
		}

		protected override void CheckPW_Password()
		{
			base.CheckPW_Password();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_PasswordInfo);
		}

		protected override void CheckPW_RX_NKCurrency()
		{
			base.CheckPW_RX_NKCurrency();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PW_RX_NKCurrencyInfo);
		}

		protected override void CheckPW_BondFiledPort()
		{
			base.CheckPW_BondFiledPort();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.PW_BondFiledPortInfo);
		}

		protected override void CheckPW_SuretyCode()
		{
			base.CheckPW_SuretyCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.PW_SuretyCodeInfo);
		}

		void AddValidationErrorForBondNumberOrBondNumber2IfRequired(ZPropertyInfo propertyInfo)
		{
			if (CheckBondNumberOrBondNumber2IsRequired)
			{
				if (Parent.PW_BondNumber.IsEmpty && Parent.PW_BondNumber2.IsEmpty)
				{
					propertyInfo.AddMessageError(AtLeastReferenceOrReference2Required);
				}
				else if (!Parent.PW_BondNumber.IsEmpty && !Parent.PW_BondNumber2.IsEmpty)
				{
					propertyInfo.AddMessageError(BothReferenceAndReference2CannotBeEnteredInSameLine);
				}
			}
		}

		#region Validation Messages

		static string AtLeastReferenceOrReference2Required => Res.GetString("6C7B9ED2-8FA8-44BB-BEBD-52673DFD3116", "At least a Reference (GRN) or a Reference 2 is required");
		static string BothReferenceAndReference2CannotBeEnteredInSameLine => Res.GetString("56FB9137-0C2C-4FC4-9CF3-F4F185FE812A", "Both Reference (GRN) and Reference 2 cannot be entered in the same line");

		#endregion

		protected virtual bool CheckBondNumberOrBondNumber2IsRequired => true;
	}
}
