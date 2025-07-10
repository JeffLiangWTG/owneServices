using CargoWise.Types;

namespace Enterprise.Billing.Integration
{
	public interface ISourceInfo
	{
		ZString DataSource { get; }
		ZGuid EDIMessagePK { get; }
		ZGuid EDIInterchangePK { get; }
		ZString InterfaceName { get; }
		ZString SenderId { get; }
		ZString FileName { get; }
		bool ShouldSuspendValidation { get; }
	}
}
