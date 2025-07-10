using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.GUI
{
	public partial class ContainersUserControl : Freight.GUI.ContainersUserControl
	{
		public ContainersUserControl()
		{
			InitializeComponent();
		}

		protected JobDeclaration JobDeclaration => ((ICusContainerCollection<CusContainer>)DataSource).Declaration as JobDeclaration;

		protected override void OnAfterFirstBinding(EventArgs e)
		{
			base.OnAfterFirstBinding(e);
			this.IsControlCheckBox.Visible = JobDeclaration?.ContainerControlCheckboxVisible ?? false;
			this.IsUnloadedCheckBox.Visible = JobDeclaration?.ContainerUnloadedCheckboxVisible ?? false;
		}
	}
}
