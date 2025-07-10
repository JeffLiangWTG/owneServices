using CargoWise.Common;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class DestinationWrapper : IDestination
	{
		DestinationWrapper(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration declaration;

		public static DestinationWrapper New(JobDeclaration declaration) => declaration == null || declaration.JE_GoodsDestination.IsEmpty ? null : new DestinationWrapper(declaration);

		public string CcQualifier => declaration.JE_CustomsOffice.StartsWith(Core.Constants.CountryCodes.France) ? ZString.Empty : Core.Constants.CountryCodes.France;

		public string CountryOfDestination => countryOfDestination ?? (countryOfDestination = declaration.JE_GoodsDestination);
		string countryOfDestination;

		public string RegionOfDestination => regionOfDestination ?? (regionOfDestination = CountryOfDestination == Core.Constants.CountryCodes.France ? (declaration.ImporterDocumentaryAddress?.Address?.OA_PostCode.Left(2) ?? ZString.Empty) : ZString.Empty);
		string regionOfDestination;
	}
}
