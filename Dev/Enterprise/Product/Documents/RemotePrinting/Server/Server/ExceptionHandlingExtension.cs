using System;
using System.IO;
using System.Web;
using System.Web.Services.Protocols;
using System.Xml;
using Enterprise.RemotePrinting.Server.RPSCore;

namespace Enterprise.RemotePrinting.Server
{
	public class ExceptionHandlingExtension : SoapExtension
	{
		Stream requestStream;

		#region Overrides

		public override object GetInitializer(Type serviceType)
		{
			return new CallInfo(true);
		}

		public override object GetInitializer(LogicalMethodInfo methodInfo, SoapExtensionAttribute attribute)
		{
			return new CallInfo(false);
		}

		public override Stream ChainStream(Stream stream)
		{
			if (requestStream == null)
			{
				requestStream = stream;
			}

			return base.ChainStream(stream);
		}

		public override void Initialize(object initializer)
		{
			callInfo = initializer as CallInfo;
		}

		public override void ProcessMessage(SoapMessage message)
		{
			switch (message.Stage)
			{
				case SoapMessageStage.BeforeDeserialize:
					streamSizeBeforeDeserialize = GetStreamLengthSafe(message.Stream);
					break;
				case SoapMessageStage.AfterDeserialize:
					streamSizeAfterDeserialize = GetStreamLengthSafe(message.Stream);
					TryHandleException(message, message.Stream);
					break;
				case SoapMessageStage.BeforeSerialize:
					streamSizeBeforeSerialize = GetStreamLengthSafe(message.Stream);
					TryHandleException(message, requestStream);
					break;
				case SoapMessageStage.AfterSerialize:
					break;
				default:
					throw new Exception("Invalid Soap Message processing stage.");
			}
		}

		CallInfo callInfo;

		#endregion

		#region Debug info

		long? streamSizeBeforeDeserialize = -1;
		long? streamSizeAfterDeserialize = -1;
		long? streamSizeBeforeSerialize = -1;

		#endregion

		#region Implementation

