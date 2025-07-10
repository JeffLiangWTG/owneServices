using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.Messaging.Integration;
using WTG.Shared.Dash.Common.Services;

namespace Enterprise.DocumentScanning.GUI
{
	public class ParseDocumentMenuItem : eDocMenuItem
	{
		public ParseDocumentMenuItem(EventHandler onClickHandler)
			: base(Constants.ParseDocumentMenuText, onClickHandler)
		{
		}

		public ParseDocumentMenuItem()
			: this(null)
		{
		}

		public override bool GetEnabledStatus(StorageDocsBase selectedElement, bool multipleSelected)
		{
			var enabled = false;
			Caption = Constants.ParseDocumentMenuText;
			if (multipleSelected)
			{
				return enabled;
			}

			// Parsing action is enabled only when the document has a parse type
			// Update the caption text to inform the user if the document will be reparsed, and what ParseType it has
			if (selectedElement?.IsValidForShipamaxParsing() != true)
			{
				return enabled;
			}

			var parseTypeDescription = GetParseTypeDescription(selectedElement);
			var parseStatus = selectedElement?.ActiveShipamaxMessage?.EM_Status ?? ZString.Empty;
			switch (parseStatus)
			{
				case EDIMessageStatusList.Codes.Cancelled:
				case EDIMessageStatusList.Codes.Error:
					Caption = ResString.GetMultilingualString("B9652A83-D319-4C86-8F76-B7BEF271973C", "&Parse as {0}", parseTypeDescription);
					enabled = true;
					break;

				case EDIMessageStatusList.Codes.Sent:
				case EDIMessageStatusList.Codes.PreProcessedOK:
				case EDIMessageStatusList.Codes.ProcessedOK:
					Caption = ResString.GetMultilingualString("B6FA7EF3-9709-4E6C-891F-1DEB0793EA8B", "&Re-parse as {0}", parseTypeDescription);
					enabled = true;
					break;

				default:
					break;
			}

			return enabled;
		}

		public static ZString GetParseTypeDescription(StorageDocsBase doc)
		{
			var parseType = doc?.DocType?.RT_ParseType ?? ZString.Empty;
			var parseTypes = ObjectFactory.Get<IDashParametersService>().SupportedParseTypes;
			return parseTypes.SingleOrDefault(x => x.TypeCode == parseType)?.Description ?? ZString.Empty;
		}
	}
}
