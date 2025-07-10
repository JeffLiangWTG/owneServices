using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Module
{
	public partial class JobDeclarationFilterStripControl : EU.Module.JobDeclarationFilterStripControl
	{
		public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterStripBusinessObject)
			: base(module, gridCollection, filterStripBusinessObject)
		{
			InitializeComponent();
		}

		protected override void InitializeAdditionalGridColumns()
		{
			base.InitializeAdditionalGridColumns();

			var zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("e3bab78f-582b-48d1-9892-75d72ae73020", "Declaration Type"),
				ColumnName = "JE_DeclarationType",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(107)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);

			var zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("5A903C25-199A-4127-8893-6C0C18968B7E", "Shed"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = "SubLocation",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(47)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);

			var zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("3F2A24DF-8D52-4029-B362-1C94ED1C175E", "ICS", "Import Clearance Status"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = "ZG_ImportClearanceStatusICS",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(41)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);

			var zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("BCA90BE2-93AE-450E-89FF-DDC769F2D119", "ICS Desc.", "Import Clearance Status Description"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = "ZG_ImportClearanceStatusICSDescription",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);

			var zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("F78D8249-2758-4227-BC09-AFABB8311FE4", "Route", "Route to Clearance"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = "JE_GBRouteOfEntry",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);

			var zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("9449BF22-A0E3-4C90-BF7B-3C84716049B3", "Route Desc.", "Route to Clearance Description"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = "JE_GBRouteOfEntryDescription",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);

			var zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("A2DC5D5A-70D3-4F12-85A4-F6341FC15C20", "Location (short)", "Location (short) of Goods for CDS"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = "JE_SubLocationOfGoods",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);

			var zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("B9DC30CA-CC1C-4A14-B3C2-267743A67274", "Location (formatted)", "Location (formatted) of Goods for CDS"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = "JE_LocationOtherInformation",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);

			var zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("CE55A610-04AD-43E6-9EA2-75781E633F2E", "Location (qualifier)", "Location (qualifier) of Goods for CDS"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Upper,
				ColumnName = "JE_LocationQualifier",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);

			var zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("AFFC12B1-C7AC-4621-9FD2-22D49F7AC622", "MUCR", "Inventory Consignment Reference (MUCR)"),
				CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
				ColumnName = "JE_MasterUCR",
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
			};
			grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
		}
	}
}
