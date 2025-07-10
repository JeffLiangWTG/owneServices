using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business.ServiceTask
{
	public class NctsGoodsItemResponseData
	{
		readonly NctsIE29CusdecResponseData parentResponseData;

		public NctsGoodsItemResponseData()
		{
			ContainerNumbers = new List<ZString>();
			SgiCodes = new List<SgiCodesResponseData>();
			SpecialMentions = new List<SpecialMentionResponseData>();
			Packages = new List<PackageResponseData>();
			SupportingDocuments = new List<SupportingDocumentResponseData>();
			PreviousDocuments = new List<PreviousDocumentResponseData>();
			GuaranteeLiabilityAmounts = new List<GuaranteeLiabilityAmountResponseData>();
		}

		public NctsGoodsItemResponseData(NctsIE29CusdecResponseData ie29ResponseData) : this()
		{
			parentResponseData = ie29ResponseData;
		}

		public NctsDepartureCargoDesc NctsCommonGoodsItem
		{
			get
			{
				NctsDepartureCargoDesc result = null;
				if (parentResponseData != null && !parentResponseData.LocalReferenceNumber.IsEmpty && ItemNumber.IsNumbersOnlyOrEmpty)
				{
					var parentBizOs = parentResponseData.factory.Load<NctsHeader>(new ZQuery(CusInBondHeaderSchema.BH_JobReference, parentResponseData.LocalReferenceNumber));
					if (parentBizOs.Length == 1)
					{
						var parentBizO = parentBizOs[0];
						var itemNumber = int.Parse(ItemNumber); // Starts at 1
						var goodsItems = parentBizO.IsPhase5 ? parentBizO.Bills.SelectMany(x => x.GoodsItems).ToArray() : parentBizO.MovementHeader.GoodsItems.ToArray();
						if (goodsItems.Length >= itemNumber)
						{
							result = goodsItems[itemNumber - 1];
						}
					}
				}
				return result;
			}
		}

		public ZString ItemNumber { get; set; }
		public ZString CommodityCode { get; set; }
		public ZString DeclarationType { get; set; }
		public ZString DescriptionOfGoods { get; set; }
		public ZString GrossMassInKilograms { get; set; }
		public ZString NetMassInKilograms { get; set; }
		public ZString CountryOfDispatch { get; set; }
		public ZString CountryOfDestination { get; set; }
		public ZString TransportChargesMoP { get; set; }
		public ZString CommercialReferenceNumber { get; set; }
		public AddressResponseData SecurityConsignor { get; set; }
		public AddressResponseData SecurityConsignee { get; set; }
		public AddressResponseData Consignee { get; set; }
		public AddressResponseData Consignor { get; set; }

		public ZString UNDangerousGoodsCode { get; set; }
		public List<ZString> ContainerNumbers { get; set; }
		public List<SgiCodesResponseData> SgiCodes { get; set; }
		public List<SpecialMentionResponseData> SpecialMentions { get; set; }
		public List<GuaranteeLiabilityAmountResponseData> GuaranteeLiabilityAmounts { get; set; }
		public List<PackageResponseData> Packages { get; set; }
		public List<SupportingDocumentResponseData> SupportingDocuments { get; set; }
		public List<PreviousDocumentResponseData> PreviousDocuments { get; set; }

		#region Phase 5
		public ZString ReferenceNumberUCR { get; set; }
		#endregion
	}
}
