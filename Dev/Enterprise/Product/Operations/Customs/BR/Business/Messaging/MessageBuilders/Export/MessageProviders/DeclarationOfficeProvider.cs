using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationOfficeProvider : IDeclarationOffice
	{
		public DeclarationOfficeProvider(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public string ID => declaration.JE_CustomsOffice;

		public IDeclarationOfficeWarehouse Warehouse => fWarehouse ?? (fWarehouse = new DeclarationOfficeWarehouseProvider(declaration));
		IDeclarationOfficeWarehouse fWarehouse;
	}
}
