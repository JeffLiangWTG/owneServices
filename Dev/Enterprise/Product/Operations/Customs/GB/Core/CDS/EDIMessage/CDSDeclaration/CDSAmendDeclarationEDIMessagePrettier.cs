using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.WorldCustomsOrganisation.Constants;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSAmendDeclarationEDIMessagePrettier : CDSEDIMessagePrettier<CDSAmendDeclarationEDIMessage>
	{
		public CDSAmendDeclarationEDIMessagePrettier(CDSAmendDeclarationEDIMessage message, JobDeclarationMessageSendingObject messageSendingObject) : base(message)
		{
			this.messageSendingObject = messageSendingObject;
		}

		CodeDescriptionPairList AmendmentCancellationReasonCodes => Message.Factory.GetCachedValue<AmendmentCancellationReasonCode>();

		public override ZString MakeHumanReadable()
		{
			var messageDataObj = Message.MessageDataObject;
			var metaDec = messageDataObj?.GetDeclaration();

			var mrn = metaDec?.ID?.Value ?? ZString.Empty;
			var functionalReferenceId = metaDec?.FunctionalReferenceID?.Value ?? ZString.Empty;
			var reasonCode = metaDec?.Amendment?.FirstOrDefault()?.ChangeReasonCode?.Value ?? ZString.Empty;
			var reasonCodeDisplay = reasonCode + " - " + AmendmentCancellationReasonCodes.GetDescriptionFromCode(reasonCode);
			var reasonDesc = metaDec?.AdditionalInformation?.FirstOrDefault()?.StatementDescription?.Value ?? ZString.Empty;

			var message = new ZStringBuilder();

			message.Append(MessagePrettierCss.CSS +
					ToH1IfNotEmpty(MessageTitle) +
					ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
						("MRN", mrn),
						("Functional Reference ID", functionalReferenceId),
						("Reason Code", reasonCodeDisplay.TrimEnd(' ', '-')),
						("Reason Description", reasonDesc)
					}));

			var details = new ZStringBuilder();
			AddAdditionsToInterpretation(details);
			AddChangesToInterpretation(details);
			AddDeletionsToInterpretation(details);

			message.Append(ToPIfNotEmpty(details.ToString()));

			return message.ToString();
		}

		void AddDeletionsToInterpretation(ZStringBuilder details)
		{
			if (HasSpecificChanges(AmendmentType.Deletion))
			{
				details.Append(ToH3IfNotEmpty("Deletions"));
				foreach (var amendment in messageSendingObject.AmendmentDetails.Amendments.Differences.Where(x => x.Type == AmendmentType.Deletion))
				{
					details.Append(ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
					("WCOID Path", amendment.WCOIDPointers.FirstOrDefault()),
					("Name Path", Parser.GetFullNamesFromPointers(string.Join("/", amendment.WCOIDPointers)))
					}));
				}
			}
		}

		void AddChangesToInterpretation(ZStringBuilder details)
		{
			if (HasSpecificChanges(AmendmentType.Change))
			{
				details.Append(ToH3IfNotEmpty("Changes"));
				foreach (Change amendment in messageSendingObject.AmendmentDetails.Amendments.Differences.Where(x => x.Type == AmendmentType.Change))
				{
					details.Append(ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
						("WCOID Path", amendment.WCOIDPointers.FirstOrDefault()),
						("Name Path", Parser.GetFullNamesFromPointers(amendment.WCOIDPointers.FirstOrDefault())),
						("New Value", amendment.NewValue)
					}));
				}
			}
		}

		void AddAdditionsToInterpretation(ZStringBuilder details)
		{
			if (HasSpecificChanges(AmendmentType.Addition))
			{
				details.Append(ToH3IfNotEmpty("Additions"));
				foreach (Addition amendment in messageSendingObject.AmendmentDetails.Amendments.Differences.Where(x => x.Type == AmendmentType.Addition))
				{
					foreach (var elementAdded in amendment.ElementsAdded)
					{
						var hasDescendants = elementAdded.Descendants().Any();
						var pointers = hasDescendants ? amendment.WCOIDPointersWithoutLastPointer.FirstOrDefault() : amendment.WCOIDPointers.FirstOrDefault();

						details.Append(ToKeyValuePairSection(new (ZString key, ZString value)[]
						{
							("WCOID Path", pointers),
							("Name Path", Parser.GetFullNamesFromPointers(pointers)),
							("Value", hasDescendants ? string.Join(", ", elementAdded.Descendants().Where(x => !x.HasElements).Select(x => (x.Parent.Name.LocalName == elementAdded.Name.LocalName ? string.Empty : (x.Parent.Name.LocalName + "\\")) +  x.Name.LocalName + " = " + x.Value)) : elementAdded.Value)
						}));
					}
				}
			}
		}

		bool HasSpecificChanges(ZString amendmentType) => messageSendingObject.AmendmentDetails.Amendments?.Differences?.Any(x => x.Type == amendmentType) ?? false;

		PointerParser Parser => parser ?? (parser = new PointerParser());

		protected virtual ZString MessageTitle => "Request to Amend";

		PointerParser parser;
		readonly JobDeclarationMessageSendingObject messageSendingObject;
	}
}
