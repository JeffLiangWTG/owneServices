using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class FECAmendmentMessageBuilder : AmendmentMessageBuilder, IGbCDSMessageBuilder
	{
		public FECAmendmentMessageBuilder(JobDeclarationMessageSendingObject objectToSend, string functionCode) : base(objectToSend, functionCode)
		{
		}

		protected override AmendmentObjectWrapper GetAmendmentWrapper()
		{
			ObjectToSend.AmendmentDetails = new AmendmentDetails(null, "", "");
			ObjectToSend.AmendmentDetails.Amendments = AmendmentMessageHelper.Instance.GetFakeFECAmendment(ObjectToSend);
			return ObjectToSend.AmendmentDetails.Amendments;
		}

		protected override void AddExtraData(CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration metaDeclaration)
		{
			var mass = ObjectToSend?.Header?.MergedLines[0].EffectiveGrossWeight.InKilogramsSafe;
			metaDeclaration.GoodsShipment = new DeclarationGoodsShipment()
			{
				GovernmentAgencyGoodsItem = new DeclarationGoodsShipmentGovernmentAgencyGoodsItem[]
				{
					new DeclarationGoodsShipmentGovernmentAgencyGoodsItem()
					{
						SequenceNumeric = 1,
						Commodity = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity()
						{
							GoodsMeasure = new DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodityGoodsMeasure
							{
								GrossMassMeasure = new GoodsMeasureGrossMassMeasureType()
								{
									Value = mass.Value
								}
							}
						}
					}
				}
			};
		}
	}
}
