using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.BillingPrices;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.GUI;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Client.EDI.Billing.Module.BillingPrices
{
	public class BillingPricesModule : ZFilterGridModule, IOperationalActionSupportable
	{
		public BillingPricesModule()
		{
			Plugins.Add(ControllerIDs.OperationalActions);
		}

		public override ModuleIdentifier ID
		{
			get { return Modules.ClientModuleRegistration.BillingPrices; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Organisation; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get	{ return Env.Licence.AlwaysAllow; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			var controller = ZControllerFactory.Create(Modules.ClientControllerRegistration.BillingPrices) as BillingPricesController;
			return controller;
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new BillingPricesFilterBusinessObject();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new BillingPricesFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new BillingPricesCollection(Factory);
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var menus = base.GetNewActionMenuItems().ToList();
			menus.Add(new ZMenuItem("Bulk Copy", (sender, e) =>
			{
				var priceItems = Grid.SelectedElements.OfType<ClientLicencePriceItem>().Select(x => x.PK).ToArray();

				if (priceItems.Any())
				{
					using (var form = new PriceItemBulkCopyForm(priceItems))
					{
						ZFormModaliser.ShowDialogWithoutDispose(form);
					}
				}
			}));
			return menus.ToArray();
		}

		#region IOperationActionSupportable Members

		public OperationalActionSupporter OperationalActionSupporter
		{
			get { return new BillingPricesSupporter(); }
		}

		#endregion
	}

	#region OperationalActionSupporter Class

	internal sealed class BillingPricesSupporter : OperationalActionSupporter
	{
		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.CargoWiseBilling; }
		}

		public override SecurityCheckpoint CustomizationSecurityCheckpoint
		{
			get { return EDISecurityCheckpoints.OrgLicenceBilling; }
		}

		public override Type RootType
		{
			get { return typeof(ClientLicencePriceItem); }
		}

		public override SecurityCheckpoint BaseCheckpoint => Env.Security.Organisation;
	}

	#endregion
}
