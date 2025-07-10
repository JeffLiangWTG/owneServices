using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class RefundDutyFeeCollection : CusEntryLineFeeCollection
	{
		public RefundDutyFeeCollection(CusEntryLine entryLine) : base(entryLine, entryLine.Factory)
		{
		}

		public CusEntryLine CusEntryLine => Master;

		public CusEntryLineFee AddNew(ZString chargeType, ZString methodOfCalculation)
		{
			var newFee = AddNew();
			newFee.CF_ChargeType = chargeType;
			newFee.CF_MethodOfCalculation = methodOfCalculation;
			return newFee;
		}

		protected override ZQuery CreateAdditionalFilter() =>
			new ZQuery(CusEntryLineFeeSchema.CF_MethodOfCalculation, CusEntryLineFeeRefundDutyMethodOfCalculation.All);
	}
}
