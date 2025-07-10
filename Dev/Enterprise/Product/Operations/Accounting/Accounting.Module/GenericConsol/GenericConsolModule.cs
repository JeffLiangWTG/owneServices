using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.GenericConsol;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.TransportBooking;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public class GenericConsolModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.GenericConsol; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var genericConsol = selectedBusinessObject as GenericConsol;
			var controllerID = ControllerIDs.JobConsol;
			if (genericConsol != null)
			{
				controllerID = genericConsol.GetParentConsolController();
			}
			return ZControllerFactory.Create(controllerID);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new GenericConsolFilterControl(GridCollection, (GenericConsolFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new GenericConsolCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new GenericConsolFilterBusinessObject();
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

			menu.Add(new ZMenuItem(ResString.GetMultilingualString("Accounting.GenericConsol.ForwardingConsol", "New Forwarding Consol"), new EventHandler(NewForwardingConsol)));
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
			return new SecurityCheckpoint[] { Env.Security.NewPayablesTransaction };
		}

		void NewForwardingConsol(object sender, EventArgs e)
		{
			ZController jobConsolController = ZControllerFactory.Create(ControllerIDs.JobConsol);
			jobConsolController.SetFormsModalTo(ParentModalForm);
			jobConsolController.ShowNewForm();
		}

		protected override IZForm ShowNewForm()
		{
			Globals.Message.Show(Res.GetString("e386d488-b6dd-4ab1-996a-9c5ce7e5146a", "You cannot create a new Consol here"));
			return null;
		}

		protected override IZForm ShowDeleteForm(BusinessObject selectedBusinessObject)
		{
			return ShowActionForm(selectedBusinessObject, DisplayAction.Delete);
		}

		protected BusinessObject GetParentConsolFromGenericConsol(GenericConsol sourceEntity)
		{
			switch (sourceEntity.VX_ParentTableCode)
			{
				case JobConsolSchema.Constants.Prefix: return Factory.Load<ForwardingConsol>(sourceEntity.PK);
				case DtbBookingConsolidationSchema.Constants.Prefix: return (BusinessObject)Factory.Load<IDtbBookingConsolidation>(sourceEntity.PK);
				case DtbConsignmentRunSheetSchema.Constants.Prefix: return (BusinessObject)Factory.Load<IDtbConsignmentRunSheet>(sourceEntity.PK);
				default: return null;
			}
		}

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider()
		{
			return new GenericConsolModuleDecisionProvider(this);
		}

		class GenericConsolModuleDecisionProvider : DefaultModuleDecisionProvider
		{
			public GenericConsolModuleDecisionProvider(GenericConsolModule module) : base(module)
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
			if (selectedBusinessObject != null && selectedBusinessObject is GenericConsol)
			{
				ZController controller = GetNewController(selectedBusinessObject);
				controller.SetFormsModalTo(ParentModalForm);

				if (action == DisplayAction.Delete)
				{
					result = controller.ShowDeleteForm(GetParentConsolFromGenericConsol((GenericConsol)selectedBusinessObject));
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
