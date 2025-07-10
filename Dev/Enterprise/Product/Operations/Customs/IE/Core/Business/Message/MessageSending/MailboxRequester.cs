using System.Linq;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.Business
{
	public static class MailboxRequester
	{
		public static void Request(ILogger logger, CancellationToken cancellationToken)
		{
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var date = ZDate.Today.AddDays(-IECustomsDataRegistry.Instance.MessageProcessingNoOfDays.Value);
			var xml = GetMailboxCollectRequestXml(factory);
			var commonURL = WebServiceEndPointProvider.GetMailboxCollectURL(factory);
			var emcsURL = WebServiceEndPointProvider.GetEMCSMailboxCollectURL(factory);
			foreach ((ZBool isEMCS, ZGuid credentialPK, ZGuid branchPK) in GetBranchPKForIECompaniesHavingMessagesInLastNoOfDays(factory, date))
			{
				cancellationToken.ThrowIfCancellationRequested();
				var newFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				try
				{
					var applicationCode = EDIInterchange.ApplicationCodes.IECustomsCommon;
					var url = commonURL;
					if (isEMCS)
					{
						applicationCode = EDIInterchange.ApplicationCodes.IECustomsEMCS;
						url = emcsURL;
					}
					var interchange = InterchangeCreator.CreateOutgoingInterchange(newFactory, applicationCode, CommonInterchangeTypeList.Codes.MailboxRequest, branchPK, url, xml, credentialPK: credentialPK);
					newFactory.Save();
					logger.Log(LogType.Information, string.Format("Created {0} Interchange '{1}'.", CommonInterchangeTypeList.Codes.MailboxRequest, interchange.EI_InterchangeNum));
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
			}
		}

		public static EDIInterchange RequestForSpecificCredentialPk(ZGuid credentialPK, ZGuid branchPK, bool isEMCS)
		{
			EDIInterchange result = null;
			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			try
			{
				var applicationCode = isEMCS ? EDIInterchange.ApplicationCodes.IECustomsEMCS : EDIInterchange.ApplicationCodes.IECustomsCommon;
				var url = isEMCS ? WebServiceEndPointProvider.GetEMCSMailboxCollectURL(factory) : WebServiceEndPointProvider.GetMailboxCollectURL(factory);
				var xml = GetMailboxCollectRequestXml(factory);
				result = InterchangeCreator.CreateOutgoingInterchange(factory, applicationCode, CommonInterchangeTypeList.Codes.MailboxRequest, branchPK, url, xml, credentialPK: credentialPK);
				factory.Save();
			}
			catch (ZSaveException ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
			}
			return result;
		}

		static ZString GetMailboxCollectRequestXml(BusinessObjectFactory factory)
		{
			var result = new RefSysConfig.Loader(factory).GetStringValue(MailboxCollectRequestConfigCode);
			return result.IsEmpty ? (ZString)MailboxCollectRequestXml : result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Xml string")]
		const string MailboxCollectRequestXml = @"<crq:MailboxCollectRequest xmlns:crq=""http://www.ros.ie/schemas/customs/collectrequest/v1"" />";
		const string MailboxCollectRequestConfigCode = "IEMAILCOLR";

		static (ZBool isEMCS, ZGuid credentialPK, ZGuid branchPK)[] GetBranchPKForIECompaniesHavingMessagesInLastNoOfDays(BusinessObjectFactory factory, ZDate lastSendDate)
		{
			const string sqlText = @"SELECT IsEMCS, CredentialPK, BranchPK
FROM
(
	SELECT IsEMCS, GC_PK, EI_GP AS CredentialPK, MAX(EI_GB) BranchPK, MAX(EI_SystemCreateTimeUtc) LastCreateTime
	FROM
	(
		SELECT CASE WHEN EI_ApplicationCode = 'IEM' THEN 'Y' ELSE 'N' END IsEMCS, GC_PK, EI_GB, EI_SystemCreateTimeUtc, EI_GP
		FROM dbo.EDIInterchange
		INNER JOIN dbo.GlbBranch ON EI_GB = GB_PK
		INNER JOIN dbo.GlbCompany ON GB_GC = GC_PK AND GC_RN_NKCountryCode = 'IE' AND GC_IsActive = 1
		WHERE EI_ApplicationCode IN ('IEE', 'IEI', 'IE5', 'IEM', 'IEN') AND EI_InterchangeType <> 'MBR' AND EI_InterchangeType <> 'TID' AND EI_ReceiveTransmit = 'TRX' AND GB_IsActive = 1 AND EI_IsActive = 1
		AND EI_SystemCreateTimeUtc >= @lastSendDate
	) Data
	WHERE EXISTS(SELECT NULL FROM dbo.GlbExternalPassword WHERE GP_PK = EI_GP AND GP_GC = GC_PK AND GP_PasswordType = CASE WHEN IsEMCS = 'Y' THEN 'IEM' ELSE 'IER' END)
	GROUP BY GC_PK, EI_GP, IsEMCS
) AS BranchData
WHERE NOT EXISTS(SELECT NULL
	FROM dbo.EDIInterchange
	INNER JOIN dbo.GlbBranch ON EI_GB = GB_PK AND GB_GC = GC_PK
	WHERE EI_GP = CredentialPK AND EI_ApplicationCode = CASE WHEN IsEMCS = 'Y' THEN 'IEM' ELSE 'IEC' END AND EI_InterchangeType = 'MBR' AND EI_ReceiveTransmit = 'TRX' AND GB_IsActive = 1 AND EI_IsActive = 1
	AND EI_SystemCreateTimeUtc >= LastCreateTime AND EI_SystemCreateTimeUtc >= @lastRequestSendDate)";
			var collection = new DynamicBusinessObjectCollection(factory);
			var queryParameters = new ZSqlParameterCollection();
			queryParameters.Add("@lastSendDate", lastSendDate, EDIMessageSchema.EM_SystemCreateTimeUtc);
			queryParameters.Add("@lastRequestSendDate", ZDateTime.UtcNow.AddMinutes(-5), EDIInterchangeSchema.EI_SystemCreateTimeUtc);
			collection.Load(sqlText, queryParameters);
			return collection.OfType<DynamicBusinessObject>().Select(x => (new ZBool(x["IsEMCS"]), new ZGuid(x["CredentialPK"]), new ZGuid(x["BranchPK"]))).ToArray();
		}
	}
}
