using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GenericCharge;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class GenericChargeModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GenericCharge; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			if (selectedBusinessObject != null && selectedBusinessObject is GenericCharge)
			{
				if (((GenericCharge)selectedBusinessObject).VC_IsGLAccount)
				{
					return ZControllerFactory.Create(ControllerIDs.AccGLHeader);
				}
				else
				{
					return ZControllerFactory.Create(ControllerIDs.AccChargeCode);
				}
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.AccChargeCode);
			}
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GenericChargeFilterControl(GridCollection, (GenericChargeFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return FindboxLookupCollections.GetGenericChargeCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			var elementType = GenericChargeFilterBusinessObject.ElementType.None;
			var checkPointsAllowed = GetSecurityCheckpointForPopups().Where(x => x.IsAllowed).ToArray();

			if (checkPointsAllowed.Length == 1)
			{
				if (checkPointsAllowed[0].Equals(ChargeCodeRight))
				{
					elementType = GenericChargeFilterBusinessObject.ElementType.ChargeCode;
				}
				else if (checkPointsAllowed[0].Equals(GLRight))
				{
					elementType = GenericChargeFilterBusinessObject.ElementType.GeneralLedger;
				}
			}
			else if (checkPointsAllowed.Length == 2)
			{
				elementType = GenericChargeFilterBusinessObject.ElementType.All;
			}

			return new GenericChargeFilterBusinessObject(elementType);
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>(base.GetNewStandardMenuItems());
			menu.Remove(NewMenuItem);
			return menu.ToArray();
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> menu = new List<MenuItem>(base.GetNewActionMenuItems());
			menu.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.GenericCharge.GLAccount", "New GL Account"), new EventHandler(NewGLAccount)));
			menu.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.GenericCharge.ChargeAccount", "New Charge Account"), new EventHandler(NewChargeAccount)));
			return menu.ToArray();
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get	{ return Env.Security.Manage; }
		}

		public override SecurityCheckpoint[] GetSecurityCheckpointForPopups()
		{
			var checkpoints = new SecurityCheckpoint[] { ChargeCodeRight, GLRight };
			var allowedCheckpoints = checkpoints.Where(x => x.IsAllowed).ToArray();
			return allowedCheckpoints.Length == 1 ? allowedCheckpoints : checkpoints;
		}

		SecurityCheckpoint ChargeCodeRight { get { return Env.Security.ChargeCodes; } }
		SecurityCheckpoint GLRight { get { return Env.Security.GLAccounts; } }

		void NewGLAccount(object sender, EventArgs e)
		{
			ZController gLHeaderController = ZControllerFactory.Create(ControllerIDs.AccGLHeader);
			gLHeaderController.SetFormsModalTo(ParentModalForm);
			gLHeaderController.ShowNewForm();
		}

		void NewChargeAccount(object sender, EventArgs e)
		{
			ZController accChargeCodeController = ZControllerFactory.Create(ControllerIDs.AccChargeCode);
			accChargeCodeController.SetFormsModalTo(ParentModalForm);
			accChargeCodeController.ShowNewForm();
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			return ShowActionForm(selectedBusinessObject, DisplayAction.Delete);
		}

		protected BusinessObject GetChargeOrGLAccountFromGenericCharge(GenericCharge sourceEntity)
		{
			BusinessObject result;
			if (sourceEntity.VC_IsGLAccount)
			{
				result = Factory.Load(typeof(AccGLHeader), sourceEntity.PK);
			}
			else
			{
				result = Factory.Load(typeof(AccChargeCode), sourceEntity.PK);
			}
			return result;
		}

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new GenericChargeModuleDecisionProvider(this);
		}

		class GenericChargeModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public GenericChargeModuleDecisionProvider(GenericChargeModule module) : base(module)
			{
			}

			public override bool AllowExcelExport
			{
				get { return false; }
			}
		}

		#region Show Action

		IZForm ShowActionForm(BusinessObject selectedBusinessObject, DisplayAction action)
		{
			IZForm result = null;
			if (selectedBusinessObject != null && selectedBusinessObject is GenericCharge)
			{
				ZController controller = GetNewController(selectedBusinessObject);
				controller.SetFormsModalTo(ParentModalForm);

				if (action == DisplayAction.Delete)
				{
					result = controller.ShowDeleteForm(GetChargeOrGLAccountFromGenericCharge((GenericCharge)selectedBusinessObject));
				}
				else if (action == DisplayAction.Edit)
				{
					result = controller.ShowEditForm(selectedBusinessObject);
				}
				else
				{
					result = controller.ShowViewForm(selectedBusinessObject);
				}
			}
			return result;
		}

		enum DisplayAction { View, Edit, Delete }

		#endregion
	}
}
