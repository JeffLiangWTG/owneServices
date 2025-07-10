using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.BE.GUI;

public partial class ContainerUserControl : CustomsCusContainersWithTrackingAndAdditionalSealUserControl
{
	public ContainerUserControl()
	{
		InitializeComponent();
		CusContainersBoundGrid.InnerGrid.SetColumnVisible(true, CusContainerSchema.CO_SecondSeal.Name);
	}

	protected override Freight.GUI.ContainersUserControl GetContainerTrackingUserControl()
	{
		return new ContainerDetailUserControl();
	}

	public new JobDeclaration JobDeclaration
	{
		get => base.JobDeclaration as JobDeclaration;
		set => base.JobDeclaration = value;
	}
}
