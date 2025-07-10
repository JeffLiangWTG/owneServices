using System.Collections.Generic;
using CargoWise.ComponentModel;
using Enterprise.DataTransfer.Business;
using Enterprise.MasterFiles.Business;
using WTG.StaticAnalysis.Annotation;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters
{
	[Immutable]
	public class WorkflowTriggerActionTypeXmlCodeMappings : EnterpriseCodeExternalCodeMappings
	{
		WorkflowTriggerActionTypeXmlCodeMappings()
		{
		}

		protected override IEnumerable<Mapping> GetMappings()
		{
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml, nameof(Xsd.WorkflowTriggerActionType.DXL));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendFormBuilderXml, nameof(Xsd.WorkflowTriggerActionType.FXL));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, nameof(Xsd.WorkflowTriggerActionType.NTF));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail, nameof(Xsd.WorkflowTriggerActionType.NBT));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback, nameof(Xsd.WorkflowTriggerActionType.XMF));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendXML, nameof(Xsd.WorkflowTriggerActionType.XML));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance, nameof(Xsd.WorkflowTriggerActionType.XMB));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified, nameof(Xsd.WorkflowTriggerActionType.XMS));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendNativeXML, nameof(Xsd.WorkflowTriggerActionType.XMN));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, nameof(Xsd.WorkflowTriggerActionType.XUS));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, nameof(Xsd.WorkflowTriggerActionType.XUE));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML, nameof(Xsd.WorkflowTriggerActionType.XUC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc, nameof(Xsd.WorkflowTriggerActionType.XUD));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallbackSimplified, nameof(Xsd.WorkflowTriggerActionType.XFS));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.StartDestinationPortClearanceProcess, nameof(Xsd.WorkflowTriggerActionType.SCP));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendEDocXml, nameof(Xsd.WorkflowTriggerActionType.EXL));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUCargoMessage, nameof(Xsd.WorkflowTriggerActionType.SAC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithAWB, nameof(Xsd.WorkflowTriggerActionType.XMA));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies, nameof(Xsd.WorkflowTriggerActionType.ISI));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.RecognizeRevenue, nameof(Xsd.WorkflowTriggerActionType.RRV));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue, nameof(Xsd.WorkflowTriggerActionType.CAR));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCosts, nameof(Xsd.WorkflowTriggerActionType.CNC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCostsAndRevenue, nameof(Xsd.WorkflowTriggerActionType.NAR));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts, nameof(Xsd.WorkflowTriggerActionType.COS));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue, nameof(Xsd.WorkflowTriggerActionType.REV));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.PostConsolCostOnly, nameof(Xsd.WorkflowTriggerActionType.PCO));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.PostAllRevenue, nameof(Xsd.WorkflowTriggerActionType.PRV));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.TransactionAllocationAndPost, nameof(Xsd.WorkflowTriggerActionType.ATP));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateJobInvoiceHeader, nameof(Xsd.WorkflowTriggerActionType.JIH));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateProfitShareCharges, nameof(Xsd.WorkflowTriggerActionType.PFC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, nameof(Xsd.WorkflowTriggerActionType.XUT));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML, nameof(Xsd.WorkflowTriggerActionType.XUL));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging, nameof(Xsd.WorkflowTriggerActionType.VCM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.PostOverseasAgentCharges, nameof(Xsd.WorkflowTriggerActionType.POA));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging, nameof(Xsd.WorkflowTriggerActionType.VAC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn, nameof(Xsd.WorkflowTriggerActionType.ROT));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.PostAllSisterCompanyCharges, nameof(Xsd.WorkflowTriggerActionType.PCA));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.PostLocalSisterCompanyChargesOnly, nameof(Xsd.WorkflowTriggerActionType.PCC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.PostAllCosts, nameof(Xsd.WorkflowTriggerActionType.PAC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendARInvoice, nameof(Xsd.WorkflowTriggerActionType.SRN));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.GenerateARInvoiceToEdocs, nameof(Xsd.WorkflowTriggerActionType.GAR));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AutoPack, nameof(Xsd.WorkflowTriggerActionType.APK));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.IncludeChargeInProfitShare, nameof(Xsd.WorkflowTriggerActionType.IPS));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AutoFinalisation, nameof(Xsd.WorkflowTriggerActionType.AFL));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML, nameof(Xsd.WorkflowTriggerActionType.XUB));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking, nameof(Xsd.WorkflowTriggerActionType.CTB));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, nameof(Xsd.WorkflowTriggerActionType.CTC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message, nameof(Xsd.WorkflowTriggerActionType.SB3));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SubmitAVSQuery, nameof(Xsd.WorkflowTriggerActionType.AVS));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce, nameof(Xsd.WorkflowTriggerActionType.TMP));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways, nameof(Xsd.WorkflowTriggerActionType.TMA));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels, nameof(Xsd.WorkflowTriggerActionType.PPL));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels, nameof(Xsd.WorkflowTriggerActionType.PCL));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SynchronizeWithBondedWarehouse, nameof(Xsd.WorkflowTriggerActionType.SBW));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendEmanifestCloseMessage, nameof(Xsd.WorkflowTriggerActionType.CAE));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ConsolidateLVXDeclaration, nameof(Xsd.WorkflowTriggerActionType.CLX));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendAllEmanifestHouseBills, nameof(Xsd.WorkflowTriggerActionType.CAH));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML, nameof(Xsd.WorkflowTriggerActionType.XUM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML, nameof(Xsd.WorkflowTriggerActionType.XME));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendWithdrawalMessage, nameof(Xsd.WorkflowTriggerActionType.SWM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, nameof(Xsd.WorkflowTriggerActionType.ECM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage, nameof(Xsd.WorkflowTriggerActionType.ECI));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.EnrolInWiseTechAcademyCourse, nameof(Xsd.WorkflowTriggerActionType.WTA));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML, nameof(Xsd.WorkflowTriggerActionType.XUA));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage, nameof(Xsd.WorkflowTriggerActionType.SEM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage, nameof(Xsd.WorkflowTriggerActionType.SRM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendISFMessage, nameof(Xsd.WorkflowTriggerActionType.SIM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUSeaCargoMessage, nameof(Xsd.WorkflowTriggerActionType.SSO));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateStandAloneDeclaration, nameof(Xsd.WorkflowTriggerActionType.CSD));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVImporterSecurityFiling, nameof(Xsd.WorkflowTriggerActionType.HIS));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVSeaFreightAMS, nameof(Xsd.WorkflowTriggerActionType.HAM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.RunHVLVPreScreening, nameof(Xsd.WorkflowTriggerActionType.PRE));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ConvertToShipment, nameof(Xsd.WorkflowTriggerActionType.CNV));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVAirFreightAMS, nameof(Xsd.WorkflowTriggerActionType.HAS));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AUAirCargoReport, nameof(Xsd.WorkflowTriggerActionType.AAR));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AUSeaCargoReport, nameof(Xsd.WorkflowTriggerActionType.ASR));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.NZAirCargoReport, nameof(Xsd.WorkflowTriggerActionType.NAC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.NZSeaCargoReport, nameof(Xsd.WorkflowTriggerActionType.NSC));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateH7Declaration, nameof(Xsd.WorkflowTriggerActionType.CH7));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendCustomsDeclaration, nameof(Xsd.WorkflowTriggerActionType.SCD));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateHVLVUSTruckEManifest, nameof(Xsd.WorkflowTriggerActionType.UTE));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendManifestMessage, nameof(Xsd.WorkflowTriggerActionType.SMM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.HVLVSendAcknowledgementACASReport, nameof(Xsd.WorkflowTriggerActionType.HAK));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ValidateAndSendAirCargoReportMessage, nameof(Xsd.WorkflowTriggerActionType.ACR));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateBrokerageOnShipment, nameof(Xsd.WorkflowTriggerActionType.CBK));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendNCTSMessage, nameof(Xsd.WorkflowTriggerActionType.SNM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendExportDemandDeTracing, nameof(Xsd.WorkflowTriggerActionType.TRE));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendImportDemandDeTracing, nameof(Xsd.WorkflowTriggerActionType.TRI));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ApplyTag, nameof(Xsd.WorkflowTriggerActionType.TAG));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent, nameof(Xsd.WorkflowTriggerActionType.DLY));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest, nameof(Xsd.WorkflowTriggerActionType.SGM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendCRESAMessage, nameof(Xsd.WorkflowTriggerActionType.SCM));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.AssignStaffandEmail, nameof(Xsd.WorkflowTriggerActionType.ASE));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendNCTSArrivalNotification, nameof(Xsd.WorkflowTriggerActionType.SAN));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.ScheduleOrSendAUAirCargoOutturnMessage, nameof(Xsd.WorkflowTriggerActionType.SAO));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendEIDO, nameof(Xsd.WorkflowTriggerActionType.EID));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendImportReleaseOrder, nameof(Xsd.WorkflowTriggerActionType.IRO));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendExitReportTransferMessage, nameof(Xsd.WorkflowTriggerActionType.TRA));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendCIN750Message, nameof(Xsd.WorkflowTriggerActionType.SCI));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CINExportNotification, nameof(Xsd.WorkflowTriggerActionType.CIN));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CGNExportNotification, nameof(Xsd.WorkflowTriggerActionType.CGN));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest, nameof(Xsd.WorkflowTriggerActionType.GHG));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob, nameof(Xsd.WorkflowTriggerActionType.CTJ));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.SendG3CustomsDeclaration, nameof(Xsd.WorkflowTriggerActionType.SC3));
			yield return new Mapping(WorkflowTriggerActionTypeConstants.Codes.CusNOEmmaMessageGenerator, nameof(Xsd.WorkflowTriggerActionType.EMG));
		}

		public static readonly WorkflowTriggerActionTypeXmlCodeMappings Instance = new WorkflowTriggerActionTypeXmlCodeMappings();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded name string")]
		protected override string Name
		{
			get { return "Workflow Trigger Action Type"; }
		}

		public new Xsd.WorkflowTriggerActionType GetExternalCode(string enterpriseCode, string errorContext, INotifications notify)
		{
			return GetEnumExternalCode(enterpriseCode, Xsd.WorkflowTriggerActionType.NTF, errorContext, notify);
		}
	}
}
