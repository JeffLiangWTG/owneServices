using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.IE.EMCS.Business
{
	public class EMCSPackageValidation : EU.EMCS.Business.EMCSPackageValidation
	{
		public EMCSPackageValidation(EMCSPackage parent)
			: base(parent)
		{
		}

		protected new EMCSPackage Parent => (EMCSPackage)base.Parent;

		protected override void CheckB5_UnitCount()
		{
			if (Parent.IsCountable())
			{
				base.CheckB5_UnitCount();
			}
		}

		protected override bool IsMarksAndNumbersRequired => Parent.IsCountable();
	}
}
