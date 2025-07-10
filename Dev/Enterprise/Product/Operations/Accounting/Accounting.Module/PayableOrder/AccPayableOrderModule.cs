using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.PayableOrder;
using Enterprise.Accounting.GUI.PayableOrder;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public class AccPayableOrderModule : ZFilterGridModule, IOrdersModule
	{
		public AccPayableOrderModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.AccPayableOrder; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.AccPayableOrder);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new AccPayableOrderFilterControl((AccPayableOrderHeaderCollection)GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new AccPayableOrderHeaderCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new AccPayableOrderFilterBusinessObject();
		}

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("c422282d-e318-418b-964e-682225c37855", "Deactivate", "Deactivates the selected item after viewing its details read-only (shortcut Del)");
		}

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		#endregion

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get
			{
				return Env.Security.PayableOrder;
			}
		}

		#endregion

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return AccPayableOrderHeaderWorkflowDescriptor.WorkflowTypeCode; }
		}

		public IZForm ShowFormForSplit(AccPayableOrderHeader orderToSplit, AccPayableOrderHeader.CreateOrderType splitType)
		{
			return ((AccPayableOrderController)GetNewController(null)).ShowSplitForm(orderToSplit, splitType);
		}
	}
}
