using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class LADTCusTempStorageLineValidation : CusTempStorageLineValidation
	{
		public LADTCusTempStorageLineValidation(LADTCusTempStorageLine parent) : base(parent)
		{
		}

		protected override void CheckTSL_LocationOfGoods()
		{
			base.CheckTSL_LocationOfGoods();

			if (Parent.Lookups.GoodsLocations.Count > 0)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.TSL_LocationOfGoodsInfo);
			}
		}
	}
}
