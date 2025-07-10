using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CA.Business;

public class CADeclarationJobDatesProvider : DeclarationJobDatesProvider
{
	public CADeclarationJobDatesProvider(BaseJobDeclaration declaration)
		: base(declaration) { }

	protected override ZDateTime GetArrivalDateCore()
	{
		return ((JobDeclaration)Parent).CA_K84AccountingDate;
	}

	protected override ZDateTime GetDepartureDateCore()
	{
		return ((JobDeclaration)Parent).CA_K84AccountingDate;
	}
}
