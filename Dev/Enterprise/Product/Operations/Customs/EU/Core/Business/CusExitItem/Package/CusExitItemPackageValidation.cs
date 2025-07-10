using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class CusExitItemPackageValidation : CusInvPackValidation
	{
		public CusExitItemPackageValidation(CusExitItemPackage parent) : base(parent)
		{
		}
		protected new CusExitItemPackage Parent => (CusExitItemPackage)base.Parent;

		protected override void CheckB5_UnitType()
		{
			base.CheckB5_UnitType();
			if (Parent.B5_UnitCount > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.B5_UnitTypeInfo);
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.B5_UnitTypeInfo);
		}
	}
}
