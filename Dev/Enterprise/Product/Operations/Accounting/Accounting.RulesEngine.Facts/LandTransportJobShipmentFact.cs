using System;
using Enterprise.MasterFiles.Business;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.JobBillingDefaulting.Facts;
using WTG.ProductionRules.Core;
using static Enterprise.Core.Constants.FreightShipmentDirection.Code;

namespace Enterprise.Accounting.RulesEngine.Facts
{
	public class LandTransportJobShipmentFact : ILandTransportJobShipmentFact
	{
		public LandTransportJobShipmentFact(IJobInvoicingSupporter invoicingSupporter, IUNLOCOFact originFact = null, IUNLOCOFact destinationFact = null)
		{
			Argument.NotNull(invoicingSupporter, nameof(invoicingSupporter));

			PK = Guid.NewGuid();
			TransportMode = invoicingSupporter.TransportMode;

			JobDirection = invoicingSupporter.IsExport ? Export
						 : invoicingSupporter.IsImport ? Import
						 : invoicingSupporter.IsCrossTrade ? CrossTrade
						 : invoicingSupporter.IsDomestic ? Domestic
						 : string.Empty;
			Origin = new FactLeftJoin<IUNLOCOFact>(originFact);
			Destination = new FactLeftJoin<IUNLOCOFact>(destinationFact);
		}

		public Guid PK { get; }

		public string TransportMode { get; }

		public string JobDirection { get; }

		public FactLeftJoin<IUNLOCOFact> Origin { get; }

		public FactLeftJoin<IUNLOCOFact> Destination { get; }
	}
}
