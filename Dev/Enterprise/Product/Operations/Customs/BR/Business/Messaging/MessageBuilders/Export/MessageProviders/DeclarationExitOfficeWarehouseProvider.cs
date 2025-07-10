using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationExitOfficeWarehouseProvider : IDeclarationOfficeWarehouse
	{
		public DeclarationExitOfficeWarehouseProvider(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public string ID => (declaration.BoardingOfficeIsCustomsEnclosure ? declaration.BoardingEnclosureCode : declaration.ClearanceLocalInvolvedParty.E2_GovRegNum).KeepNumericCharacters();

		public decimal LatitudeMeasure => decimal.Zero;

		public decimal LongitudeMeasure => decimal.Zero;

		public string AddressLine
		{
			get
			{
				var result = string.Empty;
				if (!declaration.BoardingOfficeIsCustomsEnclosure)
				{
					result = declaration.BoardingLocalAddress.E2_Address1 + " " + declaration.BoardingLocalAddress.E2_Address2;
				}
				return result;
			}
		}

		public bool InCustomsEnclosure => declaration.BoardingOfficeIsCustomsEnclosure;

		public bool HomeDispatch => declaration.ClearanceOfficeIsHomeDispatch;
	}
}
