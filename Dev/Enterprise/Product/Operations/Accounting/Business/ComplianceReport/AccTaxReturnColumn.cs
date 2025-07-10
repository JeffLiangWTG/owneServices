using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ComplianceReport
{
	public class AccTaxReturnColumn : AutoAccTaxReturnColumn, ICanApplyDataRefresh
	{
		public AccTaxReturnColumn(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[DecimalPlaces(nameof(LocalCurrencyDecimals))]
		public override ZDecimal ATC_Amount
		{
			get => base.ATC_Amount;
			set => base.ATC_Amount = value;
		}

		[RelatedBusinessObject("TaxReturn")]
		public override ZGuid ATC_ATR_AccTaxReturn
		{
			get => base.ATC_ATR_AccTaxReturn;
			set => base.ATC_ATR_AccTaxReturn = value;
		}

		public virtual AccTaxReturn TaxReturn
		{
			get { return Factory.Load<AccTaxReturn>(ATC_ATR_AccTaxReturn); }
		}

		public int LocalCurrencyDecimals => TaxReturn.ComplianceReport.Company.GetLocalDecimals();

		public bool CanApplyDataRefresh(DataRefreshAction action, BusinessObject publisher) => false;

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			ATC_ATR_AccTaxReturn = Factory.NewWithValidTestData<AccTaxReturn>().PK;
		}
#endif
	}
}