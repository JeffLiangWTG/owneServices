using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class ModeOfRepresentationCalculator
	{
		public ModeOfRepresentationCalculator(NctsHeader nctsHeader)
		{
			this.nctsHeader = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		}

		public ZString GetModeOfRepresentation() => GetModeOfRepresentationCore();

		protected virtual ZString GetModeOfRepresentationCore()
		{
			ZString result = NctsConstants.ModeOfRepresentation.Codes.Blank;

			if (nctsHeader.Declarant?.Organisation is OrgHeader declarantOrganisation && nctsHeader.Principal?.Organisation is OrgHeader principalOrganisation)
			{
				result = declarantOrganisation.PK.Equals(principalOrganisation.PK) ? NctsConstants.ModeOfRepresentation.Codes.One : NctsConstants.ModeOfRepresentation.Codes.Two;
			}

			return result;
		}

		protected readonly NctsHeader nctsHeader;
	}
}
