using CargoWise.Types;

namespace Enterprise.BufferManagement.Integration
{
	public interface IProcessHeaderLinkRow
	{
		ZString FP_LinkType { get; set; }
		ZGuid FP_FH_HeaderFrom { get; set; }
		ZGuid FP_FH_HeaderTo { get; set; }
		ZDecimal FP_TimeDelayFactor { get; set; }
		ZInt FP_TimeDelayMinutes { get; set; }
		ZBool FP_SynchroniseBufferPenetration { get; set; }
		ZBool FP_IsActive { get; set; }
		ZDateTime FP_SystemCreateTimeUtc { get; set; }
		ZString FP_SystemCreateUser { get; set; }
		ZDateTime FP_SystemLastEditTimeUtc { get; set; }
		ZString FP_SystemLastEditUser { get; set; }
	}
}
