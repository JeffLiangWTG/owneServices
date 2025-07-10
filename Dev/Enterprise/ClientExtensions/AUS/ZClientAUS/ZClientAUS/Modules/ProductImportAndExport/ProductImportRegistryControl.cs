using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AUS.Modules
{
	public partial class ProductImportRegistryControl : ZFilterStripControl
	{
		public ProductImportRegistryControl(ClientAUSProductImportRegistryCollection gridCollection, ProductImportRegistryBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
