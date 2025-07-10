using CargoWise.EntityFramework;

namespace Enterprise.Customs.IT.Business;

public interface ITopLevelBusinessObjectProvider
{
	BusinessObject TopLevelBusinessObject { get; }
}
