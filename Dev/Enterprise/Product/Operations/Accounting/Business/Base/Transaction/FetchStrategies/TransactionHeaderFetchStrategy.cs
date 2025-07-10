
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionHeaderFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public TransactionHeaderFetchStrategy(TransactionHeader header)
			: base(header)
		{
		}

		TransactionHeader Header
		{
			get { return BusinessObject as TransactionHeader; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(AccTransactionMatchLinkSchema.AP_AH, Header.PK);
			Factory.AddFetchHint(AccPaymentApprovalSchema.AV_AH, Header.PK);
			Factory.AddFetchHint(AccPaymentApprovalItemSchema.A2_AH, Header.PK);
			Factory.AddFetchHint(StmALogSchema.SL_Parent, Header.PK);

			Factory.AddFetchHint(GlbBranchSchema.Constants.TableName, Header.AH_GB);
			Factory.AddFetchHint(GlbDepartmentSchema.Constants.TableName, Header.AH_GE);
			Factory.AddFetchHint(RefCurrencySchema.RX_Code, Header.AH_RX_NKTransactionCurrency);
			Factory.AddFetchHint(OrgHeaderSchema.PK, Header.AH_OH);
			Factory.AddFetchHint(OrgCompanyDataSchema.OB_OH, Header.AH_OH);
			Factory.AddFetchHint(typeof(Job), Header.AH_JH);

			if (Header is TransactionHeaderWithLines)
			{
				Factory.AddFetchHint(vw_AccTransactionHeaderTaxSchema.Instance, new ZQuery(new ZQuery(vw_AccTransactionHeaderTaxSchema.PK, Header.PK)));
			}
		}
	}
}