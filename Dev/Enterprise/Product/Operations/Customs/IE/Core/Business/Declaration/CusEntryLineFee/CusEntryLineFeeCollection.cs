using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.IE.Business.Constants;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class CusEntryLineFeeCollection : CusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>, IEUCusEntryLineFeeCollection<CusEntryLineFee, CusEntryLine>
	{
		public CusEntryLineFeeCollection(CusEntryLine entryLine, BusinessObjectFactory factory)
			: base(entryLine, factory)
		{
			this.entryLine = entryLine;
		}

		readonly CusEntryLine entryLine;

		protected override BusinessObject AddNewCore()
		{
			var newObj = (CusEntryLineFee)base.AddNewCore();
			newObj.CF_MethodOfPayment = entryLine.Declaration?.JE_PaymentMethod ?? ZString.Empty;
			return newObj;
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			return base.CreateAdditionalFilter().AddToFilter(
				CusEntryLineFeeSchema.CF_MethodOfCalculation,
				SQLComparisonOperator.NotEqual,
				CusEntryLineFeeRefundDutyMethodOfCalculation.All
			);
		}
	}
}

