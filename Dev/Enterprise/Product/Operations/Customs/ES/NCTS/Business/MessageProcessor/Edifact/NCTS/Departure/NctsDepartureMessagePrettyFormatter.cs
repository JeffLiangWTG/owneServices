using System.Text;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.NCTS.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.NCTS.Business
{
	public class NctsDepartureMessagePrettyFormatter : EdiFactV921ESMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public NctsDepartureMessagePrettyFormatter(INctsDepartureAndTIRResponseMessageProvider messageHelperProvider) : base(messageHelperProvider)
		{
			messageProvider = messageHelperProvider;
		}
		readonly INctsDepartureAndTIRResponseMessageProvider messageProvider;

		protected override void AppendExtraData(StringBuilder messageDetails)
		{
			AppendLimitDateOfArrivalIfNotEmpty(messageDetails);
		}

		void AppendLimitDateOfArrivalIfNotEmpty(StringBuilder messageDetails)
		{
			if (!messageProvider.TransitMaxDate.IsEmpty)
			{
				AppendDataInNewTableWithBlankLineIfNotEmpty(messageDetails, LimitDateOfArrivalText, messageProvider.TransitMaxDate.ToCustomsFormatDateStringddMMyyyyWithDash());
			}
		}

		protected override ZBool HasAllClearanceData() => !messageProvider.CSVReleaseCode.IsEmpty || !messageProvider.CSVReleaseCreationDate.IsEmpty || !messageProvider.CustomsClearanceCriteria.IsEmpty || !messageProvider.PrintActionRequired.IsEmpty;

		protected override ZString GetClearanceCriteria()
		{
			return ((string)messageProvider.CustomsClearanceCriteria switch
			{
				ClearanceCriteriaCodeList.Codes.SimplifiedProcedure => ClearanceCriteriaCodeList.Descriptions.SimplifiedProcedure,
				ClearanceCriteriaCodeList.Codes.NormalProcedure => ClearanceCriteriaCodeList.Descriptions.NormalProcedure,
				_ => ZString.Empty,
			}).Replace('[', '(').Replace(']', ')');
		}

		protected override ZString GetPrintProcedure()
		{
			return (string)messageProvider.PrintActionRequired switch
			{
				TADPrintProcedureCodeList.Codes.OperatorMustPrintOutCopyAOfTad => ResString.GetMultilingualString("BD58ED99-4C90-414B-9560-7C849BC712CB", "Copy \"A\" of TAD"),
				TADPrintProcedureCodeList.Codes.OperatorMustPrintOutCopiesAAndBOfTad => ResString.GetMultilingualString("23C5053F-05DB-408B-A2BA-C9CD92CE0867", "Copies \"A\" and \"B\" of TAD"),
				TADPrintProcedureCodeList.Codes.CustomsMustPrintOutTad => ResString.GetMultilingualString("E78365BC-8680-421A-9F23-456D91318B40", "Customs must print TAD"),
				_ => ZString.Empty,
			};
		}
	}
}
