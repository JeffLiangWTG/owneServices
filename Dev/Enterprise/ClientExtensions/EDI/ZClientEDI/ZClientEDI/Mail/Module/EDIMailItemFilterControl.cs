using CargoWise.EntityFramework;
using Enterprise.MailManager.Module;

namespace Enterprise.Client.EDI.Mail.Module
{
	public partial class EDIMailItemFilterControl : MailItemFilterControl
	{
		public EDIMailItemFilterControl(IBusinessObjectCollection gridCollection, MailItemFilterBusinessObject filterBizO)
			: base(gridCollection, filterBizO)
		{
			InitializeComponent();
		}
	}
}
