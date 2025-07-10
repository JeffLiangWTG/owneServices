using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using ChargeTypes = Enterprise.Customs.BR.Business.ImportCustomsChargeTypeList.Codes;

namespace Enterprise.Customs.BR.Business
{
	public partial class GroupInvoiceCharge : AutoGroupInvoiceCharge
	{
		public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool J7_PrepaidCollect_ReadOnly => J7_ChargeType == ChargeTypes.OverseasFreightPrepaid || J7_ChargeType == ChargeTypes.OverseasFreightCollect;

		[ReadOnlyMember(nameof(J7_PrepaidCollect_ReadOnly))]
		public override ZString J7_PrepaidCollect { get => base.J7_PrepaidCollect; set => base.J7_PrepaidCollect = value; }

		protected override bool GetJ7_RX_NKCurrency_ReadOnly() => base.GetJ7_RX_NKCurrency_ReadOnly() || IsOtherExpensesICMS;

		internal bool IsFreightComponents => J7_ChargeType == ChargeTypes.FreightComponents;

		internal bool IsOtherExpensesICMS => J7_ChargeType == ChargeTypes.OtherExpensesICMS;

		public override ZString J7_ChargeType
		{
			get => base.J7_ChargeType;
			set
			{
				var oldValue = J7_ChargeType;
				base.J7_ChargeType = value;
				if (!IsCopying && oldValue != J7_ChargeType)
				{
					if (J7_ChargeType == ChargeTypes.OverseasFreightPrepaid)
					{
						J7_PrepaidCollect = Core.Constants.PaymentType.Prepaid;
					}
					if (IsFreightComponents || IsOtherExpensesICMS)
					{
						J7_RX_NKCurrency = Core.Constants.CurrencyCodes.Brazil;
					}
				}
			}
		}
	}
}
