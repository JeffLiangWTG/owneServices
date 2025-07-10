using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class REXDISCusTempStorageSumALineValidation : CusTempStorageLineValidation
	{
		public REXDISCusTempStorageSumALineValidation(REXDISCusTempStorageSumALine parent) : base(parent)
		{
		}

		public new REXDISCusTempStorageSumALine Parent => (REXDISCusTempStorageSumALine)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateReferenceNumber();
		}

		public void ValidateReferenceNumber()
		{
			ValidateCalculatedProperty(Parent.ReferenceNumberInfo);
		}

		protected void CheckReferenceNumber()
		{
			var parent = Parent;
			if (parent.IsREGDeclaration)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ReferenceNumberInfo, parent.IsAWBDeclaration ? OwnerReferenceNoHumanReadableName : ReferenceHumanReadableName);

				var atNumberToValidate = parent.ReferenceNumber.KeepAlphanumericCharacters();
				var referenceNumberValidationError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat(atNumberToValidate, parent.Factory);
				if (!string.IsNullOrWhiteSpace(referenceNumberValidationError))
				{
					parent.ReferenceNumberInfo.AddMessageError(referenceNumberValidationError);
				}
			}
		}

		protected override void CheckTSL_CustodianIdentifierMandatory()
		{
			if (declarationIsAWB_SIN)
			{
				base.CheckTSL_CustodianIdentifierMandatory();
			}
		}

		protected override void CheckTSL_CustodianIdentifierBranchNoMandatory()
		{
			if (declarationIsAWB_SIN)
			{
				base.CheckTSL_CustodianIdentifierBranchNoMandatory();
			}
		}

		protected override void CheckTSL_OwnerReferenceType()
		{
			if (declarationIsAWB_SIN)
			{
				base.CheckTSL_OwnerReferenceType();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceTypeInfo);
			}
		}

		protected override void CheckTSL_OwnerReferenceNumber()
		{
			base.CheckTSL_OwnerReferenceNumber();
			if (declarationIsAWB_SIN)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceNumberInfo);
			}
		}

		protected override void CheckTSL_LineNo()
		{
		}

		protected override void CheckTSL_ReferenceNumberLine()
		{
			if (Parent.IsREGDeclaration)
			{
				base.CheckTSL_ReferenceNumberLine();
				MandatoryValidation.MessageErrorIfIsZero(Parent.TSL_ReferenceNumberLineInfo);
			}
		}

		bool declarationIsAWB_SIN => Parent.IsAWBDeclaration || Parent.IsSINDeclaration;
	}
}
