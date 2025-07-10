using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class CUSPCSConsolidatedCusTempStorageLineValidation : CusTempStorageLineValidation
	{
		public CUSPCSConsolidatedCusTempStorageLineValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new CUSPCSConsolidatedCusTempStorageLine Parent => (CUSPCSConsolidatedCusTempStorageLine)base.Parent;

		protected override void CheckTSL_OwnerReferenceType()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_OwnerReferenceType();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceTypeInfo);
			}
		}

		protected override void CheckTSL_CustodianIdentifier()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_CustodianIdentifier();
			}
		}

		protected override void CheckTSL_CustodianIdentifierBranchNo()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_CustodianIdentifierBranchNo();
			}
		}

		protected override void CheckTSL_GoodsOwnerIdentifier()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_GoodsOwnerIdentifier();
			}
		}

		protected override void CheckTSL_GoodsOwnerIdentifierBranchNo()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_GoodsOwnerIdentifierBranchNo();
			}
		}

		protected override void CheckTSL_OwnerReferenceNumber()
		{
			base.CheckTSL_OwnerReferenceNumber();
			var parent = Parent;
			var targetInfo = parent.TSL_OwnerReferenceNumberInfo;
			MandatoryValidation.MessageErrorIfNotEntered(targetInfo, parent.IsAWBDeclaration ? OwnerReferenceNoHumanReadableName : ReferenceHumanReadableName);

			if (parent.IsREGDeclaration)
			{
				var referenceNumberValidationError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat(parent.TSL_OwnerReferenceNumber, parent.Factory);
				if (!string.IsNullOrWhiteSpace(referenceNumberValidationError))
				{
					targetInfo.AddMessageError(referenceNumberValidationError);
				}
			}
		}
	}
}
