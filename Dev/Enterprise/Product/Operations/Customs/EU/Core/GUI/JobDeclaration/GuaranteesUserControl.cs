using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	public partial class GuaranteesUserControl : ZUserControl
	{
		public GuaranteesUserControl()
		{
			InitializeComponent();
			AddAndRemoveColumns();
			GridColumnStyleDecider();
		}

		protected JobDeclaration Declaration => CurrentDataItem as JobDeclaration;

		protected virtual void GridColumnStyleDecider()
		{
		}

		protected virtual void AddAndRemoveColumns()
		{
		}
	}
}
