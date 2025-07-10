using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class RelatedDeclarationsUserControl : BaseRelatedDeclarationsUserControl
	{
		public RelatedDeclarationsUserControl()
		{
			BindingSource.DataSourceType = typeof(JobDeclaration);
			InitializeComponent();
		}
	}
}
