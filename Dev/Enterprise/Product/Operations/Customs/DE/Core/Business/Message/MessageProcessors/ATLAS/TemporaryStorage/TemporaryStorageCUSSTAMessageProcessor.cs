using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public sealed class TemporaryStorageCUSSTAMessageProcessor : TemporaryStorageMessageProcessor<AtlasInboundEDIMessage<ICUSSTA>, ICUSSTA>
	{
		public TemporaryStorageCUSSTAMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string MessageFriendlyNameCore => Res.GetString("06BCF2CF-7672-47F4-9675-0FFF9914AC01", "Temporary Storage CUSSTA Message Processor");

		protected override bool MustHaveLinkedObject => false;

		protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendTemporaryStorageUnsolicited;

		protected override BusinessObject GetLinkedObject(AtlasInboundEDIMessage<ICUSSTA> message) => null;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, AtlasInboundEDIMessage<ICUSSTA> message)
		{
			var status = EDIMessage.Status.Discarded;
			var dataProvider = message.DataProvider;
			if (dataProvider != null)
			{
				status = EDIMessage.Status.ProcessedOK;

				var missingRegisters = new ZStringBuilder();
				var invalidPackageQty = new ZStringBuilder();
				foreach (var goodsItem in dataProvider.GoodsItems)
				{
					var regHeader = (goodsItem.ReferencedRegistrationNumber.IsEmpty ? null : CusTempStorageRegHeader.Load(factory, goodsItem.ReferencedRegistrationNumber))
									?? (goodsItem.MRN.IsNullOrEmpty() ? null : CusTempStorageRegHeader.Load(factory, goodsItem.MRN));
					var regLine = regHeader?.GetRegLine(goodsItem.ReferencedSequenceNumber);
					if (regLine != null)
					{
						SubscribeDocumentLinking(regHeader);
						if (Custodian_IsCW1MessagingOrganizationAndInterchangeRecipient_Or_UnknownOrganization(factory, regLine.SRL_CustodianIdentifier, regLine.SRL_CustodianIdentifierBranchNo, dataProvider.InterchangeRecipientReferenceNumber, dataProvider.InterchangeRecipientSubsidiaryNumber)
							&& goodsItem.Quantity > 0)
						{
							var commentStatus = goodsItem.CancellationFlag ? Res.GetString("fa3fd13d-3074-45c6-bc60-5f38fbd322db", "Prohibited") : Res.GetString("ec4fa972-c6ce-433b-85d7-3f38e7d8b23d", "Authorized");
							var comment = Res.GetString("4497268a-2975-4dc1-afdd-db12197b5867", "Re-Export {0} for: {1} items", commentStatus, goodsItem.Quantity);
							CreateRegLineTransaction(regLine, ZInt.Zero, ZString.Empty, ZString.Empty, TransactionTypes.Codes.StatusChange, dataProvider.MessageIdentifier, ZDecimal.Zero, comment, invalidPackageQty);
						}
					}
					else
					{
						var referenceNumbers = string.Join(", ", goodsItem.ReferencedRegistrationNumber.Yield().Append(goodsItem.MRN).Where(x => !x.IsEmpty));

						missingRegisters.Append(Res.GetString("ad457a72-e748-4166-8afb-f9bf75c3705a", "Reference Numbers: {0}; Sequence Number: {1}", referenceNumbers, goodsItem.ReferencedSequenceNumber));
					}
				}
				FinalizeNoteText(missingRegisters, MissingRegistersNoteText);
				factory.CreateStmNoteForEdiMessage(message.PK, missingRegisters.ToStringWithNewLineBetweenAppends());

				var additionalNumber = new ZString[] { dataProvider.AdditionalReferenceNumber };
				var referencedRegistrationNumbers = dataProvider.GoodsItems
					.SelectMany(x => x.ReferencedRegistrationNumber.Yield().Append(x.MRN));
				message.SetLogbookRegistrationNumber(referencedRegistrationNumbers.Concat(additionalNumber));
				SendEmail(message);
			}
			message.EM_Status = status;
		}

		protected override EmailDef GenerateEmail(string uri, string jobNumber, string messageTypeInSubject, string body, bool isFailure, IGlbBranch branchForEmailLogo)
		{
			var result = base.GenerateEmail(uri, jobNumber, messageTypeInSubject, body, isFailure, branchForEmailLogo);
			AttachDocumentsToEmail(result, attachedDocumentsCached);
			return result;
		}

		void SendEmail(AtlasInboundEDIMessage<ICUSSTA> message)
		{
			attachedDocumentsCached = message.AttachedDocuments.Where(x => x.Type.Code.HasValue && ((string)x.Type.Code).Equals(Core.Constants.RefDocTypes.CustomsAuthority, StringComparison.OrdinalIgnoreCase)).ToArray();
			GenerateHtmlEmailAndSendToOriginalOrGroup(message.Factory
				, null
				, Res.GetString("373C3A0A-FBEE-41D8-8D59-8C153938BC59", "SumA CUSSTA - Loading permit status message")
				, GetEmailBody(message.DataProvider)
				, false
				, message.Branch
				, null
				, () => null);
		}

		static string GetEmailBody(ICUSSTA provider)
		{
			var htmlBody = new StringBuilder();
			htmlBody.Append(Res.GetString("44C0788A-9861-47DB-AC4A-11D500402525", @"You received a Loading permit status message."));
			htmlBody.Append("<br />");
			htmlBody.Append("<br />");
			var tableCreator = new HtmlTableCreator();
			tableCreator.WriteRow(Res.GetString("F93EA14A-0A6A-4769-9188-9DBEEE27C889", "Registration Number"), provider.AdditionalReferenceNumber);
			htmlBody.Append(tableCreator.ToHtml());
			return htmlBody.ToString();
		}

		IReadOnlyCollection<AttachedDocument> attachedDocumentsCached;
	}
}
