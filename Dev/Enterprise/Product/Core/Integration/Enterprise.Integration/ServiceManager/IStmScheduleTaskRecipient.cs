using CargoWise.Types;

namespace Enterprise.Integration.ServiceManager
{
	public interface IStmScheduleTaskRecipient
	{
		ZGuid S6_GG { get; set; }
		ZGuid S6_S5 { get; set; }
		ZGuid S6_OC { get; set; }
		ZString S6_DeliveryMethod { get; set; }
		ZString S6_DeliveryToType { get; set; }
		ZString S6_GS_NKRecipient { get; set; }
		ZString S6_FaxOverride { get; set; }
		ZString S6_EmailToRecipientsAsString { get; set; }
		ZString S6_EmailFromAddress { get; set; }
	}
}
