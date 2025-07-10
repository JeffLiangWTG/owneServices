using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class ExportPackageValidation : PackageValidation
	{
		public ExportPackageValidation(Package parent) : base(parent)
		{
		}

		protected override void CheckCW_MarksAndNos()
		{
			base.CheckCW_MarksAndNos();

			var parent = Parent;
			if (parent.Declaration is JobDeclaration declaration &&
				((declaration.IsTransitionPeriodAES30 && parent.CW_MarksAndNos.Length > 42) ||
				(!declaration.IsTransitionPeriodAES30 && parent.CW_MarksAndNos.Length > 512)))
			{
				parent.CW_MarksAndNosInfo.AddMessageError(Res.GetString("D5FA6757-E30C-4231-972B-B1607D53F8F1", "Shipping marks of packages can have up to {0} alpha numeric characters.", declaration.IsTransitionPeriodAES30 ? 42 : 512));
			}
		}

		protected override void CheckCW_PackQty()
		{
			base.CheckCW_PackQty();

			var parent = Parent;
			if (parent.IsBulk)
			{
				MandatoryValidation.MessageErrorIfIsEntered(parent.CW_PackQtyInfo);
			}
		}
	}
}
