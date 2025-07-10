using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionDocumentPivotLookups : StmMenuMenuPivotLookups
	{
		public OperationalActionDocumentPivotLookups(OperationalActionDocumentPivot parent)
			: base(parent) { }

		public StmMenuItemBaseCollection Documents
		{
			get
			{
				OperationalAction action = Parent.Action;

				if (action != null)
				{
					var context = action.Context;
					var businessContext = context == null ? action.SU_BusinessContext : (ZString)action.Context.Supporter.DocumentBusinessContext.ToString();
					return new DeliverableDocumentCommandCollection(Factory, businessContext, action.SU_GS_NKStaffCode, Parent.SF_IsSystemDefined);
				}
				else
				{
					return new DocumentCommandCollection(Factory, new ZQuery());
				}
			}
		}

		new OperationalActionDocumentPivot Parent
		{
			get { return (OperationalActionDocumentPivot)base.Parent; }
		}
	}
}
