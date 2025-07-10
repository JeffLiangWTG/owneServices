using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	/// <summary>
	/// Base class for the integration between AR/AP and JobInvoicing
	/// </summary>
	public abstract class BaseIntegrationTransformer
	{
		protected BaseIntegrationTransformer(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public void Transform(TransactionHeaderWithLines transaction)
		{
			var prevIgnoreValidationSuspended = transaction.IgnoreValidationSuspended;
			using (new DisposableAction(() => transaction.IgnoreValidationSuspended = false, () => transaction.IgnoreValidationSuspended = prevIgnoreValidationSuspended))
			{
				Factory.SuspendValidation();
				try
				{
					var context = GetUserContextForCorrectCompany(transaction);
					if (context != Env.CurrentUserContext)
					{
						using (Env.SetTemporaryUserContext(context))
						{
							TransformCore(transaction);
						}
					}
					else
					{
						TransformCore(transaction);
					}
				}
				finally
				{
					Factory.ResumeValidation();
				}
			}
		}

		protected abstract void TransformCore(TransactionHeaderWithLines transaction);

		#region Class Members

		protected readonly BusinessObjectFactory Factory;

		#endregion

		#region Implementation

		static IUserContext GetUserContextForCorrectCompany(TransactionHeaderWithLines transaction)
		{
			var result = Env.CurrentUserContext;
			var branch = transaction.Branch;
			if (branch != null && branch.IsInDatabase && Env.CurrentCompany.PK != branch.GB_GC)
			{
				var departmentPK = result.Department.PK;
				result = new UserContext(result.User.LoginName, branch.PK.ToGuid(), departmentPK);
			}
			return result;
		}

		#endregion
	}
}
