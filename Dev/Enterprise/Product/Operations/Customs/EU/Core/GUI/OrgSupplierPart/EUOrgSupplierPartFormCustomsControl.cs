using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using TariffFormatter = Enterprise.Customs.EU.Business.TariffFormatter;

namespace Enterprise.Customs.EU.GUI
{
#if DEBUG
	[SuppressFormsLocalizedTest]
#endif
	public partial class EUOrgSupplierPartFormCustomsControl : OrgSupplierPartFormCustomsControlGlobal
	{
		public EUOrgSupplierPartFormCustomsControl() : base(false)
		{
			InitializeComponent();
			InitializeTariffFindBoxAndColumn();
			new ControlRebinder().Rebind(this, "FilteredInvoiceLines.", "PivotsForBinding.");
			supportingDocsTabPage.RunWhenBindingOrFirstShown((s, args) => InitSupportingDocumentsUserControl());
			previousDocsTabPage.RunWhenBindingOrFirstShown((s, args) => InitPreviousDocumentsUserControl());
			additionalInfosTabPage.RunWhenBindingOrFirstShown((s, args) => InitAdditionalInfosUserControl());
			taxTabPage.RunWhenBindingOrFirstShown((s, args) => InitSupplierPartTaxUserControl());
		}

		protected new CusClassPartPivot currentPartPivot => (CusClassPartPivot)base.currentPartPivot;

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			InitTabsVisibility();
		}

		void InitTabsVisibility()
		{
			if (currentPartPivot != null)
			{
				var declarationConfiguration = !DesignModeFinder.IsDesigning ? DeclarationConfiguration.GetConfiguration(currentPartPivot.Factory, currentPartPivot.Country?.Code ?? GlbCompany.CurrentCompany.GC_RN_NKCountryCode) : null;

				if (declarationConfiguration != null)
				{
					supportingDocsTabPage.TabVisible = declarationConfiguration.InvoiceLineConfiguration.SupportingDocumentsSupport(currentPartPivot);
					additionalInfosTabPage.TabVisible = declarationConfiguration.InvoiceLineConfiguration.AdditionalInfosSupport(currentPartPivot);
					taxTabPage.TabVisible = declarationConfiguration.InvoiceLineConfiguration.TaxSupport(currentPartPivot);
					previousDocsTabPage.TabVisible = declarationConfiguration.InvoiceLineConfiguration.PreviousDocumentsSupport(currentPartPivot);
				}
			}

			BothDetailsTabPage.TabVisible = currentPartPivot != null && currentPartPivot.IsClassificationBoth;
			DetailsTabPage.TabVisible = !BothDetailsTabPage.TabVisible;
		}

		protected override void HookPartPivotEvents(BaseCusClassPartPivot partPivot)
		{
			base.HookPartPivotEvents(partPivot);
			if (currentPartPivot != null)
			{
				currentPartPivot.OnChildTypeToBeBothAndClearUnrelatedDetails += CurrentPartPivotOnChildTypeToBeBothAndClearUnrelatedDetails;
			}
		}

		protected override void UnHookPartPivotEvents(BaseCusClassPartPivot partPivot)
		{
			base.UnHookPartPivotEvents(partPivot);
			if (currentPartPivot != null)
			{
				currentPartPivot.OnChildTypeToBeBothAndClearUnrelatedDetails -= CurrentPartPivotOnChildTypeToBeBothAndClearUnrelatedDetails;
			}
		}

		void CurrentPartPivotOnChildTypeToBeBothAndClearUnrelatedDetails(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = Globals.Message.ShowConfirmation(
				Res.GetString("4c32bdde-b2b8-406c-ba40-e81a23aef466", "System is about to delete all existing Supporting Documents, Additional Infos, Previous Documents, Taxes and some details on the Details tab."),
				Res.GetString("ffe9e522-f4e9-4b60-889e-d0918b4ee395", "Switching Line Type"),
				Res.GetString("96c40f53-8b5e-47c9-af1f-df62dc45b0e4", "Do you want to continue?"),
				"YES", MessageBoxIcon.Exclamation) != DialogResult.OK;
		}

		void AdditionalCPCMoreButton_Click(object sender, EventArgs e)
		{
			if (currentPartPivot != null)
			{
				ZFormModaliser.ShowDialogAndDispose(new AdditionalProcedureCodeForm(currentPartPivot));
			}
		}

