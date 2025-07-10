using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CargoWise.Common;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Web.Business;
using Newtonsoft.Json;
using ZClientEDI.Business;
using Res = ZClientEDI.Res;

namespace Enterprise.Client.EDI.Web.Admin
{
	public class EDIWebUserAdminManager : WebUserAdminManager
	{
		readonly HttpClient httpClient;

		public EDIWebUserAdminManager(OrgContact contact, HttpClient httpClient = null)
			: base(contact)
		{
			this.httpClient = httpClient ?? new HttpClient();
			this.httpClient.Timeout = TimeSpan.FromSeconds(5);
		}

		protected override WebUserConfirmationEmailSender CreateMailSender() => new WebUserConfirmationEmailSender(contact, IncidentConstants.SupportDisplayName, SupportIncidentLookups.SupportEmailAddress);

		protected override string PasswordChangeFailure
		{
			get { return @"Unable to update your password. Please contact " + SupportIncidentLookups.SupportEmailAddress; }
		}

		protected override string AccountInfo => Res.GetString("04929a5a-6bb4-49f5-bc75-66adc089d79b", "account");

		protected override ChangePasswordResult GetFollowOnPasswordChangeResult(IPasswordStored record, Guid[] contactPks = default)
		{
			if (IsBorderWiseUserManagementPortalAccessible && EDIDataRegistry.Instance.BorderWiseUmpApiEnabled.Value && !string.IsNullOrWhiteSpace(EDIDataRegistry.Instance.BorderWiseUmpApiAddress.Value) && Environment.Env.Instance.IsProductionSystem && EdiProdDbHelper.IsRunningOnEdiProdDatabase)
			{
				return GetUpdateBorderWiseUmpPasswordResult(record, contactPks);
			}
			else
			{
				return base.GetFollowOnPasswordChangeResult(record, contactPks);
			}
		}

		protected virtual bool IsBorderWiseUserManagementPortalAccessible => !Globals.IsTest;

		ChangePasswordResult GetUpdateBorderWiseUmpPasswordResult(IPasswordStored record, Guid[] contactPks)
		{
			var updateUsersPassword = contactPks != null;
			var relativeUri = updateUsersPassword
				? "/v1/ump/users/password"
				: $"/v1/ump/users/{record.Identifier}/password";

			if (!Uri.TryCreate(EDIDataRegistry.Instance.BorderWiseUmpApiAddress.Value.TrimEnd('/') + relativeUri, UriKind.Absolute, out var requestUri))
			{
				ErrorReporter.ReportOnce(string.Format(CultureInfo.InvariantCulture, "Invalid URI settings in Registry '{0}'.", EDIDataRegistry.Instance.BorderWiseUmpApiAddress.HumanReadableRegistryPath()));
				return ChangePasswordResult.Failure(string.Empty);
			}

			var updatePasswordRequest = new UpdatePasswordRequest
			{
				ApiKey = EDIDataRegistry.Instance.BorderWiseUmpApiUpdatePasswordApiKey.Value,
				PasswordHash = record.PasswordHash,
				PasswordSalt = record.PasswordSalt,
				PasswordHashIterations = record.PasswordHashIterations,
				ContactPks = updateUsersPassword ? contactPks.ToList() : null
			};

			var httpRequestMessage = new HttpRequestMessage
			{
				RequestUri = requestUri,
				Method = HttpMethod.Post,
				Content = new StringContent(JsonConvert.SerializeObject(updatePasswordRequest), Encoding.UTF8, "application/json"),
			};

			try
			{
				var response = httpClient.SendAsync(httpRequestMessage).GetAwaiter().GetResult();

				if (response.IsSuccessStatusCode)
				{
					return ChangePasswordResult.Success(string.Empty);
				}

				// The password will be synced via Kafka queue.
				if (response.StatusCode == HttpStatusCode.NotFound)
				{
					return ChangePasswordResult.Failure(string.Empty);
				}

				var responseText = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
				ErrorReporter.ReportOnce(
					$"Update BorderWise UMP{(updateUsersPassword ? " users" : string.Empty)} password failed for record {record.Identifier}. StatusCode: {response.StatusCode}, Reason: {response.ReasonPhrase}, Message: {responseText}");
			}
			catch (Exception ex)
			{
				if (ex is TaskCanceledException)
				{
					return ChangePasswordResult.Failure(string.Empty);
				}
				ErrorReporter.ReportOnce($"Update BorderWise UMP{(updateUsersPassword ? " users" : string.Empty)} password failed for record {record.Identifier}.", ex);
			}

			// The password will be synced via Kafka queue.
			var userMessage = Res.GetString("A18A47E2-9CAA-4FA3-B418-6C5285E787DA", @"Your password has been changed with My Account. BorderWise is being synchronized and will be updated in a few minutes. Please contact WiseTech Global support should you have any problems logging in.");

			return ChangePasswordResult.Failure(userMessage);
		}
	}
}
