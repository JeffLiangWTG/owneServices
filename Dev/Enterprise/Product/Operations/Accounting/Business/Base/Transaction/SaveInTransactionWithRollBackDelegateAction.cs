using System;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public class SaveInTransactionWithRollBackAction : SaveInTransactionAction
	{
		public SaveInTransactionWithRollBackAction(IDbConnected connection, Func<ChangedTableNames> methodOnSaving, Action methodOnRollback)
		{
			this.Connection = connection ?? throw new ArgumentNullException(nameof(connection));
			this.methodOnSaving = methodOnSaving;
			this.methodOnRollback = methodOnRollback;
		}

		protected sealed override ITransactionManager BeginTransactionWithManager()
		{
			return Connection.Connection.BeginTransactionWithManager(methodOnRollback);
		}

		protected sealed override bool IsInTransaction
		{
			get { return Connection.Connection.IsInTransactionOtherThanTransactionedTestCase; }
		}

		protected override IChangedTableNames SaveInTransaction()
		{
			return methodOnSaving();
		}

		readonly Func<ChangedTableNames> methodOnSaving;
		readonly Action methodOnRollback;

		readonly IDbConnected Connection;
	}
}
