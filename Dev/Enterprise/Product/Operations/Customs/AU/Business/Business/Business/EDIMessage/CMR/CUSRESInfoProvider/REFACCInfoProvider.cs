using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class REFACCInfoProvider : D99BCUSRESInfoProvider
	{
		public REFACCInfoProvider(CUSRESMessage edifactMessage, CMRCUSRESMessage message)
			: base(edifactMessage)
		{
			var consolidatedDeclaration = message.EM_LinkedObject as ConsolidatedDeclaration;
			this.entryHeader = consolidatedDeclaration != null ? ((JobDeclaration)consolidatedDeclaration.LeadDeclaration).EntryHeader : message.EM_LinkedObject as CusEntryHeader;
			this.message = message;
		}

		public override ZString DocumentName
		{
			get { return "REFACC"; }
		}

		#region EFTRunNumber

		public ZString EFTRunNumber
		{
			get
			{
				if (fEFTRunNumber.IsEmpty && CUSRES != null)
				{
					foreach (SegmentGroup3 currentGroup3 in CUSRES.Group3)
					{
						if (currentGroup3.RFF[0].Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.BanksCommonTransactionReferenceNumber)
						{
							fEFTRunNumber = currentGroup3.RFF[0].Reference.ReferenceIdentifier;
							break;
						}
					}
				}
				return fEFTRunNumber;
			}
		}
		ZString fEFTRunNumber;

		#endregion

		#region ClaimNumber

		public ZString ClaimNumber
		{
			get
			{
				if (fClaimNumber.IsEmpty && CUSRES != null)
				{
					foreach (FTXSegment fTX in CUSRES.FTX)
					{
						if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.AdditionalInformation)
						{
							fClaimNumber = fTX.TextLiteral.FreeTextValue1;
							break;
						}
					}
				}
				return fClaimNumber;
			}
		}
		ZString fClaimNumber;

		#endregion

		#region Payment Finalised Date

		protected override ZDateTime PaymentFinalisedDateCore
		{
			get { return payInfo == null ? message.EM_MessageDateTime : payInfo.C9_PaymentDate; }
		}

		#endregion

		public ZString ICSReceiptNumber
		{
			get
			{
				return ZString.Empty;
			}
		}

		#region Bank Details

		public ZString BankAccountName
		{
			get
			{
				ZString result = ZString.Empty;
				if (outBoundMessage != null)
				{
					result = outBoundMessage.BankAccountNameInMessage;
					if (result.IsEmpty)
					{
						if (outBoundMessage.PaymentPartyInMessage == PaymentParty.Broker)
						{
							result = "BROKER";
						}
						else if (outBoundMessage.PaymentPartyInMessage == PaymentParty.Importer)
						{
							result = "IMPORTER";
						}
					}
				}
				return result;
			}
		}

		public ZString BankAccountNumber
		{
			get
			{
				return outBoundMessage == null ? ZString.Empty : outBoundMessage.BankAccountNoInMessage;
			}
		}

		public ZString BSBNumber
		{
			get
			{
				return outBoundMessage == null ? ZString.Empty : outBoundMessage.BSBInMessage;
			}
		}

		#endregion

		#region TotalPayable

		public ZDecimal TotalAmountToBeRefunded
		{
			get
			{
				ZDecimal result = 0m;
				if (entryHeader != null)
				{
					result = payInfo != null ? payInfo.C9_PaymentAmount : entryHeader.TotalPayableAdvisedInLastClearanceMessage;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		CMRIMDMessage outBoundMessage
		{
			get
			{
				if (_outBoundMessage == null && entryHeader != null)
				{
					_outBoundMessage = message.GetOutgoingMessageByBGMRefAndVersion(entryHeaderMessages) as CMRIMDMessage;
				}
				return _outBoundMessage;
			}
		}
		CMRIMDMessage _outBoundMessage;

		CusEntryPayInfo payInfo
		{
			get
			{
				if (_payInfo == null && outBoundMessage != null)
				{
					var iMDR = outBoundMessage.GetSingleIncomingMessageByBGMRefVersionAndType(entryHeaderMessages, "IMDR");
					if (iMDR != null)
					{
						_payInfo = entryHeader.EntryPayInfos.GetItemByMessageNum(iMDR.EM_MessageNum);
					}
				}
				return _payInfo;
			}
		}
		CusEntryPayInfo _payInfo;

		EDIMessageCollection entryHeaderMessages => ((IStatusNeedsRecalculationProvider)entryHeader).Messages;

		readonly CusEntryHeader entryHeader;
		readonly CMRCUSRESMessage message;

		#endregion
	}
}
