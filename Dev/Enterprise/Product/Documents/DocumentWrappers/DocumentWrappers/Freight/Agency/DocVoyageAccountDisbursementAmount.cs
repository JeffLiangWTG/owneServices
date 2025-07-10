using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocVoyageAccountDisbursementAmount : DocBaseWrapper
	{
		DocVoyageAccountDisbursementAmount(ZString title, Money localAmount, BusinessObjectFactory factory)
			: base(null, factory)
		{
			this.title = title;
			this.localAmount = localAmount;
		}

		public static DocVoyageAccountDisbursementAmount New(ZString title, Money localAmount, BusinessObjectFactory factory)
		{
			return new DocVoyageAccountDisbursementAmount(title, localAmount, factory);
		}

		public static DocVoyageAccountDisbursementAmount New(DocVoyageAccountDisbursementAmount amount, BusinessObjectFactory factory)
		{
			return new DocVoyageAccountDisbursementAmount(amount.Title, amount.LocalAmount.AmountAsMoney, factory);
		}

		public ZString Title
		{
			get { return title; }
		}

		public MoneyWrapper LocalAmount
		{
			get { return new MoneyWrapper(localAmount, Factory); }
		}

		#region Implementation

		readonly ZString title;
		readonly Money localAmount;

		#endregion
	}
}
