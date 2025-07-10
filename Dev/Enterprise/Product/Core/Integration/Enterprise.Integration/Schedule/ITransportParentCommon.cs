using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Integration.Schedule
{
	public interface ITransportParentCommon
	{
		ZGuid PK { get; }
		ZString TypeCode { get; }

		BusinessObjectFactory Factory { get; }
		void MarkAsNeedingValidation();
	}
}
