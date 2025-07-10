using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public class EDIMenu : Customs.GUI.EDIMenu
	{
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
			set { base.Declaration = value; }
		}

		protected override void SetupTopLevelMenu()
		{
			base.SetupTopLevelMenu();
			sendToCustomsMenuItem = new ZMenuItem(Res.GetString("5D29BA32-5A70-4276-A10F-5C683A411FF9", "Send to Customs"), SendMessageMenuItem_Click);
			importNfeMenuItem = new ZMenuItem(Res.GetString("171f9bb2-a59b-4fec-a61a-7cd84e532fed", "Import NF-e"), ImportNfeMenuItem_Click);
			importLicenseLoadResponseMenuItem = new ZMenuItem(Res.GetString("e39ef33a-f60d-429e-95af-299739466921", "Load Response from Customs"), ImportLicenseLoadResponseMenuItem_Click);
			exportDataFromEntriesMenuItem = new ZMenuItem(Res.GetString("8BB1F00E-C587-4E94-A88D-28DA2A026753", "Export Data from Entries"), ExportNfeMenuItem_Click);
			generateAdditionalInformationMenuItem = new ZMenuItem(Res.GetString("82BE2F96-7A81-4AA2-9FEE-4049C66885AF", "Generate Additional Information"), GenerateAdditionalInformationMenuItem_Click);
			updateImportLicenseStatusMenuItem = new ZMenuItem(Res.GetString("CF0348F0-395B-4877-9505-ED9BBA3B356E", "Update Import License Status"), UpdateImportLicenseStatusMenuItem_Click);
			importLicenseFromXMLMenuItem = new ZMenuItem(Res.GetString("d07dc4fc-c57d-45ca-9c5e-e6ca78e6240f", "Load Import License(s) from XML"), ImportLicenseFromXMLMenuItem_Click);
			updateImportEntryNumberMenuItem = new ZMenuItem(Res.GetString("1aac2d25-683d-4642-b2e4-f3c3d2aa37f6", "Update Entry Number"), UpdateImportEntryNumberMenuItem_Click);
			updateImportEntryStatusMenuItem = new ZMenuItem(Res.GetString("BF707A78-F821-4592-8A64-FDBD3CDF5262", "Update Entry Status"), UpdateImportEntryStatusMenuItem_Click);
			copyCommercialInvoiceLineMenuItem = new ZMenuItem(Res.GetString("0760FEB7-598D-4A28-B89C-73E2EC60E2D1", "&Copy/Import Invoice Line"), CopyInvoiceLinesMenu_Click);
			MenuItems.Add(sendToCustomsMenuItem);
			MenuItems.Add(importNfeMenuItem);
			MenuItems.Add(importLicenseLoadResponseMenuItem);
			MenuItems.Add(exportDataFromEntriesMenuItem);
			MenuItems.Add(generateAdditionalInformationMenuItem);
			MenuItems.Add(updateImportLicenseStatusMenuItem);
			MenuItems.Add(importLicenseFromXMLMenuItem);
			MenuItems.Add(updateImportEntryNumberMenuItem);
			MenuItems.Add(updateImportEntryStatusMenuItem);
			MenuItems.Add(copyCommercialInvoiceLineMenuItem);
		}

		public override void RefreshMenu()
		{
			base.RefreshMenu();
			sendToCustomsMenuItem.Visible = !(Declaration?.IsInterface ?? false);
			importNfeMenuItem.Visible = IsExport;
			importLicenseLoadResponseMenuItem.Visible = IsImportLicense;
			exportDataFromEntriesMenuItem.Visible = IsImportExcludingLicense;
			generateAdditionalInformationMenuItem.Visible = IsImportExcludingLicense;
			updateImportLicenseStatusMenuItem.Visible = IsImportLicense;
			GenerateEntriesMenuItem.Text = GenerateEntriesMenuOptionText;
			importLicenseFromXMLMenuItem.Visible = IsImportSiscomex;
			copyCommercialInvoiceLineMenuItem.Visible = IsImportLicense;
			updateImportEntryNumberMenuItem.Visible = IsImportSiscomex;
			updateImportEntryStatusMenuItem.Visible = IsImportSiscomex;
		}

		protected override string GenerateEntriesMenuOptionText
		{
			get
			{
				if (IsImportLicense)
				{
					return Res.GetString("b1bb07b6-65c3-4848-b442-c0c21cbcbff2", "Generate Licenses");
				}
				else
				{
					return base.GenerateEntriesMenuOptionText;
				}
			}
		}

		protected override bool DisplayGenerateEntriesMenuOption => true;

		#region MenuItems

		internal ZMenuItem sendToCustomsMenuItem;
		internal ZMenuItem importNfeMenuItem;
		internal ZMenuItem importLicenseLoadResponseMenuItem;
		internal ZMenuItem exportDataFromEntriesMenuItem;
		internal ZMenuItem generateAdditionalInformationMenuItem;
		internal ZMenuItem updateImportLicenseStatusMenuItem;
		internal ZMenuItem importLicenseFromXMLMenuItem;
		internal ZMenuItem copyCommercialInvoiceLineMenuItem;
		internal ZMenuItem updateImportEntryNumberMenuItem;
		internal ZMenuItem updateImportEntryStatusMenuItem;

		protected void SendMessageMenuItem_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration) && DeclarationHasEntry(Declaration))
			{
				var messageSendingObjectParent = CreateNewMessageSendingObjectParent();
				if (messageSendingObjectParent != null)
				{
					using (var form = new JobDeclarationMessageSendingForm(messageSendingObjectParent as BaseMessageSendingObjectParent))
					{
						if (ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
						{
							var countOfMessages = messageSendingObjectParent.SendMessagesAndSave();
							var msg = Res.GetString("6549E057-A028-4063-ACD6-28F3EF4EBF9B", "{0} message(s) have been sent", countOfMessages);
							if (countOfMessages == 0 && IsImportOnly)
							{
								msg += Res.GetString("E5D91FDC-A889-49A8-9004-C2667283EFD1", " because no changes have been detected at either the Header or Line level");
							}
							Globals.Message.Show(msg + (NoResString)".");
						}
					}
				}
			}
		}

		protected void ImportNfeMenuItem_Click(object sender, EventArgs e)
		{
			if (IsExport && PreSaveDeclaration(Declaration))
			{
				MenuHelper.ShowNFEImportForm(Declaration);
			}
		}

		protected void ImportLicenseLoadResponseMenuItem_Click(object sender, EventArgs e)
		{
			if (IsImportLicense && PreSaveDeclaration(Declaration) && DeclarationHasEntryWithoutMRN(Declaration))
			{
				MenuHelper.ShowImportLicenseLoadResponseForm(Declaration);
			}
		}

		protected void UpdateImportLicenseStatusMenuItem_Click(object sender, EventArgs e)
		{
			if (IsImportLicense && PreSaveDeclaration(Declaration) && DeclarationHasEntryWithMRN(Declaration))
			{
				MenuHelper.ShowUpdateImportLicenseStatusForm(Declaration);
			}
		}

		protected void GenerateAdditionalInformationMenuItem_Click(object sender, EventArgs e)
		{
			if (IsImportExcludingLicense && PreSaveDeclaration(Declaration))
			{
				new AdditionalInformationGenerator(Declaration).GenerateAdditionalInformation();
				Globals.Message.Show(Res.GetString("CA737E3F-F2E9-40FB-A7D0-3E4554AC9D31", "Additional Information Generation succeed."));
			}
		}

		protected void ExportNfeMenuItem_Click(object sender, EventArgs e)
		{
			if (IsImportExcludingLicense && PreSaveDeclaration(Declaration) && DeclarationHasEntry(Declaration))
			{
				MenuHelper.ShowNFeExportForm(Declaration);
			}
		}

		protected void ImportLicenseFromXMLMenuItem_Click(object sender, EventArgs e)
		{
			if (IsImportSiscomex && PreSaveDeclaration(Declaration))
			{
				MenuHelper.ShowImportLicenseFromXMLForm(Declaration);
			}
		}

		#endregion

		bool DeclarationHasEntryWithMRN(JobDeclaration declaration)
		{
			bool result = true;
			if (declaration == null || !declaration.ActiveEntryHeaders.Cast<Business.CusEntryHeader>().Any(x => !x.MovementReferenceNumber.IsEmpty))
			{
				Globals.Message.Show(Res.GetString("215AEACC-E461-4F28-8A44-26944F1ABAA4", "The Declaration {0} has no Movement Reference Number - Please register an Entry before attempting to update its status.", Declaration.JE_DeclarationReference));
				result = false;
			}
			return result;
		}

		bool DeclarationHasEntryWithoutMRN(JobDeclaration declaration)
		{
			bool result = true;
			if (declaration == null || !declaration.ActiveEntryHeaders.Cast<Business.CusEntryHeader>().Any(x => x.MovementReferenceNumber.IsEmpty))
			{
				Globals.Message.Show(Res.GetString("5c6d3083-a8d8-4d7f-b68d-6a7ea19ac3a4", "The Declaration {0} has no Entries without Movement Reference Number", Declaration.JE_DeclarationReference));
				result = false;
			}
			return result;
		}

		bool DeclarationHasEntry(JobDeclaration declaration)
		{
			bool result = true;
			if (declaration == null || declaration.ActiveEntryHeaders.Count == 0)
			{
				Globals.Message.Show(Res.GetString("ea23c317-dc5e-45d8-bd32-ebb3b0bab17c", "Declaration {0} has no entry.", declaration.JE_DeclarationReference));
				result = false;
			}
			return result;
		}

		IMessageSendingObjectParent CreateNewMessageSendingObjectParent()
		{
			IMessageSendingObjectParent result = null;

			if (IsExport)
			{
				result = new ExportJobDeclarationMessageSendingObjectParent(Declaration);
			}
			else if (IsImportLicense)
			{
				result = new ImportLicenseMessageSendingObjectParent(Declaration);
			}
			else if (IsImportSiscomex)
			{
				result = new ImportSiscomexMessageSendingObjectParent(Declaration);
			}
			else if (IsImportExcludingLicense)
			{
				result = new DuimpMessageSendingObjectParent(Declaration);
			}
			else if (IsLPCO)
			{
				result = new LPCODeclarationMessageSendingObjectParent(Declaration);
			}
			return result;
		}

		void CopyInvoiceLinesMenu_Click(object sender, EventArgs e)
		{
			if (PreSaveDeclaration(Declaration))
			{
				var invoicesToChoose = GetInvoicesToChooseForCopy();
				var chooser = new ZRecordChooser<JobComInvoiceHeader>(ModuleIDs.CommercialInvoice, invoicesToChoose);
				chooser.ShowModal(Form, selectedInvoices =>
				 {
					 var copyingOptions = new JobComInvoiceHeaderCopyOptions();
					 using (var form = new JobComInvoiceHeaderCopyOptionsForm(copyingOptions))
					 {
						 if (ZFormModaliser.ShowDialogWithoutDispose(form) == System.Windows.Forms.DialogResult.OK)
						 {
							 foreach (var header in selectedInvoices)
							 {
								 var invoice = Declaration.Invoices.AddNew();
								 var copyInvoiceBo = new BRJobComInvoiceHeaderCopyBO(header, invoice, copyingOptions);
								 copyInvoiceBo.CopyInvoice();
							 }
						 }
					 }
				 });
			}
		}

		protected void UpdateImportEntryNumberMenuItem_Click(object sender, EventArgs e)
		{
			if (IsImportSiscomex && PreSaveDeclaration(Declaration) && DeclarationHasEntry(Declaration))
			{
				MenuHelper.ShowUpdateImportEntryNumberForm(Declaration);
			}
		}

		protected void UpdateImportEntryStatusMenuItem_Click(object sender, EventArgs e)
		{
			if (IsImportSiscomex && PreSaveDeclaration(Declaration) && DeclarationHasEntry(Declaration))
			{
				MenuHelper.ShowUpdateImportEntryStatusForm(Declaration);
			}
		}

		IBusinessObjectCollection GetInvoicesToChooseForCopy()
		{
			var invoicesToChoose = new CopyCommercialInvoiceModuleCollection(Declaration);

			var filterDefaults = ((IFilterBusinessObjectDefaultsProvider)invoicesToChoose).FilterBusinessObjectDefaults;
			filterDefaults.Add(new FilterBusinessObjectDefault(Constants.FilterConstants.CommercialInvoice.ShipmentType, "Property", new ZString(BRJobMessageTypeList.Codes.Import), false));
			filterDefaults.Add(new FilterBusinessObjectDefault(Constants.FilterConstants.CommercialInvoice.AttachedToDeclaration, "Property", new ZString(AttachedToDeclarationFilterOptions.Codes.All), true));
			filterDefaults.Add(new FilterBusinessObjectDefault(Constants.FilterConstants.CommercialInvoice.ImporterSupplier, "Property1", ZString.Empty, true));

			return invoicesToChoose;
		}

		#region DataTransferImpl

		protected override Customs.DataTransfer.DataTransferImpl GetDataTransferImpl()
		{
			return new DataTransfer.DataTransferImpl();
		}

		#endregion

		bool IsImportSiscomex => Declaration?.IsImportSiscomex ?? false;
		bool IsImportLicense => Declaration?.IsImportLicense ?? false;
		bool IsImportExcludingLicense => Declaration?.IsImportExcludingLicense ?? false;
		bool IsImportOnly => Declaration?.IsImportOnly ?? false;
		bool IsExport => Declaration?.IsExport ?? false;
		bool IsLPCO => Declaration?.IsLPCO ?? false;
	}
}
