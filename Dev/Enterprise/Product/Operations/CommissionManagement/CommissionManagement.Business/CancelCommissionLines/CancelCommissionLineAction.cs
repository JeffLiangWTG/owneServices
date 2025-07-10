using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public class CancelCommissionLineAction : NonPersistentBusinessObject
	{
		public CancelCommissionLineAction(ViewCommissionLine commissionLine)
			: this(commissionLine.Factory)
		{
			this.commissionLine = commissionLine;
		}

		internal CancelCommissionLineAction(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region CommissionLine

		readonly ViewCommissionLine commissionLine;

		public ViewCommissionLine CommissionLine
		{
			get { return Factory.Load<ViewCommissionLine>(commissionLine.PK); }
		}

		#endregion

		#region Execute

		public void Execute()
		{
			commissionLine.Cancel();
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			CheckUnpaidAndUncanceled();
		}

		void CheckUnpaidAndUncanceled()
		{
			ClearRowNotifications();
			if (commissionLine.IsPaid)
			{
				AddRowError(Res.GetString("8cafd301-f6b1-45dd-b7b7-1530d8ca5afc", "Already paid."));
			}
			else if (commissionLine.IsCancelled)
			{
				AddRowError(Res.GetString("a2b76b01-3a3d-463d-9c3d-52e91bcc4fcb", "Already canceled."));
			}
		}

		#endregion

		#region Human Readable Name

		protected override ZString HumanReadableNameCore
		{
			get { return commissionLine != null ? commissionLine.HumanReadableName : base.HumanReadableName; }
		}

		#endregion
	}
}
