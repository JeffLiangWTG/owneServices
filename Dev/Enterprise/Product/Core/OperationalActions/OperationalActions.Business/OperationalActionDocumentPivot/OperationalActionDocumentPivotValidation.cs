using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class OperationalActionDocumentPivotValidation : StmMenuMenuPivotValidation
	{
		public OperationalActionDocumentPivotValidation(OperationalActionDocumentPivot parent)
			: base(parent)
		{
		}

		protected override void CheckSF_IsSystemDefined()
		{
			base.CheckSF_IsSystemDefined();
			OperationalAction action = Parent.Action;
			if (action != null)
			{
				if (Parent.SF_IsSystemDefined)
				{
					if (!action.SU_IsSystemDefined)
					{
						Parent.SF_IsSystemDefinedInfo.AddError(Res.GetString("{D682B72C-20BF-4106-AC83-298276528978}", "System-defined relationships cannot be added to user-defined Operational Actions."));
					}
				}
				else if (action.SU_IsSystemDefined)
				{
					Parent.SF_IsSystemDefinedInfo.AddError(Res.GetString("{0BCA61D4-31C4-444e-A3D4-46BAA14DFDCD}", "User-defined relationships cannot be added to system-defined Operational Actions."));
				}
			}
		}

		protected override void CheckSF_SU_Outward()
		{
			base.CheckSF_SU_Outward();
			MandatoryValidation.CheckEntered(Parent.SF_SU_OutwardInfo);
			if (!Parent.SF_SU_OutwardInfo.HasErrors()) // To avoid the same error being added twice (one by the FindBox validation).
			{
				ListValidation.ErrorIfInvalidPK(Parent.SF_SU_OutwardInfo, Parent.Lookups.Documents);
			}
			PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.SF_SU_OutwardInfo, Parent.Factory.Load<OperationalActionDocumentPivot>(new ZQuery(StmMenuMenuPivotSchema.SF_SU_Inward, Parent.SF_SU_Inward)));
		}

		new OperationalActionDocumentPivot Parent
		{
			get { return (OperationalActionDocumentPivot)base.Parent; }
		}
	}
}
