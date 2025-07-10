using CargoWise.Common;
using CargoWise.Customs.BE.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.BE.NCTS.Business
{
	public class PlaceOfLoadingProvider : IPlace
	{
		readonly NctsCommonMovementHeader moveHeader;

		public PlaceOfLoadingProvider(NctsCommonMovementHeader moveHeader)
		{
			this.moveHeader = Argument.NotNull(moveHeader, nameof(moveHeader));
		}

		public string UnLocode => CodeIs2OrLessCharacters ? null : moveHeader.BM_PortOfPresentationCode;

		public string Country => CodeIs2OrLessCharacters ? moveHeader.BM_PortOfPresentationCode : null;

		public string Location => CodeIs2OrLessCharacters ? moveHeader.BM_PlaceOfLoading : null;

		bool CodeIs2OrLessCharacters => moveHeader.BM_PortOfPresentationCode.Length <= 2;
	}
}
