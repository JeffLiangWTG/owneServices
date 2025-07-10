using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class RFPMultiMessageManager : MultiMessageManager
	{
		public RFPMultiMessageManager(BusinessObject masterBusinessObject, string messageType)
		{
			JobDeclaration = (JobDeclaration)masterBusinessObject;
			this.messageType = messageType;

			if (JobDeclaration != null && JobDeclaration.Invoices.Count > 0)
			{
				quarantineExDocHeader = JobDeclaration.Invoices[0].QuarantineExDocHeader;
			}
		}

		public bool SendMessages(Customs.Business.ISendsMessagesToCustoms sender, object additionalInformation = null)
		{
			var result = false;

			if (IsNEXDOCMessageType)
			{
				if (MessageManagerCreditCheckWithSecurityHelper.CheckDeniedParty(JobDeclaration))
				{
					if (messageType == NEXDOCMessageType.Codes.ReissueCertificate)
					{
						result = SendRequestReissueCertificate(additionalInformation);
					}
					else
					{
						result = SendNexdocsMessage();
					}

					if (result)
					{
#pragma warning disable CW1113 // Do Not Show Message Box From Business Layer
						Globals.Message.Show("Message has been generated.");
#pragma warning restore CW1113 // Do Not Show Message Box From Business Layer
					}
				}
			}
			else
			{
				switch (messageType)
				{
					case EXDOCMessageTypeCodes.Codes.ORD:
					case EXDOCMessageTypeCodes.Codes.LDG:
						result = (SendOriginalMessages(sender).Count > 0);
						break;
					case EXDOCMessageTypeCodes.Codes.RPL:
						result = AmendMessages(JobDeclaration.MessageInitiator, AllMessageManagers);
						break;
					case EXDOCMessageTypeCodes.Codes.CAN:
						result = WithdrawMessages(sender);
						break;
					case EXDOCMessageTypeCodes.Codes.TRF:
						result = SendRFPMessages(sender, manager => manager.CanSendTransfer);
						break;
					case EXDOCMessageTypeCodes.Codes.AcceptTransferIn:
					case EXDOCMessageTypeCodes.Codes.DeclineTransferIn:
						result = SendRFPMessages(sender, manager => manager.CanSendTransferIn);
						break;
					case EXDOCMessageTypeCodes.Codes.CPY:
						result = SendRFPMessages(sender, manager => manager.CanSendCopy);
						break;
					case EXDOCMessageTypeCodes.Codes.CRQ:
						result = SendRFPMessages(sender, manager => manager.CanSendCertificateRequest);
						break;
				}
			}

			return result;
		}

		bool SendRequestReissueCertificate(object additionalInformation)
		{
			var result = false;

			if (additionalInformation is CertificateReissueHeader certificateReissueHeader)
			{
				var quarantineHeader = quarantineExDocHeader;
				try
				{
					var reissueRequests = certificateReissueHeader.CertificateReissueRequests.Cast<CertificateReissueRequest>();
					foreach (var reissueRequest in reissueRequests)
					{
						quarantineHeader.ReissueCertificateNameForMessaging = reissueRequest.CertificateNumber;
						quarantineHeader.ReissueCertificateReasonForMessaging = reissueRequest.ReissueReason;
						result |= SendNexdocsMessage(reissueRequest != reissueRequests.First());
					}
				}
				finally
				{
					quarantineHeader.ReissueCertificateNameForMessaging = ZString.Empty;
					quarantineHeader.ReissueCertificateReasonForMessaging = ZString.Empty;
				}
			}

			return result;
		}

		bool IsNEXDOCMessageType => (isNEXDOCMessageType ?? (isNEXDOCMessageType = new NEXDOCMessageType().ContainsCode(messageType))).Value;
		bool? isNEXDOCMessageType;

		bool IsNEXDOCSoapMessageType => NEXDOCMessageType.IsSoapMessageType(messageType);

		ZBool SendNexdocsMessage(bool pending = false)
		{
			if (IsNEXDOCSoapMessageType)
			{
				return new NEXDOCMessageSender(quarantineExDocHeader, messageType).SendMessage();
			}
			else
			{
				return new UniversalShipmentMessageBuilder(quarantineExDocHeader, messageType, new NotificationBuffer()).GenerateMessage(pending);
			}
		}

		#region  Can Send Properties

		public bool CanSendOrderMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendOrder); }
		}

		public bool CanSendOriginalMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendOriginal); }
		}

		public bool CanSendCertificateRequestMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendCertificateRequest); }
		}

		public bool CanSendAmendmentMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendAmmendment); }
		}

		public bool CanSendWithdrawlMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendWithdrawal); }
		}

		public bool CanSendTransferMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendTransfer); }
		}

		public bool CanSendTransferInMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendTransferIn); }
		}

		public bool CanSendForwardMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendForward); }
		}

		public bool CanSendCopyMessage
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendCopy); }
		}

		public bool CanSendNEXDOCMessage
		{
			get { return HasValidNEXDOCTokens; }
		}

		public bool CanSendRequestReplacementCertificate
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendRequestReplacementCertificate); }
		}

		public bool CanSendRequestReissueCertificate
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendRequestReissueCertificate); }
		}

		public bool CanSendPreviewCertificate
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendPreviewCertificate); }
		}

		public bool CanSendTransferEDN
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendTransferOrCancelEDN); }
		}

		public bool CanSendCancelEDN
		{
			get { return CanSendMessage(singleManager => singleManager.CanSendTransferOrCancelEDN); }
		}

		public bool CanSendNEXDOCCancellation => CanSendMessage(singleManager => singleManager.CanSendNEXDOCCancellation);

		public bool CandSendReadREX => CanSendMessage(singleManager => singleManager.CanSendReadREX);

		#endregion

		public string Notifications
		{
			get { return notifications.ToStringWithNewLineBetweenAppends(); }
		}

		public override IMessageManageableBizObj TopLevelBizObjToManage
		{
			get { return JobDeclaration; }
		}

		protected override void OnMessagesSent(Customs.Business.ISendsMessagesToCustoms sender)
		{
			base.OnMessagesSent(sender);
			if (JobDeclaration.IsQuarantine && new string[] { EXDOCMessageTypeCodes.Codes.ORD, EXDOCMessageTypeCodes.Codes.LDG, EXDOCMessageTypeCodes.Codes.RPL }.Contains(messageType))
			{
				foreach (var header in JobDeclaration.Invoices.Cast<JobComInvoiceHeader>())
				{
					var lines = header.InvoiceLines;
					var newValue = (lines.Count > 0) ? lines.Max(x => (x as JobComInvoiceLine).JI_LineNo) : 0;
					header.QuarantineExDocHeader.QH_QuarantineMessageMaxLine = Math.Max(header.QuarantineExDocHeader.QH_QuarantineMessageMaxLine, newValue);
				}
			}
		}

		public JobDeclaration JobDeclaration { get; private set; }

		#region Implementation

		protected override SingleMessageManager[] GetAllMessageManagers()
		{
			var result = new List<SingleMessageManager>();
			if (JobDeclaration != null)
			{
				if (JobDeclaration.Invoices.Count == 0)
				{
					notifications.Append("Message cannot be sent until an invoice header is entered");
				}
				foreach (var invoiceHeader in JobDeclaration.Invoices.Cast<JobComInvoiceHeader>())
				{
					result.Add(new RFPMessageManager(invoiceHeader.QuarantineExDocHeader, messageType));
				}
			}

			return result.ToArray();
		}

		protected override bool CanSendOriginal(Customs.Business.ISendsMessagesToCustoms sender, params SingleMessageManager[] managersToSend)
		{
			var result = true;
			if (messageType != EXDOCMessageTypeCodes.Codes.ORD && messageType != EXDOCMessageTypeCodes.Codes.CRQ)
			{
				result = base.CanSendOriginal(sender, managersToSend);
			}

			return result;
		}

		protected override bool SendWheneverPossibleOnceMessagingActive
		{
			get { return true; }
		}

		bool CanSendMessage(CanSendDelegate canSendDelegate)
		{
			var result = false;

			if (!IsNEXDOCMessageType || HasValidNEXDOCTokens)
			{
				foreach (var singleManager in AllMessageManagers.Cast<RFPMessageManager>())
				{
					if (canSendDelegate(singleManager))
					{
						result = true;
						break;
					}
					notifications.AppendIfNotEmpty(singleManager.Notification);
				}
			}

			return result;
		}

		internal bool HasValidNEXDOCTokens
		{
			get
			{
				var hasValidNEXDOCGroupToken = AUCustomsDataRegistry.Instance.NEXDOCSGroupToken.Value.PasswordStatus == Core.Constants.PasswordOK;
				if (!hasValidNEXDOCGroupToken)
				{
					notifications.Append(Res.GetString("FAA5C26E-A56F-479D-BE35-86AB09F7BE74", "Please register your company for NEXDOC with DAWR, then enter your company's Group Token in the registry under Customs > Australia > NEXDOCS > NEXDOCS Group Token."));
				}

				var hasValidNEXDOCUserToken = AUGlbStaffWrapper.Get(GlbStaff.CurrentUser).NUTPassword.HasValidCredential;
				if (!hasValidNEXDOCUserToken)
				{
					notifications.Append(Res.GetString("68854DAD-7F07-4ABC-87FF-F6BCFC526EC8", "Your NEXDOCS User Token has not been entered or is invalid.  Please enter a valid User Token into your staff record."));
				}

				return hasValidNEXDOCGroupToken && hasValidNEXDOCUserToken;
			}
		}

		bool SendRFPMessages(Customs.Business.ISendsMessagesToCustoms sender, CanSendDelegate canSendDelegate)
		{
			Initialise();
			var result = false;
			var singleMessageManagers = GetSingleMessageManagers(canSendDelegate);
			if (singleMessageManagers.Length > 0)
			{
				result = (SendOriginal(sender, singleMessageManagers).Count > 0);
			}

			return result;
		}

		SingleMessageManager[] GetSingleMessageManagers(CanSendDelegate canSendDelegate)
		{
			var result = new List<SingleMessageManager>();
			foreach (var manager in AllMessageManagers.Cast<RFPMessageManager>())
			{
				if (canSendDelegate(manager))
				{
					result.Add(manager);
				}
			}

			return result.ToArray();
		}

		public override bool ShouldSendMessagesInTestMode
		{
			get { return Env.Registry.AQISMessagingTestMode; }
		}

		readonly string messageType;
		readonly ZStringBuilder notifications = new ZStringBuilder();
		readonly QuarantineExDocHeader quarantineExDocHeader;
		delegate bool CanSendDelegate(RFPMessageManager manager);

		#endregion
	}
}
