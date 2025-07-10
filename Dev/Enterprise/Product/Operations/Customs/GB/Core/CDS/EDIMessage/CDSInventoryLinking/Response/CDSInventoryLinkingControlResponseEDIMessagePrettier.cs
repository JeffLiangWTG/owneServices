using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.CDS.CodeDescriptionPairLists;
using static System.FormattableString;

namespace Enterprise.Customs.GB.CDS
{
	public class CDSInventoryLinkingControlResponseEDIMessagePrettier : CDSEDIMessagePrettier<CDSInventoryLinkingControlResponseEDIMessage>
	{
		public CDSInventoryLinkingControlResponseEDIMessagePrettier(CDSInventoryLinkingControlResponseEDIMessage responseMessage) : base(responseMessage)
		{
			response = Message.MessageDataObject;
		}

		public override ZString MakeHumanReadable()
		{
			var interpretation = MessagePrettierCss.CSS
				+ ToH3IfNotEmpty(Invariant($"Inventory Linking Control Response from CDS:"))
				+ ToKeyValuePairSection(new (ZString key, ZString value)[]
				{
					("UCR", response.UCR),
					("Action Code", Invariant($"{response.ActionCode} - {new CDSActionCodeList().GetDescriptionFromCode(response.ActionCode)}")),
					("Error Code", FormatErrorMessages(response.ErrorCodes)),
					("Movement Reference", response.MovementReference)
				});

			return interpretation;
		}

		ZString FormatErrorMessages(ZString[] errors)
		{
			var stringBuilder = new ZStringBuilder();
			foreach (var error in errors)
			{
				stringBuilder.Append(FormatErrorMessage(error));
			}
			return stringBuilder.ToStringWithDelimiterBetweenAppends(BR);
		}

		ZString FormatErrorMessage(ZString error)
		{
			var result = error;
			var segments = error.Split(" ");
			if (segments.Length > 0)
			{
				var errorCode = segments[0];
				var errorDescription = new CDSErrorCodeList().GetDescriptionFromCode(errorCode);
				if (!string.IsNullOrEmpty(errorDescription))
				{
					var stringBuilder = new ZStringBuilder();
					stringBuilder.Append(errorCode);
					stringBuilder.Append(errorDescription);
					if (segments.Length > 1)
					{
						stringBuilder.Append(error.Substring(errorCode.Length + 1));
					}
					result = stringBuilder.ToStringWithDelimiterBetweenAppends(" - ");
				}
			}
			return result;
		}

		readonly InventoryLinkingControlResponse response;
	}
}
