using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSCancelDeclarationEDIMessagePrettier : CDSEDIMessagePrettier<CDSCancelDeclarationEDIMessage>
	{
		public CDSCancelDeclarationEDIMessagePrettier(CDSCancelDeclarationEDIMessage message) : base(message)
		{
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

			return MessagePrettierCss.CSS +
					ToH1IfNotEmpty("Request to Cancel") +
					ToKeyValuePairSection(new (ZString key, ZString value)[]
					{
						("MRN", mrn),
						("Functional Reference ID", functionalReferenceId),
						("Reason Code", reasonCodeDisplay.TrimEnd(' ', '-')),
						("Reason Description", reasonDesc)
					});
		}
	}
}
