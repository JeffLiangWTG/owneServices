using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitConsignmentPackageValidation : EU.ExitControl.Business.CusExitConsignmentPackageValidation
	{
		public CusExitConsignmentPackageValidation(CusExitConsignmentPackage parent) : base(parent)
		{
		}

		protected override void CheckCXP_Quantity()
		{
			base.CheckCXP_Quantity();
			var parent = Parent;
			var targetInfo = parent.CXP_QuantityInfo;

			MandatoryValidation.CheckNotNegative(targetInfo);

			if (parent.IsBreakBulk)
			{
				CommonValidation.CheckGreaterThanZero(targetInfo);
			}
			else if (parent.CXP_Quantity != 0 && parent.IsBulk)
			{
				targetInfo.AddMessageError(Res.GetString("2C5DA28E-D39E-45EC-8467-EFF12A9C6498", "Package quantity must be zero when package type is bulk"));
			}
		}

		protected override void CheckCXP_PackageType()
		{
			base.CheckCXP_PackageType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.CXP_PackageTypeInfo);
		}

		protected override void CheckCXP_MarksAndNumbers()
		{
			base.CheckCXP_MarksAndNumbers();
			if (!(Parent.IsBulk || Parent.IsBreakBulk))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CXP_MarksAndNumbersInfo);
			}
		}
	}
}
