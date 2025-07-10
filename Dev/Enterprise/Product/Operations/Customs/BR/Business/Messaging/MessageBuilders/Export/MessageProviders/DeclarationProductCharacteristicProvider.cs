using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	class DeclarationProductCharacteristicProvider : IDeclarationProductCharacteristic
	{
		public DeclarationProductCharacteristicProvider(AttributeCusCodeData attributes)
		{
			this.attributes = Argument.NotNull(attributes, nameof(attributes));
		}

		readonly AttributeCusCodeData attributes;

		public string Type => attributes.CY_Code;
		public string Description => attributes.CY_Data;
	}
}
