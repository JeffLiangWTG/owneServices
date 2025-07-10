using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.xTMessaging.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IE.Messaging
{
	public static class InterchangeCreator
	{
		public static EDIInterchange CreateOutgoingInterchange(BusinessObjectFactory factory, ZString applicationCode, ZString interchangeType, ZGuid branchPK, ZString webServiceEndPoint, ZString bodyText, ZString? transactionID = null, ZGuid? credentialPK = null)
		{
			var outgoingInterchange = factory.New<EDIInterchange>();
			outgoingInterchange.EI_ApplicationCode = applicationCode;
			outgoingInterchange.EI_InterchangeType = interchangeType;
			outgoingInterchange.EI_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			outgoingInterchange.EI_TransportType = EDIInterchangeTransportTypeList.Codes.xT;
			outgoingInterchange.EI_SessionGUID = ZGuid.NewZGuid();
			outgoingInterchange.EI_Priority = EDIInterchangePriorityList.Codes.High;
			outgoingInterchange.EI_IsActive = true;
			outgoingInterchange.EI_To = GetToRecipient();
			outgoingInterchange.EI_GB = branchPK;
			var company = outgoingInterchange.Company;
			outgoingInterchange.EI_From = company.LicenceKeyIdentifier;
			outgoingInterchange.EI_GP = credentialPK ?? company.GetCredentialPK();
			outgoingInterchange.EI_BodyText = bodyText;
			outgoingInterchange.EI_Status = EDIInterchange.Status.Queued;
			var dictionary = new Dictionary<string, string>()
				{
					{ CustomMsgAttributes.Endpoint, webServiceEndPoint }
				};
			if (transactionID.HasValue)
			{
				dictionary.Add(CustomMsgAttributes.Transaction, transactionID.Value);
			}
			outgoingInterchange.SetHeaderTextWithAttributeDictionary(dictionary);
			return outgoingInterchange;
		}

		public static class CustomMsgAttributes
		{
			public const string Endpoint = "custom.IE.Endpoint";
			public const string Transaction = "custom.IE.Transaction";
			public const string SigningOption = "custom.IE.SigningOption";
			public const string Rest = nameof(Rest);
		}

		public static string GetToRecipient() => EnvProxy.Instance.IsProductionSystem ? IERevenueOnlineSystem : IECustomsTest;

		public const string IERevenueOnlineSystem = "IECustomsROS";
		public const string IECustomsTest = "IECustomsTest";
		public static readonly Regex HttpSchemesRegex = new Regex(@$"^({Uri.UriSchemeHttps}|{Uri.UriSchemeHttp})://", RegexOptions.IgnoreCase | RegexOptions.Compiled);
	}
}
