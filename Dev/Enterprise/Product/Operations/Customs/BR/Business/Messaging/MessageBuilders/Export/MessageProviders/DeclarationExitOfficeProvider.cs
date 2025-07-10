using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationExitOfficeProvider : IDeclarationOffice
	{
		public DeclarationExitOfficeProvider(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public string ID => declaration.BoardingOfficeCode;

		public IDeclarationOfficeWarehouse Warehouse => fWarehouse ?? (fWarehouse = new DeclarationExitOfficeWarehouseProvider(declaration));
		IDeclarationOfficeWarehouse fWarehouse;
	}
}
