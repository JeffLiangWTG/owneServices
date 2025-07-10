using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class DependentTransactionLineFetchStrategy : TransactionLinesFetchStrategy
	{
		public DependentTransactionLineFetchStrategy(DependentTransactionLine line)
			: base(line)
		{
		}

		DependentTransactionLine Line
		{
			get { return BusinessObject as DependentTransactionLine; }
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(col => col.ColumnName.StartsWith(DependentTransactionLine.Schema.BranchName, StringComparison.OrdinalIgnoreCase)))
			{
				Factory.AddFetchHint(GlbBranchSchema.Constants.TableName, Line.AL_GB);
			}

			if (columns.Any(col => col.ColumnName.StartsWith(DependentTransactionLine.Schema.DepartmentDescription, StringComparison.OrdinalIgnoreCase)))
			{
				Factory.AddFetchHint(GlbDepartmentSchema.Constants.TableName, Line.AL_GE);
			}
		}
	}
}