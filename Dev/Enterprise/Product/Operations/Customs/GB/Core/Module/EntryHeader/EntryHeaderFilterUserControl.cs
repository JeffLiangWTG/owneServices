using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Module
{
	public class EntryHeaderFilterUserControl : EU.Module.EntryHeaderFilterUserControl
	{
		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
		}

		#region Schema

		public new static class Schema
		{
			public const string ExitActualOffice = "CH_ExitActualOffice";
			public const string ExitDate = "CH_ExitDate";
			public const string InventoryConsignmentReference = "CH_MasterUCR";
		}

		#endregion

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();
			grid.ColumnStyles.AddRange(new Core.Forms.ZGridColumnInfo[]
			{
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("2A4C1A45-3485-433B-A863-C00A9AD4D0AB", "Actual Office Of Exit"),
					ColumnName = Schema.ExitActualOffice,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("43052E66-7082-4EB6-839B-B38F9C0758A9", "Date Of Exit"),
					ColumnName = Schema.ExitDate,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100)
				},
				new ZArchitecture.ZTextBoxColumnStyleInfo
				{
					CaptionResourceString = Enterprise.Customs.GB.Module.Res.GetData("AFFC12B1-C7AC-4621-9FD2-22D49F7AC622", "MUCR", "Inventory Consignment Reference (MUCR)"),
					CharacterCasing = System.Windows.Forms.CharacterCasing.Normal,
					ColumnName = Schema.InventoryConsignmentReference,
					Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140)
				},
			});
		}
	}
}
