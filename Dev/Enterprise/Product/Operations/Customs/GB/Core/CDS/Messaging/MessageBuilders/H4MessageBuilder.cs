using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class H4MessageBuilder : H5MessageBuilder
	{
		public H4MessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCode) : base(cusEntryHeader, errorCollector, functionCode)
		{
		}

		protected override void PopulateObligationGuarantees()
		{
			PopulateObligationGuaranteesBase();
		}

		protected override void PopulateLoadingLocationID(DeclarationGoodsShipmentConsignment consignment, IConsignment consignmentData)
		{
			PopulateLoadingLocationIDBase(consignment, consignmentData);
		}

		protected override void PopulateWriteOff(DeclarationGoodsShipmentGovernmentAgencyGoodsItemAdditionalDocument result, IAdditionalDocument additionalDocument)
		{
			if (!JobDeclaration.IsWritingOffQuantityForbiddenForH4)
			{
				base.PopulateWriteOff(result, additionalDocument);
			}
		}
	}
}
