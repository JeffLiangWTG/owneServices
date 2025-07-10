using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.DE.Business.ESumA
{
	public abstract class ESumAMessageProcessor<TEDIMessage, TDataProvider> : DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage>
		where TDataProvider : IESumADataProvider
		where TEDIMessage : AtlasInboundEDIMessage<TDataProvider>
	{
		protected ESumAMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.DECustomsAtlasSystem;

		protected override List<AttachedDocument> GetAttachedDocuments(TEDIMessage message) => message.AttachedDocuments;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => ZGuid.Invalid;

		protected override bool MustHaveLinkedObject => false;

		protected override BusinessObject GetLinkedObject(TEDIMessage message) => null;

		protected override ZString GetMessageIdentifier(TEDIMessage message) => ZString.Empty;

		protected override void ProcessMessageCore(BusinessObjectFactory factory, TEDIMessage message)
		{
			var dataProvider = (IESumADataProvider)message.DataProvider;
			var status = EDIMessage.Status.Discarded;
			if (dataProvider != null)
			{
				var interchangeRecipientEORInumber = dataProvider.InterchangeRecipientReferenceNumber;
				var orgHeader = factory.GetOrgHeaderFromEoriCode(interchangeRecipientEORInumber);
				if (orgHeader != null)
				{
					var interchangeRecipientEORIbranch = dataProvider.InterchangeRecipientSubsidiaryNumber;
					var premissesAddress = orgHeader.GetOrgCusCode(GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix, interchangeRecipientEORIbranch)?.PremisesAddress;
					if (premissesAddress != null)
					{
						var eMailAddress = premissesAddress.OA_Email;
						if (!eMailAddress.IsEmpty)
						{
							var referenceNumber = dataProvider.ReferenceNumber;
							if (new HtmlResponseEmailGenerator().TryGenerateEmail(ZString.Empty
								, ZString.Empty
								, Res.GetString("F5C55F06-7634-491C-83AF-9C9A5F48F1AB", "ESumA Declaration Message Status")
								, GetEmailBody(referenceNumber, dataProvider.LocalReferenceNumber)
								, false
								, out var email
								, message.Branch))
							{
								AttachDocumentsToEmail(email, message.AttachedDocuments);
								SetupEmailAndSendToOriginalOrGroup(factory, email, message.Branch, null, () => eMailAddress);
								message.SetLogbookRegistrationNumber(referenceNumber);
								status = EDIMessage.Status.ProcessedOK;
							}
							else
							{
								factory.CreateStmNoteForEdiMessage(message.PK, (NoResString)"Could not generate Email");// Debug Note
							}
						}
						else
						{
							factory.CreateStmNoteForEdiMessage(message.PK, GetStmNoteText(FormattableString.Invariant($"recipients Email-Address for Organization-Code: '{orgHeader.OH_Code}', Address: '{premissesAddress.OA_Address1}, {premissesAddress.OA_City}'")));// Debug Note
						}
					}
					else
					{
						factory.CreateStmNoteForEdiMessage(message.PK, GetStmNoteText(FormattableString.Invariant($"premisses address for Organization-Code: '{orgHeader.OH_Code}' and EORIBranch: '{interchangeRecipientEORIbranch}'")));// Debug Note
					}
				}
				else
				{
					factory.CreateStmNoteForEdiMessage(message.PK, GetStmNoteText(FormattableString.Invariant($"Organization with EORINumber: '{interchangeRecipientEORInumber}'")));// Debug Note
				}
			}
			message.EM_Status = status;
		}

		ZString GetEmailBody(ZString referenceNumber, ZString localReferenceNumber)
		{
			var emailBody = new ZStringBuilder();
			emailBody.Append(Res.GetString("39C025AC-EA51-4C38-80B5-4E83977523E4", "MRN: ") + referenceNumber);
			emailBody.Append((NoResString)"</br>"); // Html
			emailBody.Append(Res.GetString("7EDEC4E9-19C2-4C64-94F1-7F71A52E1C02", "Local Reference: ") + localReferenceNumber);
			emailBody.Append((NoResString)"</br></br>"); // Html
			emailBody.Append(Res.GetString("AD8157B8-CDB9-41DD-BA3F-C188FBB48FF4", "For details please see attached PDF-Report."));
			return emailBody.ToString();
		}

		ZString GetStmNoteText(ZString reason) => FormattableString.Invariant($"Couldn't send Email due to missing or invalid {reason}.");// Debug Note
	}
}
