using CargoWise.EntityFramework;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.IT.Module;

public partial class JobDeclarationFilterStripControl : EU.Module.JobDeclarationFilterStripControl
{
	public JobDeclarationFilterStripControl(JobDeclarationModule module, IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
		: base(module, gridCollection, filterBusinessObject)
	{
		InitializeComponent();
	}

	protected override void InitializeAdditionalGridColumns()
	{
		base.InitializeAdditionalGridColumns();

		Grid.ColumnStyles.Add(new ZArchitecture.ZTextBoxColumnStyleInfo
		{
			CaptionResourceString = Enterprise.Customs.IT.Module.Res.GetData("08E57CE7-795D-4FD3-BC91-DB591C899DD0", "Message Version"),
			ColumnName = JobDeclaration.Schema.MessageVersion,
			Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100),
			IsVisible = false
		});
	}
}
