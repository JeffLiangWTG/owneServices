using System;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class EntryInstructionDetailsUserControl : BaseCustomsEntryUserControl
	{
		public EntryInstructionDetailsUserControl()
		{
			InitializeComponent();

			AdditionalInfoTabPage.RunWhenBindingOrFirstShown(InitAdditionalInfosUserControl);
			PreviousDocumentsTabPage.RunWhenBindingOrFirstShown(InitPreviousDocumentsUserControl);
			SupportingDocumentsTabPage.RunWhenBindingOrFirstShown(InitSupportingDocumentsUserControl);

			EntryInstructionGridUserControl.HostedControlCreated += EntryInstructionGridUserControl_HostedControlCreated;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			EntryInstructionGridUserControl.UserControlType = GetGridUserControl();
			AttachDetachStyleValueChangedEvent(CurrentEntryInstruction, CurrentEntryInstruction);
			DetailsUserControl.UserControlType = GetDetailsUserControlType();
			FiscalReferencesUserControl.UserControlType = GetFiscalReferencesUserControlType();
			AuthorisationsUserControl.UserControlType = GetAuthorisationsUserControlType();
			GuaranteesUserControl.UserControlType = GetGuaranteesUserControlType();
			SealsUserControl.UserControlType = GetSealsUserControlType();
			SpecialProceduresUserControl.UserControlType = GetSpecialProceduresUserControlType();

			ShowOrHideDV1DetailsTabPage();
		}

		protected override void ChangeControlsVisibility()
		{
			base.ChangeControlsVisibility();
			SetAuthorisationsTabPageCaption();
			SetSupplyChainActorTabPageCaption();
			SetAdditionalInfosTabCaption();
			SetSupportingDocumentsTabPageCaption();
			SetPreviousDocumentsTabPageCaption();
		}

		protected virtual void SetSupplyChainActorTabPageCaption()
		{
			SupplyChainActorTabPage.CaptionResourceString = Res.GetData("ADBE8027-30E8-4C3A-9A28-1541A69C7EB9", "Add. Supply Chain Actors");
		}

		protected virtual void SetAuthorisationsTabPageCaption()
		{
			if (IsUCC5)
			{
				AuthorisationsTabPage.CaptionResourceString = Res.GetData("7D9E7E1B-EFE8-419C-A1E9-E6AA7017C6BB", "[UCC 3/39] Authorizations");
			}
			else if (IsUCC6AndIsExport)
			{
				AuthorisationsTabPage.CaptionResourceString = Res.GetData("F0A2D189-C4E9-4681-B6C8-B77442540CE7", "Authorizations");
			}
			else
			{
				AuthorisationsTabPage.CaptionResourceString = Res.GetData("E179D6D-68A2-4453-BD15-D0E8DAC0377C", "[3/39] Authorizations");
			}
		}

		protected virtual void SetAdditionalInfosTabCaption()
		{
			if (GetAdditionalInfosTabCaption() is ResourceStringData additionalInfoTabPageCaption)
			{
				AdditionalInfoTabPage.CaptionResourceString = additionalInfoTabPageCaption;
			}
		}

		protected virtual void SetSupportingDocumentsTabPageCaption()
		{
			if (SupportingDocumentsTabPageCaption is ResourceStringData supportingDocumentsTabPageCaption)
			{
				SupportingDocumentsTabPage.CaptionResourceString = supportingDocumentsTabPageCaption;
			}
		}

		protected virtual void SetPreviousDocumentsTabPageCaption()
		{
			if (PreviousDocumentsTabPageCaption is ResourceStringData previousDocumentsTabPageCaption)
			{
				PreviousDocumentsTabPage.CaptionResourceString = previousDocumentsTabPageCaption;
			}
		}

		bool IsUCC5 => CurrentDataItem?.IsUCC5 ?? false;
		protected bool IsUCC6AndIsExport => CurrentDataItem?.IsUCC6AndIsExport ?? false;

		protected override void OnCurrentDataItemChanging(EventArgs e)
		{
			base.OnCurrentDataItemChanging(e);
			if (CurrentDataItem is JobDeclaration jobDeclaration)
			{
				jobDeclaration.ZG_IsHighValueOvrdInfo.ValueChanged -= JobDeclaration_JE_IsHighValueOvrdChanged;
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			if (CurrentDataItem is JobDeclaration jobDeclaration)
			{
				jobDeclaration.ZG_IsHighValueOvrdInfo.ValueChanged += JobDeclaration_JE_IsHighValueOvrdChanged;
			}
			base.OnCurrentDataItemChanged(e);
		}

		protected virtual Type GetGridUserControl() => typeof(EntryInstructionGridUserControl);

		protected virtual Type GetDetailsUserControlType() => typeof(EntryInstructionDetailBasicUserControl);

		protected virtual Type GetFiscalReferencesUserControlType() => typeof(EntryInstructionFiscalReferencesUserControl);

		protected virtual Type GetAuthorisationsUserControlType() => typeof(EntryInstructionAuthorisationsUserControl);

		protected virtual Type GetGuaranteesUserControlType() => typeof(EntryInstructionGuaranteesUserControl);

		protected virtual Type GetSealsUserControlType() => typeof(EntryInstructionExportSealsUserControl);

		protected virtual Type GetPreviousDocumentsUserControlType() => typeof(LayoutPreviousDocumentsUserControl);

		protected virtual ResourceStringData PreviousDocumentsTabPageCaption => null;

		protected virtual Type GetSupportingDocumentsUserControlType() => typeof(InvoiceLayoutSupportingDocumentsUserControl);

		protected virtual ResourceStringData SupportingDocumentsTabPageCaption => null;

		public void SetTabPagesVisibility() => SetTabPagesVisibilityCore();

		protected virtual void OnEntryInstructionChanging(CusEntryInstruction oldEntryInstruction, CusEntryInstruction newEntryInstruction)
		{
			AttachDetachStyleValueChangedEvent(oldEntryInstruction, newEntryInstruction);
		}

		protected virtual void OnEntryInstructionChanged()
		{
			SetGuaranteesTabPageVisibility();
		}

		protected EntryInstructionGridUserControl EntryInstructionGrid => EntryInstructionGridUserControl.HostedControl as EntryInstructionGridUserControl;
		public new JobDeclaration CurrentDataItem => (JobDeclaration)base.CurrentDataItem;
		protected CusEntryInstruction CurrentEntryInstruction => EntryInstructionGrid?.CurrentEntryInstruction;

		protected virtual Type GetAdditionalInfosUserControlType()
		{
			return typeof(AdditionalInfosUserControlWithGrid);
		}

		protected virtual ResourceStringData GetAdditionalInfosTabCaption()
		{
			return Res.GetData("76c5bc00-e3e4-4380-b3a9-20fe00279ea1", "Additional Info");
		}

		protected const string SupportingInfoColumnLayoutContext = "DEC";

		protected virtual Type GetSpecialProceduresUserControlType()
		{
			return typeof(SpecialProceduresUserControl);
		}

		#region Implementation

		void InitAdditionalInfosUserControl(object sender, EventArgs e)
		{
			AdditionalInfoUserControl.UserControlType = GetAdditionalInfosUserControlType();
			AdditionalInfoUserControl.HostedControlCreated += (sender, args) =>
			{
				if (AdditionalInfoUserControl.HostedControl is ISupportingInfoUserControls)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(AdditionalInfoUserControl.HostedControl, "CustomsEntryInstructions", SupportingInfoColumnLayoutContext);
				}

				if (AdditionalInfoUserControl.HostedControl is AdditionalInfosUserControlWithGrid additionalInfosUserControlWithGrid)
				{
					additionalInfosUserControlWithGrid.AdditionalInfosGroupBox.CaptionResourceString = GetAdditionalInfosTabCaption();
				}
			};
		}

		void AttachDetachStyleValueChangedEvent(CusEntryInstruction oldEntryInstruction, CusEntryInstruction newEntryInstruction)
		{
			if (oldEntryInstruction != null)
			{
				oldEntryInstruction.CEI_StyleInfo.ValueChanged -= CEI_StyleInfo_ValueChanged;
			}
			if (newEntryInstruction != null)
			{
				newEntryInstruction.CEI_StyleInfo.ValueChanged += CEI_StyleInfo_ValueChanged;
			}
		}

		void CEI_StyleInfo_ValueChanged(object sender, EventArgs e)
		{
			SetGuaranteesTabPageVisibility();
			SetSpecialProceduresTabPageVisibility();
		}

		void SetGuaranteesTabPageVisibility() => GuaranteesTabPage.TabVisible = CurrentDataItem?.Configuration?.InstructionConfiguration?.GuaranteesSupport(CurrentDataItem, CurrentEntryInstruction) ?? false;

		void SetSpecialProceduresTabPageVisibility() => SpecialProceduresTabPage.TabVisible = CurrentDataItem?.Configuration?.InstructionConfiguration?.SpecialProceduresSupport(CurrentDataItem, CurrentEntryInstruction) ?? false;

		protected virtual void SetTabPagesVisibilityCore()
		{
			var jobDeclaration = CurrentDataItem;
			var configuration = jobDeclaration?.Configuration.InstructionConfiguration;
			if (configuration != null)
			{
				FiscalReferencesTabPage.TabVisible = configuration.FiscalReferencesSupport(jobDeclaration);
				AuthorisationsTabPage.TabVisible = configuration.AuthorisationsSupport(jobDeclaration);
				SupplyChainActorTabPage.TabVisible = configuration.AdditionalSupplyChainActorSupport(jobDeclaration);
				SealsTabPage.TabVisible = configuration.SealsSupport(jobDeclaration) && SealsTabPageVisibleCore(jobDeclaration);
				AdditionalInfoTabPage.TabVisible = configuration.AdditionalInfosSupport(jobDeclaration, CurrentEntryInstruction);
				PreviousDocumentsTabPage.TabVisible = configuration.PreviousDocumentsSupport(jobDeclaration);
				SupportingDocumentsTabPage.TabVisible = configuration.SupportingDocumentsSupport(jobDeclaration);
				GuaranteesTabPage.TabVisible = configuration.GuaranteesSupport(jobDeclaration, CurrentEntryInstruction);
				SpecialProceduresTabPage.TabVisible = configuration.SpecialProceduresSupport(jobDeclaration, CurrentEntryInstruction);
			}
		}

		protected virtual ZBool SealsTabPageVisibleCore(JobDeclaration jobDeclaration) => true;

		void JobDeclaration_JE_IsHighValueOvrdChanged(object sender, EventArgs e)
		{
			ShowOrHideDV1DetailsTabPage();
		}

		void ShowOrHideDV1DetailsTabPage()
		{
			var declaration = JobDeclaration as JobDeclaration;
			DV1DetailsTabPage.TabRelevant = declaration != null && declaration.ZG_IsHighValueOvrd && declaration.DV1DetailsSupport;
		}

		void EntryInstructionGridUserControl_HostedControlCreated(object sender, EventArgs e)
		{
			EntryInstructionGrid.OnEntryInstructionChanging += EntryInstructionGrid_OnEntryInstructionChanging;
			EntryInstructionGrid.OnEntryInstructionChanged += EntryInstructionGrid_OnEntryInstructionChanged;
		}

		void EntryInstructionGrid_OnEntryInstructionChanging(object sender, EntryInstructionChangingEvent e)
		{
			OnEntryInstructionChanging(e.OldEntryInstruction, e.NewEntryInstruction);
		}

		void EntryInstructionGrid_OnEntryInstructionChanged(object sender, EventArgs e)
		{
			OnEntryInstructionChanged();
		}

		void InitPreviousDocumentsUserControl(object sender, EventArgs args)
		{
			PreviousDocumentsUserControl.UserControlType = GetPreviousDocumentsUserControlType();
			PreviousDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (PreviousDocumentsUserControl.HostedControl is ISupportingInfoUserControls)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(PreviousDocumentsUserControl.HostedControl, "CustomsEntryInstructions", SupportingInfoColumnLayoutContext);
				}
			};
		}

		void InitSupportingDocumentsUserControl(object sender, EventArgs args)
		{
			SupportingDocumentsUserControl.UserControlType = GetSupportingDocumentsUserControlType();
			SupportingDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				if (SupportingDocumentsUserControl.HostedControl is ISupportingInfoUserControls)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(SupportingDocumentsUserControl.HostedControl, "CustomsEntryInstructions", SupportingInfoColumnLayoutContext);
				}
			};
		}

		#endregion

		#region Dispose

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				components?.Dispose();
				if (EntryInstructionGridUserControl != null)
				{
					EntryInstructionGridUserControl.HostedControlCreated -= EntryInstructionGridUserControl_HostedControlCreated;
				}
				if (EntryInstructionGrid != null)
				{
					EntryInstructionGrid.OnEntryInstructionChanging -= EntryInstructionGrid_OnEntryInstructionChanging;
					EntryInstructionGrid.OnEntryInstructionChanged -= EntryInstructionGrid_OnEntryInstructionChanged;
				}
				if (CurrentEntryInstruction != null)
				{
					CurrentEntryInstruction.CEI_StyleInfo.ValueChanged -= CEI_StyleInfo_ValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
