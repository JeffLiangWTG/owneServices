using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Grpc.Core;
using Newtonsoft.Json;

namespace Enterprise.xTMessaging.Shared
{
	public static class Utils
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string ReceiveErrorReportKey = "Direct xT Client - xT Receive error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string DeadlineExceededErrorReportKey = "Direct xT Client - DeadlineExceeded Error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string RpcErrorReportKey = "Direct xT Client - Rpc Error";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string RpcErrorMessage = "Failed to connect to xT, please check with the data in Registry and reference database.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public const string GetSessionTimeoutErrorMessage = ": The gRPC session to the xT server expired, the Service Task is terminating.";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public const string SessionTimeoutErrorReportKey = "Direct xT Client - gRPC Session Expired";

		public static T RunActionAndThrowMsgServerConnectionExceptionIfNeeded<T>(Func<T> action, string actionName, TimeSpan messageTimeout)
		{
			try
			{
				return action();
			}
			catch (Exception ex) when (ex is MsgServerConnectionException or RpcException)
			{
				throw GetMsgServerConnectionException(actionName, ex, messageTimeout);
			}
			catch (AggregateException ex)
			{
				if (ex.InnerExceptions.FirstOrDefault(e => e is RpcException) is RpcException innerRpcException)
				{
					throw GetMsgServerConnectionException(actionName, innerRpcException, messageTimeout);
				}
				throw;
			}
		}

		public static async Task<T> RunActionAndThrowMsgServerConnectionExceptionIfNeededAsync<T>(Func<Task<T>> asyncAction, string actionName, TimeSpan messageTimeout)
		{
			try
			{
				return await asyncAction();
			}
			catch (Exception ex) when (ex is MsgServerConnectionException or RpcException)
			{
				throw GetMsgServerConnectionException(actionName, ex, messageTimeout);
			}
			catch (AggregateException ex)
			{
				if (ex.InnerExceptions.FirstOrDefault(e => e is RpcException) is RpcException innerRpcException)
				{
					throw GetMsgServerConnectionException(actionName, innerRpcException, messageTimeout);
				}
				throw;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Exception message")]
		public static string ClientSystemHostedInfo => (EnvProxy.IsWiseTechGlobalInternalSystem ?? false) ? "WTG internal system client" : EnvProxy.IsHostedWithCargowise ? $"{EnvProxy.HostedLocation} hosted client" : "self-hosted client";

		public static string GetDeadlineExceededErrorMessage(TimeSpan messageTimeout) => $" {(int)messageTimeout.TotalSeconds} seconds timeout, please try it again.";

		public static MsgServerConnectionException GetMsgServerConnectionException(string message, Exception exception, TimeSpan messageTimeout)
		{
			switch (exception)
			{
				case MsgServerConnectionException msgServerConnectionException:
					return new MsgServerConnectionException(message, msgServerConnectionException);
				case RpcException { StatusCode: StatusCode.DeadlineExceeded } ex:
					{
						var extraMessage = message + GetDeadlineExceededErrorMessage(messageTimeout);
						return new MsgServerConnectionException(DeadlineExceededErrorReportKey, ex, extraMessage);
					}
				case RpcException { StatusCode: StatusCode.Unauthenticated } ex:
					{
						var extraMessage = message + GetSessionTimeoutErrorMessage;
						return new MsgSessionTimeoutException(SessionTimeoutErrorReportKey, ex, extraMessage);
					}
				case RpcException ex:
					return new MsgServerConnectionException(RpcErrorReportKey, ex, RpcErrorMessage);
				default:
					return null;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "String used for logging/internal error recording")]
		internal static void AppendError(StringBuilder sb, string errorMessage)
		{
			if (sb.Length == 0)
			{
				_ = sb.Append("Could not create Message attributes dictionary: ");
			}
			_ = sb.Append(errorMessage + " ");
		}

		public static long ConvertUnsignedLongToLong(ulong unsigned) => unsigned > long.MaxValue ? 0L : (long)unsigned;

		public static void AddToDictionaryIfValid(this Dictionary<string, string> dict, string keyToAdd, string valueToAdd, bool overrideIfExist = false)
		{
			if (!string.IsNullOrEmpty(valueToAdd) && !string.IsNullOrEmpty(keyToAdd))
			{
				if (!dict.ContainsKey(keyToAdd) || overrideIfExist)
				{
					dict[keyToAdd] = valueToAdd;
				}
			}
		}

		public static void AddRangeToDictionaryIfValid(this Dictionary<string, string> dict, IDictionary<string, string> incomingDict, bool overrideIfExist = false)
		{
			if (incomingDict != null)
			{
				foreach (var kvp in incomingDict.Where(k => !k.Key.IsSystemMessageAttribute()))
				{
					dict.AddToDictionaryIfValid(kvp.Key, kvp.Value, overrideIfExist);
				}
			}
		}

		public static bool IsSystemMessageAttribute(this string keyToCheck)
		{
			return keyToCheck == Constants.CustomMsgAttributes.ApplicationCode ||
				keyToCheck == Constants.CustomMsgAttributes.DestinationParty ||
				keyToCheck == Constants.CustomMsgAttributes.MessageTrackingID ||
				keyToCheck == Constants.CustomMsgAttributes.MessageType ||
				keyToCheck == Constants.CustomMsgAttributes.SourceParty ||
				keyToCheck == Constants.CustomMsgAttributes.ReceivingRetryCount;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Constant value - Log Message")]
		public static Dictionary<string, string> GetHeaderTextDictionary(this string headerText, ILogger logger = null)
		{
			var result = new Dictionary<string, string>();
			if (!string.IsNullOrEmpty(headerText))
			{
				try
				{
					var headerData = JsonConvert.DeserializeObject<Dictionary<string, string>>(headerText);
					foreach (var dataKey in headerData.Keys)
					{
						result.AddToDictionaryIfValid(dataKey, headerData[dataKey]);
					}
				}
				catch (Exception ex) when (ex is JsonSerializationException || ex is JsonReaderException)
				{
					logger?.Log(LogType.Warning, "Invalid Header Text for xT Message Attribute");
				}
			}
			return result;
		}

		public const string UXmlNameSpace = "http://www.cargowise.com/Schemas/Universal";
		public const string NativeXmlNameSpace = "http://www.cargowise.com/Schemas/Native";
		public const string LegacyXmlNameSpace = "http://www.edi.com.au/EnterpriseService";

		public static readonly List<string> WTGSchemaNameSpace = new List<string>()
		{
			UXmlNameSpace,
			NativeXmlNameSpace,
			LegacyXmlNameSpace
		};

#pragma warning disable CW1061 // Do not use System.DateTime.UtcNow Rule
		//This is designed to be used outside of CW1
		public static DateTime GetDeadline(TimeSpan messageTimeout) => DateTime.UtcNow.Add(messageTimeout);
#pragma warning restore CW1061 // Do not use System.DateTime.UtcNow Rule
	}
}
