using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.CusPollingTransaction;
using CHPasswordStatusList = Enterprise.Customs.CH.Business.UniversalReferenceConstants.PasswordStatusList;

namespace Enterprise.Customs.CH.Business
{
	class TokensRefreshMessageSender : BasePassarCompanyMessageSender
	{
		public TokensRefreshMessageSender(LoggingInformation logger) : base(logger)
		{
		}

		protected override string FriendlyName => (NoResString)"Token Refresh message";

		protected override ZString ApplicationCode => ApplicationCodes.CHCustomsPassar;

		protected override ZString MessageType => MessageTypeCodeList.Codes.TRE;

		protected override ZString MessageSubType => ZString.Empty;

		protected override bool CanSend(GlbCompanyTokenCredentials token)
		{
			var result = false;

			if (token.GP_PasswordStatus == CHPasswordStatusList.Queued)
			{
				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
				query.AddToFilter(EDIMessageSchema.EM_MessageType, MessageType);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, ReceiveTransmitList.Codes.Transmit);
				query.AddToFilter(EDIMessageSchema.EM_LinkTable, token.Company.TableName);
				query.AddToFilter(EDIMessageSchema.EM_LinkUniqueID, token.Company.PK);
				query.OrderBy = EDIMessageSchema.Constants.EM_SystemLastEditTimeUtc + OrderByClause.Descending;
				var ediMessage = token.Factory.LoadTop1<EDIMessage>(query);
				if (ediMessage != null && ediMessage.EM_Status != EDIMessage.Status.Queued && ediMessage.EM_SystemLastEditTimeUtc < ZDateTime.UtcNow.AddMinutes(-10))
				{
					result = true;
				}
			}
			else
			{
				if (token.GP_ExpiryDate <= ZDateTime.UtcNow.AddHours(1))
				{
					result = true;
				}
			}

			return result;
		}

		protected override int SendCore(CancellationToken cancellationToken, GlbCompany company, GlbCompanyTokenCredentials tokenCredentials)
		{
			var requestBody = new TokenRefreshRequestBodyDataProvider(tokenCredentials).GetRequestBody();

			var message = CreateEDIMessage(company.Factory, company, requestBody, ZString.Empty);

			company.Logs.AddNew(Events.CommunicationTokensRefresh, EventReferenceConstants.Reference);

			tokenCredentials.GP_PasswordStatus = CHPasswordStatusList.Queued;

			ZExceptionReporting.ProcessWithSaveExceptionHandling(company.Factory.Save, null);

			return 1;
		}
	}
}
