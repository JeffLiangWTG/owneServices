using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.JP.Manifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	public partial class PortOfLoadingUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public PortOfLoadingUserControl()
		{
			InitializeComponent();
			Extensions = new DefaultControlExtensionCollection(this);
		}

		public Control Host => this;

		public IControlExtensionCollection Extensions { get; }

		public string ResourceStringBindingMember => nameof(AsycudaManifestHeader.AMA_RL_NKPortOfLoading);

		public const string IsVisibleForBindingString = "IsVisibleForBinding";

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			PortOfLoadingTextBox.DataBindings.RemoveBinding(IsVisibleForBindingString);
			base.SetDataBinding(dataSource, dataMember);
			if (dataSource is AsycudaManifestHeader header)
			{
				PortOfLoadingTextBox.DataBindings.Add(new KBinding(IsVisibleForBindingString, BindingSource.DataSource, nameof(header.IsAir), false, DataSourceUpdateMode.Never));
			}
		}
	}
}
