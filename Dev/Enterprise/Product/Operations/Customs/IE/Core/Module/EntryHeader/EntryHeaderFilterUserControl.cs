using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IE.Module
{
	public partial class EntryHeaderFilterUserControl : EU.Module.EntryHeaderFilterUserControl
	{
		public EntryHeaderFilterUserControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}

		protected override void InitialiseGridCore()
		{
			base.InitialiseGridCore();

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = JobDeclarationFilterStripControl.ResourceStrings.CustomsDocStatus,
				ColumnName = EU.Business.Declaration.CusEntryHeader.Schema.CustomsDocStatus,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80)
			});

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				CaptionResourceString = JobDeclarationFilterStripControl.ResourceStrings.CustomsDocStatusDesc,
				ColumnName = EU.Business.Declaration.CusEntryHeader.Schema.CustomsDocStatusDesc,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			});
		}
	}
}
