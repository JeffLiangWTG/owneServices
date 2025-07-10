using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using CargoWise.Types;

namespace Enterprise.Accounting.DataTransfer
{
	public sealed class WebExceptionErrorLogBuilder
	{
		WebExceptionErrorLogBuilder()
		{
		}

		public static string BuildErrorLog(WebException ex)
		{
			var builder = new ZStringBuilder();

			if (ex != null)
			{
				builder.Append(Res.GetString("c5479536-ad7b-46cf-a2c5-30d12b8153c8", "Exception Message: {0}", ex.Message));
				builder.Append(Res.GetString("1ca28b36-57fb-4777-a0f5-27f4a8acb812", "Exception Status: {0}", ex.Status.ToString()));
				if (ex.Response != null)
				{
					builder.Append(Res.GetString("28a14091-5277-4f89-aeb3-01b771c0a46a", "Web Response Content Length: {0}", GetContentLengthMessage(ex.Response)));
					builder.Append(Res.GetString("57aea77d-6fc2-4f0f-91f7-17424416b0cf", "Web Response Content Type: {0}", GetContentTypeMessage(ex.Response)));
					BuildResponseStreamDetailsLog(ex.Response, builder);
				}
				else
				{
					builder.Append(Res.GetString("67de869c-cfe0-4321-9ac0-42a6915b8429", "Web Response was null"));
				}
				builder.Append(ex.StackTrace);
			}
			else
			{
				builder.Append(Res.GetString("326e9bce-15ff-4f44-953d-eae3da8beaa4", "Web Exception was null"));
			}
			return builder.ToStringWithNewLineBetweenAppends();
		}

		static void BuildResponseStreamDetailsLog(WebResponse response, ZStringBuilder builder)
		{
			try
			{
				using (var responseStream = response.GetResponseStream())
				{
					if (responseStream.CanRead)
					{
#if DEBUG
						CloseStreamEventHandler_ForTestOnly?.Invoke(responseStream);
						CloseStreamEventHandler_ForTestOnly = null;
#endif
						builder.Append(Res.GetString("9cbce708-7b73-437c-a675-129fa7fa93c5", "Response Stream: {0}", GetResponseStreamMessage(responseStream)));
					}
					else
					{
						builder.Append(Res.GetString("91c20e0c-4953-48c9-9f66-d0c9595ab6e0", "Response Stream does not support reading"));
					}
				}
			}
			catch (NotImplementedException ex)
			{
				builder.Append(Res.GetString("e583690a-8331-4273-97a6-1e7ecb3f3293", "Get Response Stream: {0}", ex.Message));
			}
		}

		static string GetResponseStreamMessage(Stream responseStream)
		{
			var result = ZString.Empty;
			try
			{
				using (var reader = new StreamReader(responseStream))
				{
					result = reader.ReadToEnd();
				}
			}
			catch (ArgumentException ex)
			{
				result = ex.Message;
			}
			return result;
		}

		static ZString GetContentLengthMessage(WebResponse webResponse)
		{
			return GetContentTypeAndLength(webResponse, true);
		}

		static ZString GetContentTypeMessage(WebResponse webResponse)
		{
			return GetContentTypeAndLength(webResponse, false);
		}

		static ZString GetContentTypeAndLength(WebResponse webResponse, ZBool isContentLengthRequested)
		{
			var result = ZString.Empty;
			try
			{
				if (isContentLengthRequested)
				{
					result = webResponse.ContentLength.ToString(CultureInfo.CurrentCulture);
				}
				else
				{
					result = webResponse.ContentType;
				}
			}
			catch (NotImplementedException ex)
			{
				result = ex.Message;
			}
			return result;
		}

#if DEBUG
		[ThreadStatic]
		[SuppressMessage("Microsoft.Usage", "CA2211: Non-constant fields should not be visible", Justification = "For test only")]
		public static Action<Stream> CloseStreamEventHandler_ForTestOnly;
#endif
	}
}
