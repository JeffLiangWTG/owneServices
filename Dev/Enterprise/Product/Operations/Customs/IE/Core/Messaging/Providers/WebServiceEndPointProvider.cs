using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Messaging
{
	public static class WebServiceEndPointProvider
	{
		public static string GetMailboxAcknowledgeURL(BusinessObjectFactory factory) => GetURL(factory, Constants.MailboxAcknowledge);
		public static string GetEMCSMailboxAcknowledgeURL(BusinessObjectFactory factory) => GetURL(factory, EMCSConstants.MailboxAcknowledge);
		public static string GetMailboxCollectURL(BusinessObjectFactory factory) => GetURL(factory, Constants.MailboxCollect);
		public static string GetEMCSMailboxCollectURL(BusinessObjectFactory factory) => GetURL(factory, EMCSConstants.MailboxCollect);
		public static string GetTransactionIDURL(BusinessObjectFactory factory, ZString applicationCode) => GetURL(factory, applicationCode == EDIMessage.ApplicationCodes.IECustomsEMCS ? EMCSConstants.TransactionID : Constants.TransactionID);
		public static string GetCustomsAndExciseReportURL(BusinessObjectFactory factory) => GetURL(factory, Constants.CustomsAndExciseReport);
		public static string GetSuffix(bool isProductionSystem) => isProductionSystem ? "P" : "T";
		static string GetURL(BusinessObjectFactory factory, string configCode)
		{
			return new RefSysConfig.Loader(factory).GetStringValue(configCode + GetSuffix(EnvProxy.Instance.IsProductionSystem));
		}

		public static string GetSubmissionURL(BusinessObjectFactory factory, string applicationCode, string messageType)
		{
			(string configKeyApplicationCode, string configKeyMessageType) = applicationCode switch
			{
				EDIMessage.ApplicationCodes.IECustomsEMCS => (Constants.Submission, messageType),
				EDIMessage.ApplicationCodes.IECustomsPBN => ("SUBP", messageType),
				_ => (applicationCode, Constants.Submission)
			};

			var configKeyProduction = GetSuffix(EnvProxy.Instance.IsProductionSystem);

			const string configKeyTemplate = "IE{0}{1}{2}";
			var configKey = string.Format(configKeyTemplate, configKeyApplicationCode, configKeyMessageType, configKeyProduction);
			return new RefSysConfig.Loader(factory).GetStringValue(configKey);
		}

		public static class EMCSConstants
		{
			public const string MailboxCollect = "IEMAILCLE";
			public const string MailboxAcknowledge = "IEMAILAKE";
			public const string TransactionID = "IETRANIDE";
		}

		public static class Constants
		{
			public const string MailboxCollect = "IEMAILCOL";
			public const string MailboxAcknowledge = "IEMAILACK";
			public const string TransactionID = "IETRANSID";
			public const string Submission = "SUBM";
			public const string CustomsAndExciseReport = "IEIERSUBM";
		}
	}
}
