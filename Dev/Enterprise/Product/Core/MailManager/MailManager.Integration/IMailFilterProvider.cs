using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MailManager
{
	public interface IMailFilter
	{
		string Code { get; }
		bool CanProcess(IMailItem item);
		bool IsEnabled { get; }
		ZQuery LoadQuery(int limit = -1);
		IMailItem[] Load(BusinessObjectFactory factory, int n);
	}

	public interface IMailFilterProvider
	{
		bool TryGetFilter(string code, out IMailFilter filter);
		IEnumerable<IMailFilter> GetFilters();
	}

	public interface IMailItem
	{
		ZString MI_Subject { get; }
		ZDateTime MI_ReceivedDateTime { get; }
		ZString MI_Application { get; set; }
		ZString MI_Status { get; set; }
	}
}
