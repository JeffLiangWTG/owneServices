using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.EMCS.Module
{
	public partial class DeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		[Obsolete("Use the constructor that takes a module, collection and filter strip, this constructor is just for the designer", true)]
		public DeclarationFilterStripControl()
		{
			InitializeComponent();
		}

		public DeclarationFilterStripControl(IBusinessObjectCollection gridCollection, ZArchitecture.Business.FilterStripBusinessObject filterStripBusinessObject)
			: base(null, gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			RemoveIrrelevantColumns();
			AddAndRenameColumns();
			SetDefaultColumns();
		}

		void RemoveIrrelevantColumns()
		{
			RemoveColumn(EMCSJobDeclaration.Schema.JE_MessageType);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_ContainerMode);
			RemoveColumn(EMCSJobDeclaration.Schema.FreightContainerMode);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_DateOfFirstArrival);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_EntryStatus);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_ETAOfDischarge);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_ETDOfLoading);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_ExportGoodsType);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_RL_NKFinalDestination);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_DateAtFinalDestination);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_RL_NKPortOfFirstArrival);
			RemoveColumn(EMCSJobDeclaration.Schema.ForwarderName);
			RemoveColumn(EMCSJobDeclaration.Schema.ImporterName);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_TotalVolume);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_TotalWeight);
			RemoveColumn(EMCSJobDeclaration.Schema.JE_EntrySubmittedDate);
		}

		void AddAndRenameColumns()
		{
			grid.GetColumnStyle(EMCSJobDeclaration.Schema.JE_MessageSubType).CaptionResourceString = null;
			grid.SetColumnVisible(true, EMCSJobDeclaration.Schema.JE_MessageSubType);
			grid.SetColumnWidth(EMCSJobDeclaration.Schema.JE_MessageSubType, 105);
			grid.GetColumnStyle(EMCSJobDeclaration.Schema.JE_OH_Supplier).CaptionResourceString = null;
			grid.GetColumnStyle(EMCSJobDeclaration.Schema.JE_OH_Importer).CaptionResourceString = null;
			grid.GetColumnStyle(EMCSJobDeclaration.Schema.JE_EntryAuthorisationDate).CaptionResourceString = null;
			grid.GetColumnStyle(EMCSJobDeclaration.Schema.JE_OwnerRef).CaptionResourceString = null;
			grid.SetColumnVisible(true, EMCSJobDeclaration.Schema.JE_OwnerRef);
			grid.SetColumnWidth(EMCSJobDeclaration.Schema.JE_OwnerRef, 140);
			grid.GetColumnStyle(EMCSJobDeclaration.Schema.JE_EntryStatusDescription).CaptionResourceString = null;

			AddColumn(EMCSJobDeclaration.Schema.JE_DeclarantType, 105);
			AddColumn(EMCSJobDeclaration.Schema.EADNumber, 140);
			AddColumn(EMCSJobDeclaration.Schema.ZG_SubmissionType, 105);
			AddColumn(EMCSJobDeclaration.Schema.ZG_DeferredSubmission, 120);
			AddColumn(EMCSJobDeclaration.Schema.ZG_GuarantorType, 85);
			AddColumn(EMCSJobDeclaration.Schema.ZG_OriginType, 80);
			AddColumn(EMCSJobDeclaration.Schema.InvoiceNumber, 136);
			AddColumn(GoodsOwner, 90, Res.GetString("4135BDA6-0AE4-4EC7-B10F-D0B73411571F", "Goods Owner"));
			AddColumn(DispatchWarehouse, 122, Res.GetString("8F31A8B2-93A4-4DA0-B33B-079D4B264960", "Dispatch Warehouse"));
			AddColumn(DestinationWarehouse, 135, Res.GetString("DFB76DF3-5999-41EF-BA32-0A56662BA625", "Destination Warehouse"));
			AddColumn(CarrierAgent, 90, Res.GetString("E63F456C-B2A0-4D7C-B44F-D3B7570E7F82", "Carrier Agent"));
			AddColumn(Transporter, 80, Res.GetString("1C909248-1AB9-43A2-93CC-E077FE997948", "Transporter"));
			AddColumn(EMCSJobDeclaration.Schema.JE_DateAtOrigin, 105);
		}

		void SetDefaultColumns()
		{
			string[] defaultColumnsOrder =
			{
				EMCSJobDeclaration.Schema.JE_DeclarantType,
				EMCSJobDeclaration.Schema.JE_GB,
				EMCSJobDeclaration.Schema.JE_DeclarationReference,
				EMCSJobDeclaration.Schema.JE_DateAtOrigin,
				EMCSJobDeclaration.Schema.JE_OwnerRef,
				EMCSJobDeclaration.Schema.EADNumber,
				EMCSJobDeclaration.Schema.JE_OH_Supplier,
				EMCSJobDeclaration.Schema.JE_OH_Importer,
				GoodsOwner,
				DispatchWarehouse,
				DestinationWarehouse,
				EMCSJobDeclaration.Schema.JE_MessageSubType,
				EMCSJobDeclaration.Schema.ZG_SubmissionType,
				EMCSJobDeclaration.Schema.ZG_DeferredSubmission,
				EMCSJobDeclaration.Schema.ZG_GuarantorType,
				EMCSJobDeclaration.Schema.ZG_OriginType,
				EMCSJobDeclaration.Schema.InvoiceNumber,
				CarrierAgent,
				Transporter,
			};

			grid.SetAllColumnsVisible(false);
			grid.SetColumnVisible(true, defaultColumnsOrder);
			grid.ReOrderColumns(defaultColumnsOrder);
		}

		void RemoveColumn(string columnName)
		{
			var columnStyle = grid.GetColumnStyle(columnName);
			if (columnStyle != null)
			{
				grid.ColumnStyles.Remove(columnStyle);
			}
		}

		void AddColumn(string columnName, int columnWidth, string columnCaption = null)
		{
			var column = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				ColumnName = columnName,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(columnWidth),
				Caption = columnCaption
			};
			grid.ColumnStyles.Add(column);
		}

		public const string GoodsOwner = nameof(EMCSJobDeclaration.OwnerDocumentaryAddress) + JobDocAddressOrgCodeSuffix;
		public const string DispatchWarehouse = nameof(EMCSJobDeclaration.DispatchWarehouseDocumentaryAddress) + JobDocAddressOrgCodeSuffix;
		public const string DestinationWarehouse = nameof(EMCSJobDeclaration.DestinationWarehouseDocumentaryAddress) + JobDocAddressOrgCodeSuffix;
		public const string CarrierAgent = nameof(EMCSJobDeclaration.CarrierAgentDocumentaryAddress) + JobDocAddressOrgCodeSuffix;
		public const string Transporter = nameof(EMCSJobDeclaration.TransporterDocumentaryAddress) + JobDocAddressOrgCodeSuffix;
		const string JobDocAddressOrgCodeSuffix = "+Organisation+" + OrgHeader.Schema.OH_Code;
	}
}
