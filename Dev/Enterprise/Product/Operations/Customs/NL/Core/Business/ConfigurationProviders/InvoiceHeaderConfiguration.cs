using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.NL.Business.Declaration;

namespace Enterprise.Customs.NL.Business;

public class InvoiceHeaderConfiguration : EU.Business.InvoiceHeaderConfiguration
{
	protected override ZBool ValueIndicatorsSupportCore(BusinessObject businessObject) => ((JobDeclaration)businessObject).IsImport;

	protected override ZBool AgreedPlaceCodeSupportCore(BusinessObject businessObject) => true;

	protected override ZBool PreviousDocumentsSupportCore(EU.Business.Declaration.JobDeclaration declaration) => false;

	protected override ZBool ExportCostCalculationsTotalsUISupportCore => true;
}
