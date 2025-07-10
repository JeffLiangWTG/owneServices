using CargoWise.Common;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.GB.Business.NCTS
{
	public class PlaceOfLoadingProvider : IPlace
	{
		public PlaceOfLoadingProvider(NctsCommonMovementHeader moveHeader)
		{
			this.moveHeader = Argument.NotNull(moveHeader, nameof(moveHeader));
		}

		public string UnLocode => CodeIs2OrLessCharacters ? null : moveHeader.BM_PortOfPresentationCode;

		public string Country => CodeIs2OrLessCharacters ? moveHeader.BM_PortOfPresentationCode : null;

		public string Location => CodeIs2OrLessCharacters ? moveHeader.BM_PlaceOfLoading : null;

		bool CodeIs2OrLessCharacters => moveHeader.BM_PortOfPresentationCode.Length <= 2;

		readonly NctsCommonMovementHeader moveHeader;
	}
}
