using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.ES.Business.MessageProcessorConstants;
using CusEntryHeader = Enterprise.Customs.ES.Business.Declaration.CusEntryHeader;

namespace Enterprise.Customs.ES.Business
{
	public abstract class ExportGenericResponseMessageProcessor : ESResponseMessageProcessor<IExportResponseMessageProvider>
	{
		public ExportGenericResponseMessageProcessor(LoggingInformation logger, IEDocsDelayedSaver eDocsSaver) : base(logger, eDocsSaver)
		{
		}

		protected override void ProcessMessageCore(EDIMessage message, CusEntryHeader linkedBusinessObject, IExportResponseMessageProvider provider)
		{
			SetMRN(linkedBusinessObject, provider);
			SetEADPrintProcedure(linkedBusinessObject, provider);
			SetCSVT2L(linkedBusinessObject, provider);
			SetCSVClearanceAndTriggerDocumentRequest(linkedBusinessObject, message, provider.CSVReleaseCode);
			SetCircuitAndEntryStatus(message, linkedBusinessObject, provider);
			SetClearanceResult(linkedBusinessObject, provider);
			SetResponseDates(linkedBusinessObject, provider);

			if (provider.FreeTextErrors != null && provider.FreeTextErrors.Any())
			{
				ProcessRejectedDeclaration(provider, message);
				SetMessageSubTypeAsRejected(message);
			}
			else
			{
				ProcessAcceptedDeclaration(provider, message);
				SetMessageSubTypeAsAccepted(message);
				ProcessDocuments(linkedBusinessObject);
			}

			SetMessageStatusAsReceived(message);
			SetCHStatusAsReceived(linkedBusinessObject);
		}

		void SetMRN(CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			var registrationNumber = response.RegistrationNumber;
			if (!registrationNumber.IsEmpty && !response.RegistrationNumber.Equals(response.UniqueReferenceNumber))
			{
				entryHeader.MovementReferenceNumberSetter(registrationNumber);
			}
		}

		void SetCircuitAndEntryStatus(EDIMessage message, CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			var initialEntryStatus = entryHeader.CH_EntryStatus;

			SetEntryStatus(message, entryHeader, response);
			SetEntryStatusCAN(entryHeader, response);

			if (message.EM_MessageType == DeclarationMessageTypeList.Codes.ExportAmendment && initialEntryStatus == EntryStatusCodes.Cleared)
			{
				entryHeader.CH_EntryStatus = initialEntryStatus;
			}
		}

		void SetEntryStatus(EDIMessage message, CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			if (response.MessageFunction == MessageFunctionCodeList.Codes.ComplementaryDue)
			{
				SetEntryStatusComplementaryDue(message, entryHeader, response);
				return;
			}

			if (response.MessageFunction == MessageFunctionCodeList.Codes.PreDue)
			{
				entryHeader.CH_EntryStatus = EntryStatusCodes.PreDeclarationAccepted;

				TriggerInboxRequest(entryHeader, message, new ZString[] { DeclarationMessageTypeList.Codes.InBoxNotificationForExport });
				return;
			}

			var (entryStatus, circuitCode) = (string)response.MessageFunction switch
			{
				MessageFunctionCodeList.Codes.GreenCircuit when entryHeader.EntryInstruction?.IsSubStyleBOrC == true => (EntryStatusCodes.ClearedWithPendingComplementaryDeclarations, CircuitCodeList.Codes.GREEN),
				MessageFunctionCodeList.Codes.GreenCircuit => (EntryStatusCodes.Cleared, CircuitCodeList.Codes.GREEN),
				MessageFunctionCodeList.Codes.RedCircuit => (EntryStatusCodes.CustomsDeclarationAccepted, CircuitCodeList.Codes.RED),
				MessageFunctionCodeList.Codes.OrangeCircuit => (EntryStatusCodes.CustomsDeclarationAccepted, CircuitCodeList.Codes.ORANGE),
				_ => (null, null)
			};

			if (entryStatus is not null)
			{
				entryHeader.CH_EntryStatus = entryStatus;
			}

			if (circuitCode is not null)
			{
				entryHeader.SetMovementReferenceNumberEntryStatus(circuitCode);
			}
		}

		void SetEntryStatusComplementaryDue(EDIMessage message, CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			entryHeader.CH_EntryStatus = EntryStatusCodes.Cleared;
			entryHeader.SetCSVClearanceNum(response.CSVReleaseCode);

			var admissionDate = response.AdmissionDate;
			var interchangeCreationDate = message.Interchange?.EI_SystemCreateTimeUtc.ToLocalBranchTime() ?? ZDateTime.Empty;
			entryHeader.MovementReferenceNumberIssueDate = (!admissionDate.IsEmpty && admissionDate.IsValid) ? admissionDate : interchangeCreationDate;
		}

