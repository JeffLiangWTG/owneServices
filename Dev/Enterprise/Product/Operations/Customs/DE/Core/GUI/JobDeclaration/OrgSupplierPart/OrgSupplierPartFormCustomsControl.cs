using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.EU.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public partial class OrgSupplierPartFormCustomsControl : EU.GUI.EUOrgSupplierPartFormCustomsControl
	{
		public OrgSupplierPartFormCustomsControl() : base()
		{
			InitializeComponent();
		}

		protected override Type GetSupportingDocumentsUserControlType() => typeof(SupportingDocumentsUCWrapperForOrgSupplierPart);

		protected override void SupportingDocumentsUserControlHostedControlCreated()
		{
			SupportingDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				supportingDocumentsUcWrapper = SupportingDocumentsUserControl.HostedControl as SupportingDocumentsUCWrapperForOrgSupplierPart;
				if (supportingDocumentsUcWrapper != null)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(supportingDocumentsUcWrapper.ExportSupportingDocumentsUserControl, nameof(OrgSupplierPart.PivotsForBinding), SupportingInfoColumnLayoutContext);
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(supportingDocumentsUcWrapper.ImportSupportingDocumentsUserControl, nameof(OrgSupplierPart.PivotsForBinding), SupportingInfoColumnLayoutContext);
					SetVisibilityForSupportingDocumentsUserControl();
				}
			};
		}

		SupportingDocumentsUCWrapperForOrgSupplierPart supportingDocumentsUcWrapper;

		protected override Type GetAdditionalInfosUserControlType() => typeof(AdditionalInfosUCWrapperForOrgSupplierPart);

		protected override void AdditionalInfosUserControlHostedControlCreated()
		{
			additionalInfosUserControl1.HostedControlCreated += (sender, args) =>
			{
				additionalInfosUcWrapper = additionalInfosUserControl1.HostedControl as AdditionalInfosUCWrapperForOrgSupplierPart;
				if (additionalInfosUcWrapper != null)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(additionalInfosUcWrapper.ExportAdditionalInfosUserControl, nameof(OrgSupplierPart.PivotsForBinding), SupportingInfoColumnLayoutContext);
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(additionalInfosUcWrapper.ImportAdditionalInfosUserControl, nameof(OrgSupplierPart.PivotsForBinding), SupportingInfoColumnLayoutContext);
					SetCaptionAndVisibilityForAdditionalInfosUserControl();
				}
			};
		}

		AdditionalInfosUCWrapperForOrgSupplierPart additionalInfosUcWrapper;

		protected override Type GetPreviousDocumentsUserControlType() => typeof(PreviousDocumentsUCWrapperForOrgSupplierPart);

		protected override void PreviousDocumentsUserControlHostedControlCreated()
		{
			PreviousDocumentsUserControl.HostedControlCreated += (sender, args) =>
			{
				previousDocumentsUcWrapper = PreviousDocumentsUserControl.HostedControl as PreviousDocumentsUCWrapperForOrgSupplierPart;
				if (previousDocumentsUcWrapper != null)
				{
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(previousDocumentsUcWrapper.ExportPreviousDocumentsUserControl, nameof(OrgSupplierPart.PivotsForBinding), SupportingInfoColumnLayoutContext);
					SupportingInfoUserControlHelper.ChangeParentAndSetGridDetails(previousDocumentsUcWrapper.ImportPreviousDocumentsUserControl, nameof(OrgSupplierPart.PivotsForBinding), SupportingInfoColumnLayoutContext);
					SetVisibilityForPreviousDocumentsUserControl();
				}
			};
		}

		PreviousDocumentsUCWrapperForOrgSupplierPart previousDocumentsUcWrapper;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);

			if (PivotGrid.ListManager != null)
			{
				PivotGrid.ListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
				ListManager_CurrentChanged(this, new EventArgs());
			}
		}

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			if (currentPivot != null && !currentPivot.IsDeleted)
			{
				currentPivot.CI_ChildTypeInfo.ValueChanged -= new EventHandler(CI_ChildTypeInfo_ValueChanged);
			}

			if (PivotGrid.ListManager != null && PivotGrid.CurrentRowIndex >= 0)
			{
				currentPivot = (CusClassPartPivot)PivotGrid.ListManager.GetCurrent();

				if (currentPivot != null && !currentPivot.IsDeleted)
				{
					currentPivot.CI_ChildTypeInfo.ValueChanged += new EventHandler(CI_ChildTypeInfo_ValueChanged);
				}
			}

			CI_ChildTypeInfo_ValueChanged(this, new EventArgs());
		}

		CusClassPartPivot currentPivot;

		void CI_ChildTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			SetVisibilityForSupportingDocumentsUserControl();
			SetCaptionAndVisibilityForAdditionalInfosUserControl();
			SetVisibilityForPreviousDocumentsUserControl();
		}

		void SetVisibilityForSupportingDocumentsUserControl()
		{
			if (supportingDocumentsUcWrapper != null)
			{
				var isExport = IsExport;
				supportingDocumentsUcWrapper.ExportSupportingDocumentsUserControl.Visible = isExport;
				supportingDocumentsUcWrapper.ImportSupportingDocumentsUserControl.Visible = !isExport;
			}
		}

		void SetCaptionAndVisibilityForAdditionalInfosUserControl()
		{
			var isExport = IsExport;
			if (additionalInfosUcWrapper != null)
			{
				additionalInfosUcWrapper.ExportAdditionalInfosUserControl.Visible = isExport;
				additionalInfosUcWrapper.ImportAdditionalInfosUserControl.Visible = !isExport;
			}

			if (isExport)
			{
				var expAdditionalInfosTabPageCaptionResourceString = EXPAdditionalInfosTabPageCaptionResourceString;
				if (additionalInfosTabPage.CaptionResourceString.Key != expAdditionalInfosTabPageCaptionResourceString.Key)
				{
					additionalInfosTabPage.CaptionResourceString = expAdditionalInfosTabPageCaptionResourceString;
					CaptionRenderingSupport.UpdateCaption(additionalInfosTabPage);
				}
			}
			else
			{
				var impAdditionalInfosTabPageCaptionResourceString = IMPAdditionalInfosTabPageCaptionResourceString;
				if (additionalInfosTabPage.CaptionResourceString.Key != impAdditionalInfosTabPageCaptionResourceString.Key)
				{
					additionalInfosTabPage.CaptionResourceString = impAdditionalInfosTabPageCaptionResourceString;
					CaptionRenderingSupport.UpdateCaption(additionalInfosTabPage);
				}
			}
		}

		void SetVisibilityForPreviousDocumentsUserControl()
		{
			if (previousDocumentsUcWrapper != null)
			{
				var isExport = IsExport;
				previousDocumentsUcWrapper.ExportPreviousDocumentsUserControl.Visible = isExport;
				previousDocumentsUcWrapper.ImportPreviousDocumentsUserControl.Visible = !isExport;
			}
		}

		bool IsExport => currentPivot != null && currentPivot.CI_ChildType == ClassificationType.EXP;

		ResourceStringData EXPAdditionalInfosTabPageCaptionResourceString => Res.GetData("43E7A3B4-4F87-4722-87E8-4EFC2025CDC7", "[44] Additional Documents");

		ResourceStringData IMPAdditionalInfosTabPageCaptionResourceString => Res.GetData("88989CCE-09AA-4094-B553-E18D40EA5017", "[44] Additional Infos");
	}
}