		public new Business.MasterFiles.OrgSupplierPart CurrentDataItem => (Business.MasterFiles.OrgSupplierPart)base.CurrentDataItem;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (CurrentDataItem != null)
			{
				// The use of SupplementaryCodeProvider.GetByCountryCode here should match the use in CusClassPartPivot. Please update in both places if needed.
				var supplementaryCodeProvider = SupplementaryCodeProvider.GetByCountryCode(CusClassPartPivot.GetCountryCodeForSupplementaryCodeHelper(CurrentDataItem));
				SetControlVisibility(supplementaryCodeProvider.NumberOfCodes > 0, SupplementLabel2, CI_AdditionalSupplementsTextBox, AdditionalSupplementaryCodesEditButton);
			}
		}

		protected virtual Type GetSupportingDocumentsUserControlType()
		{
			return typeof(SupportingDocumentsUserControl);
		}

		void InitSupportingDocumentsUserControl()
		{
			SupportingDocumentsUserControl.UserControlType = GetSupportingDocumentsUserControlType();
			SupportingDocumentsUserControlHostedControlCreated();
		}

		protected virtual void SupportingDocumentsUserControlHostedControlCreated()
		{
			SupportingDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (SupportingDocumentsUserControl.HostedControl is ISupportingDocumentsUserControl supportingDocumentsUserControl && supportingDocumentsUserControl is Control control)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "PivotsForBinding", SupportingInfoColumnLayoutContext);
				}
			};
		}

		protected virtual Type GetSupplierPartTaxUserControlType() => typeof(OrgSupplierPartTaxUserControl);

		void InitSupplierPartTaxUserControl()
		{
			orgSupplierPartTaxUserControl1.UserControlType = GetSupplierPartTaxUserControlType();
			orgSupplierPartTaxUserControl1.HostedControlCreated += (sender, args) =>
			{
				var control = orgSupplierPartTaxUserControl1.HostedControl as OrgSupplierPartTaxUserControl;
				if (control != null)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "PivotsForBinding", SupportingInfoColumnLayoutContext);
				}
			};
		}

		protected virtual Type GetPreviousDocumentsUserControlType()
		{
			return typeof(PreviousDocumentsUserControl);
		}

		void InitPreviousDocumentsUserControl()
		{
			PreviousDocumentsUserControl.UserControlType = GetPreviousDocumentsUserControlType();
			PreviousDocumentsUserControlHostedControlCreated();
		}

		protected virtual void PreviousDocumentsUserControlHostedControlCreated()
		{
			PreviousDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (PreviousDocumentsUserControl.HostedControl is ISupportingInfoUserControls)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(PreviousDocumentsUserControl.HostedControl, "PivotsForBinding", SupportingInfoColumnLayoutContext);
				}
			};
		}

		protected virtual Type GetAdditionalInfosUserControlType()
		{
			return typeof(AdditionalInfosUserControlWithGrid);
		}

		void InitAdditionalInfosUserControl()
		{
			additionalInfosUserControl1.UserControlType = GetAdditionalInfosUserControlType();
			AdditionalInfosUserControlHostedControlCreated();
		}

		protected virtual void AdditionalInfosUserControlHostedControlCreated()
		{
			additionalInfosUserControl1.HostedControlCreated += (sender, args) =>
			{
				if (additionalInfosUserControl1.HostedControl is ISupportingInfoUserControls supportingDocumentsUserControl && supportingDocumentsUserControl is Control control)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(control, "PivotsForBinding", SupportingInfoColumnLayoutContext);
				}
			};
		}

		protected const string SupportingInfoColumnLayoutContext = "PIVOT";

		void SetControlVisibility(bool condition, params Control[] controls)
		{
			foreach (var control in controls)
			{
				control.Visible = condition;
			}
		}

		void AdditionalSupplementaryCodesEditButton_Click(object sender, EventArgs e)
		{
			if (currentPartPivot != null)
			{
				AdditionalSupplementaryCodesForm.ShowDialog(currentPartPivot);
			}
		}

		void InitializeTariffFindBoxAndColumn()
		{
			var tariffFindBox = GetTariffFindBox();
			tariffFindBox.Name = "TariffFindBox";
			tariffFindBox.ShowDescriptionBox = false;
			tariffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 15, true);
			tariffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			tariffFindBox.TabIndex = 0;
			BindingSource.SetBindingMember(tariffFindBox, "PivotsForBinding.CI_FormattedTariffNum");
			DetailsTabPage.Controls.Add(tariffFindBox);

			var tariffFindBoxForBoth = GetTariffFindBox();
			tariffFindBoxForBoth.Name = "TariffFindBoxForBoth";
			tariffFindBoxForBoth.ShowDescriptionBox = false;
			tariffFindBoxForBoth.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 15, true);
			tariffFindBoxForBoth.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			tariffFindBoxForBoth.TabIndex = 0;
			BindingSource.SetBindingMember(tariffFindBoxForBoth, "PivotsForBinding.CI_FormattedTariffNum");
			BothDetailsTabPage.Controls.Add(tariffFindBoxForBoth);

			var tariffColumnStyleInfo = GetTariffColumnStyleInfo();
			tariffColumnStyleInfo.ColumnName = CusClassPartPivot.Schema.CI_FormattedTariffNum;
			tariffColumnStyleInfo.IsMandatory = true;

			var childTypeInfo = PivotGrid.GetColumnStyle(CusClassPartPivot.Schema.CI_ChildType);
			if (childTypeInfo != null)
			{
				var index = PivotGrid.ColumnStyles.IndexOf(childTypeInfo);
				PivotGrid.ColumnStyles.Insert(index, tariffColumnStyleInfo);
			}
			else
			{
				PivotGrid.ColumnStyles.Add(tariffColumnStyleInfo);
			}
		}

		protected virtual ZCodeFindBox GetTariffFindBox()
		{
			var unTariffFindBox = new Universal.GUI.TariffFindBox();
			unTariffFindBox.GetCountryCode = GetCustomsCountryCode;
			unTariffFindBox.GetDataGrouping = GetDataGroupingForUniversalTariff;
			unTariffFindBox.GetTariffType = GetTariffType;
			return unTariffFindBox;
		}

		protected virtual ZArchitecture.ZTextBoxColumnStyleInfo GetTariffColumnStyleInfo()
		{
			var unTariffColumnStyleInfo = new Universal.GUI.TariffColumnStyleInfo();
			unTariffColumnStyleInfo.GetCountryCode = GetCustomsCountryCode;
			unTariffColumnStyleInfo.GetDataGrouping = GetDataGroupingForUniversalTariff;
			unTariffColumnStyleInfo.GetTariffType = GetTariffType;
			return unTariffColumnStyleInfo;
		}

		public ZString GetTariffType() => GetTariffTypeCore();
		protected virtual ZString GetTariffTypeCore()
		{
			return TariffFormatter.GetTariffType(currentPartPivot != null && !currentPartPivot.IsDeleted && currentPartPivot.CI_ChildType == ClassificationType.EXP);
		}
	}
}