		void SetEntryStatusCAN(CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			var (entryStatus, circuitCode) = (string)response.MessageFunctionCAN switch
			{
				MessageFunctionCodeList.Codes.GreenCircuit => (null, CircuitCodeList.Codes.GREEN),
				MessageFunctionCodeList.Codes.RedCircuit => (EntryStatusCodes.CustomsDeclarationAccepted, CircuitCodeList.Codes.RED),
				MessageFunctionCodeList.Codes.OrangeCircuit => (EntryStatusCodes.CustomsDeclarationAccepted, CircuitCodeList.Codes.ORANGE),
				_ => (null, null)
			};

			if (entryStatus is not null)
			{
				entryHeader.CH_EntryStatus = entryStatus;
			}

			if (circuitCode is not null)
			{
				entryHeader.SetCircuitCan(circuitCode);
			}
		}

		void SetClearanceResult(CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			if ((string)response.CustomsClearanceStatus
				is ClearanceResultCodeList.Codes.A1
				or ClearanceResultCodeList.Codes.A2)
			{
				entryHeader.ZG_ClearanceResult = response.CustomsClearanceStatus;
			}
		}

		void SetEADPrintProcedure(CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			if ((string)response.PrintActionRequired
				is EADPrintProcedureCodeList.Codes._0NoEADPrint
				or EADPrintProcedureCodeList.Codes._1EADPrintedByCustomsAuthorities
				or EADPrintProcedureCodeList.Codes._2EADCanBePrintedByDeclarant)
			{
				entryHeader.EUH_EADPrintProcedure = response.PrintActionRequired;
			}
		}

		void SetResponseDates(CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			var admissionDate = response.AdmissionDate;
			if (!admissionDate.IsEmpty && admissionDate.IsValid)
			{
				entryHeader.MovementReferenceNumberIssueDate = admissionDate;
			}

			var transitMaxDate = response.TransitMaxDate;
			if (!transitMaxDate.IsEmpty && transitMaxDate.IsValid)
			{
				entryHeader.ZG_LimitDateOfArrival = transitMaxDate;
			}

			var csvReleaseCreationDate = response.CSVReleaseCreationDate;
			if (!csvReleaseCreationDate.IsEmpty && csvReleaseCreationDate.IsValid)
			{
				entryHeader.CH_EntryReleaseDate = csvReleaseCreationDate;
			}
		}

		void SetCSVT2L(CusEntryHeader entryHeader, IExportResponseMessageProvider response)
		{
			var csvT2LFCode = response.CSVT2LFCode;
			if (!csvT2LFCode.IsEmpty)
			{
				entryHeader.ZG_CSVT2L = csvT2LFCode;
			}
		}

		void ProcessRejectedDeclaration(IExportResponseMessageProvider response, EDIMessage message)
		{
			message.EM_MessageInterpretation = new ExportMessagePrettyFormatter(response).CreateMessageDetailsRejected();
		}

		void ProcessAcceptedDeclaration(IExportResponseMessageProvider response, EDIMessage message)
		{
			message.EM_MessageInterpretation = new ExportMessagePrettyFormatter(response).CreateMessageDetailsAccepted();
		}

		protected override IExportResponseMessageProvider GetMessageProviderCore(EDIMessage message)
		{
			return message.EM_MessageText.Contains(EdifactCodes.UNHSegmentCode)
				? CUSRESV921ESMessageHelper.New(message)
				: (IExportResponseMessageProvider)EdifactProcessorHelper.ProcessEdifactErrorResponse(message);
		}

		protected override CommonDocumentRequest<CusEntryHeader> GetNewDocumentRequest(CusEntryHeader businessObject, ZString certName, EDIMessage message) => new ExportDocumentRequest(businessObject, certName);

		protected override ZBool ForceChangeExistingEdocsFileNames(EDIMessage message) => message.EM_MessageType == DeclarationMessageTypeList.Codes.TypeXExport;

		protected override Dictionary<string, string> FileNamesDict(string mrn, string oldCSVClearance, EDIMessage message) => EDocHelper.FileNamesDictExport(mrn, oldCSVClearance);

		protected abstract void ProcessDocuments(CusEntryHeader entryHeader);
	}
}
