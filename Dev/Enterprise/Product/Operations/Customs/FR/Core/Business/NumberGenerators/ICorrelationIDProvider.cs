using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business
{
	public interface ICorrelationIDProvider : IBusiness
	{
		ZString CorrelationID { get; set; }
		ZPropertyInfo CorrelationIDInfo { get; }
		ZString CorrelationIDPrefix { get; }
	}
}
