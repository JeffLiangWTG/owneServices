using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers
{
	[DefaultField("TotalAmount")]
	public class DocAmountByChargeCode : GenericWrapper
	{
		DocAmountByChargeCode(AmountByChargeCode amountSplittedByChargeCodeObject, BusinessObjectFactory factory)
			: base(amountSplittedByChargeCodeObject, factory)
		{
			this.amountSplittedByChargeCodeObject = amountSplittedByChargeCodeObject;
		}

		public static DocAmountByChargeCode New(AmountByChargeCode amountSplittedByChargeCodeObject, BusinessObjectFactory factory)
		{
			return amountSplittedByChargeCodeObject != null ? new DocAmountByChargeCode(amountSplittedByChargeCodeObject, factory) : null;
		}

		public ZString ChargeCode
		{
			get
			{
				return amountSplittedByChargeCodeObject.ChargeCode;
			}
		}

		public ZDecimal TotalAmount
		{
			get
			{
				return amountSplittedByChargeCodeObject.TotalAmount;
			}
		}

		public ZDecimal TotalAmountExTax
		{
			get
			{
				return amountSplittedByChargeCodeObject.TotalAmountExTax;
			}
		}

		readonly AmountByChargeCode amountSplittedByChargeCodeObject;
	}

	public class AmountByChargeCode : NonPersistentBusinessObject
	{
		public AmountByChargeCode()
		{ }

		public AmountByChargeCode(ZString chargeCode, ZDecimal totalAmount, ZDecimal totalAmountExTax)
		{
			this.ChargeCode = chargeCode;
			this.TotalAmount = totalAmount;
			this.TotalAmountExTax = totalAmountExTax;
		}

		public ZString ChargeCode
		{
			get; set;
		}

		public ZDecimal TotalAmount
		{
			get; set;
		}

		public ZDecimal TotalAmountExTax
		{
			get;
			set;
		}
	}
}
