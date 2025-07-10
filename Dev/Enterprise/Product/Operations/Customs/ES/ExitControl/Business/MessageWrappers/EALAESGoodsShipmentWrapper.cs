using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.ExitControl.Business
{
	public class EALAESGoodsShipmentWrapper : IEALAESGoodsShipment
	{
		public EALAESGoodsShipmentWrapper(CusExitReport exitReport)
		{
			this.exitReport = Argument.NotNull(exitReport, nameof(exitReport));
			Argument.NotNull(exitReport.Consignment, nameof(exitReport.Consignment));
		}
		readonly CusExitReport exitReport;

		public IEALAESConsignment Consignment => consignment ?? (consignment = new EALAESConsignmentWrapper(exitReport));
		EALAESConsignmentWrapper consignment;

		public IReadOnlyCollection<IEALAESGoodsItem> GoodsItem => goodItem ?? (goodItem = exitReport.CER_Calc_Discrepancies ? EALAESGoodsItemWrapper.GetGoodsItemsList(exitReport).AsReadOnly() : new List<EALAESGoodsItemWrapper>().AsReadOnly());
		IReadOnlyCollection<EALAESGoodsItemWrapper> goodItem;
	}
}
