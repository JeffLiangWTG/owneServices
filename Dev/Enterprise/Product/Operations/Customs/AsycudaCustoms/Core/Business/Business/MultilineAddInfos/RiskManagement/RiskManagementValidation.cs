using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.AsycudaCustoms.Business
{
	public class RiskManagementValidation : CusSupportingInfoValidation
	{
		public RiskManagementValidation(RiskManagement parent)
			: base(parent)
		{
			isRiskManagementEnabled = Parent.Parent?.JobDeclaration?.IsRiskManagementEnabled ?? false;
		}
		readonly bool isRiskManagementEnabled;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.CheckEntered(Parent.CSI_CodeInfo);
				ListValidation.ErrorIfInvalidCode(Parent.CSI_CodeInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.CheckEntered(Parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.CheckEntered(Parent.CSI_DateOfIssueInfo);
			}
		}

		protected override void CheckCSI_ValueIsValidMoney()
		{
			TypeValidation.CheckValidMoney(Parent.CSI_ValueInfo, 19, 2);
		}

		protected override void CheckCSI_Value()
		{
			base.CheckCSI_Value();
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.CheckNotNegative(Parent.CSI_ValueInfo);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ValueInfo);
			}
		}

		protected override void CheckCSI_QuantityIsValidZDecimal()
		{
			TypeValidation.CheckValidDecimal(Parent.CSI_QuantityInfo, 19, 3);
		}

		protected override void CheckCSI_Quantity()
		{
			base.CheckCSI_Quantity();
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_QuantityInfo);
			}
		}

		protected override void CheckCSI_Quantity2()
		{
			base.CheckCSI_Quantity2();
			if (isRiskManagementEnabled)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_Quantity2Info);
			}
		}

		protected new RiskManagement Parent => (RiskManagement)base.Parent;
	}
}
