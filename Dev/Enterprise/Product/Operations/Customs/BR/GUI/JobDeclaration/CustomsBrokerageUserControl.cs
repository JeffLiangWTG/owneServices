using CargoWise.Windows.UI;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.GUI
{
	public partial class CustomsBrokerageUserControl : BaseCustomsBrokerageUserControl
	{
		public CustomsBrokerageUserControl()
		{
			InitializeComponent();
		}

		protected override BaseCustomsEntryUserControl GetDeclarationUserControl()
		{
			return new CustomsDeclarationUserControl();
		}

		public new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;

		#region Create New User Controls for each tab

		protected override Customs.GUI.BaseCustomsSupplierHeaderUserControl GetSupplierHeaderUserControl()
		{
			Customs.GUI.BaseCustomsSupplierHeaderUserControl result;

			if (JobDeclaration.IsImportLicense)
			{
				result = new ImportLicenseSupplierHeaderUserControl();
			}
			else if (JobDeclaration.IsImport)
			{
				result = new ImportSupplierHeaderUserControl();
			}
			else
			{
				result = new ExportSupplierHeaderUserControl();
			}

			return result;
		}

		protected override Customs.GUI.BaseInvoiceLineUserControl GetInvoiceLinesUserControl()
		{
			Customs.GUI.BaseInvoiceLineUserControl result;
			if (JobDeclaration.IsImportSiscomex)
			{
				result = new ImportSiscomexInvoiceLineUserControl();
			}
			else if (JobDeclaration.IsImportLicense)
			{
				result = new ImportLicenseInvoiceLineUserControl();
			}
			else if (JobDeclaration.IsImport)
			{
				result = new ImportInvoiceLineUserControl();
			}
			else
			{
				result = new ExportInvoiceLineUserControl();
			}

			return result;
		}

		protected override BaseCustomsEntryUserControl GetEntryInstructionUserControl()
		{
			if (JobDeclaration.IsImportSiscomex)
			{
				return new ImportSiscomexEntryInstructionDetailsUserControl();
			}
			else if (JobDeclaration.IsImportLicense)
			{
				return new ImportLicenseEntryInstructionDetailsUserControl();
			}
			else if (JobDeclaration.IsImport)
			{
				return new ImportEntryInstructionDetailsUserControl();
			}
			else
			{
				return new ExportEntryInstructionDetailsUserControl();
			}
		}

		protected override void RemoveUserControlOfEachTabPage()
		{
			base.RemoveUserControlOfEachTabPage();
			RemoveControl(EntryInstructionDetailsTabPage);
		}

		protected override void HandleDeclarationControlVisibilityChangedCore()
		{
			base.HandleDeclarationControlVisibilityChangedCore();
			RenameTabPageCaptionResourceString();
		}

		void RenameTabPageCaptionResourceString()
		{
			MessagesTabPage.CaptionResourceString = JobDeclaration.IsImportLicense ? Res.GetData("105c7644-ff0a-468c-bc62-aad7d6a7596d", "Licenses") : Res.GetData("87335720-54d9-4c35-a520-b95849b6c85e", "Entries");
			MessagesTabPage.GetExtension<LabelCaptionRenderer>().Refresh();
		}

		public override bool EntryInstructionsTabVisibleForCountry => true;

		protected override BaseCustomsEntryUserControl GetMessageUserControl()
		{
			if (JobDeclaration.JE_ApplicationCode == Customs.Business.DeclarationApplicationCodeList.Codes.Builtin)
			{
				if (JobDeclaration.IsImportLicense)
				{
					return new ImportLicenseMessageUserControl();
				}
				else
				{
					return new MessageUserControlSubmitionTypeBLT();
				}
			}
			return base.GetMessageUserControl();
		}

		protected override void ShowOrHidePickupTabPage()
		{
			if (JobDeclaration.IsImportLicense)
			{
				PickupTabPage.TabRelevant = false;
			}
			else
			{
				base.ShowOrHidePickupTabPage();
			}
		}

		protected override void ShowOrHideDeliveryTabPage()
		{
			if (JobDeclaration.IsImportLicense || JobDeclaration.IsLPCO)
			{
				DeliveryTabPage.TabRelevant = false;
			}
			else
			{
				base.ShowOrHideDeliveryTabPage();
			}
		}

		protected override void ShowOrHidePackingTabPage()
		{
			if (JobDeclaration.IsLPCO)
			{
				PackingTabPage.TabRelevant = false;
			}
			else
			{
				base.ShowOrHidePackingTabPage();
			}
		}

		#endregion
	}
}
