using System.Text;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.Incoming;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public class ImportCommonMessagePrettyFormatter<TResponse> : ImportGenericCommonMessagePrettyFormatter<TResponse>
		where TResponse : IImportCommon, IMRNField
	{
		public ImportCommonMessagePrettyFormatter(TResponse response, CusEntryHeader entryHeader) : base(response, entryHeader)
		{
		}

		protected override ZString GetCircuitCan() => response.CircuitCanSpecified ? circuitoTdList.ContainsKey(response.CircuitCan) ? GetColourCircuitStringXML(response.CircuitCan) : ZString.Empty : ZString.Empty;

		protected override ZString GetReleaseDateString() => response.ReleaseDate + response.ReleaseTime;

		protected override ZString GetAcceptanceDateString() => response.AcceptanceDate + response.AcceptanceTime;

		protected override void AppendDescriptionDataIfNotEmpty(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response.RegisteredOperationDescription))
			{
				AppendDataInNewTableIfNotEmpty(messageDetails, DescriptionText, "(" + response.RegisteredOperationCode + ")" + response.RegisteredOperationDescription);
			}
		}

		protected override string GetReleaseDateFormat() => CustomsDateTimeExtension.DateTimeFormatLongWithSeconds;

		protected override void AppendMrnDataIfNotEmpty(StringBuilder messageDetails, HtmlTableCreator tableCreatorExternal)
		{
			if (!string.IsNullOrEmpty(response.MRN))
			{
				WriteRowIfNotEmpty(tableCreatorExternal, ReferenceText, response.MRN);
			}
		}

		protected override void AppendAcceptanceDataIfNotEmpty(StringBuilder messageDetails, ZString serviceSegmentText, HtmlTableCreator tableCreatorExternal)
		{
			if (!string.IsNullOrEmpty(serviceSegmentText))
			{
				ZDateTime.TryParseExact(serviceSegmentText, out var acceptanceDate, CustomsDateTimeExtension.DateTimeFormatLongWithSeconds);

				WriteRowIfNotEmpty(tableCreatorExternal, AcceptanceText, acceptanceDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());
			}
		}
	}
}
