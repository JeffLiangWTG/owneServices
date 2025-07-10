using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.MessageBuilders
{
	public class CENMessageBuilder : ExportsMessageBuilder
	{
		public CENMessageBuilder(CusEntryHeader cusEntryHeader, ErrorCollector errorCollector, string functionCodeNewAmendDelete)
			: base(cusEntryHeader, errorCollector, functionCodeNewAmendDelete)
		{
		}

		protected override void PopulateTotalPackageQuantity()
		{
			if (TotalPackageQuantity > 0 && IsMovementThroughAnInventoryLinkingLocation())
			{
				decMessage.TotalPackageQuantity = new DeclarationTotalPackageQuantityType
				{
					Value = TotalPackageQuantity
				};
			}
		}

		protected override void PopulateExporter()
		{
			decMessage.Exporter = new DeclarationExporter { ID = new ExporterIdentificationIDType { Value = Exporter?.ID ?? string.Empty } };
		}

		protected override void PopulateConsignee()
		{
		}

		protected override void PopulateAgent()
		{
			var agent = Agent;
			if (agent != null && !agent.FunctionCode.IsEmpty && agent.Agent != null && agent.Agent is IOrganisation orgAgent && !orgAgent.ID.IsEmpty)
			{
				decMessage.Agent = new DeclarationAgent
				{
					ID = new AgentIdentificationIDType { Value = orgAgent.ID },
					FunctionCode = new AgentFunctionCodeType { Value = agent.FunctionCode }
				};
			}
		}

		protected override void PopulateClassification(DeclarationGoodsShipmentGovernmentAgencyGoodsItemCommodity commodity, ICommodity giCommodity)
		{
		}

		protected override void PopulateOrigins(IGovernmentAgencyGoodsItem goodsItem)
		{
		}
	}
}
