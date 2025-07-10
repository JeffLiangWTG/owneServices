using System.Text;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageProcessors;

namespace Enterprise.Customs.ES.Business
{
	public abstract class EdiFactV921ESMessagePrettyFormatter : EdiFactMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public EdiFactV921ESMessagePrettyFormatter(ICUSRESV921ESMessageProvider messageHelperProvider) : base(messageHelperProvider)
		{
			messageHelper = Argument.NotNull(messageHelperProvider, nameof(messageHelperProvider));
		}
		readonly ICUSRESV921ESMessageProvider messageHelper;

		protected override ZString CreateMessageDetailsAcceptedCore()
		{
			var messageDetails = new StringBuilder();
			messageDetails.Append(AcceptedDeclarationText);

			AppendRegisterDataIfNotEmpty(messageDetails);
			AppendExtraData(messageDetails);
			AppendCircuitIfNotEmpty(messageDetails, GetCircuit());
			AppendClearanceDataIfNotEmpty(messageDetails);

			return messageDetails.ToString();
		}

		protected virtual void AppendExtraData(StringBuilder messageDetails) { }

		protected void AppendRegisterDataIfNotEmpty(StringBuilder messageDetails)
		{
			if (!messageHelper.AdmissionDate.IsEmpty || !messageHelper.RegistrationNumber.IsEmpty)
			{
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, AcceptanceText, messageHelper.AdmissionDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());
				WriteRowIfNotEmpty(tableCreator, ReferenceText, messageHelper.RegistrationNumber);

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected void AppendClearanceDataIfNotEmpty(StringBuilder messageDetails)
		{
			if (HasAllClearanceData())
			{
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, CSVClearanceText, messageHelper.CSVReleaseCode);
				WriteRowIfNotEmpty(tableCreator, ReleaseDateText, messageHelper.CSVReleaseCreationDate.ToCustomsFormatDateStringddMMyyyyHHmmssWithDash());
				WriteRowIfNotEmpty(tableCreator, ClearanceCriteria, GetClearanceCriteria());
				WriteRowIfNotEmpty(tableCreator, PrintProcedure, GetPrintProcedure());

				messageDetails.Append(blankLine);
				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		protected virtual ZBool HasAllClearanceData() => !messageHelper.CSVReleaseCode.IsEmpty || !messageHelper.CSVReleaseCreationDate.IsEmpty || !messageHelper.PrintActionRequired.IsEmpty;

		protected virtual ZString GetClearanceCriteria() => ZString.Empty;

		protected virtual ZString GetPrintProcedure() => ZString.Empty;

		string ClearanceCriteria => ResString.GetMultilingualString("BF021E97-538F-43B3-9DCC-281ACC721D50", "Clearance Criteria:");
		string PrintProcedure => ResString.GetMultilingualString("49D213B9-F4A3-4CF2-AE00-55732F3BD522", "Print Procedure:");
	}
}
