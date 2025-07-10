using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public class PRLCONCusTempStorageLineToConsolidateValidation : PRLCONCusTempStorageLineValidation
	{
		public PRLCONCusTempStorageLineToConsolidateValidation(AutoCusTempStorageLine parent) : base(parent)
		{
		}

		public new PRLCONCusTempStorageLineToConsolidate Parent => (PRLCONCusTempStorageLineToConsolidate)base.Parent;

		protected override void CheckTSL_OwnerReferenceType()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_OwnerReferenceType();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceTypeInfo);
			}
		}

		protected override void CheckTSL_LineNo()
		{
			if (!Parent.IsAWBDeclaration)
			{
				base.CheckTSL_LineNo();
			}
		}

		protected override void CheckTSL_ReferenceNumberLine()
		{
			if (Parent.IsREGDeclaration)
			{
				base.CheckTSL_ReferenceNumberLine();
				MandatoryValidation.MessageErrorIfIsZero(Parent.TSL_ReferenceNumberLineInfo);
			}
		}

		protected override void CheckTSL_OwnerReferenceNumber()
		{
			if (Parent.IsAWBDeclaration)
			{
				base.CheckTSL_OwnerReferenceNumber();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.TSL_OwnerReferenceNumberInfo, OwnerReferenceNoHumanReadableName);
			}
		}

		protected override void CheckTSL_ReferenceNumber()
		{
			base.CheckTSL_ReferenceNumber();

			var parent = Parent;
			var targetInfo = parent.TSL_ReferenceNumberInfo;
			if (parent.IsREGDeclaration)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.TSL_ReferenceNumberInfo, ReferenceHumanReadableName);
				if (!IsUniqueLineNoAndATBNoCombination((x) => x.TSL_ReferenceNumberLine == parent.TSL_ReferenceNumberLine && x.TSL_ReferenceNumber == parent.TSL_ReferenceNumber))
				{
					targetInfo.AddMessageError(Res.GetString("3AED3953-A623-4E38-A7CA-2756F50A1E22", "The combination of ATB Line No. {0} and ATB No. {1} already exists for this declaration.", Parent.TSL_ReferenceNumberLine, Parent.FormattedReferenceNumber));
				}

				var referenceNumberValidationError = RegistrationNumberValidationHelper.ValidateRegistrationNumberLengthAndMrnFormat(parent.TSL_ReferenceNumber, parent.Factory);
				if (!string.IsNullOrWhiteSpace(referenceNumberValidationError))
				{
					targetInfo.AddMessageError(referenceNumberValidationError);
				}
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
	}
}
