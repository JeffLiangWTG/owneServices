using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.IE.MessageContracts.AES.Interfaces;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.AES;

namespace Enterprise.Customs.IE.ExitControl.Business.AES
{
	public class IE507ConsignmentProvider : IIE507Consignment
	{
		public IE507ConsignmentProvider(CusExitReport exitReport, CusExitHeader exitHeader)
		{
			this.exitReport = exitReport;
			this.exitHeader = exitHeader;
		}
		readonly CusExitReport exitReport;
		readonly CusExitHeader exitHeader;

		#region IIE507Consignment Members
		public string BorderModeOfTransport => exitReport.CER_TransportMode.IsEmpty ? ZString.Empty : exitHeader.TransportModeTranslator.TranslateToWCOCode(exitReport.CER_TransportMode);

		public string UCR => exitReport.Consignment?.CXC_UniqueConsignmentReference ?? ZString.Empty;

		public IParty ExitCarrier => CachedValueHelper.GetValue(ref exitCarrierCached, () => PartyProvider.New(exitHeader.Carrier));
		CachedValue<IParty> exitCarrierCached;

		public IReadOnlyCollection<ITransportEquipmentWithSeals> TransportEquipments => transportEquipments ?? (transportEquipments = exitReport.GetContainersOrEquipments().Where(c => c.container.AllSealNumbers.Count > 0 || !c.container.CXN_IsEquipment).Select(x => new TransportEquipmentsProvider(x.container, x.consignmentItems.Select(i => i.CCI_LineNumber.ToString()))).ToArray());
		IReadOnlyCollection<ITransportEquipmentWithSeals> transportEquipments;

		public ILocationOfGoods LocationOfGoods => CachedValueHelper.GetValue(ref locationOfGoodsCached, () => LocationOfGoodsProvider.New(exitReport.GetGoodsLocation()));
		CachedValue<ILocationOfGoods> locationOfGoodsCached;

		public IReadOnlyCollection<IDocument> TransportDocuments => transportDocuments ?? (transportDocuments = exitReport.CusExitReportItems.SelectMany(x => x.AdditionalInfos.Cast<AdditionalInfo>()).Select(t => new AdditionalInfosProvider(t)).ToArray());
		IReadOnlyCollection<IDocument> transportDocuments;

		public ITransportMeans ActiveBorderTransportMeans => activeBorderTransportMeans ?? (activeBorderTransportMeans = new ActiveTransportMeansProvider(exitReport));
		ITransportMeans activeBorderTransportMeans;
		#endregion
	}
}
