using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business.Declaration;

sealed class JobDeclarationShipmentIncoTermCleanUpStrategy : ICleanUpStrategy
{
	public JobDeclarationShipmentIncoTermCleanUpStrategy(JobDeclaration declaration)
	{
		this.declaration = Argument.NotNull(declaration, nameof(declaration));
	}

	void ICleanUpStrategy.CleanUp()
	{
		if (declaration.IsUcc6ExportAndIsShipmentIncoTermOther)
		{
			declaration.EUD_AgreedPlaceCode = ZString.Empty;
			declaration.JE_ShipmentIncoTermPlace = ZString.Empty;
			declaration.ZG_AgreedPlaceCode = ZString.Empty;
			return;
		}

		declaration.ZG_AdditionalDeliveryTerms = ZString.Empty;
	}

	readonly JobDeclaration declaration;
}
