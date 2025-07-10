using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.DE.Module
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

			grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
			{
				Caption = null,
				CaptionResourceString = Enterprise.Customs.DE.Module.Res.GetData("CF45F18D-A394-457B-A4D6-9F164556E3CF", "Pres. End Date", "Present. End Date", "Presentation End Date", ""),
				ColumnName = JobDeclaration.Schema.ZG_PresentationEndDate,
				Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110)
			});
		}

		protected override bool SupportsExitControlCore => true;
	}
}
