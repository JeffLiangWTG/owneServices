using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Module
{
	public partial class EntryHeaderFilterUserControl : Customs.Module.EntryHeaderFilterUserControl
	{
		#region Schema

		public new static class Schema
		{
			public const string EntryStyle = "Declaration+JE_EntryStyle";
			public const string Warehouse = "EntryInstructionWarehouseCode";
			public const string CustomsOfficeOfPresentation = "Declaration+JE_CustomsOffice";
			public const string SupplierCode = "Declaration+JE_OH_Supplier";
			public const string ImporterCode = "Declaration+JE_OH_Importer";
			public const string DeclarationOrigin = "Declaration+JE_GoodsOrigin";
			public const string DeclarationDestination = "Declaration+JE_GoodsDestination";
			public const string ApprovalDeferNo = "Declaration+JE_DefermentAccountNumber";
			public const string TotalGrossWeight = "TotalGrossWeight";
			public const string TotalCustomsQuantity = "TotalCustomsQuantity";
			public const string LocalClientCode = "Declaration+Job+LocalZAddressWithContact+OrgPK";
			public const string FromWarehouse = "EntryInstruction+CEI_OA_Warehouse";
			public const string ToWarehouse = "EntryInstruction+CEI_OA_Warehouse2";
			public const string InvoiceCurrency = "InvoiceCurrency";
			public const string InvoiceAmount = "TotalPriceAmount";
			public const string EntryLinesCount = "MergedLinesCount";
			public const string Mrn = "MovementReferenceNumber";
			public const string ExitStatus = "CH_ExitedStatus";
		}

		#endregion

		[System.Obsolete("Use the constructor that takes a collection and/or business object, this constructor is just for the designer")]
		public EntryHeaderFilterUserControl()
		{
			InitializeComponent();
		}

		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("ABB173F0-8B27-4495-8176-CED018237A48", "Entry Style"),
					ColumnName = Schema.EntryStyle,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("83EC1B95-1F88-40E3-AC64-663F8CD588DE", "Warehouse"),
					ColumnName = Schema.Warehouse,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("9EDA0A7B-5FEB-4FB2-802B-47BA81B65E36", "Office of Presentation"),
					ColumnName = Schema.CustomsOfficeOfPresentation,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("48B54E93-848A-4300-B270-9031D386AAE2", "Supplier Code"),
					ColumnName = Schema.SupplierCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("496B075E-AEEF-45A1-A471-84D35308B041", "Importer Code"),
					ColumnName = Schema.ImporterCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("5A13942A-1CFA-454F-B44A-604A71909E6F", "Declaration Dispatch"),
					ColumnName = Schema.DeclarationOrigin,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("073C48B5-F775-4DED-89EB-1ED0F1D99022", "Declaration Destination"),
					ColumnName = Schema.DeclarationDestination,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("17332843-60C0-42DE-9468-5B8E4E01EF5C", "Approval Defer No"),
					ColumnName = Schema.ApprovalDeferNo,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("E93575D4-5A14-4BAA-AA79-79D2179DCB4C", "Total Gross Weight"),
					ColumnName = Schema.TotalGrossWeight,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("20B9AE59-38F7-45A4-A913-F6EF4B74EA4A", "Total Customs Quantity"),
					ColumnName = Schema.TotalCustomsQuantity,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
					Decimals = JobComInvoiceLineSchema.JI_CustomsQuantity.Scale
				},
				new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("5A8F405E-0CEF-446B-A9BC-428FD0ADB25C", "Local Client Code"),
					ColumnName = Schema.LocalClientCode,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("8F70F276-9AE5-4267-B4EF-0C7F1971851B", "From Warehouse"),
					ColumnName = Schema.FromWarehouse,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("8AE848DC-B2EF-4D74-ACC6-973E8A5DF31A", "To Warehouse"),
					ColumnName = Schema.ToWarehouse,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("C024B41A-B884-4E89-A8A2-04B2FB48A048", "Invoice Currency"),
					ColumnName = Schema.InvoiceCurrency,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("6347EF72-438C-407F-B349-19D2D21FFC4B", "Invoice Amount"),
					ColumnName = Schema.InvoiceAmount,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZCalcEditColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("4E195A4E-D4DA-4351-B829-CA6B65AFAA34", "Entry Lines"),
					ColumnName = Schema.EntryLinesCount,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("D86530CF-5FE9-4C60-B89D-957745D22871","MRN"),
					ColumnName = Schema.Mrn,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("22D3BA93-DD45-4C30-91BF-4763221FB115","Exit Status"),
					ColumnName = Schema.ExitStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
				}
			});
		}
	}
}
