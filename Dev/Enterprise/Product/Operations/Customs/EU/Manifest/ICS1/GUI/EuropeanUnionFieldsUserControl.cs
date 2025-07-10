using System.ComponentModel;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Customs.EU.Manifest.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class EuropeanUnionFieldsUserControl : ZUserControl
	{
		public EuropeanUnionFieldsUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			var newDataSource = dataSource as AsycudaManifestHeader;
			base.SetDataBinding(newDataSource, "");
		}
	}
}
