using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.GUI
{
	public partial class AUCOLSUserControl : ZUserControl
	{
		public AUCOLSUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var declaration = (JobDeclaration)dataSource;
			if (declaration != null)
			{
				base.SetDataBinding(((JobDeclaration)dataSource).QuarantineCOLSHeader, "");
			}
		}
	}
}
