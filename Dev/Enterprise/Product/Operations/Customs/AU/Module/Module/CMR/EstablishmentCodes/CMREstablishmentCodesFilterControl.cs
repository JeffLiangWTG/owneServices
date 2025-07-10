using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Module
{
	public partial class CMREstablishmentCodesFilterControl : ZFilterStripControl
	{
		public CMREstablishmentCodesFilterControl(IBusinessObjectCollection gridCollection, CMREstablishmentCodesFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
