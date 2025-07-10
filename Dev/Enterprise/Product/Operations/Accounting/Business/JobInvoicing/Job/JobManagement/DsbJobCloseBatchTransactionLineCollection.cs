using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class DsbJobCloseBatchTransactionLineCollection : DependentBusinessObjectCollection<AccTransactionLines, DsbJobCloseBatch>
	{
		public DsbJobCloseBatchTransactionLineCollection(DsbJobCloseBatch jobCloseBatch)
			: base(jobCloseBatch)
		{
		}

		protected override string FkColumnName
		{
			get
			{
				return AccTransactionLinesSchema.Constants.AL_JBB;
			}
		}
	}
}
