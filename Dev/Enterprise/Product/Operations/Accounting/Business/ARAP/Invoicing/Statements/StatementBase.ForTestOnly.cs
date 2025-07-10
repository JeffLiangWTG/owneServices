#if DEBUG

using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public partial class StatementBase
	{
		public string[] GetNotIncludedBatchTypeCodes_ForTestOnly()
		{
			return GetNotIncludedBatchTypeCodes();
		}

		public ZSqlParameter BatchTypeParam_ForTestOnly
		{
			get { return BatchTypeParam; }
			set { BatchTypeParam = value; }
		}
	}
}

#endif
