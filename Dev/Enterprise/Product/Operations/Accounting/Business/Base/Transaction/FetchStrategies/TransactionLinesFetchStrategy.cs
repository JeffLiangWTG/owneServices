using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class TransactionLinesFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public TransactionLinesFetchStrategy(TransactionLine line)
			: base(line)
		{
		}

		TransactionLine Line
		{
			get { return BusinessObject as TransactionLine; }
		}

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();
			Factory.AddFetchHint(AccChargeCodeSchema.Constants.TableName, Line.AL_AC);
			Factory.AddFetchHint(AccGLHeaderSchema.Constants.TableName, Line.AL_AG);
			Factory.AddFetchHint(typeof(Job), Line.AL_JH);
			Factory.AddFetchHint(OrgHeaderSchema.Constants.TableName, Line.AL_OH);
			Factory.AddFetchHint(GlbBranchSchema.Constants.TableName, Line.AL_GB);
			Factory.AddFetchHint(GlbDepartmentSchema.Constants.TableName, Line.AL_GE);
		}
	}
}
