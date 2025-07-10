using System;
using Enterprise.Customs.CH.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CH.GUI;

public partial class AdditionalInformationUserControl : ZUserControl
{
	public AdditionalInformationUserControl()
	{
		InitializeComponent();

		if (!DesignModeFinder.IsDesigning)
		{
			AddDynamicLayoutUserControl();
		}
	}

	bool isImport = true;
	bool isParentInvoiceLine = true;

	protected override void OnAfterFirstBinding(EventArgs e)
	{
		base.OnAfterFirstBinding(e);
		if (DataSource != null)
		{
			if (DataSource is JobDeclaration dec)
			{
				isImport = dec.IsImport;
			}

			if (DataMember.StartsWith($"{nameof(JobDeclaration.CustomsEntryInstructions)}."))
			{
				isParentInvoiceLine = false;
			}

			using (AdditionalInformationGrid.SuspendRefreshTableStylesAndRefreshAtDisposal())
			{
				AdditionalInformationGrid.SetAllAvailability(false);
				InitializeGridLayout();
				AdditionalInformationGrid.SetAvailability(true, GetAvailableColumnsForLinkedDocument());
			}
		}
	}

	void InitializeGridLayout()
	{
		if (isImport)
		{
			AdditionalInformationGrid.ColumnStyles.Add(new ZDropEditColumnStyleInfo
			{
				ColumnName = AdditionalInformation.Schema.CSI_ReferenceNumber,
			});
		}
		else
		{
			AdditionalInformationGrid.ColumnStyles.Add(new ZMultiControlColumnStyleInfo
			{
				ColumnName = AdditionalInformation.Schema.CSI_Description,
				FieldTypeColumnName = AdditionalInformation.Schema.CSI_DescriptionFieldType
			});
		}
	}

	string[] GetAvailableColumnsForLinkedDocument() => isImport
														? columnOrderImport
														: isParentInvoiceLine
															? columnOrderExportInvoiceLine
															: columnOrderExportEntryInstruction;

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "This is a readonly static field.")]
	static readonly string[] columnOrderImport = new[]
	{
			AdditionalInformation.Schema.CSI_LineNo,
			AdditionalInformation.Schema.CSI_Code,
			AdditionalInformation.Schema.CSI_ReferenceNumber,
		};

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "This is a readonly static field.")]
	static readonly string[] columnOrderExportInvoiceLine = new[]
	{
			AdditionalInformation.Schema.CSI_LineNo,
			AdditionalInformation.Schema.CSI_Code,
			AdditionalInformation.Schema.CSI_Description,
		};

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:Static Fields Are Thread Static Rule", Justification = "This is a readonly static field.")]
	static readonly string[] columnOrderExportEntryInstruction = new[]
	{
			AdditionalInformation.Schema.CSI_Code,
			AdditionalInformation.Schema.CSI_Description,
		};

	#region Dynamic Layout

	void AddDynamicLayoutUserControl()
	{
		DynamicAdditionalInformationPanel.UpdateLayout(AdditionalInformationPanelLayout);
	}

	IPanelLayoutProvider AdditionalInformationPanelLayout => additionalInformationPanelLayout ?? (additionalInformationPanelLayout = GetNewAdditionalInformationLayout());
	IPanelLayoutProvider additionalInformationPanelLayout;

	protected virtual IPanelLayoutProvider GetNewAdditionalInformationLayout() => new AdditionalInformationFieldsLayout();

	#endregion
}
