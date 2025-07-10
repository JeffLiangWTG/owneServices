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
	public class TransactionLineConsolOrJobTypeXmlMapping : EnterpriseCodeExternalCodeMappings
	{
		protected TransactionLineConsolOrJobTypeXmlMapping()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(JobInvoicingConsumerTypes.CusMAWB.Code, nameof(Xsd.TxnLineConsolOrJobType.ACR));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencyBooking.Code, nameof(Xsd.TxnLineConsolOrJobType.AGB));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencyBillOfLading.Code, nameof(Xsd.TxnLineConsolOrJobType.AGS));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencyDetentionInvoice.Code, nameof(Xsd.TxnLineConsolOrJobType.ACD));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencyVoyageAccounting.Code, nameof(Xsd.TxnLineConsolOrJobType.AVA));
			yield return new Mapping(JobInvoicingConsumerTypes.AgencySundryCharges.Code, nameof(Xsd.TxnLineConsolOrJobType.ASC));
			yield return new Mapping(JobInvoicingConsumerTypes.MasterAWB.Code, nameof(Xsd.TxnLineConsolOrJobType.AWB));
			yield return new Mapping(JobInvoicingConsumerTypes.Brokerage.Code, nameof(Xsd.TxnLineConsolOrJobType.BRK));
			yield return new Mapping(JobInvoicingConsumerTypes.PostClearanceBrokerage.Code, nameof(Xsd.TxnLineConsolOrJobType.PCB));
			yield return new Mapping(JobInvoicingConsumerTypes.CFSLoadList.Code, nameof(Xsd.TxnLineConsolOrJobType.CLL));
			yield return new Mapping(JobInvoicingConsumerTypes.CFSShipment.Code, nameof(Xsd.TxnLineConsolOrJobType.CSH));
			yield return new Mapping(JobInvoicingConsumerTypes.Consol.Code, nameof(Xsd.TxnLineConsolOrJobType.CSL));
			yield return new Mapping(JobInvoicingConsumerTypes.ForwardingConsol.Code, nameof(Xsd.TxnLineConsolOrJobType.FCN));
			yield return new Mapping(JobInvoicingConsumerTypes.GatewayConsol.Code, nameof(Xsd.TxnLineConsolOrJobType.GCN));
			yield return new Mapping(JobInvoicingConsumerTypes.CTOCusMAWB.Code, nameof(Xsd.TxnLineConsolOrJobType.CTO));
			yield return new Mapping(JobInvoicingConsumerTypes.CTOCusImportHAWB.Code, nameof(Xsd.TxnLineConsolOrJobType.AHW));
			yield return new Mapping(JobInvoicingConsumerTypes.CTOCusExportHAWB.Code, nameof(Xsd.TxnLineConsolOrJobType.AHE));
			yield return new Mapping(JobInvoicingConsumerTypes.Shipment.Code, nameof(Xsd.TxnLineConsolOrJobType.SHP));
			yield return new Mapping(JobInvoicingConsumerTypes.QuotedBooking.Code, nameof(Xsd.TxnLineConsolOrJobType.QSH));
			yield return new Mapping(JobInvoicingConsumerTypes.LocalCartage.Code, nameof(Xsd.TxnLineConsolOrJobType.TRN));
			yield return new Mapping(JobInvoicingConsumerTypes.AgentBooking.Code, nameof(Xsd.TxnLineConsolOrJobType.ABK));
			yield return new Mapping(JobInvoicingConsumerTypes.TransportBooking.Code, nameof(Xsd.TxnLineConsolOrJobType.TBM));
			yield return new Mapping(JobInvoicingConsumerTypes.TransportBookingWithAgent.Code, nameof(Xsd.TxnLineConsolOrJobType.ATB));
			yield return new Mapping(JobInvoicingConsumerTypes.CusUnderbond.Code, nameof(Xsd.TxnLineConsolOrJobType.UBR));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseInwards.Code, nameof(Xsd.TxnLineConsolOrJobType.WIN));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseOutwards.Code, nameof(Xsd.TxnLineConsolOrJobType.WOU));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseStorage.Code, nameof(Xsd.TxnLineConsolOrJobType.WST));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseStocktake.Code, nameof(Xsd.TxnLineConsolOrJobType.WSC));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseAdHocServiceJob.Code, nameof(Xsd.TxnLineConsolOrJobType.WSJ));
			yield return new Mapping(JobInvoicingConsumerTypes.WarehouseVASOrder.Code, nameof(Xsd.TxnLineConsolOrJobType.WVO));
			yield return new Mapping(JobInvoicingConsumerTypes.FCLStorage.Code, nameof(Xsd.TxnLineConsolOrJobType.CST));
			yield return new Mapping(JobInvoicingConsumerTypes.ImporterSecurityFiling.Code, nameof(Xsd.TxnLineConsolOrJobType.ISF));
			yield return new Mapping(JobInvoicingConsumerTypes.Organisation.Code, nameof(Xsd.TxnLineConsolOrJobType.ORG));
			yield return new Mapping(JobInvoicingConsumerTypes.eManifest.Code, nameof(Xsd.TxnLineConsolOrJobType.MAN));
			yield return new Mapping(JobInvoicingConsumerTypes.TransportBookingConsignment.Code, nameof(Xsd.TxnLineConsolOrJobType.TCW));
			yield return new Mapping(JobInvoicingConsumerTypes.TransportConsignment.Code, nameof(Xsd.TxnLineConsolOrJobType.LTC));
			yield return new Mapping(JobInvoicingConsumerTypes.CAeManifest.Code, nameof(Xsd.TxnLineConsolOrJobType.CAE));
			yield return new Mapping(JobInvoicingConsumerTypes.WorkItem.Code, nameof(Xsd.TxnLineConsolOrJobType.WKI));
			yield return new Mapping(JobInvoicingConsumerTypes.Project.Code, nameof(Xsd.TxnLineConsolOrJobType.WKP));
			yield return new Mapping(JobInvoicingConsumerTypes.WorkRequest.Code, nameof(Xsd.TxnLineConsolOrJobType.WKR));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitReceive.Code, nameof(Xsd.TxnLineConsolOrJobType.TRC));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitDispatch.Code, nameof(Xsd.TxnLineConsolOrJobType.TDC));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitDispatchLoadList.Code, nameof(Xsd.TxnLineConsolOrJobType.TDL));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitReceiveTransportationUnit.Code, nameof(Xsd.TxnLineConsolOrJobType.TRU));
			yield return new Mapping(JobInvoicingConsumerTypes.TransitDispatchTransportationUnit.Code, nameof(Xsd.TxnLineConsolOrJobType.TDU));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDReceiveAdvice.Code, nameof(Xsd.TxnLineConsolOrJobType.YRA));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDReleaseAdvice.Code, nameof(Xsd.TxnLineConsolOrJobType.YRE));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDTransportationUnit.Code, nameof(Xsd.TxnLineConsolOrJobType.YTU));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDAdHocServiceOrder.Code, nameof(Xsd.TxnLineConsolOrJobType.YAO));
			yield return new Mapping(JobInvoicingConsumerTypes.MNRWorkOrderHeader.Code, nameof(Xsd.TxnLineConsolOrJobType.MWO));
			yield return new Mapping(JobInvoicingConsumerTypes.CustomsTransitNCTS.Code, nameof(Xsd.TxnLineConsolOrJobType.NCT));
			yield return new Mapping(JobInvoicingConsumerTypes.BRLPCO.Code, nameof(Xsd.TxnLineConsolOrJobType.LPC));
			yield return new Mapping(JobInvoicingConsumerTypes.CustomsTemporaryStorage.Code, nameof(Xsd.TxnLineConsolOrJobType.STO));
			yield return new Mapping(JobInvoicingConsumerTypes.CYDPeriodicInvoicing.Code, nameof(Xsd.TxnLineConsolOrJobType.YPI));
		}

		public new Xsd.TxnLineConsolOrJobType GetExternalCode(string enterpriseCode, string errorContext, INotifications notifications)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.TxnLineConsolOrJobType.UNK, errorContext, notifications);
		}

		static readonly Lazy<TransactionLineConsolOrJobTypeXmlMapping> LazyInstance = new Lazy<TransactionLineConsolOrJobTypeXmlMapping>(() => OverridableNewDelegate.Value != null ? OverridableNewDelegate.Value() : new TransactionLineConsolOrJobTypeXmlMapping());

		public static TransactionLineConsolOrJobTypeXmlMapping Instance
		{
			get { return LazyInstance.Value; }
		}

		protected delegate TransactionLineConsolOrJobTypeXmlMapping NewDelegate();

		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		protected override string Name
		{
			get { return "Consol or Job Type"; }
		}
	}
}
