using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Common
{
	public interface IAdditionalReferenceNumberSupporter
	{
		void CreateOrUpdate(ZString numberType, ZString countryCode, ZString value, INotifications notify);
		bool IncludeSpecialCustomsInstructionsItems { get; }
		void OnEntryNumChanged(CusEntryNumber additionalReferenceNumber);
		CusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers { get; }
		void AdditionalEntryNumberValidation(ZPropertyInfo info, ZString type, ZString number);
	}
}
