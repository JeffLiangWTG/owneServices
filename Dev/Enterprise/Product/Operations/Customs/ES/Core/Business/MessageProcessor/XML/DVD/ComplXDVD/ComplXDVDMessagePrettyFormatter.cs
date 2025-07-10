using System.Text;
using CargoWise.Common;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.DeclaComplemVinculV2Sal;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Business
{
	public class ComplXDVDMessagePrettyFormatter : DVDCommonMessagePrettyFormatter, IMessagePrettyFormatter
	{
		public ComplXDVDMessagePrettyFormatter(DeclaComplemVinculV2Sal response)
		{
			this.response = Argument.NotNull(response, nameof(response));
		}
		readonly DeclaComplemVinculV2Sal response;

		public ZString CreateMessageDetailsAccepted(string extraDataFromProcessing = "")
		{
			var messageDetails = new StringBuilder();

			messageDetails.Append(AcceptedDeclarationText);

			AppendDescriptionDataIfNotEmpty(messageDetails);
			messageDetails.Append(blankLine);
			AppendMrnDataIfNotEmpty(messageDetails, response.NumeroReferenciaDvd);
			messageDetails.Append(blankLine);
			AppendCsvElectronicDeclarationDataIfNotEmpty(messageDetails, response.CsVdeDeclaracionElectronica);

			return messageDetails.ToString();
		}

		protected void AppendDescriptionDataIfNotEmpty(StringBuilder messageDetails)
		{
			if (!string.IsNullOrEmpty(response.CodigoOperacionRegistrada))
			{
				messageDetails.Append(blankLine);
				var tableCreator = GetNewNonVisibleTableCreator();

				WriteRowIfNotEmpty(tableCreator, DescriptionText, "(" + response.CodigoOperacionRegistrada + ") " + response.DescripOperacionRegistrada);

				messageDetails.Append(tableCreator.ToHtml());
			}
		}

		public ZString CreateMessageDetailsRejected() => SetMessageDetailsRejectedComplete(response);
	}
}
