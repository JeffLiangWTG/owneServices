using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusStatementLineChargeCollection : DependentBusinessObjectCollection<CusStatementLineCharge, CusStatementLine>
	{
		public CusStatementLineChargeCollection(CusStatementLine statementLine)
			: base(statementLine)
		{
		}

		public new CusStatementLine Master
		{
			get { return base.Master; }
		}

		/// <summary>
		/// If there is an existing charge and amount == 0, then the charge is to be deleted
		/// If there is no charge, then create a new charge and update amount.
		/// </summary>
		public void UpdateLineChargeFor(ZString chargeType, ZDecimal amount, string paymentParty = null, bool allowExistedType = false)
		{
			var result = allowExistedType ? null : GetFirstCharge(chargeType);

			if (result != null && amount == 0)
			{
				result.Delete();
			}
			else if (amount != 0)
			{
				if (result == null)
				{
					result = AddNew();
					result.B4_ChargeType = chargeType;
				}
				result.B4_ChargeAmount = amount;
				if (paymentParty != null)
				{
					result.B4_PaymentParty = paymentParty;
				}
			}
		}

		public CusStatementLineCharge GetFirstCharge(ZString chargeType)
		{
			var lines = GetChargesFor(chargeType);
			return lines.Length > 0 ? lines[0] : null;
		}

		public ZDecimal GetAmountFor(params ZString[] chargeType)
		{
			var lines = GetChargesFor(chargeType);
			return lines.Cast<CusStatementLineCharge>().Sum(x => x.B4_ChargeAmount);
		}

		CusStatementLineCharge[] GetChargesFor(params ZString[] chargeType)
		{
			return (CusStatementLineCharge[])Find(new ZQuery(CusStatementLineChargeSchema.B4_ChargeType, chargeType));
		}
	}
}
