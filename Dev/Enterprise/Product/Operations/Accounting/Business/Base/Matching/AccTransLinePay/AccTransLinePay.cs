using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class AccTransLinePay : AutoAccTransLinePay
	{
		public AccTransLinePay(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, System.ComponentModel.PropertyDescriptor[] propertyPath)
		{
			A7_AP = Factory.NewWithValidTestData<TransactionMatchLink>().PK;
			A7_AL = Factory.NewWithValidTestData<AccTransactionLines>().PK;
			TransactionMatchLink.AP_MatchGroupNum = "000";
			TransactionMatchLinkGroup group = new TransactionMatchLinkGroup(Factory);
			group.Add(TransactionMatchLink);
		}
#endif
		public override AccTransactionMatchLink TransactionMatchLink
		{
			get { return Factory.Load<TransactionMatchLink>(A7_AP); }
		}

		[DecimalPlaces(nameof(LocalDecimals))]
		public override ZDecimal A7_Amount
		{
			get => base.A7_Amount;
			set => base.A7_Amount = value;
		}

		[DecimalPlaces(nameof(OSDecimals))]
		public override ZDecimal A7_OSAmount
		{
			get => base.A7_OSAmount;
			set => base.A7_OSAmount = value;
		}

		public int LocalDecimals => TransactionLines != null ? TransactionLines.Company.GetLocalDecimals() : GlbCompany.CurrentCompany.GetLocalDecimals();

		public int OSDecimals => TransactionLines?.CurrencyDecimals ?? LocalDecimals;
	}
}
