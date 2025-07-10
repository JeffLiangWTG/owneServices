using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Export.Outgoing;

namespace Enterprise.Customs.BR.Business.Export
{
	public class DeclarationOfficeWarehouseProvider : IDeclarationOfficeWarehouse
	{
		public DeclarationOfficeWarehouseProvider(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public string ID => (declaration.ClearanceOfficeIsCustomsEnclosure ? declaration.JE_LocationOfGoods : declaration.ClearanceLocalInvolvedParty.E2_GovRegNum).KeepNumericCharacters();

		public decimal LatitudeMeasure => declaration.ClearanceLocalInvolvedParty.E2_Latitude;

		public decimal LongitudeMeasure => declaration.ClearanceLocalInvolvedParty.E2_Longitude;

		public string AddressLine
		{
			get
			{
				if (!declaration.ClearanceLocalInvolvedParty.E2_Address1.IsEmpty)
				{
					return declaration.ClearanceLocalInvolvedParty.E2_Address1 + " " + declaration.ClearanceLocalInvolvedParty.E2_Address2;
				}
				return string.Empty;
			}
		}

		public bool InCustomsEnclosure => declaration.ClearanceOfficeIsCustomsEnclosure;

		public bool HomeDispatch => declaration.ClearanceOfficeIsHomeDispatch;
	}
}
