using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RFPMessageManager : SingleMessageManager
	{
		public RFPMessageManager(QuarantineExDocHeader exDocHeader, string messageType)
		{
			this.exDocHeader = exDocHeader;
			this.messageType = messageType;
		}

		public override string MessageFriendlyName
		{
			get { return "Request For Permit"; }
		}

		public override BusinessObject BusinessObject
		{
			get { return exDocHeader; }
		}

		public override bool IsWaitingForResponse
		{
			get { return exDocHeader.IsWaitingForResponse; }
		}

		#region Can Send Properties

		public bool CanSendForward
		{
			get
			{
				bool result = CheckCanSendAndCreateNotifications(ValidCanSendTransferStatusCode, ZString.Empty, true);
				if (result && (exDocHeader.QH_ForwardeeEDIUserIdentifier.IsEmpty))
				{
					Notification = Res.GetString("00BF2A02-BCE6-4487-ACDE-D7A50A5566BD", "Forward to EDI user must be entered before a Forward message can be sent.");
					result = false;
				}

				return result;
			}
		}

		public bool CanSendTransfer
		{
			get
			{
				bool result = CheckCanSendAndCreateNotifications(ValidCanSendTransferStatusCode, ZString.Empty, true);
				if (result && TransferDetailsHaveNotBeenEntered)
				{
					Notification = Res.GetString("1131557F-4057-4336-BD17-EAED11EF8DF3", "Transfer to EDI user and Transfer to Exporter must be entered before a Transfer message can be sent.");
					result = false;
				}

				return result;
			}
		}

		bool TransferDetailsHaveNotBeenEntered => exDocHeader.QH_TransfereeEDIUserIdentifier.IsEmpty || exDocHeader.QH_TransfereeExporterNumber.IsEmpty;

		public bool CanSendTransferIn
		{
			get
			{
				bool result = CheckCanSendAndCreateNotifications(ValidCanSendTransferInStatusCode, ZString.Empty, true);
				if (EntryStatus.IsEmpty)
				{
					Notification = Res.GetString("924D59FE-E19F-49A5-9577-71A2F8F12B16", "Message cannot be sent when no Transfer message has been received.");
					result = false;
				}

				var au = exDocHeader.InvoiceHeader.Factory.Load<RefCountry>(Core.Constants.CountryGuids.Australia);
				OrgHeader supplier = exDocHeader.InvoiceHeader.Supplier_Effective;
				if (result && (supplier == null || supplier.CustomsCodes.GetCustomsRegNo(OrgCusCode.AUQuarantineCodeTypes.EXDOCExporterNumber, au).IsEmpty))
				{
					Notification = Res.GetString("A5DB96BA-E36D-4AE6-812E-0022E0CE2240", "Supplier must be entered and have an EXDOC Exporter Number.");
					result = false;
				}

				return result;
			}
		}

		public bool CanSendCopy
		{
			get
			{
				return CheckCanSendAndCreateNotifications(
					ValidCanSendCopyStatusCode,
					Res.GetString("C47078E7-BFB4-45C1-B0CF-B1A18A416589", "A copy request message cannot be sent when the RFP has not been reported to Quarantine."), true);
			}
		}

		public bool CanSendOrder
		{
			get { return CheckCanSendAndCreateNotifications(ValidCanSendOrderStatusCodes, ZString.Empty, true); }
		}

		public override bool CanSendOriginal
		{
			get { return CheckCanSendAndCreateNotifications(ValidCanSendOriginalStatusCodes, ZString.Empty, true); }
		}

		public bool CanSendAmmendment
		{
			get
			{
				bool result = CheckCanSendAndCreateNotifications(ValidCanSendAmendmentStatusCodes, ZString.Empty, true);
				if (EntryStatus.IsEmpty)
				{
					Notification = Res.GetString("A14A8737-2EA0-4A1F-9F6B-57242EB8F511", "Message cannot be sent when no successful lodgement message has been received.");
					result = false;
				}

				return result;
			}
		}

		public override bool CanSendWithdrawal
		{
			get
			{
				bool result = CheckCanSendAndCreateNotifications(ValidCanSendWithdrawalStatusCodes, ZString.Empty, true);
				if (EntryStatus.IsEmpty)
				{
					Notification = Res.GetString("3AC3A134-6D82-4530-B8EC-795D74B12295", "Message cannot be sent when no successful lodgement message has been received.");
					result = false;
				}

				return result;
			}
		}

		public bool CanSendCertificateRequest
		{
			get
			{
				return CheckCanSendAndCreateNotifications(
					ValidCanSendCertRequestStatusCodes,
					"A Certificate Request must be separate job to any RFP, i.e. an RFP must not have been requested on this job.", false);
			}
		}

		public bool CanSendRequestReplacementCertificate
		{
			get
			{
				var result = CheckCanSendAndCreateNotifications(ValidCanSendAmendmentStatusCodes, ZString.Empty, false);

				if (result && (exDocHeader.Declaration?.EXDOCAmendmentReason.IsEmpty ?? true))
				{
					var description = PredefinedNoteTypes.Instance.EXDOCAmendmentReason.Description;

					Notification = Res.GetString("C52D1F49-0FB5-4C0D-A2C5-30368574262D", "Message cannot be sent until an amendment reason is entered. You can add it under Declaration - Notes and use '{0}' as Description.", description);
					result = false;
				}

				return result;
			}
		}

		public bool CanSendRequestReissueCertificate
		{
			get
			{
				bool result = CheckCanSendAndCreateNotifications(ValidCanSendRequestReissueCertificateStatusCodes, ZString.Empty, false);
				if (EntryStatus.IsEmpty)
				{
					Notification = Res.GetString("3CDCE2ED-4DBE-4A3B-AA76-FB46899870E1", "Message cannot be sent when no successful lodgement message has been received.");
					result = false;
				}

				return result;
			}
		}

		public bool CanSendPreviewCertificate
		{
			get
			{
				var statusCodes = new List<ZString>
				{
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady
				};

				return CheckCanSendAndCreateNotifications(statusCodes, Res.GetString("37130C16-52D3-4DC9-AD94-0DFC65B391FA", "Message cannot be sent.  The current REX status is not set to 'Certificate Ready'."), false);
			}
		}

		public bool CanSendTransferOrCancelEDN
		{
			get
			{
				var result = CheckCanSendAndCreateNotifications(ValidCanSendTransferInStatusCode, ZString.Empty, false);
				if (result && (exDocHeader.Declaration?.CustomsAuthorityNumber.IsEmpty ?? true))
				{
					Notification = Res.GetString("C998B395-DB98-4E35-AA44-9DC3470C4DCB", "Message cannot be sent until the declaration has an Export Declaration Number.");
					result = false;
				}

				return result;
			}
		}

		public bool CanSendNEXDOCCancellation
		{
			get
			{
				var result = true;
				if (exDocHeader.Declaration == null || exDocHeader.Declaration.NEXDOCCancellationReason.IsEmpty)
				{
					result = false;
					Notification = Res.GetString("26479F1B-9723-4E94-89FC-78A93ECB8520", "Please input a NEXDOC Cancellation Reason in Notes.");
				}

				return result;
			}
		}

		public bool CanSendReadREX
		{
			get
			{
				return CheckCanSendAndCreateNotifications(ValidCanSendReadREXStatusCodes, ZString.Empty, false);
			}
		}

		#endregion

		#region Implementation

		bool CheckCanSendAndCreateNotifications(List<ZString> list, ZString overrideCannotSendMessage, bool allowSendWhileInReview)
		{
			bool result = true;
			Notification = ZString.Empty;
			if (exDocHeader.QH_ProduceType.IsEmpty)
			{
				Notification = Res.GetString("F55C17EE-A948-4008-8E5F-4ADB8589488D", "Message cannot be sent until a produce type is entered.");
				result = false;
			}
			else if (IsWaitingForResponse)
			{
				Notification = Res.GetString("1FECA595-A9C1-41CB-812C-DA4880C84208", "Unable to send message until a response to the current message is received.");
				result = false;
			}
			else if (!list.Contains(EntryStatus))
			{
				if (EntryInReview && allowSendWhileInReview)
				{
					Notification = Res.GetString("FD448B1C-1556-49F7-8EF3-0B7603AD9AB4", "Message not sent, the current REX status is set to Review.");
					var message = Res.GetString("81819D81-92EE-4732-9489-74117BBD7FA1", "The current REX status is set to Review and may be rejected.\r\nDo you want to continue?");
					result = exDocHeader.Declaration.MessageInitiator.ContinueWithAction(message, "Send Request For Export?");
				}
				else if (!overrideCannotSendMessage.IsEmpty)
				{
					Notification = overrideCannotSendMessage;
					result = false;
				}
				else
				{
					Notification = Res.GetString("B87C4DFB-2965-4759-B900-AE223BEA587B", "Message cannot be sent when status is {0}.", exDocHeader.QH_RequestForPermitNumberStatusDescription);
					result = false;
				}
			}

			return result;
		}

		bool EntryInReview => EntryType == CusEntryNumberTypes.Australia.RFS && EntryStatus == EXDOCComplianceStatusCodesForCusEntryNumber.Codes.ReviewInReview;

		internal string MessageStatus => exDocHeader.Declaration.JE_MessageStatus;

		internal ZString EntryStatus => exDocHeader.RequestForPermitStatus;

		ZString EntryType => exDocHeader.RequestForPermitEntryType;

		#region Generate Message

		protected override EDIMessage[] GenerateOriginalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as QuarantineExDocHeader);
		}

		protected override EDIMessage[] GenerateAmendmentMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as QuarantineExDocHeader);
		}

		protected override EDIMessage[] GenerateWithdrawalMessagesCore(BusinessObject bizo)
		{
			return GenerateMessage(bizo as QuarantineExDocHeader);
		}

		EDIMessage[] GenerateMessage(QuarantineExDocHeader header)
		{
			RFPMessage result = messageType == EXDOCMessageTypeCodes.Codes.CRQ ? GenerateCertRequestMessage(header) : GenerateRFPMessage(header);
			return result == null ? System.Array.Empty<EDIMessage>() : new EDIMessage[] { result };
		}

		RFPMessage GenerateCertRequestMessage(QuarantineExDocHeader header)
		{
			var requestData = new CertificateRequestDataWrapper(header);
			var certificateRequestMessageBuilder = new CertificateRequestMessageBuilder(requestData);
			RFPMessage result = certificateRequestMessageBuilder.GenerateMessage();
			header.Declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
			return result;
		}

		RFPMessage GenerateRFPMessage(QuarantineExDocHeader header)
		{
			RFPMessage messageToSend = null;
			switch (exDocHeader.QH_ProduceType)
			{
				case EXDOCCommodityCodes.Codes.Dairy:
					messageToSend = GetRFPMessageToSend(new RequestForPermitDairyHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
				case EXDOCCommodityCodes.Codes.Eggs:
					messageToSend = GetRFPMessageToSend(new RequestForPermitEggsHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
				case EXDOCCommodityCodes.Codes.Fish:
					messageToSend = GetRFPMessageToSend(new RequestForPermitFishHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
				case EXDOCCommodityCodes.Codes.GrainsAndPlants:
					messageToSend = GetRFPMessageToSend(new RequestForPermitGrainsAndPlantsHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
				case EXDOCCommodityCodes.Codes.Horticulture:
					messageToSend = GetRFPMessageToSend(new RequestForPermitHorticultureHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
				case EXDOCCommodityCodes.Codes.InedibleMeat:
					messageToSend = GetRFPMessageToSend(new RequestForPermitInedibleMeatHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
				case EXDOCCommodityCodes.Codes.Meat:
					messageToSend = GetRFPMessageToSend(new RequestForPermitMeatHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
				case EXDOCCommodityCodes.Codes.SkinsAndHides:
					messageToSend = GetRFPMessageToSend(new RequestForPermitSkinsAndHidesHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
				case EXDOCCommodityCodes.Codes.Wool:
					messageToSend = GetRFPMessageToSend(new RequestForPermitWoolHeaderMessageBuilder(header.InvoiceHeader, messageType));
					break;
			}

			return messageToSend;
		}

		RFPMessage GetRFPMessageToSend(RequestForPermitHeaderMessageBuilder rfpBuilder)
		{
			RFPMessage result = null;
			switch (messageType)
			{
				case EXDOCMessageTypeCodes.Codes.CAN:
					result = rfpBuilder.GenerateWithdrawlRFPMessage();
					break;
				case EXDOCMessageTypeCodes.Codes.RPL:
					result = rfpBuilder.GenerateAmendmentRFPMessage();
					break;
				case EXDOCMessageTypeCodes.Codes.ORD:
				case EXDOCMessageTypeCodes.Codes.LDG:
					result = rfpBuilder.GenerateRFPMessage();
					break;
				case EXDOCMessageTypeCodes.Codes.AcceptTransferIn:
				case EXDOCMessageTypeCodes.Codes.DeclineTransferIn:
					result = rfpBuilder.GenerateTransferInRFPMessage();
					break;
				case EXDOCMessageTypeCodes.Codes.TRF:
					result = rfpBuilder.GenerateTransferRFPMessage();
					break;
				case EXDOCMessageTypeCodes.Codes.CPY:
					result = rfpBuilder.GenerateCopyRFPMessage();
					break;
			}

			return result;
		}

		#endregion

		#region Valid Status Codes

		internal List<ZString> ValidCanSendOrderStatusCodes
		{
			get
			{
				return new List<ZString>
				{
					ZString.Empty,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder
				};
			}
		}

		internal List<ZString> ValidCanSendOriginalStatusCodes
		{
			get
			{
				return new List<ZString>
				{
					ZString.Empty,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected
				};
			}
		}

		internal List<ZString> ValidCanSendAmendmentStatusCodes
		{
			get
			{
				return new List<ZString>
				{
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate
				};
			}
		}

		internal List<ZString> ValidCanSendWithdrawalStatusCodes
		{
			get
			{
				return new List<ZString>
				{
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted
				};
			}
		}

		internal List<ZString> ValidCanSendTransferStatusCode
		{
			get
			{
				return new List<ZString>
				{
					ZString.Empty,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.ReviewInReview,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended
				};
			}
		}

		internal List<ZString> ValidCanSendTransferInStatusCode
		{
			get
			{
				return new List<ZString>
				{
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder
				};
			}
		}

		internal List<ZString> ValidCanSendCopyStatusCode
		{
			get
			{
				return new List<ZString>
				{
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CtrdCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InitInitial,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.OrdrOrder,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended
				};
			}
		}

		internal List<ZString> ValidCanSendCertRequestStatusCodes
		{
			get { return new List<ZString> { ZString.Empty }; }
		}

		List<ZString> ValidCanSendRequestReissueCertificateStatusCodes
		{
			get
			{
				return new List<ZString>
				{
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.CompCompleted,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.EmhcEmergencyHealthCertificate,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.FinlFinal,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.HcrdHealthCertificateReady,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.InspInspected,
					EXDOCComplianceStatusCodesForCusEntryNumber.Codes.SuspSuspended
				};
			}
		}

		List<ZString> ValidCanSendReadREXStatusCodes
		{
			get
			{
				var result = new List<ZString>();
				result.AddRange(new EXDOCComplianceStatusCodesForCusEntryNumber().GetAllCodes().Select(code => new ZString(code)));
				return result;
			}
		}

		#endregion

		#region OnSent

		protected override void OnAmendmentSentCore()
		{
			base.OnAmendmentSentCore();
			exDocHeader.Messages.Load();
		}

		protected override void OnOriginalSentCore()
		{
			base.OnOriginalSentCore();
			exDocHeader.Messages.Load();
		}

		protected override void OnWithdrawalSentCore()
		{
			base.OnWithdrawalSentCore();
			exDocHeader.Messages.Load();
		}

		#endregion

		readonly QuarantineExDocHeader exDocHeader;
		readonly string messageType;
		public string Notification { get; set; }

		#endregion
	}
}
