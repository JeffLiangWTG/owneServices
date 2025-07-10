using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AUS.Modules
{
	public partial class OriginPreferenceMappingFilterControl : ZFilterStripControl
	{
		public OriginPreferenceMappingFilterControl(ClientAUSOriginPreferenceMappingCollection gridCollection, OriginPreferenceMappingBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
