using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FI.Business;

sealed class DeclarationConfiguration : EU.Business.DeclarationConfiguration
{
	protected override ZBool IsUCC6Core(BusinessObject businessObject) => businessObject is JobDeclaration jobDeclaration && (jobDeclaration.IsExport || jobDeclaration.IsImport);
}
