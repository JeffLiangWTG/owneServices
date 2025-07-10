using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.EU.NCTS.Module
{
	/// <summary>
	/// Module for Ncts Declarations
	/// </summary>
	public class NctsMovementModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public NctsMovementModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		protected override System.Windows.Forms.MenuItem[] GetNewStandardMenuItems()
		{
			var result = base.GetNewStandardMenuItems();
			if (NewMenuItem != null)
			{
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("227F9791-2ED2-4BBE-A7E1-FD7E404C0236", "New Departure Declaration"), CreateDepartureDeclaration));
				NewMenuItem.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("D74FDFCA-ED26-4BB0-815B-0049E2ABABB1", "New Arrival Notification"), CreateArrivalNotification));
			}
			return result;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var nctsHeader = selectedBusinessObject as NctsHeader;
			if (nctsHeader != null && nctsHeader.Shipment != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementInShipmentController);
			}
			else if (nctsHeader != null && nctsHeader.Consol != null)
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementInConsolController);
			}
			else
			{
				return ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementController);
			}
		}

		protected override ZArchitecture.Business.FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new NctsMovementFilterStripBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new NctsMovementFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new NctsHeaderCollection(Factory, GlbCompany.CurrentCompany);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.NctsMovementModule; }
		}

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.NCTS; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.EuNctsMovement; }
		}

		protected virtual NctsMovementController GetControllerForStandAlone()
		{
			NctsMovementController controller = (NctsMovementController)ZControllerFactory.Create(ControllerIDs.Customs.EU.NctsMovementController);
#if DEBUG
			((IFilterGridModuleInternalsForTesting)this).LastController = controller;
#endif
			return controller;
		}

		void CreateArrivalNotification(object sender, System.EventArgs e)
		{
			ShowNewNctsMovementForm(NctsMovementType.Codes.Arrival);
		}

		void CreateDepartureDeclaration(object sender, System.EventArgs e)
		{
			ShowNewNctsMovementForm(NctsMovementType.Codes.Departure);
		}

		public override bool SupportsWorkflow
		{
			get { return true; }
		}

		public override string WorkflowType
		{
			get { return WorkflowDescriptors.NctsHeaderWorkflowDescriptorCode; }
		}

		protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopupCore(IFindBox findbox) => new NCTSPopupModuleDecisionProvider(findbox);

		void ShowNewNctsMovementForm(ZString headerType)
		{
			var controller = GetControllerForStandAlone();
			controller.ShowNewNctsMovementForm(headerType);
		}

		#region IOperationalActionSupportable

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter => operationalActionSupporter ?? (operationalActionSupporter = new NctsMovementOperationalActionSupporter());
		OperationalActionSupporter operationalActionSupporter;

		#endregion
	}
}
