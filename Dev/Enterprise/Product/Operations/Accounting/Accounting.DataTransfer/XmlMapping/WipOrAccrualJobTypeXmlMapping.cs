using System;
using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.Common;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.XmlMapping
{
	[Immutable]
	[ImmutableObject(true)]
	public class WipOrAccrualJobTypeXmlMapping : EnterpriseCodeExternalCodeMappings
	{
		protected WipOrAccrualJobTypeXmlMapping()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(JobInvoicingConsumerTypes.Shipment.Code, nameof(Xsd.WipOrAccrualJobType.SHP));
			yield return new Mapping(JobInvoicingConsumerTypes.QuotedBooking.Code, nameof(Xsd.WipOrAccrualJobType.QSH));
			yield return new Mapping(JobInvoicingConsumerTypes.Consol.Code, nameof(Xsd.WipOrAccrualJobType.CON));
			yield return new Mapping(JobInvoicingConsumerTypes.ForwardingConsol.Code, nameof(Xsd.WipOrAccrualJobType.FCN));
			yield return new Mapping(JobInvoicingConsumerTypes.GatewayConsol.Code, nameof(Xsd.WipOrAccrualJobType.GCN));
			yield return new Mapping(JobInvoicingConsumerTypes.Brokerage.Code, nameof(Xsd.WipOrAccrualJobType.BRK));
			yield return new Mapping(JobInvoicingConsumerTypes.PostClearanceBrokerage.Code, nameof(Xsd.TxnLineConsolOrJobType.PCB));
			yield return new Mapping(JobInvoicingConsumerTypes.MasterAWB.Code, nameof(Xsd.WipOrAccrualJobType.AWB));
			yield return new Mapping(JobInvoicingConsumerTypes.CFSShipment.Code, nameof(Xsd.WipOrAccrualJobType.CSL));
			yield return new Mapping(JobInvoicingConsumerTypes.CFSLoadList.Code, nameof(Xsd.WipOrAccrualJobType.CLL));
			yield return new Mapping(JobInvoicingConsumerTypes.FCLStorage.Code, nameof(Xsd.WipOrAccrualJobType.CST));
			yield return new Mapping(JobInvoicingConsumerTypes.LocalCartage.Code, nameof(Xsd.WipOrAccrualJobType.TRN));
			yield return new Mapping(JobInvoicingConsumerTypes.AgentBooking.Code, nameof(Xsd.WipOrAccrualJobType.ABK));
			yield return new Mapping(JobInvoicingConsumerTypes.TransportBookingWithAgent.Code, nameof(Xsd.WipOrAccrualJobType.ATB));
			yield return new Mapping(JobInvoicingConsumerTypes.TransportBooking.Code, nameof(Xsd.WipOrAccrualJobType.TBM));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseInwards.Code, nameof(Xsd.WipOrAccrualJobType.WIN));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseOutwards.Code, nameof(Xsd.WipOrAccrualJobType.WOU));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseStorage.Code, nameof(Xsd.WipOrAccrualJobType.WST));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseStocktake.Code, nameof(Xsd.WipOrAccrualJobType.WSC));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob.Code, nameof(Xsd.WipOrAccrualJobType.WSJ));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseVASOrder.Code, nameof(Xsd.WipOrAccrualJobType.WVO));
			yield return new Mapping(JobInvoicingConsumerTypes.OneOffQuotation.Code, nameof(Xsd.WipOrAccrualJobType.QTE));
			yield return new Mapping(JobInvoicingConsumerTypes.CusMAWB.Code, nameof(Xsd.WipOrAccrualJobType.ACR));
			yield return new Mapping(JobInvoicingConsumerTypes.CusUnderbond.Code, nameof(Xsd.WipOrAccrualJobType.UBR));
			yield return new Mapping(JobInvoicingConsumerTypes.CTOCusMAWB.Code, nameof(Xsd.WipOrAccrualJobType.CTO));
			yield return new Mapping(JobInvoicingConsumerTypes.CTOCusImportHAWB.Code, nameof(Xsd.WipOrAccrualJobType.AHW));
			yield return new Mapping(JobInvoicingConsumerTypes.CTOCusExportHAWB.Code, nameof(Xsd.WipOrAccrualJobType.AHE));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencyBooking.Code, nameof(Xsd.WipOrAccrualJobType.AGB));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, nameof(Xsd.WipOrAccrualJobType.AGS));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code, nameof(Xsd.WipOrAccrualJobType.ACD));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code, nameof(Xsd.WipOrAccrualJobType.AVA));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencySundryCharges.Code, nameof(Xsd.WipOrAccrualJobType.ASC));
			yield return new Mapping(JobInvoicingConsumerTypes.ImporterSecurityFiling.Code, nameof(Xsd.WipOrAccrualJobType.ISF));
			yield return new Mapping(JobInvoicingConsumerTypes.Organisation.Code, nameof(Xsd.WipOrAccrualJobType.ORG));
			yield return new Mapping(JobInvoicingConsumerTypes.eManifest.Code, nameof(Xsd.WipOrAccrualJobType.MAN));
			yield return new Mapping(JobInvoicingConsumerTypes.TransportBookingConsignment.Code, nameof(Xsd.WipOrAccrualJobType.TCW));
			yield return new Mapping(JobInvoicingConsumerTypes.TransportConsignment.Code, nameof(Xsd.WipOrAccrualJobType.LTC));
			yield return new Mapping(JobInvoicingConsumerTypes.CAeManifest.Code, nameof(Xsd.WipOrAccrualJobType.CAE));
			yield return new Mapping(JobInvoicingConsumerTypes.WorkItem.Code, nameof(Xsd.WipOrAccrualJobType.WKI));
			yield return new Mapping(JobInvoicingConsumerTypes.Project.Code, nameof(Xsd.WipOrAccrualJobType.WKP));
			yield return new Mapping(JobInvoicingConsumerTypes.WorkRequest.Code, nameof(Xsd.WipOrAccrualJobType.WKR));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitReceive.Code, nameof(Xsd.WipOrAccrualJobType.TRC));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitDispatch.Code, nameof(Xsd.WipOrAccrualJobType.TDC));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitDispatchLoadList.Code, nameof(Xsd.WipOrAccrualJobType.TDL));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code, nameof(Xsd.TxnLineConsolOrJobType.TRU));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code, nameof(Xsd.TxnLineConsolOrJobType.TDU));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDReceiveAdvice.Code, nameof(Xsd.WipOrAccrualJobType.YRA));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDReleaseAdvice.Code, nameof(Xsd.WipOrAccrualJobType.YRE));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDTransportationUnit.Code, nameof(Xsd.WipOrAccrualJobType.YTU));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDAdHocServiceOrder.Code, nameof(Xsd.WipOrAccrualJobType.YAO));
			yield return new Mapping(JobInvoicingConsumerTypes.MNRWorkOrderHeader.Code, nameof(Xsd.WipOrAccrualJobType.MWO));
			yield return new Mapping(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, nameof(Xsd.WipOrAccrualJobType.NCT));
			yield return new Mapping(JobInvoicingConsumerTypes.BRLPCO.Code, nameof(Xsd.WipOrAccrualJobType.LPC));
			yield return new Mapping(JobInvoicingConsumerTypes.CustomsTemporaryStorage.Code, nameof(Xsd.WipOrAccrualJobType.STO));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDPeriodicInvoicing.Code, nameof(Xsd.WipOrAccrualJobType.YPI));
		}

		public new Xsd.WipOrAccrualJobType GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.WipOrAccrualJobType.SHP, errorContext, notifications);
		}

		static readonly Lazy<WipOrAccrualJobTypeXmlMapping> LazyInstance = new Lazy<WipOrAccrualJobTypeXmlMapping>(() => OverridableNewDelegate.Value != null ? OverridableNewDelegate.Value() : new WipOrAccrualJobTypeXmlMapping());

		public static WipOrAccrualJobTypeXmlMapping Instance
		{
			get { return LazyInstance.Value; }
		}

		protected delegate WipOrAccrualJobTypeXmlMapping NewDelegate();

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected override string Name
		{
			get { return Res.GetString("7dd7ce97-54d6-44b5-b82c-1dba8e6bd20c", "WIP or Accrual Job Type"); }
		}
	}
}
