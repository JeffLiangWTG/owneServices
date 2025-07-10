using CargoWise.ComponentModel;
using CargoWise.EntityFramework;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public class MultiCompaniesGLJournalDataAdapter : GLJournalDataAdapter
	{
		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{
			return;
		}
	}
}
