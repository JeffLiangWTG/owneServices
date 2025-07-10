using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.CommissionManagement.Business
{
	public class UndoCancelCommissionLineAction : NonPersistentBusinessObject
	{
		public UndoCancelCommissionLineAction(ViewCommissionLine commissionLine)
			: this(commissionLine.Factory)
		{
			this.commissionLine = commissionLine;
		}

		internal UndoCancelCommissionLineAction(BusinessObjectFactory factory)
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
			commissionLine.UndoCancel();
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			CheckCanceled();
		}

		void CheckCanceled()
		{
			ClearRowNotifications();
			if (!commissionLine.IsCancelled)
			{
				AddRowError(Res.GetString("0d1552da-ca4e-4813-b56b-3e4aaf8e228f", "Not canceled."));
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
