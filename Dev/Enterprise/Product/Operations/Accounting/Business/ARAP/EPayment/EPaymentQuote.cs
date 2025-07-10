using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.EPayment
{
	public class EPaymentQuote : AccEPaymentQuote, IAccountingNumberFountainDataSource
	{
		public EPaymentQuote(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDateTime PostDate => QU_SystemCreateTimeUtc;

		public ZDateTime LastResponseReceivedLocalTime => QU_LastResponseReceivedUtc.ToLocalBranchTime();

		public GlbBranch Branch => GlbBranch.CurrentBranch;

		public GlbDepartment Department => GlbDepartment.CurrentDepartment;

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				QU_InternalReference = AccountingNumberFountainWrapperFactory.Instance.EPaymentQuoteInternalRef.Generate(this);
			}
			base.OnSaving();
		}

		public override void Delete()
		{
			DeleteInactiveDeals();
			base.Delete();
		}

		void DeleteInactiveDeals()
		{
			var deals = Factory.Load<EPaymentDeal>(new ZQuery(AccEPaymentDealSchema.AED_QU_Quote, PK)).Where(deal => EPaymentStatusCodes.Deal.InactiveStatusCodes.ToList().Contains(deal.AED_Status));
			deals.ForEach(deal => deal.Delete());
		}

#if DEBUG
		/*
		 * DO NOT REMOVE. Without this, following test cases in PaymentBatchFormTestCase will fail.
		 *	 TestBashingForm
		 *	 TestBoundListsAreNotLoadedOnAccess
		 *	 TestFormIsFullyTranslatable
		 * This property is used to bind the JH_JobNum column in FilteredGrid of AccountingOnFormFilterControl
		 */
		public ZString JH_JobNum => ZString.Empty;
#endif
	}
}
