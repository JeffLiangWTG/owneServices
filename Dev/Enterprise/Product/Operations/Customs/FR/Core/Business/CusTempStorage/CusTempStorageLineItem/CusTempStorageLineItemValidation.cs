using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.CusTempStorage;

namespace Enterprise.Customs.FR.Business.CusTempStorage
{
	public class CusTempStorageLineItemValidation : EU.Business.CusTempStorage.CusTempStorageLineItemValidation
	{
		public CusTempStorageLineItemValidation(AutoCusTempStorageLineItem parent) : base(parent)
		{
		}

		protected override void CheckTSI_GuaranteedValue()
		{
			base.CheckTSI_GuaranteedValue();
			MandatoryValidation.MessageErrorIfIsZero(Parent.TSI_GuaranteedValueInfo);
		}

		protected override void CheckTSI_RX_NKCurrency()
		{
			base.CheckTSI_RX_NKCurrency();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.TSI_RX_NKCurrencyInfo);
		}

		protected override void CheckTSI_GoodsOrigin()
		{
			base.CheckTSI_GoodsOrigin();
			ListValidation.MessageErrorIfInvalidCode(Parent.TSI_GoodsOriginInfo);
		}

		public new CusTempStorageLineItem Parent => (CusTempStorageLineItem)base.Parent;
	}
}
