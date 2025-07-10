using System.Collections.Generic;
using System.Text;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public abstract class NctsMessageProcessor<TEDIMessage, TDataProvider> : DEBranchCustomsApplicationTypeMessageProcessor<TEDIMessage>
		where TDataProvider : IDataProvider
		where TEDIMessage : AtlasInboundEDIMessage<TDataProvider>
	{
		protected NctsMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected sealed override string ApplicationCodeCore => EDIMessage.ApplicationCodes.DECustomsAtlasSystem;

		protected override ZString GetMessageIdentifier(TEDIMessage message) => message.DataProvider?.MessageIdentifier ?? ZString.Empty;

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject)
		{
			var result = ZGuid.Invalid;
			if (linkedObject is NctsHeader header)
			{
				result = header.BH_GB;
			}
			else if (linkedObject is NctsDepartureMovementHeader movementHeader)
			{
				result = movementHeader.Header.BH_GB;
			}
			return result;
		}

		protected override BusinessObject GetDocumentLinkingObject(BusinessObject businessObject) => businessObject is NctsDepartureMovementHeader departureMovement ? departureMovement.Header : businessObject;

		protected override List<AttachedDocument> GetAttachedDocuments(TEDIMessage message) => message.AttachedDocuments;

		protected ZString GetStatusEmailBody(NctsHeader header, ZString mrnNumber, ZString status, string codeType)
		{
			var statusCombinedCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(header.Factory, status, Core.Constants.CountryCodes.Germany, codeType, ZDateTime.UtcNow);
			var statusDescription = statusCombinedCode == null ? ZString.Empty : statusCombinedCode.ZZD_Description;

			var emailBody = new StringBuilder();
			emailBody.Append(ZString.Format(Res.GetString("F93D9D40-F627-4EAF-886A-C54C307BE696", "Your Declaration {0} has a Status Message. For details please follow the Link to the Job.", header.BH_JobReference)) + " <br />");
			emailBody.Append("<br />");

			var emailTable = new HtmlTableCreator();
			emailTable.WriteRow(new string[] { Res.GetString("3AB7B50C-161D-4310-8D1E-90E929CF2682", "MRN"), mrnNumber });
			emailTable.WriteRow(new string[] { Res.GetString("D0C2BE40-8432-4075-BCF8-F9E7CC261EE1", "Status"), status });
			emailTable.WriteRow(new string[] { Res.GetString("9FB4EE31-2E37-4705-A90D-87C821F8942C", "Status Text"), statusDescription });
			emailBody.Append(emailTable.ToHtml());
			return emailBody.ToString();
		}
	}
}