		#region Testing stuff
#if DEBUG
		public bool SkipHttpContextForTesting { get; set; }
#endif
		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Exception handler, ignore handler's errors")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		void TryHandleException(SoapMessage msg, Stream stream)
		{
			if (msg.Exception == null)
			{
				return;
			}

			if (!callInfo.IsCommonCall)
			{
#if DEBUG
				if (!SkipHttpContextForTesting)
#endif
				{
					HttpContext.Current.ApplicationInstance.Response.StatusCode = DetermineStatusCode(msg.Exception);
					HttpContext.Current.ApplicationInstance.CompleteRequest();
				}

				if (ShouldReportError(msg.Exception))
				{
					string message;
					try
					{
						string action;
						try
						{
							// This can cause exception if SOAP message was not parsed.
							action = msg.Action;
						}
						catch
						{
							action = string.Empty;
						}

						message = FormattableString.Invariant($@"A Web Print Error occurred.
Soap Message Type: {msg.GetType().Name}
Soap Message Stage: {msg.Stage.ToString()}
Soap Message Action: {action}
Soap Message Url: {msg.Url}

Soap Request Stream Type: {stream?.GetType().Name}
Soap Request Stream Length: {GetStreamLengthSafe(stream)}
Soap Request Stream Can Seek: {GetStreamCanSeekSafe(stream)}

Soap Message Stream Type: {msg.Stream?.GetType().Name}
Soap Message Stream Length: {GetStreamLengthSafe(msg.Stream)}
Soap Message Stream Can Seek: {GetStreamCanSeekSafe(msg.Stream)}

Soap Message Stream Length Before Deserialize: {streamSizeBeforeDeserialize}
Soap Message Stream Length After Deserialize: {streamSizeAfterDeserialize}
Soap Message Stream Length Before Serialize: {streamSizeBeforeSerialize}");

						if (stream != null && GetStreamLengthSafeNotNull(stream) > 0 && GetStreamCanSeekSafeNotNull(stream))
						{
							stream.Position = 0;
							using (var reader = new StreamReader(stream))
							{
								var requestInfo = reader.ReadToEnd();
								message += "\r\nSoap Request Info:\r\n" + requestInfo;
							}
						}
					}
					catch (Exception ex)
					{
						message = @"A Web Print Error occurred.
Could not prepare error message: " + ex;
					}

					ErrorReportSender.TrySend(message, msg.Exception);
				}
			}
		}

		internal static int DetermineStatusCode(Exception ex)
		{
			var statusCode = 500;

			while (!(ex is RemotePrintingDbConnectionException || ex.InnerException == null))
			{
				ex = ex.InnerException;
			}

			if (ex is RemotePrintingDbConnectionException dbConnectionException)
			{
				statusCode = dbConnectionException.ErrorCode;
			}

			return statusCode;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static bool ShouldReportError(Exception ex)
		{
			while (ex != null)
			{
				if (ex is XmlException xex)
				{
					if (xex.Message.StartsWith("Unexpected end of file", StringComparison.InvariantCultureIgnoreCase) ||
						xex.Message.StartsWith("Root element is missing", StringComparison.InvariantCultureIgnoreCase) ||
						xex.Message.StartsWith("There is an unclosed literal string", StringComparison.InvariantCultureIgnoreCase) ||
						xex.Message.StartsWith("Data at the root level is invalid", StringComparison.InvariantCultureIgnoreCase))
					{
						// It may be malformed XML when request is prepared manually, but there are no such requests yet, and all such cases are probably caused by connection issues
						return false;
					}
				}

				if (ex is RemotePrintingDbConnectionException)
				{
					return false;
				}

				if (IsRelatedDBConnectionException(ex))
				{
					return false;
				}

				ex = ex.InnerException;
			}

			return true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static bool IsRelatedDBConnectionException(Exception ex)
		{
			if (ex is InvalidOperationException exception &&
					exception.Message.Equals("Timeout expired. The timeout period elapsed prior to obtaining a connection from the pool. This may have occurred because all pooled connections were in use and max pool size was reached.", StringComparison.InvariantCultureIgnoreCase))
			{
				return true;
			}
			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
		public static void ReportErrorToClient(int statusCode, string message)
		{
			HttpContext.Current.Response.StatusCode = statusCode;
			HttpContext.Current.Response.StatusDescription = message;
			HttpContext.Current.Response.ContentType = "text/plain;charset=utf-8";
			HttpContext.Current.Response.Write(message);
			HttpContext.Current.Response.End();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "SoapMessage.Stream can be wrapper throwing 'stream not ready' exceptions")]
		long? GetStreamLengthSafe(Stream stream)
		{
			try
			{
				return stream?.Length;
			}
			catch
			{
				return -2L;
			}
		}

		long GetStreamLengthSafeNotNull(Stream stream) => GetStreamLengthSafe(stream) ?? 0;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "SoapMessage.Stream can be wrapper throwing 'stream not ready' exceptions")]
		bool? GetStreamCanSeekSafe(Stream stream)
		{
			try
			{
				return stream?.CanSeek;
			}
			catch
			{
				return null;
			}
		}

		bool GetStreamCanSeekSafeNotNull(Stream stream) => GetStreamCanSeekSafe(stream) ?? false;

		#endregion
	}

	// M.K 2020-04-14: ExceptionHandlingExtension is already defined in Web.config in <soapExtensionTypes> and will be applied to all methods.
	// In fact, during debug I have seen double instances of ExceptionHandlingAttribute being invoked.
	// TODO for future: consider removing either this attribute or option in Web.config.
	public class ExceptionHandlingAttribute : SoapExtensionAttribute
	{
		public override Type ExtensionType => typeof(ExceptionHandlingExtension);

		public override int Priority { get; set; }
	}

	public class CallInfo
	{
		public CallInfo()
		{
		}

		public CallInfo(bool isCommonCall)
		{
			IsCommonCall = isCommonCall;
		}

		public bool IsCommonCall { get; }
	}
}
