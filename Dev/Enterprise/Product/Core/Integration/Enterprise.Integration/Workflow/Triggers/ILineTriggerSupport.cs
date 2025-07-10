using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ILineTriggerSupport : IBaseTrigger
	{
		ZString LineTriggerType { get; set; }
	}
}
