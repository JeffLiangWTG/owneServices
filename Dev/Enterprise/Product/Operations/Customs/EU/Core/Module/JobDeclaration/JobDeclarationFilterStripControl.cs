using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module
{
	public partial class JobDeclarationFilterStripControl : Customs.Module.JobDeclarationFilterStripControl
	{
		[Obsolete("Use the constructor that takes a module, collection and filter strip, this constructor is just for the designer", true)]
		public JobDeclarationFilterStripControl()
		{
			InitializeComponent();
		}

		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(module, gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
			InitializeAdditionalGridColumns();
		}

		public bool SupportsExitControl => SupportsExitControlCore;
		protected virtual bool SupportsExitControlCore => false;

		protected virtual void InitializeAdditionalGridColumns()
		{
			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("0AA4A40E-0B84-4EED-B0E8-26C6079A7888", "Pack Types"),
				ColumnName = JobDeclaration.Schema.PackTypes,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("ce79f364-d512-4d44-af34-ad4bb8d6d1c1", "Location Of Goods"),
				ColumnName = JobDeclaration.Schema.JE_LocationOfGoods,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(114)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("08b19bbd-9afa-458e-9788-0ed094fb6785", "Shipment Entry Type"),
				ColumnName = JobDeclaration.Schema.ZG_ShipmentType,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("c963b472-da0f-4c86-9da0-a08a9eb09eff", "CT Status"),
				ColumnName = JobDeclaration.Schema.ZG_CTStatusID,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZDateEditColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("294ef32d-3b2a-4a67-b7f5-ff4494e887ee", "Clearance Date"),
				ColumnName = JobDeclaration.Schema.JE_Calc_DateOfClearance,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97)
			});

			grid.ColumnStyles.Add(new ZGuidFindBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("5DFA0DAE-3DE1-423A-9B0C-D7E278339AD5", "Carrier"),
				ColumnName = JobDeclaration.Schema.JE_OH_ShippingLine,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(71)
			});

			if (SupportsExitControl)
			{
				grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					Caption = null,
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("AEDC431F-76A8-4BE1-865D-C89EB485087E", "Exit Presentation Status"),
					ColumnName = JobDeclaration.Schema.ExitPresentationStatus,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128)
				});

				grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					Caption = null,
					CaptionResourceString = Enterprise.Customs.EU.Module.Res.GetData("{06D1AB89-3152-4298-9398-C6ECF3DD8EB8}", "Exit Presentation Status Desc.", "Exit Presentation Status Desc.", "Exit Presentation Status Description"),
					ColumnName = JobDeclaration.Schema.ExitPresentationStatusDesc,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150)
				});
			}
		}

		protected override ZFilterStrip NewZFilterStrip() => new JobDeclarationModuleStrip();

		protected override bool ShouldAddDeclarantFieldsToGrid => true;
	}
}
