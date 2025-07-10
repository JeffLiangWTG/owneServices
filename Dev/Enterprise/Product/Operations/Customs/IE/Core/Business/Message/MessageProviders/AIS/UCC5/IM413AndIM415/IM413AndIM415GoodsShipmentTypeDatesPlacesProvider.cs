using System;
using CargoWise.Customs.IE.MessageContracts.AIS.UCC5.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class IM413AndIM415GoodsShipmentTypeDatesPlacesProvider : IGoodsShipmentTypeDatesPlaces
	{
		public IM413AndIM415GoodsShipmentTypeDatesPlacesProvider(EntryHeaderWrapper entryHeaderWrapper)
		{
			declaration = entryHeaderWrapper.Declaration;
			instruction = entryHeaderWrapper.Instruction;
		}
		protected readonly JobDeclaration declaration;
		readonly CusEntryInstruction instruction;

		public string CountryDestination => declaration.JE_GoodsDestination;

		public string RegionDestination => null;

		public string CountryDispatch => declaration.JE_GoodsOrigin;

		public IGoodsLocation LocationGoods => CachedValueHelper.GetValue(ref goodsLocation, () => new GoodsLocationProvider(instruction.GoodsLocation));
		CachedValue<IGoodsLocation> goodsLocation;

		public DateTime? AcceptanceDate => instruction.CEI_SubStyle == EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationForCodeCOrCodeF || instruction.CEI_SubStyle == EU.Business.EntrySubStyleList.Codes.SupplementaryDeclarationPeriodic ? DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(instruction.CEI_DateForDuty, true) : null;
	}
}
