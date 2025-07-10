using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Web.UI;
using CargoWise.Async;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Web.Common
{
	public static class ExceptionExtensions
	{
		public static bool Contains(this Exception exception, Type targetExceptionType, out Exception targetException)
		{
			targetException = null;
			var ex = exception;

			while (ex != null)
			{
				if (ex.GetType().IsAssignableFrom(targetExceptionType))
				{
					targetException = ex;
					return true;
				}

				ex = ex.InnerException;
			}

			return false;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Exception message")]
		public static bool IsIgnorable(this Exception exception)
		{
			switch (exception)
			{
				case ViewStateException _:
				case UriFormatException _:
				case CrossThreadAccessException _:
				case ThreadAbortException _:
				case InvalidProgramException _:
				case ReflectionTypeLoadException _:
				case FileNotFoundException _:
				case IOException _:
					return true;
				case HttpException httpException:
					if (httpException is HttpRequestValidationException && httpException.Message.Contains("A potentially dangerous Request"))
					{
						return true;
					}

					var httpCode = httpException.GetHttpCode();
					if (HandledHttpCodes.Contains(httpCode))
					{
						return true;
					}

					if (httpCode == (int)HttpStatusCode.InternalServerError && httpException.Message.Equals("Request timed out."))
					{
						return true;
					}

					break;
			}

			return exception.IsIISInternalCommunicationErrorWhenClientConnected()
				|| IsIgnorableWarmupException(exception);
		}

		public static bool IsIISInternalCommunicationErrorWhenClientConnected(this Exception exception)
		{
			var handledCOMHResultCodes = new[]
			{
				unchecked((int)0x80070032),
				unchecked((int)0x800704CD)
			};

			return exception is HttpException && exception.InnerException is COMException comException && handledCOMHResultCodes.Contains(comException.HResult);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Exception message")]
		static bool IsIgnorableWarmupException(Exception exception)
		{
			if (!ApplicationIsWarmingUp)
			{
				return false;
			}

			switch (exception)
			{
				case FileLoadException _:
				case HttpCompileException _:
					return true;
				case HttpException httpException:
					return httpException.Message.Equals("Unable to validate data.", StringComparison.OrdinalIgnoreCase);
				case BadImageFormatException badImageFormatException:
					return
						badImageFormatException.Message.StartsWith("Could not load file or assembly", StringComparison.OrdinalIgnoreCase)
						&& badImageFormatException.Message.EndsWith("or one of its dependencies. The module was expected to contain an assembly manifest.", StringComparison.OrdinalIgnoreCase);
				default:
					return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "User agent")]
		static bool ApplicationIsWarmingUp => HttpContext.Current?.Request?.ServerVariables["HTTP_USER_AGENT"] == "IIS Application Initialization Warmup";

		static IEnumerable<int> HandledHttpCodes => new[] { 400, 404, 405, 204 };
	}
}
