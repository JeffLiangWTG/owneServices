using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionDocumentPivotCollection : MenuEditableBusinessObjectCollection<OperationalActionDocumentPivot>
	{
		public OperationalActionDocumentPivotCollection(OperationalAction action)
			: base(action.Factory, GetAdditionalFilter(action))
		{
			this.action = action;
		}

		static ZQuery GetAdditionalFilter(OperationalAction action)
		{
			return new ZQuery(StmMenuMenuPivotSchema.SF_SU_Inward, action.PK);
		}

		#region BusinessObjectCollection Overrides

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((OperationalActionDocumentPivot)child).SF_SU_Inward = action.PK;
		}

		protected override bool AllowNewCore
		{
			get { return !OperationalActionMenuEditableHelper.ReadOnly(action); }
		}

		protected override bool AllowRemoveCore
		{
			get { return !OperationalActionMenuEditableHelper.ReadOnly(action); }
		}

		#endregion

		readonly OperationalAction action;
	}
}
