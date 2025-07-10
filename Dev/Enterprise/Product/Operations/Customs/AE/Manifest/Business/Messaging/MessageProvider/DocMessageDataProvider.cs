using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.AE.Business;
using Enterprise.Edifact.D23A.Elements;
using Enterprise.Edifact.D23A.Segments;
using Enterprise.Edifact.Generic.V4;

namespace Enterprise.Customs.AE.Manifest.Business;

public class DocMessageDataProvider
{
	public DocMessageDataProvider(SupportingDocSendingObject sendingObject)
	{
		SendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}
	SupportingDocSendingObject SendingObject { get; }

	public string SenderIdentification => CUSCARMessageUNBSegment.InterchangeSender.SenderIdentification;

	public string SenderInternalIdentification => CUSCARMessageUNBSegment.InterchangeSender.InterchangeSenderInternalIdentification;

	public string SenderInternalSubIdentification => CUSCARMessageUNBSegment.InterchangeSender.InterchangeSenderInternalSubIdentification;

	public string ReferenceDocumentID => CUSCARMessageBGMSegment.DocumentMessageIdentification.DocumentIdentifier;

	public string DocumentIdentifier => CUSRESMessageBGMSegment.DocumentMessageIdentification.DocumentIdentifier;

	BGMSegment CUSRESMessageBGMSegment => cUSRESMessageBGMSegment ??= Utils.RetrieveBGMSegment(SendingObject.CUSRESMessage.EM_MessageText);
	BGMSegment cUSRESMessageBGMSegment;

	BGMSegment CUSCARMessageBGMSegment => cUSCARMessageBGMSegment ??= Utils.RetrieveBGMSegment(SendingObject.CUSCARMessage.EM_MessageText);
	BGMSegment cUSCARMessageBGMSegment;

	UNBSegment CUSCARMessageUNBSegment => cUSCARMessageUNBSegment ??= Utils.RetrieveUNBSegment(SendingObject.CUSCARMessage.Interchange.EI_BodyText);
	UNBSegment cUSCARMessageUNBSegment;

	public static bool CUSCARMessageMatcher(ZString messageText, ReferenceElements reference)
	{
		if (reference == null)
		{
			return false;
		}

		var bgm = Utils.RetrieveBGMSegment(messageText);
		if (bgm != null)
		{
			if (reference.ReferenceIdentifier == bgm.DocumentMessageIdentification?.DocumentIdentifier && CompareDocVersion(reference.VersionIdentifier, bgm.DocumentMessageIdentification?.VersionIdentifier))
			{
				return true;
			}
		}
		return false;
	}

	static bool CompareDocVersion(string version, string comparedVersion)
	{
		return int.TryParse(version, out var num1) && int.TryParse(comparedVersion, out var num2) && num1 == num2;
	}

	public static bool IsRFICUSRESMessage(ZString messageText)
	{
		var gei = Utils.RetrieveGEISegment(messageText);
		if (gei == null || gei.ProcessingIndicator.ProcessingIndicatorDescriptionCode != "RFI")
		{
			return false;
		}
		return true;
	}

	public static bool GetInterchangeControl(ZString messageText, out string controlReference, out ReferenceElements reference)
	{
		var unb = Utils.RetrieveUNBSegment(messageText);
		if (unb != null && unb.InterchangeControlReference != null)
		{
			reference = Utils.RetrieveRFFSegment(messageText).Reference;
			controlReference = unb.InterchangeControlReference;
			return true;
		}
		controlReference = null;
		reference = null;
		return false;
	}
}
