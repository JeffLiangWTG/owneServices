using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.GUI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI
{
	public partial class CusClassificationForm : BaseClassificationForm
	{
		internal readonly PGATabCollection pgaTabCollection;

		public CusClassificationForm(CusClassification classification)
			: base(classification)
		{
			this.pGATabPage.TabVisible = classification.IsHTS;
			pgaTabCollection = new PGATabCollection(MainTabControl, null, false);
			this.pgaTabCollection.Visible = classification.IsHTS;
			if (classification.IsHTS)
			{
				this.MainTabControl.SelectedIndexChanged += MainTabControl_SelectedIndexChanged;
				MainTabControl_SelectedIndexChanged(null, null);
				pgaTabCollection.Update(classification.PGARequirements);

				classification.OnRefreshSIMAMeasureEvent += delegate
				{
					var simaMeasuresForm = new SIMADumpingNumberForm(classification.SIMAMeasures, classification.CC_TariffNum);
					var parentForm = FindForm();
					simaMeasuresForm.Icon = parentForm.Icon;
					if (simaMeasuresForm.ShowDialog(parentForm) == DialogResult.OK)
					{
						return simaMeasuresForm.SelectedDumpingNumber;
					}

					return null;
				};
			}
			else
			{
				ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 230);
			}
		}

		void MainTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (this.MainTabControl.SelectedTab == MainTabPage)
			{
				ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 445);
			}
			else
			{
				ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 550);
			}
		}

		protected override BaseClassificationUserControl GetUserControl()
		{
			return IsHTS ? new HTSClassificationUserControl() : new CAExportClassificationUserControl();
		}

		protected override void ShowNewForm()
		{
			var controller = ZControllerFactory.Create(ControllerID);
			var collection = IsHTS
				? (IBusinessObjectCollection)new HTSClassificationCollection(controller.Factory)
				: new ExportClassificationCollection(controller.Factory);
			controller.SetCollectionForDefaultsAndValidation(collection);
			controller.ShowNewForm();
		}

		protected override SecurityCheckpoint AuditSecurity => IsHTS ? Env.Security.HTSClassificationAudit : Env.Security.ExportClassificationAudit;

		public override string FormCaption => IsHTS ? Res.GetString("bfbcc7b2-9773-4b35-83b9-78e2954e1b15", "HS Classification Lookup") : Res.GetString("3148a337-944f-4e1e-abbe-fb8dec4beae6", "Export Classification Lookup");

		bool IsHTS => BusinessEntity.CC_ClassificationType == CusClassification.ClassificationType.IMP;

		protected override ZBool IsPromptAuditOnSavedEnabled
		{
			get
			{
				var companyPK = GlbCompany.CurrentCompany.PK.ToGuid();
				var branchPK = GlbBranch.CurrentBranch.PK.ToGuid();
				return CACustomsDataRegistry.Instance.ReleaseLowValueProductAudit.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) != ProductAuditActions.Codes.NoAction ||
					CACustomsDataRegistry.Instance.ReleaseHighValueProductAudit.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) != ProductAuditActions.Codes.NoAction ||
					CACustomsDataRegistry.Instance.EntryLowValueProductAudit.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) != ProductAuditActions.Codes.NoAction ||
					CACustomsDataRegistry.Instance.EntryHighValueProductAudit.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty) != ProductAuditActions.Codes.NoAction;
			}
		}

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (IsDisposing)
			{
				if (pgaTabCollection != null)
				{
					pgaTabCollection.Dispose();
				}

				if (BusinessEntity is CusClassification classification && classification.OnRefreshSIMAMeasureEvent != null)
				{
					classification.OnRefreshSIMAMeasureEvent = null;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
