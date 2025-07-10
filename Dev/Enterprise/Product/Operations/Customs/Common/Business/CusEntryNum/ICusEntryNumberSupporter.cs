using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public interface ICusEntryNumberSupporter
	{
		void CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify);
		void UpdateCustomsEntryIssueDate(ZString numberType, ZString countryCode, ZDateTime dateTime);
	}
}
