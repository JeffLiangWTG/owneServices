using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class CusGoodsLocationValidation : EU.H7.Business.CusGoodsLocationValidation
	{
		public CusGoodsLocationValidation(EU.H7.Business.CusGoodsLocation parent) : base(parent)
		{
		}

		protected new AsycudaBill bill => Parent.Parent as AsycudaBill;

		protected AsycudaManifestHeader header => Parent.Parent as AsycudaManifestHeader;

		protected override ZString ShipmentType => bill?.ABL_ShipmentType
			?? header?.Bills.FirstOrDefault()?.ABL_ShipmentType
			?? string.Empty;

		protected override void CheckCGL_Qualifier()
		{
			base.CheckCGL_Qualifier();
			ListValidation.ErrorIfInvalidCode(Parent.CGL_QualifierInfo);
		}

		protected override void CheckCGL_Type()
		{
			base.CheckCGL_Type();
			ListValidation.ErrorIfInvalidCode(Parent.CGL_TypeInfo);
		}
	}
}
