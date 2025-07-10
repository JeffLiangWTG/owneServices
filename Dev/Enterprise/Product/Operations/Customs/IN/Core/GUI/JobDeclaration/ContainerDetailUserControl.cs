namespace Enterprise.Customs.IN.GUI;

public partial class ContainerDetailUserControl : Freight.GUI.ContainersUserControl
{
	public ContainerDetailUserControl()
	{
		InitializeComponent();
	}

	public void SetSealTypeVisibility(bool isVisible)
	{
		SealTypeDropEdit.Visible = isVisible;
	}
}
