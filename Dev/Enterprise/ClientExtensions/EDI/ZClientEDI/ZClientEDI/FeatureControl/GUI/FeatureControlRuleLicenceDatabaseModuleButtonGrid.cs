using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.FeatureControl.GUI
{
	public partial class FeatureControlRuleLicenceDatabaseModuleButtonGrid : ZModuleButtonGrid
	{
		FeatureControlRule ControlRule => (FeatureControlRule)Form.BusinessEntity;

		public FeatureControlRuleLicenceDatabaseModuleButtonGrid()
		{
			ModuleID = ClientModuleRegistration.LicenceDatabase;
		}

		protected override void ShowEditForm(BusinessObject selected)
		{
			base.ShowEditForm(((FeatureControlRuleLicenceDatabasePivot)selected).Database);
		}

		protected override void Detach(BusinessObject selected)
		{
			selected?.Delete();
			base.Detach(selected);
		}

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new FeatureControlRuleGridAttacher(destinationCollection, findBoxList, moduleID, ControlRule);
		}

		#region Attacher

		/// <summary>
		/// Since our Grid List is bounded to FeatureControlRuleLicenceDatabasePivot
		/// We needed to create a class to override some of the functions in ZRecordAttacher
		/// </summary>
		internal class FeatureControlRuleGridAttacher : ZRecordAttacher
		{
			readonly FeatureControlRule ControlRule;

			public FeatureControlRuleGridAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID, FeatureControlRule rule)
				: base(destinationCollection, findBoxList, moduleID)
			{
				this.ControlRule = rule;
			}

			protected override bool AttachCore(BusinessObject bizO, List<BusinessObject> listToBulkAdd)
			{
				var loadedDatabase = bizO.Factory.Load<LicenceDatabase>(bizO.PK);
				if (loadedDatabase != null && !loadedDatabase.IsDeleted)
				{
					ControlRule.LicenceDatabasePivots.Reload(true);
					var pivot = ControlRule.Factory.New<FeatureControlRuleLicenceDatabasePivot>();
					pivot.FCD_LD_LicenceDatabase = loadedDatabase.PK;
					pivot.FCD_LD_DatabaseNumber = loadedDatabase.LD_DatabaseNumber;
					pivot.FCD_FCR_FeatureControlRule = ControlRule.PK;
					listToBulkAdd.Add(pivot);
					return true;
				}

				return false;
			}

			protected override ZGuid GetSelectedPKFromDestinationCollection(BusinessObject bizObj)
			{
				return ((FeatureControlRuleLicenceDatabasePivot)bizObj).FCD_LD_LicenceDatabase;
			}
		}

		#endregion

		public override void Refresh()
		{
			InnerGrid?.ListManager?.Refresh();
			base.UpdateButtonsReadOnly();
			base.Refresh();
		}
	}
}
