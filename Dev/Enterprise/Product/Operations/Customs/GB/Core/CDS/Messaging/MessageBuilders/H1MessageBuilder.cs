using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class H1MessageBuilder : MessageBuilder
	{
		public H1MessageBuilder(CusEntryHeader cusEntryHeader, EU.Business.ErrorCollector errorCollector, string functionCode)
			: base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateGoodsShipment()
		{
			base.PopulateGoodsShipment();
			PopulateSeller();
			PopulateBuyer();
		}

		protected virtual void PopulateSeller()
		{
			var sellerObj = Wrapper.GoodsShipment.Seller;
			var seller = sellerObj == null ? new DeclarationGoodsShipmentSeller() : new DeclarationGoodsShipmentSeller
			{
				ID = new SellerIdentificationIDType { Value = sellerObj.ID },
				Name = new SellerNameTextType { Value = sellerObj.Name },
				Address = GetIOrgAddress<DeclarationGoodsShipmentSellerAddress>(sellerObj)
			};
			decShipment.Seller = seller;
		}

		protected virtual void PopulateBuyer()
		{
			var buyerObj = Wrapper.GoodsShipment.Buyer;
			var buyer = buyerObj == null ? new DeclarationGoodsShipmentBuyer() : new DeclarationGoodsShipmentBuyer
			{
				ID = new BuyerIdentificationIDType { Value = buyerObj.ID },
				Name = new BuyerNameTextType { Value = buyerObj.Name },
				Address = GetIOrgAddress<DeclarationGoodsShipmentBuyerAddress>(buyerObj)
			};
			decShipment.Buyer = buyer;
		}

		protected override void PopulateGovernmentAgencyGoodsItem(IGovernmentAgencyGoodsItem goodsItemWrapper, int index)
		{
			base.PopulateGovernmentAgencyGoodsItem(goodsItemWrapper, index);
			PopulateSeller(goodsItemWrapper);
			PopulateBuyer(goodsItemWrapper);
		}

		protected virtual void PopulateSeller(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giSeller = goodsItem.Seller;
			if (giSeller != null)
			{
				decGoodsItem.Seller = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemSeller
				{
					ID = new SellerIdentificationIDType { Value = giSeller.ID },
					Name = new SellerNameTextType { Value = giSeller.Name },
					Address = GetIOrgAddress<DeclarationGoodsShipmentGovernmentAgencyGoodsItemSellerAddress>(giSeller)
				};
			}
		}

		protected virtual void PopulateBuyer(IGovernmentAgencyGoodsItem goodsItem)
		{
			var giBuyer = goodsItem.Buyer;
			if (giBuyer != null)
			{
				decGoodsItem.Buyer = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemBuyer
				{
					ID = new BuyerIdentificationIDType { Value = giBuyer.ID },
					Name = new BuyerNameTextType { Value = giBuyer.Name },
					Address = GetIOrgAddress<DeclarationGoodsShipmentGovernmentAgencyGoodsItemBuyerAddress>(giBuyer)
				};
			}
		}
	}
}
