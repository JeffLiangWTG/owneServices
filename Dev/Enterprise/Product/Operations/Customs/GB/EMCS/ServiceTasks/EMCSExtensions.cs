using System;
using CargoWise.Customs.GB.MessageDefinitions;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.EMCS.ServiceTasks
{
	public static class EMCSExtensions
	{
		public static GBCustomsRequest NewGBCustomsRequest(EDIMessage message)
		{
			GBCustomsRequest request = null;

			var dataProvider = GetRequestDataProvider(message);
			if (dataProvider != null)
			{
				var credentials = dataProvider.Credentials;
				if (!string.IsNullOrEmpty(credentials.Key))
				{
					request = new GBCustomsRequest
					{
						JobNumber = dataProvider.JobNumber,
						Provider = ProviderType.EMCS,
						Credentials = credentials,
						Service = GetServiceType(message),
						ServiceReference = dataProvider.ServiceReference,
						ContentType = "XML",
						Version = emcsVersion
					};
				}
			}
			return request;
		}

		static EMCSRequestDataProvider GetRequestDataProvider(EDIMessage message)
		{
			switch (message.EM_LinkedObject)
			{
				case EMCSJobDeclaration declaration:
					return new EMCSRequestDataProvider(declaration);
			}
			return null;
		}

		internal static ServiceType GetServiceType(EDIMessage message)
		{
			switch (message.EM_MessageType)
			{
				case EMCSGBOutgoingMessageTypeList.Codes.SubmitDraftEAD:
				case EMCSGBOutgoingMessageTypeList.Codes.CancellationOfEAD:
				case EMCSGBOutgoingMessageTypeList.Codes.ChangeOfDestination:
					return ServiceType.Consignor;
				case EMCSGBOutgoingMessageTypeList.Codes.ReportOfReceipt:
				case EMCSGBOutgoingMessageTypeList.Codes.AlertOrRejectionOfAnEAD:
					return ServiceType.Consignee;
				case EMCSGBOutgoingMessageTypeList.Codes.Splitting:
					return ServiceType.IE825;
				case EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnDelayForDelivery:
					return GetExplanationServiceType(message) ?? ServiceType.IE837;
				case EMCSGBOutgoingMessageTypeList.Codes.ExplanationOnReasonForShortage:
					return GetExplanationServiceType(message) ?? ServiceType.IE871;
				case EMCSGBOutgoingMessageTypeList.Codes.PreValidateTrader:
					return ServiceType.PVT;
				default:
					throw new NotSupportedException(FormattableString.Invariant($"Message type {message.EM_MessageType} is not supported."));
			}
		}

		public static ZString Serialize(this GBCustomsRequest requestData) => SerializationHelper.Serialize(requestData);

		static ServiceType? GetExplanationServiceType(EDIMessage message)
		{
			var declaration = message?.EM_LinkedObject as EMCSJobDeclaration;
			return declaration != null ? !declaration.IsConsignee ? declaration.IsConsignor ? ServiceType.Consignor : null : ServiceType.Consignee : null;
		}

		const string emcsVersion = "1.0";
	}
}
