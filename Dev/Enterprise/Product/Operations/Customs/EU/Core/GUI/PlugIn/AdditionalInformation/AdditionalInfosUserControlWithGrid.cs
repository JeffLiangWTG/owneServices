using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI.PlugIn
{
	public partial class AdditionalInfosUserControlWithGrid : ZUserControl, ISupportingInfoUserControls, ISupportMultipleResourceStringDataSupporter, ISupportMultipleResourceStringData
	{
		public AdditionalInfosUserControlWithGrid()
		{
			InitializeComponent();
			AdditionalInfosGrid.AfterBind += AdditionalInfosGrid_AfterBind;

			DetailsLayoutControl.CaptionRenderingEnabled = true;
		}

		protected virtual void AdditionalInfosGridColumnsVisible()
		{
			var availableColumnNames = AvailableColumnNames;
			if (availableColumnNames.Count > 0)
			{
				AdditionalInfosGrid.SetAvailability(false, availableColumnNames);
			}
		}

		protected virtual IReadOnlyList<string> AvailableColumnNames
		{
			get
			{
				var availableColumnNames = Array.Empty<string>();

				if (IsBoundToInvoiceHeaders || IsBoundToEntryInstructions)
				{
					availableColumnNames = new[]
					{
						AdditionalInfo.Schema.CSI_ReferenceNumber2,
						AdditionalInfo.Schema.CSI_RX_NKCurrency,
						AdditionalInfo.Schema.CSI_Value
					};
				}
				else if (IsBoundToUCC6TemporaryStorageBill)
				{
					availableColumnNames = new[]
					{
						AdditionalInfo.Schema.CSI_ReferenceNumber2,
						AdditionalInfo.Schema.CSI_RX_NKCurrency,
						AdditionalInfo.Schema.CSI_Value,
						AdditionalInfo.Schema.CSI_SubType,
						AdditionalInfo.Schema.CSI_Code,
						AdditionalInfo.Schema.CSI_Description
					};
				}

				return availableColumnNames;
			}
		}

		void AdditionalInfosGrid_AfterBind(object sender, EventArgs e)
		{
			AdditionalInfosGridColumnsVisible();
			SetAdditionalInfosLayout();
			AdjustControlProperties();
		}

		protected virtual void SetAdditionalInfosLayout()
		{
			if (IsBoundToInvoiceLines)
			{
				DetailsLayoutControl.SetLayout(CreateNewInvoiceLineAdditionalInformationDetailsLayout());
			}
			else if (IsBoundToClassPartPivot)
			{
				DetailsLayoutControl.SetLayout(CreateNewClassPartPivotAdditionalInformationDetailsLayout());
			}
			else if (IsBoundToInvoiceHeaders)
			{
				DetailsLayoutControl.SetLayout(CreateNewInvoiceHeaderAdditionalInformationDetailsLayout());
			}
			else if (IsBoundToCusExitDetails)
			{
				DetailsLayoutControl.SetLayout(CreateNewExitSummaryAdditionalInformationDetailsLayout());
			}
			else if (IsBoundToEntryInstructions)
			{
				DetailsLayoutControl.SetLayout(CreateNewInvoiceHeaderAdditionalInformationDetailsLayout());
			}
			else if (IsBoundToDeclaration)
			{
				DetailsLayoutControl.SetLayout(CreateNewDeclarationAdditionalInformationDetailsLayout());
			}
		}

		protected virtual void AdjustControlProperties()
		{
			if (IsBoundToUCC6TemporaryStorageBill)
			{
				AdditionalInfosGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("A01E8130-4BE0-4557-97F9-E61716E8BE08", "Additional Information");
				AdditionalInfosPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 50, true);
				AdditionalInfosPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1209, 50, true);
				this.Dock = System.Windows.Forms.DockStyle.Fill;
			}
		}

		public string GridBindingMember => nameof(JobDeclaration.FilteredInvoiceLines);

		public ZGrid Grid => AdditionalInfosGrid;

		protected virtual IPanelLayoutProvider CreateNewInvoiceHeaderAdditionalInformationDetailsLayout() => new InvoiceHeaderAdditionalInformationDetailsLayout();
		protected virtual IPanelLayoutProvider CreateNewInvoiceLineAdditionalInformationDetailsLayout() => new InvoiceLineAdditionalInformationDetailsLayout();
		protected virtual IPanelLayoutProvider CreateNewExitSummaryAdditionalInformationDetailsLayout() => new ExitSummaryAdditionalInformationDetailsLayout();
		protected virtual IPanelLayoutProvider CreateNewDeclarationAdditionalInformationDetailsLayout() => new DeclarationAdditionalInformationDetailsLayout();
		protected virtual IPanelLayoutProvider CreateNewClassPartPivotAdditionalInformationDetailsLayout() => new InvoiceLineAdditionalInformationDetailsLayout();

		protected bool IsBoundToInvoiceLines => AdditionalInfosGrid.DataMember == InvoiceLinesAdditionalInfoBindingMemberName;
		protected bool IsBoundToInvoiceHeaders => AdditionalInfosGrid.DataMember == InvoiceHeadersAdditionalInfoBindingMemberName;
		protected bool IsBoundToCusExitDetails => AdditionalInfosGrid.DataMember == CusExitDetailAdditionalInfoBindingMemberName;
		protected bool IsBoundToEntryInstructions => AdditionalInfosGrid.DataMember == CusEntryInstructonInfoBindingMemberName;
		protected bool IsBoundToDeclaration => AdditionalInfosGrid.DataMember == DeclarationAdditionalInfoBindingMemberName;
		protected bool IsBoundToUCC6TemporaryStorageBill => AdditionalInfosGrid.DataMember == UCC6TemporaryStorageBillAdditionalInfoBingdingMemberName;
		protected bool IsBoundToClassPartPivot => AdditionalInfosGrid.DataMember == ClassPartPivotAdditionalInfoBindingMemberName;

		const string InvoiceLinesAdditionalInfoBindingMemberName = nameof(JobDeclaration.FilteredInvoiceLines) + "." + nameof(JobComInvoiceLine.AdditionalInfos);
		const string InvoiceHeadersAdditionalInfoBindingMemberName = nameof(JobDeclaration.Invoices) + "." + nameof(JobComInvoiceLine.AdditionalInfos);
		const string CusExitDetailAdditionalInfoBindingMemberName = nameof(CusExitControlHeader.CusExitDetails) + "." + nameof(JobComInvoiceLine.AdditionalInfos);
		const string CusEntryInstructonInfoBindingMemberName = nameof(JobDeclaration.CustomsEntryInstructions) + "." + nameof(JobComInvoiceLine.AdditionalInfos);
		const string DeclarationAdditionalInfoBindingMemberName = "." + nameof(JobComInvoiceLine.AdditionalInfos);
		const string UCC6TemporaryStorageBillAdditionalInfoBingdingMemberName = nameof(TemporaryStorageHeader.Bills) + "." + nameof(TemporaryStorageBill.AdditionalInfos);
		const string ClassPartPivotAdditionalInfoBindingMemberName = nameof(OrgSupplierPart.PivotsForBinding) + "." + nameof(CusClassPartPivot.AdditionalInfos);
		ISupportMultipleResourceStringData ISupportMultipleResourceStringDataSupporter.SupportMultipleResourceStringData => CurrentDataItem as ISupportMultipleResourceStringData;

		public IReadOnlyList<string> MultipleKeysToUse => new[] { JobDeclaration.CaptionKeySAD };
	}
}
