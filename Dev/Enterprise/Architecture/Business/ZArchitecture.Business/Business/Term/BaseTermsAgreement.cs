using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.TrustedMessaging.Intergration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Newtonsoft.Json;
using WTG.TrustedMessaging.Models;
using static WTG.TrustedMessaging.Constants;

namespace Enterprise.ZArchitecture.Business
{
	public abstract class BaseTermsAgreement
	{
		readonly IUserPortalClient requestClient = ObjectFactory.Get<IUserPortalClient>();
		public string Title { get; private set; }
		public string Contents { get; private set; }
		public ZBlob ContentsAsBlob => string.IsNullOrEmpty(Contents) ? ZBlob.Empty :
										ZBlob.FromUTF8(ORtfTextUtil.IsRtf(Contents) ? Contents : ORtfTextUtil.TextToRtf(Contents));
		public ZBlob ContentsAsBlob_HTML
		{
			get
			{
				return ORtfTextUtil.RtfToHtml(ContentsAsBlob);
			}
		}
		public int VersionNo { get; private set; }
		public bool ShouldSendAgreementCopy { get; set; }
		public bool HasBeenAcknowledged { get; private set; }
		public bool ParentConditionSatisfied { get; set; } = true;
		public abstract string Type { get; }
		protected abstract bool IsLocalDisplayConditionSatisfied();
		public abstract bool IsCurrentUserAllowedToAcknowledgeAgreement { get; }
		public abstract string ErrorMessageForAcknowledgementNotAllowed { get; }
		protected ErrorMessage ResponseErrorMessage { get; set; }
		public async Task<bool> IsDisplayConditionSatisfied()
		{
			var result = ParentConditionSatisfied && IsLocalDisplayConditionSatisfied();
			if (result)
			{
				if (!HasBeenAcknowledged && (string.IsNullOrEmpty(Title) || string.IsNullOrEmpty(Contents) || VersionNo == default))
				{
					for (; ; )
					{
						var loadSucceed = await TryLoadTerm();
						var succeed = loadSucceed || TryFallbackFormLocal();
						if (!succeed && ShowTermsFetchError() == ZDialogResult.Retry)
						{
							continue;
						}
						result = succeed && !HasBeenAcknowledged;
						break;
					}
				}
				else
				{
					result = !HasBeenAcknowledged;
				}
			}

			return result;
		}

		public virtual async Task<bool> TryLoadTerm()
		{
			var succeed = false;
			var result = await requestClient.GetUserAgreementAsync(Type);
			ResponseErrorMessage = result.Messages?.FirstOrDefault();
			if (result.Success)
			{
				HasBeenAcknowledged = !result.Response.Required;
				if (!HasBeenAcknowledged)
				{
					Title = result.Response.Title;
					Contents = result.Response.Content;
					VersionNo = result.Response.VersionNumber;
				}

				succeed = true;
			}
			else
			{
				ReportServerSideErrorSilently(result.Messages, result.InnerException);
			}

			return succeed;
		}

		public virtual async Task TryPostAcknowledgement()
		{
			var result = await requestClient.AcknowledgeAgreementAsync(Type, ShouldSendAgreementCopy);

			if (result.Success)
			{
				HasBeenAcknowledged = result.Response;
			}
			else
			{
				//Regardless AcknowledgeAgreementAsync is successful or not, the term is deemed to be agreed to. If the response is not successful, the term will be shown again next time the user use the feature.
				ReportServerSideErrorSilently(result.Messages, result.InnerException);
				HasBeenAcknowledged = true;
			}
		}

		protected virtual (string Title, string Contents, int VersionNo) GetLocalFallbackTerm()
		{
			return (string.Empty, string.Empty, 0);
		}

		protected ZDialogResult ShowTermsFetchError()
		{
			return (ResponseErrorMessage != null) ?
				Globals.Message.Show(
					Res.GetString("0FCF0B3F-EA55-4F7C-AA91-DA435F25958C", "There was a connection error, please try again later. (Error Code:{0})", ResponseErrorMessage.Code),
					Res.GetString("ADAC4904-6119-4F91-A047-DB00D7BDA072", "Network Error"), ZMessageBoxButtons.RetryCancel, ZMessageBoxIcon.Error)
				: Globals.Message.Show(
					Res.GetString("152C4BD3-0543-4EEE-A32C-F3A41E15910D", "There was a connection error, please try again later."),
					Res.GetString("ADAC4904-6119-4F91-A047-DB00D7BDA072", "Network Error"), ZMessageBoxButtons.RetryCancel, ZMessageBoxIcon.Error);
		}

		readonly List<string> reportableErrorCodes = [ErrorCodes.UnhandledExceptionThrown, ErrorCodes.InternalServerError, ErrorCodes.HttpRequestFailed];

		protected void ReportServerSideErrorSilently(IEnumerable<ErrorMessage> messages, Exception innerException)
		{
			if (messages != null && messages.Any(x => x.Code != null && reportableErrorCodes.Contains(x.Code, StringComparer.OrdinalIgnoreCase)))
			{
				if (innerException == null || !ShouldIgnoreException(innerException))
				{
					var errorMessageBuilder = new StringBuilder();
					messages.ForEach(o =>
						errorMessageBuilder.AppendFormat(CultureInfo.InvariantCulture, "ErrorCode:{0},ErrorMessage:{1},InnerException:{2}.{3}", o.Code, o.Message, innerException == null ? "null" : innerException.Message, // Developer exception message
							System.Environment.NewLine));
					Globals.Message.ShowDeveloperException("Server side or network problems, for details please see the error",
						new DeveloperNotificationException(errorMessageBuilder.ToString()));
				}
			}
		}

		bool ShouldIgnoreException(Exception exception)
		{
			switch (exception)
			{
				case DatabaseUpgradeException _:
				case UnauthorizedAccessException _:
				case DirectoryNotFoundException _:
					return true;

				case SqlException sqlEx:
					{
						var handler = new DbErrorHandler(sqlEx, null);
						if (handler.IsInfrastructureDbError || handler.ShouldBeRetried)
						{
							return true;
						}
						break;
					}

				case JsonSerializationException jsonSerializationException:
					{
						if (jsonSerializationException.InnerException is OutOfMemoryException)
						{
							return true;
						}
						break;
					}

				default:
					{
						break;
					}
			}

			return false;
		}

		bool TryFallbackFormLocal()
		{
			var isLoadFromLocalSucceed = false;
			try
			{
				var localTerm = GetLocalFallbackTerm();
				var isFallbackAssigned = localTerm != (string.Empty, string.Empty, 0);
				if (isFallbackAssigned)
				{
					Argument.NotNullOrEmpty(localTerm.Title, nameof(localTerm.Title));
					Argument.NotNullOrEmpty(localTerm.Contents, nameof(localTerm.Contents));
					Argument.GreaterThanOrEqualToZero(localTerm.VersionNo, nameof(localTerm.VersionNo));
					(Title, Contents, VersionNo) = (localTerm.Title, localTerm.Contents, localTerm.VersionNo);
					isLoadFromLocalSucceed = true;
				}
			}
			catch (Exception ex)
			{
				Globals.Message.ShowDeveloperException(ex);
			}

			return isLoadFromLocalSucceed;
		}
	}
}
