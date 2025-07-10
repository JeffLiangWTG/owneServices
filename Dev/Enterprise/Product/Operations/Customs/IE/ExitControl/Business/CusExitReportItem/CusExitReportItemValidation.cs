using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitReportItemValidation : EU.ExitControl.Business.CusExitReportItemValidation
	{
		public CusExitReportItemValidation(CusExitReportItem parent) : base(parent)
		{
		}

		protected override void CheckERI_GrossMass()
		{
			base.CheckERI_GrossMass();

			var targetInfo = Parent.ERI_GrossMassInfo;

			if (!Parent.ERI_CXP_Package.IsEmpty)
			{
				CommonValidation.CheckGreaterThanZero(targetInfo);
				CommonValidation.CheckValueShouldGreaterThanOrEqualTo(targetInfo, Parent.ERI_NetMassInfo, targetInfo);
			}
		}

		protected override void CheckERI_NetMass()
		{
			base.CheckERI_NetMass();
			var targetInfo = Parent.ERI_NetMassInfo;
			MandatoryValidation.CheckNotNegative(targetInfo);

			if (!Parent.ERI_CXP_Package.IsEmpty)
			{
				CommonValidation.CheckGreaterThanZero(targetInfo);
				CommonValidation.CheckValueShouldGreaterThanOrEqualTo(Parent.ERI_GrossMassInfo, targetInfo, targetInfo);
				if (Parent.ERI_NetMass == 0)
				{
					targetInfo.AddMessageError(CommonValidation.GetShouldBePositiveOrShouldBeRemovedMessage(targetInfo));
				}
			}
		}

		protected override void CheckERI_Quantity()
		{
			base.CheckERI_Quantity();
			var parent = Parent;
			var targetInfo = parent.ERI_QuantityInfo;

			MandatoryValidation.CheckNotNegative(targetInfo);

			if (parent.Package?.IsBreakBulk ?? true)
			{
				CommonValidation.CheckGreaterThanZero(targetInfo);
			}
			else if (parent.ERI_Quantity != 0 && (parent.Package?.IsBulk ?? false))
			{
				targetInfo.AddMessageError(Res.GetString("41946EA7-1825-48AF-B57A-2C461B4E2277", "Package quantity must be zero when package type is bulk"));
			}
		}

		protected new CusExitReportItem Parent => (CusExitReportItem)base.Parent;
	}
}
