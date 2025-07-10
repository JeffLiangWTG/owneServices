using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.Billing.Module;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.GUI;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Module;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.MasterFiles.Module
{
	public class EDIOrganisationModule : OrganisationModule, IOperationalActionSupportable
	{
		#region Construction

		public EDIOrganisationModule()
		{
		}

		#endregion

		#region Standard Module overrides

		protected override IFilterControl GetNewFilterControl()
		{
			return new EDIOrganisationFilterControl(GridCollection, (EDIOrganisationFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EDIOrganisationFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new EDIOrgHeaderCollection(Factory);
		}

		#endregion

		#region Upgrade Package

		protected override MenuItem[] GetNewActionMenuItems()
		{
			List<MenuItem> result = new List<MenuItem>(base.GetNewActionMenuItems());

			bool sendAllowed = EDISecurityCheckpoints.OrgLicenceModifySendUpgrade.IsAllowed;
			if (sendAllowed)
			{
				MenuItem upgradesMenu = new ZMenuItem("Upgrades");
				result.Add(upgradesMenu);

				upgradesMenu.MenuItems.Add("Send Upgrade Package", new EventHandler(SendUpgradePackage));
			}

			result.Add(new ZMenuItem("Import STL Price Updates", ImportStlPrices));
			result.Add(new ZMenuItem("Import ODPL Price Updates", ImportOdmPrices));
			result.Add(new ZMenuItem("Import Memberships", ImportMemberships));
			result.Add(new ZMenuItem("Import Licence Billing", ImportLicenceBilling));
			result.Add(new ZMenuItem("Import Fees", ImportFees));

			return result.ToArray();
		}

		void SendUpgradePackage(object sender, EventArgs e)
		{
			EDIOrgHeader[] selectedElements;
			if (Grid.ListManager.Position >= 0 && (selectedElements = Grid.GetSelectedElements<EDIOrgHeader>()).Length > 0)
			{
				BusinessObjectFactory factory = new BusinessObjectFactory();
				UpgradeForm upgradeForm = new UpgradeForm(new UpgradeRequestCollectionContainer(factory, selectedElements));
				upgradeForm.Show();
			}
		}

		#endregion

		#region Import

		void ImportStlPrices(object sender, EventArgs args)
		{
			var checkpoint = EDISecurityCheckpoints.OrgLicenceBilling;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			var collection = new PriceHeaderLinkImportCollection(Factory);
			var importInfo = new PriceHeaderLinkImportInfo(collection);
			var importFactory = new BusinessObjectFactory();
			var processor = new PriceHeaderLinkDataTransferProcessor(EdiPriceHeaderLinkCollection.CreateAdhocCollection(importFactory), importInfo);

			bool isCancelled = false;
			var form = new MultistepDataImportWizardForm(importInfo, "ImportStlPrices", new DataTransferProcessor[] { processor }, ID.Description.ToString());
			form.Cancelled += (s, e) => { processor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayResult(processor.HeadersToCreate, processor.HeadersCreated, processor.HeadersExcluded, processor.Log, isCancelled); };
			form.Show();
		}

		void ImportOdmPrices(object sender, EventArgs arg)
		{
			var checkpoint = EDISecurityCheckpoints.OrgLicenceBilling;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			var collection = new PriceHeaderImportCollection(Factory);
			var importInfo = new PriceHeaderImportInfo(collection);
			var importFactory = new BusinessObjectFactory();
			var processor = new PriceHeaderDataTransferProcessor(ClientLicencePriceHeaderCollection.CreateAdhocCollection(Factory), importInfo);

			bool isCancelled = false;
			var form = new MultistepDataImportWizardForm(importInfo, "ImportStlPrices", new DataTransferProcessor[] { processor }, ID.Description.ToString());
			form.Cancelled += (s, e) => { processor.Rollback(); isCancelled = true; };
			form.Imported += (s, e) => { DisplayResult(processor.HeadersToCreate, processor.HeadersCreated, processor.HeadersExcluded, processor.Log, isCancelled); };
			form.Show();
		}

		void DisplayResult(int numberToCreate, int numberCreated, int numberExcluded, string log, bool isCancelled)
		{
			string mesgs;
			string caption;

			if (isCancelled)
			{
				mesgs = Res.GetString("3663C992-5C43-4244-B85A-4A25EAFFF480", "No prices were created.");
				caption = Res.GetString("909079C2-B858-4803-BB32-FC0DD16094B0", "Import canceled");
			}
			else
			{
				mesgs = Res.GetString("763E0CBD-01EB-4171-ACE2-7D116C58A029", "Prices to Import = {0}", numberToCreate) + "\r\n";
				mesgs += log;
				mesgs += "\r\n" + Res.GetString("326F0A84-F417-44D4-BFE0-930EB8B28445", "TOTAL: Prices created = {0}, Prices excluded = {1}",
					numberCreated, numberExcluded) + "\r\n";

				caption = Res.GetString("7DC5103F-5CF3-4E3F-9259-3EFF7582BAF0", "Import completed");
			}

			using (ZMessageBox notification = new ZMessageBox(mesgs, caption, MessageBoxButtons.OK, MessageBoxIcon.Information))
			{
				notification.ShowDialog();
			}
		}

		void ImportMemberships(object sender, EventArgs arg)
		{
			var checkpoint = EDISecurityCheckpoints.OrgLicenceBilling;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			new EdiOrgMembershipImporter().Import();
		}

		void ImportLicenceBilling(object sender, EventArgs arg)
		{
			var checkpoint = EDISecurityCheckpoints.OrgLicenceBilling;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			new ClientLicenceBillingImporter().Import();
		}

		void ImportFees(object sender, EventArgs arg)
		{
			var checkpoint = EDISecurityCheckpoints.OrgLicenceBilling;
			if (!checkpoint.IsAllowed)
			{
				checkpoint.ShowError();
				return;
			}

			new ClientLicenceFeeImporter().Import();
		}

		#endregion

		#region IOperationalActionSupportable Members

		OperationalActionSupporter IOperationalActionSupportable.OperationalActionSupporter
		{
			get { return new EdiOrganisationActionSupporter(); }
		}
		#endregion
	}
}
