using CargoWise.Types;

namespace Enterprise.Integration
{
	public interface ICustomsStatusReasonHelper
	{
		ZString ConvertCustomsStatusReasonCodeToGenericCustomsStatus(ZString reasonCode);
		ZString GetCustomsStatusReasonDescription(ZString reasonCode);
	}
}
