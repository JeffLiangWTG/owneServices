using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Web;
using CargoWise.Billing.Kafka.API;
using Confluent.Kafka;

namespace CargoWise.eHub.Gateway
{
	public class ServiceHelper
	{

		public static string GetClientIPAddressInformation()
		{
			string result = string.Empty;
			try
			{
				var clientIpAddress = GetClientIPAddress();
				if (string.IsNullOrEmpty(clientIpAddress))
				{
					throw new Exception("endpoint.Address is empty");
				}

				result += string.Format("\n\tRequest IP Address is {0}", clientIpAddress);
			}
			catch (Exception e)
			{
				result += "\n\tCan't get request IP address, Reason: " + e.Message;
			}
			return result;
		}


		public static string GetSOAPAction()
		{
			HttpContext httpContext = HttpContext.Current;
			if (httpContext != null)
			{
				string text = httpContext.Request.Headers["SOAPAction"];
				var parts = text.Split('/');
				if (parts.Length > 0) return parts[parts.Length - 1];
			}

			return "";
		}

		public static string GetClientIPAddress()
		{
			HttpContext httpContext = HttpContext.Current;
			if (httpContext != null)
			{
				string ipAddresses = httpContext.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
				if (!string.IsNullOrEmpty(ipAddresses) && !ipAddresses.Equals("unknown", StringComparison.OrdinalIgnoreCase))
				{
					return ipAddresses.Split(',')[0];
				}

				return httpContext.Request.ServerVariables["REMOTE_ADDR"];
			}

			OperationContext operationContext = OperationContext.Current;
			if (operationContext != null)
			{
				RemoteEndpointMessageProperty msgProps = operationContext.IncomingMessageProperties[RemoteEndpointMessageProperty.Name] as RemoteEndpointMessageProperty;
				if (msgProps != null)
				{
					return msgProps.Address;
				}
			}

			return "<UNKNOWN>";
		}
		public static ProducerConfig GetKafkaProducerConfig() 
		{
			var baseClientConfig = GetSettings("billingKafkaClientSettings");
			var producerSettings = GetSettings("billingKafkaProducerSettings");
			return BillingKafkaClient.GetKafkaConfig<ProducerConfig>(baseClientConfig, producerSettings);
		}
		private static Dictionary<string, string> GetSettings(string sectionName)
		{
			var settings = (Hashtable)ConfigurationManager.GetSection(sectionName);
			var settingsDic = settings.Cast<DictionaryEntry>().ToDictionary(x => x.Key.ToString(), y => y.Value.ToString());
			return settingsDic;
		}
	}
}
