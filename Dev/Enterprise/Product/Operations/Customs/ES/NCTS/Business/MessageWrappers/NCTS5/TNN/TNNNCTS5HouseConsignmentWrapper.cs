using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TNNNCTS5HouseConsignmentWrapper : NCTS5CommonHouseConsignmentDepartureAndAmendmentAndTNNWrapper, ITNNNCTSHouseConsignment
	{
		public TNNNCTS5HouseConsignmentWrapper(NctsBill houseConsignment, ZBool shouldDeclareCountryOfDestinationInItem) : base(houseConsignment)
		{
			this.shouldDeclareCountryOfDestinationInItem = shouldDeclareCountryOfDestinationInItem;
			shouldDeclareDeclarationTypeInItem = houseConsignment.Header.MovementHeader.BM_InBondEntryType == NctsPhase5DeclarationTypeList.Codes.T;
		}
		readonly ZBool shouldDeclareCountryOfDestinationInItem;
		readonly ZBool shouldDeclareDeclarationTypeInItem;

		protected override ZString ReferenceNumberUCRCore => houseConsignment.B0_ReferenceID;

		public IReadOnlyCollection<ITNNNCTSConsignmentItem> ConsignmentItem => consignmentItem ?? (consignmentItem = houseConsignment.GoodsItems.Cast<NctsDepartureCargoDesc>().Select(x => new TNNNCTS5ConsignmentItemWrapper(x, shouldDeclareDeclarationTypeInItem, shouldDeclareCountryOfDestinationInItem, shouldDeclareReferenceNumberUCRInItem: true)).ToList().AsReadOnly());
		IReadOnlyCollection<TNNNCTS5ConsignmentItemWrapper> consignmentItem;
	}
}
