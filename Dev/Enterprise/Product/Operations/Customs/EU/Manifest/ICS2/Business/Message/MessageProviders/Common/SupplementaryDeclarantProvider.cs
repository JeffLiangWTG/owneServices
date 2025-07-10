using CargoWise.Common;
using CargoWise.Customs.EU.MessageContracts.ICS2;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class SupplementaryDeclarantProvider : IIdentifierTypePair
	{
		SupplementaryDeclarantProvider(SupplementaryDeclarant supplementaryDeclarant)
		{
			this.supplementaryDeclarant = Argument.NotNull(supplementaryDeclarant, nameof(supplementaryDeclarant));
		}

		public static SupplementaryDeclarantProvider NewOrNull(SupplementaryDeclarant supplementaryDeclarant)
		{
			return supplementaryDeclarant is null ? null : new(supplementaryDeclarant);
		}

		readonly SupplementaryDeclarant supplementaryDeclarant;

		public string Identifier => supplementaryDeclarant.CY_Data;

		public string Type => supplementaryDeclarant.CY_Code;
	}
}
