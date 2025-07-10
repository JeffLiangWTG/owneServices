using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalActionInfo
	{
		ZString ActionType { get; }
		ZString PurposeCode { get; }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		RecipientRoleDetail[] RecipientRoleDetails { get; }
		IOrgHeader RecipientOrganization { get; }
		ZString TriggerEventCode { get; }
		ZString TriggerReference { get; }
		TriggerType TriggerType { get; }
		ZString TriggerDescription { get; }
		ZDateTimeOffset TriggerScheduledDate { get; }
		ZDateTimeOffset TriggerActualDate { get; }
		ZInt TriggerCount { get; }
		IStmALog TriggeringEvent { get; }
		INotifications Notifications { get; set; }
		BusinessObjectFactory FactoryForProcessing { get; }
		BusinessObject ParentBO { get; }

		void PopulateRecipientRoleDetails(ZString recipientTypeCode, ZString recipientServiceCode);
	}
}
