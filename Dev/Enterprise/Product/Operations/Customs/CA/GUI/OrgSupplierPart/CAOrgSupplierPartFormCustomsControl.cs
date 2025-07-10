using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	public partial class CAOrgSupplierPartFormCustomsControl : OrgSupplierPartFormCustomsControl
	{
		protected PGATabCollection pgaTabCollection;

		public CAOrgSupplierPartFormCustomsControl()
		{
			InitializeComponent();
			pgaTabCollection = new PGATabCollection(detailTabControl, PivotGrid, false);
			exportPanel.Visible = false;

			ClassificationTariffUserControlHelper.UpdateTariffColumnStyleInfoAndTariffFindBoxToGetTariffFromSRDb(
				PivotGrid,
				CusClassPartPivot.Schema.CI_FormattedTariffNum,
				classificationNumberFindBox,
				"classificationNumberFromRefDbFindBox",
				() => { return ZDateTime.Today; },
				(x) =>
				{
					this.BindingSource.SetBindingMember(x, "PivotsForBinding.CI_FormattedTariffNum");
					CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_FormattedTariffNum);
				}
				);

			ClassificationTariffUserControlHelper.UpdateTariffFindBoxToGetTariffFromSRDb(
				exportTariffCodeFindBox,
				"exportTariffFromSRDbCodeFindBox",
				() => { return ZDateTime.Today; },
				(x) =>
				{
					this.BindingSource.SetBindingMember(x, "PivotsForBinding.CI_FormattedTariffNum");
					CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CusClassPartPivot)(((System.Collections.IList)(((OrgSupplierPart)(null)).PivotsForBinding)).SyncRoot)).CI_FormattedTariffNum);
				}
				);
		}

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			if (PivotGrid.ListManager != null)
			{
				PivotGrid.ListManager.CurrentChanged += ListManager_CurrentChanged;
				ListManager_CurrentChanged(this, new EventArgs());
			}
		}

		#region Events

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (currentPivot != null && !currentPivot.IsDeleted)
			{
				currentPivot.CI_ChildTypeInfo.ValueChanged -= CI_ChildTypeInfo_ValueChanged;
				currentPivot.OnRefreshSIMAMeasureEvent = null;
				currentPivot.CI_CCInfo.ValueChanged -= CI_CCInfo_ValueChanged;
			}

			if (PivotGrid.ListManager != null)
			{
				currentPivot = (CusClassPartPivot)PivotGrid.ListManager.GetCurrent();

				if (currentPivot != null && !currentPivot.IsDeleted)
				{
					currentPivot.CI_ChildTypeInfo.ValueChanged += CI_ChildTypeInfo_ValueChanged;
					currentPivot.OnRefreshSIMAMeasureEvent += delegate
					{
						var simaMeasuresForm = new SIMADumpingNumberForm(currentPivot.SIMAMeasures, currentPivot.CI_TariffNum);
						var parentForm = FindForm();
						simaMeasuresForm.Icon = parentForm.Icon;
						if (simaMeasuresForm.ShowDialog(parentForm) == DialogResult.OK)
						{
							return simaMeasuresForm.SelectedDumpingNumber;
						}

						return null;
					};
					currentPivot.CI_CCInfo.ValueChanged += CI_CCInfo_ValueChanged;
				}
			}

			SetVisibility();
			SetSIMAVisibility();

			pgaTabCollection.Update(currentPivot?.PGARequirements);
		}

		void CI_ChildTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetVisibility();
		}

		void CI_CCInfo_ValueChanged(object sender, EventArgs e)
		{
			SetSIMAVisibility();
			pgaTabCollection.Update(currentPivot?.PGARequirements);
		}

		void SetVisibility()
		{
			if (currentPivot != null && !currentPivot.IsDeleted)
			{
				var isImport = currentPivot.IsImport;
				exportPanel.Visible = currentPivot.IsExport;
				importPanel.Visible = isImport;

				this.pGATabPage.TabVisible = isImport;
				this.pgaTabCollection.Visible = isImport;
			}
		}

		void SetSIMAVisibility()
		{
			if (currentPivot != null && !currentPivot.IsDeleted)
			{
				if (currentPivot.Classification is CusClassification)
				{
					this.sIMATabPage.TabVisible = false;
					this.sIMAForCCTabPage.TabVisible = true;
				}
				else
				{
					this.sIMATabPage.TabVisible = true;
					this.sIMAForCCTabPage.TabVisible = false;
				}
			}
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (PivotGrid != null && PivotGrid.ListManager != null)
				{
					PivotGrid.ListManager.CurrentChanged -= ListManager_CurrentChanged;
				}

				if (currentPivot != null && !currentPivot.IsDeleted)
				{
					currentPivot.CI_ChildTypeInfo.ValueChanged -= CI_ChildTypeInfo_ValueChanged;
					currentPivot.CI_CCInfo.ValueChanged -= CI_CCInfo_ValueChanged;
				}

				if (pgaTabCollection != null)
				{
					pgaTabCollection.Dispose();
					pgaTabCollection = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
