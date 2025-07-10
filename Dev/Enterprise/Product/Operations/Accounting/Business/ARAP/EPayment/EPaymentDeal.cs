using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business
{
	public class EPaymentDeal : AccEPaymentDeal, IAccountingNumberFountainDataSource
	{
		public EPaymentDeal(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZDateTime PostDate => AED_SystemCreateTimeUtc;

		public GlbBranch Branch => GlbBranch.CurrentBranch;

		public GlbDepartment Department => GlbDepartment.CurrentDepartment;

		IDbConnected IAccountingNumberFountainDataSource.Factory => Factory;

		public override void OnSaving()
		{
			if (!IsInDatabase)
			{
				AED_InternalReference = AccountingNumberFountainWrapperFactory.Instance.EPaymentDealInternalReference.Generate(this);
			}
			base.OnSaving();
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (saveSucceeded && AED_Status == EPaymentStatusCodes.Deal.Queued)
			{
				var paymentApprovalObejctsAlreadyLoadedInFactory = Factory.GetBizOsForPK(Quote.PaymentApproval.PK.ToGuid());
				foreach (var approvalObject in paymentApprovalObejctsAlreadyLoadedInFactory)
				{
					if (approvalObject is PaymentApprovalBase approvalBase)
					{
						approvalBase.ResetCurrentDeal();
					}
				}
			}
		}
	}
}
