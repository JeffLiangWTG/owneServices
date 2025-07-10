using CargoWise.EntityFramework;

namespace Enterprise.Customs.JP.Common;

public interface INACCSMessageImportSupporter
{
	bool IsValidParent(IBusiness parent);
}
