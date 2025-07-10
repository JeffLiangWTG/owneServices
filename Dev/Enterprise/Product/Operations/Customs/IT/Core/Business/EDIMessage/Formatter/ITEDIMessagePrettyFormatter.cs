using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

abstract class ITEDIMessagePrettyFormatter : IITEDIMessagePrettyFormatter
{
	protected ITEDIMessagePrettyFormatter(BusinessObjectFactory factory)
	{
		this.factory = Argument.NotNull(factory, nameof(factory));
	}

	protected readonly BusinessObjectFactory factory;

	public ZString GetFormattedText(ZString originalText)
	{
		if (originalText.IsEmpty)
		{
			return WrapTextInH2(MessageIsEmptyCaption);
		}

		try
		{
			return GetFormattedTextCore(originalText);
		}
		catch (CustomsMessageProcessorException customsMessageProcessorException)
		{
			return WrapTextInH2(customsMessageProcessorException.Message);
		}
		catch (Exception ex) when (!ex.IsCriticalException())
		{
			return new ZStringBuilder()
				.Append(WrapTextInH2(UnableToParseTheReceivedMessageCaption))
				.Append("<br />")
				.Append(WrapTextInH2(UseMessageTextTabCaption))
				.ToString();
		}
	}

	protected abstract ZString GetFormattedTextCore(ZString originalText);

	static ZString MessageIsEmptyCaption => Res.GetString("4A8D238E-5B0A-4431-9F19-CDFF86584F9B", "Message is empty");
	static ZString UnableToParseTheReceivedMessageCaption => Res.GetString("D036F50D-87A2-4038-ABF9-68662E641EFC", "The message formatter was unable to parse received message.");
	static ZString UseMessageTextTabCaption => Res.GetString("FF2E1D79-9D66-439F-80E7-ED5A69EF1D55", "Please use \"Message Text\" tab page to inspect the raw message content.");

	static string WrapTextInH2(ZString contentToWrap) => FormattableString.Invariant($"<h2>{contentToWrap}</h2>");
}
