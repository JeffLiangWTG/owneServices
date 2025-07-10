using System;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5UA;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Common;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class ExemptionRequestOfPenalty : NonPersistentBusinessObject
	{
		public ExemptionRequestOfPenalty(EDIMessage message5UA) : base(message5UA.Factory)
		{
			this.message5UA = message5UA;
			Populate(message5UA);
		}

		readonly EDIMessage message5UA;

		[ResourceStringData("ExemptionRequestOfPenalty|AmendmentDeclarationDate", Caption = "Amend Declaration Date")]
		public ZDateTime AmendmentDeclarationDate { get; private set; }

		[ResourceStringData("ExemptionRequestOfPenalty|AmendmentVersionNo", Caption = "Amend Sequence No")]
		public ZInt AmendmentVersionNo { get; private set; }

		public ZString PenaltyType { get; private set; }

		[ResourceStringData("ExemptionRequestOfPenalty|PenaltyTypeDescription", Caption = "Penalty Type")]
		public ZString PenaltyTypeDescription { get; private set; }

		public ZString PenaltyExemptionReasonsCode { get; private set; }

		[ResourceStringData("ExemptionRequestOfPenalty|PenaltyExemptionReasonsCodeDescription", Caption = "Penalty Exemption Reasons Code")]
		public ZString PenaltyExemptionReasonsCodeDescription { get; private set; }

		[ResourceStringData("ExemptionRequestOfPenalty|PenaltyExemptionAmount", Caption = "Penalty Exemption Amount")]
		public ZDecimal PenaltyExemptionAmount { get; private set; }

		[ResourceStringData("ExemptionRequestOfPenalty|AcceptedDate", Caption = "Accepted Date")]
		public ZDateTime AcceptedDate
		{
			get
			{
				if (acceptedDate.IsEmpty)
				{
					var entry = (CusEntryHeader)message5UA.EM_LinkedObject;
					var entryNum5UA = entry?.EntryNumbers.Cast<CusEntryNumber>().FirstOrDefault(x => x.CE_EntryType == ElectronicDocumentTypeList.Codes._5UA && x.CE_EntryLineReference == message5UA.EM_ApplicationReference);
					acceptedDate = entryNum5UA?.CE_IssueDate ?? ZDateTime.Empty;
				}
				return acceptedDate;
			}
		}
		ZDateTime acceptedDate;

		[ResourceStringData("ExemptionRequestOfPenalty|ApprovalDate", Caption = "Review Date")]
		public ZDateTime ReviewDate => MessageData5UB?.ApprovalDate ?? ZDateTime.Empty;

		[ResourceStringData("ExemptionRequestOfPenalty|ResultTypeDescription", Caption = "Review Result Desc.")]
		public ZString ReviewResultDescription => MessageData5UB?.ResultTypeDescription ?? ZString.Empty;

		GOVCBR5UBMessageData MessageData5UB
		{
			get
			{
				if (messageData5UB == null)
				{
					var message5UB = message5UA.EM_LinkUniqueID.GetIncomingMessage(Factory, message5UA.EM_MessageNum, ElectronicDocumentTypeList.Codes._5UB);

					if (message5UB != null)
					{
						using (var reader = message5UB.GetEM_MessageTextReader())
						{
							messageData5UB = new GOVCBR5UBDataProvider().GetMessageData(Factory, reader);
						}
					}
				}
				return messageData5UB;
			}
		}
		GOVCBR5UBMessageData messageData5UB;

		void Populate(EDIMessage message)
		{
			if (message.EM_MessageType != ElectronicDocumentTypeList.Codes._5UA)
			{
				throw new ArgumentException("expected to receive an EDIMessage of '5UA'");
			}

			Declaration declaration = null;

			using (var reader = message.GetEM_MessageTextReader())
			{
				declaration = KRXmlObjectSerializer.DeserializeWithoutSchemaValidation<Declaration>(reader);
			}

			if (DateTime.TryParseExact(declaration.Amendment.AmendmentDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				AmendmentDeclarationDate = dt;
			}
			AmendmentVersionNo = int.Parse(declaration.Amendment.Pointer.SequenceNumeric.ToString());
			PenaltyType = declaration.SubTypeCode.Value;
			PenaltyTypeDescription = Factory.GetCachedValue<PenaltyExemptionCodeList>().GetDescriptionFromCode(PenaltyType);
			PenaltyExemptionReasonsCode = declaration.ReasonCode.Value;
			PenaltyExemptionReasonsCodeDescription = Factory.GetCachedValue<PenaltyExemptionReasonCodeList>().GetDescriptionFromCode(PenaltyExemptionReasonsCode);
			PenaltyExemptionAmount = declaration.InvoiceAmount.Value;
		}
	}
}
