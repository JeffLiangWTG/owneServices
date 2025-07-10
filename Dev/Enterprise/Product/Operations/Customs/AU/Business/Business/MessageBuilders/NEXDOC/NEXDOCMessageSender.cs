using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business.RexOwnershipSoap;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class NEXDOCMessageSender
	{
		public NEXDOCMessageSender(QuarantineExDocHeader header, string messageType)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.messageType = messageType;
		}

		readonly QuarantineExDocHeader header;
		readonly string messageType;

		#region Constants

		const string RecipientID = "NEXDOCS"; // Xml Content
		const string RecipientIDForTest = "NEXDOCSTest"; // Xml Content

		#endregion

		public ZBool SendMessage()
		{
			var interchange = header.IsNEXDOCSActive ? GenerateNEXDOCInterchange() : null;
			if (interchange != null)
			{
				try
				{
					var declaration = header.Declaration;
					if (declaration != null)
					{
						declaration.JE_MessageStatus = RFPMessage.Status.AwaitingResponse;
					}

					header.Factory.Save();
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}

				header.Messages.Load();
			}

			return interchange != null;
		}

		#region Interchange

		EDIInterchange GenerateNEXDOCInterchange()
		{
			var ediMessage = GetEDIMessage();
			if (ediMessage != null)
			{
				var interchange = header.Factory.New<EDIInterchange>();
				interchange.ContainedMessages.Add(ediMessage);

				interchange.EI_ApplicationCode = ApplicationCodeList.Codes.AUCustomsNEXDOC;
				interchange.EI_InterchangeType = ApplicationCodeList.Codes.AUCustomsNEXDOC;
				interchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;

				interchange.EI_From = GlbCompany.CurrentCompany.LicenceKeyIdentifier;
				interchange.EI_To = AUCustomsDataRegistry.Instance.IsNEXDOCSTestingSystem ? RecipientIDForTest : RecipientID;
				interchange.EI_Status = EDIInterchangeStatusList.Codes.eHubQueued;
				interchange.EI_GB = header.Declaration?.JE_GB ?? GlbBranch.CurrentBranch.PK;

				interchange.EI_BodyText = new NEXDOCAcknowledgeInterchange
				(
					interchange.EI_From,
					interchange.EI_To,
					StaticCurrentFetcher.Instance.CurrentUserCode,
					GetFullMessageType().Name,
					((INEXDOCMessageParent)header).RexNumber,
					header.Declaration?.JE_DeclarationReference ?? ZString.Empty,
					ediMessage.EM_MessageText
				).Serialize();

				return interchange;
			}

			return null;
		}

		XmlEDIMessage GetEDIMessage()
		{
			switch (messageType)
			{
				case NEXDOCMessageType.Codes.REXForward:
					{
						return new NEXDOCForwardMessageBuilder(header).CreateNewMessage();
					}

				case NEXDOCMessageType.Codes.REXTransfer:
					{
						return new NEXDOCTransferMessageBuilder(header).CreateNewMessage();
					}

				case NEXDOCMessageType.Codes.WithdrawalOwnership:
					{
						return new NEXDOCWithdrawMessageBuilder(header).CreateNewMessage();
					}

				default:
					{
						return null;
					}
			}
		}

		Type GetFullMessageType()
		{
			switch (messageType)
			{
				case NEXDOCMessageType.Codes.REXForward:
					{
						return typeof(RexForwardOwnership);
					}

				case NEXDOCMessageType.Codes.REXTransfer:
					{
						return typeof(RexTransferOwnership);
					}

				case NEXDOCMessageType.Codes.WithdrawalOwnership:
					{
						return typeof(RexWithdrawOwnership);
					}

				default:
					{
						return null;
					}
			}
		}

		#endregion
	}
}
