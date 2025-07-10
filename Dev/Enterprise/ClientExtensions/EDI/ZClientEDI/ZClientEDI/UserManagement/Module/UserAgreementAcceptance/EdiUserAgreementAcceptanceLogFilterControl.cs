using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.UserManagement.Module
{
	public partial class EdiUserAgreementAcceptanceLogFilterControl : ZFilterStripControl
	{
		public EdiUserAgreementAcceptanceLogFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
		}
	}
}
