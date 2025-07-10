using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.BR.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Module
{
	public partial class LPCODeclarationFilterStripControl : ZFilterStripControl
	{
		public LPCODeclarationFilterStripControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			AddGridColumns();
			SetupDefaultColumns();
			ReorderColumns();
		}

		protected void AddGridColumns()
		{
			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZGuidFindBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("07735EC8-F15A-4DA0-9568-954C5F01B66B", "Importer"),
						ColumnName = JobDeclaration.Schema.JE_OH_Importer,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZGuidFindBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("856E96F3-9B9C-47C2-8D31-AA2916587D97", "Supplier"),
						ColumnName = JobDeclaration.Schema.JE_OH_Supplier,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZGuidFindBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("4116380A-A3FA-49A6-A880-8E77183E6F0D", "Declaration Branch"),
						ColumnName = JobDeclaration.Schema.JE_GB,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("95245BF8-0624-498F-827F-42D5F7FDFD19", "Job Number"),
						ColumnName = JobDeclaration.Schema.JE_DeclarationReference,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("6B9A9F79-587C-4977-99F7-83395AE27703", "Screening"),
						ColumnName = JobDeclaration.Schema.JE_ScreeningStatus,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("4F624B47-A0CF-49BC-AD72-90DE426B8F34", "License Number"),
						ColumnName = JobDeclaration.Schema.EntryNumbersConcatenated,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("3205F8C4-5662-4EBD-B841-5AE077EE956A", "License Status"),
						ColumnName = JobDeclaration.Schema.EntryStatusesConcatenated,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("4E13F514-3608-47C0-AC48-837CF1EF0294", "License Status Description"),
						ColumnName = JobDeclaration.Schema.EntryStatusDescriptionsConcatenated,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("EDD69CCF-EEF1-44DD-89F6-895F6AD4E69A", "Message Status"),
						ColumnName = JobDeclaration.Schema.JE_MessageStatus,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("F42E854D-7499-4B67-B7FF-28717EE575CD", "Message Status Description"),
						ColumnName = JobDeclaration.Schema.JE_MessageStatusDescription,
						IsReadOnly = true,
						IsVisible = true,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("1CFB7F0A-441F-4CBE-92ED-7F36FEFEDB2A", "License Auth. Date"),
						ColumnName = JobDeclaration.Schema.JE_EntryAuthorisationDate,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("C1A93A51-0922-4924-B15A-D4FF35E64141", "License Style"),
						ColumnName = JobDeclaration.Schema.JE_MessageSubType,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("023BFA6B-09BB-4A53-8FD1-E0A4430733A2", "License Submitted"),
						ColumnName = JobDeclaration.Schema.EntrySubmitDateAsString,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("EA9A6C89-2E3D-4478-A50D-DA71F6207C51", "Issue Date"),
						ColumnName = JobDeclaration.Schema.EntryIssueDateAsString,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("004836F6-0DA1-46DF-992F-0ECF4598FF86", "Importer Name"),
						ColumnName = JobDeclaration.Schema.ImporterName,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
				new ZArchitecture.ZTextBoxColumnStyleInfo
					{
						CaptionResourceString = Res.GetData("19B8629F-40DD-47D2-BF90-65006208DF0B", "Supplier Name"),
						ColumnName = JobDeclaration.Schema.SupplierName,
						IsReadOnly = true,
						IsVisible = false,
						Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120)
					},
			});
		}

		void SetupDefaultColumns()
		{
			grid.SetAllColumnsVisible(false);
			grid.SetColumnVisible(true, DefaultColumnsForGrid);
		}

		void ReorderColumns()
		{
			grid.ReOrderColumns(DefaultColumnsForGrid);
		}

		string[] defaultColumnsForGrid;

		string[] DefaultColumnsForGrid
		{
			get
			{
				if (defaultColumnsForGrid == null)
				{
					var columnList = new List<string>
					{
						JobDeclaration.Schema.JE_GB,
						JobDeclaration.Schema.JE_DeclarationReference,
						JobDeclaration.Schema.JE_OH_Importer,
						JobDeclaration.Schema.JE_OH_Supplier,
						JobDeclaration.Schema.JE_ScreeningStatus,
						JobDeclaration.Schema.EntryNumbersConcatenated,
						JobDeclaration.Schema.EntryStatusesConcatenated,
						JobDeclaration.Schema.EntryStatusDescriptionsConcatenated,
						JobDeclaration.Schema.JE_MessageStatus,
						JobDeclaration.Schema.JE_MessageStatusDescription
					};

					defaultColumnsForGrid = columnList.ToArray();
				}
				return defaultColumnsForGrid;
			}
		}
	}
}
