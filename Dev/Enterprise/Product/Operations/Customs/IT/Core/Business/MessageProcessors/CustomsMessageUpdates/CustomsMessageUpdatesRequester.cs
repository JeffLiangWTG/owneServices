using System;
using System.Xml.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IT.Business;

public class CustomsMessageUpdatesRequester
{
	public ZString RequestUpdates(ITEDIMessage message)
	{
		Argument.NotNull(message, nameof(message));
		CheckMessageType(message.EM_MessageType);
		var interchange = Argument.NotNull(message.Interchange as ITEDIInterchange, nameof(message.Interchange));

		var filename = interchange.GetFileNameFromHeaderText();
		var factory = new BusinessObjectFactory();
		var requestInterchange = factory.New<EDIInterchange>();
		requestInterchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.eHub;
		requestInterchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.ITCustomsRequestResponse;
		requestInterchange.IsTransmitInterchange = ZBool.True;
		requestInterchange.EI_From = message.Company.GC_Code;
		requestInterchange.EI_To = "eHub";
		requestInterchange.EI_Status = EDIInterchange.Status.eHubQueued;
		requestInterchange.EI_BodyText = CreateBodyText(filename);
		requestInterchange.EI_TransportType = EDIInterchange.TransportType.eHub;
		factory.Save();
		return filename;
	}

	#region Implementation

	void CheckMessageType(ZString messageType)
	{
		if (messageType != SADConstants.CustomsInterchangeType.IdocR)
		{
			throw new InvalidOperationException(FormattableString.Invariant($"'{messageType}' is not a supported message type."));
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Constant Files, File, Name")]
	ZString CreateBodyText(ZString filename)
	{
		XNamespace requestResponseNamespace = "http://www.wisetechglobal.com/eServices/Schemas/ITCustoms/RequestResponse";

		const string ITCustoms = "ITCustoms";
		const string Files = "Files";
		const string File = "File";
		const string Name = "Name";

		return new XDocument(
				new XElement(requestResponseNamespace + ITCustoms,
					new XElement(requestResponseNamespace + Files,
						new XElement(requestResponseNamespace + File,
							new XElement(requestResponseNamespace + Name, filename)
						)
					)
				)
			).ToString(SaveOptions.DisableFormatting);
	}

	#endregion
}
