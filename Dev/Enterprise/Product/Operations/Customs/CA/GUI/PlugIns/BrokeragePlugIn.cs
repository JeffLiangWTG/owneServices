using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.CA.GUI
{
	public class BrokeragePlugIn : BrokeragePlugInOneToOne
	{
		public BrokeragePlugIn(ForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override Customs.Business.CreateDeclarationHelper GetCreateDeclarationHelperCore() => new Business.CreateDeclarationHelper();

		protected override BaseCustomsBrokerageUserControl CreateBrokerageUserControl() => new CustomsBrokerageUserControl();

		protected MenuItem fTopLevelMenu;
		protected override MenuItem GetNewTopLevelMenuCore()
		{
			if (fTopLevelMenu == null)
			{
				fTopLevelMenu = new EDIMenu();
			}
			return fTopLevelMenu;
		}

		public override void OnSaveCompletedOrAborted(bool saved)
		{
			base.OnSaveCompletedOrAborted(saved);
			if (saved)
			{
				(BusinessEntity as JobDeclaration).ResendDeferredMessageIfRequired();
			}
		}

		public override ContinueWithSave ShowPreSaveDialogsCore()
		{
			var result = base.ShowPreSaveDialogsCore();
			if (result == ContinueWithSave.Yes)
			{
				if (BusinessEntity is JobDeclaration declaration && (declaration.HasChanges || !declaration.IsInDatabase))
				{
					result = declaration.AskForB3ActionIfDeferredMessageExists();
				}
			}
			return result;
		}

		protected override BaseShipmentAndBrokerageCommon GetShipmentAndBrokergeCommon(BaseJobDeclaration declaration) => new ShipmentAndBrokerageCommon(declaration);

		internal Customs.Business.CreateDeclarationHelper CreateDeclarationHelperInternal => CreateDeclarationHelper;
	}
}
