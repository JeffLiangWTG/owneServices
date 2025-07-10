using System;
using System.Linq;
using System.Net;
using System.Web.UI;
using System.Web.UI.WebControls;
using CargoWise.Data;
using CargoWise.Definitions.Authentication;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.UserManagement.Business;
using Enterprise.Environment;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.ZClientWebCargoWiseEDI
{
	public partial class UserAgreement : BasePage
	{
		protected override bool PageRequiresLogin(Uri url) => false;

		public static class Constants
		{
			public const string NotificationTitle = "User Agreement";

			public const string UrlExpiredMessage = "This link has expired. Please contact WiseTech Global support for assistance.";

			public const string AgreementCompletedMessage = "This agreement has already been completed.";

			public const string VerifyEmailSentMessage = "A verification link was sent to your email address. Please follow the link in the email to finalize the agreement";

			public const string AcknowledgedAgreement = "Thank you for accepting this agreement. {0} You can now close this browser window.";

			public const string SendCopy = "A copy will be sent to your email.";

			public const string CancelledAgreement = "You have cancelled the agreement. If you wish to accept the agreement, please follow the link in the email again.";

			public const string CannotOnlineClickthrough = "Online license agreement acceptance is not currently supported for your organization. Please contact the WiseTech Global License Management Team via eRequest for more information";

			public const string AgreementTypeInvalid = "Invalid agreement URL";

			public const string CannotAcceptAgreement = "Failed to complete agreement. Please try again.";

			public const string NotificationResent = "We noticed that the link you tried to access is no longer valid. But don’t worry, a new email with an updated link has been sent to your inbox. Please check your email to find the new click through agreement link.";
		}

		protected NLogWrapper Logger
		{
			get
			{
				if (logger == null)
				{
					logger = new NLogWrapper(GetType());
				}

				return logger;
			}

			set { logger = value; }
		}
		NLogWrapper logger;

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			queryTokenString = WebUtility.UrlDecode(Request.QueryString["data"]);
			var tokenControl = (ITokenizedAccessControl)new TokenizedAccessControl();
			using (Db.DisposableActionForDbConnection())
			{
				var isTokenValid = !string.IsNullOrEmpty(queryTokenString);
				if (isTokenValid && (isTokenValid = tokenControl.TryPeek(queryTokenString, AccessTokenTypes.MyAccountUserAgreement, out var tokenInfo)))
				{
					queryToken = UserAgreementQueryToken.FromJson(tokenInfo.Scope);
					if (queryToken == null ||
						string.IsNullOrEmpty(queryToken.Source) ||
						string.IsNullOrEmpty(queryToken.Type) ||
						string.IsNullOrEmpty(queryToken.AgreementType) ||
						(queryToken.FromContact.IsEmpty && queryToken.Enterprise.IsEmpty && queryToken.Database.IsEmpty) ||
						(queryToken.Type == UserAgreementTokenTypes.Accept && string.IsNullOrEmpty(queryToken.RecipientEmail)))
					{
						isTokenValid = false;
					}
				}
				else if (TryResendEmail(queryTokenString))
				{
					Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.NotificationResent, Notification.NotificationType.Success);
					Response.End();
					return;
				}

				if (!isTokenValid)
				{
					Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.UrlExpiredMessage, Notification.NotificationType.Warning);
					Response.End();
					return;
				}

				if (EdiUserAgreementTypesMapper.GetBindingTableCodes(queryToken.AgreementType).Count == 0)
				{
					Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.AgreementTypeInvalid, Notification.NotificationType.Error);
					Response.End();
					return;
				}
			}
		}

		string queryTokenString;
		UserAgreementQueryToken queryToken;

		bool TryResendEmail(string outdatedTokenString)
		{
			if (string.IsNullOrEmpty(outdatedTokenString))
			{
				return false;
			}

			var query = new ZQuery(StmAccessTokenSchema.SAT_Token, outdatedTokenString)
				.AddToFilter(StmAccessTokenSchema.SAT_Type, AccessTokenTypes.MyAccountUserAgreement)
				.AddToFilter(StmAccessTokenSchema.SAT_ParentTableCode, GlbCompanyCampaignItemSchema.Constants.Prefix);

			var token = Factory.LoadTop1<StmAccessToken>(query);
			if (token == null || string.IsNullOrEmpty(token.SAT_Scope))
			{
				return false;
			}

			var result = true;
			var user = WebAppEnvironment.GetWebUserFromDatabase();
			using (Env.SetTemporaryUserContext(new UserContext(user, DataRegistry.Instance.WebBranch, DataRegistry.Instance.WebDepartment)))
			{
				var campaignItem = Factory.Load<GlbCompanyCampaignItem>(token.SAT_ParentId);
				if (campaignItem != null)
				{
					if (campaignItem.G8_LastSentTimeUtc.IsEmpty || campaignItem.G8_LastSentTimeUtc.AddDays(UserAgreementUrlProvider.TokenKeepsAliveDays) < ZDateTime.UtcNow)
					{
						var sender = new GlbCompanyCampaignSender(campaignItem.CompanyCampaign, [campaignItem]);
						sender.MessageOnCampaignSending += (s, e) =>
						{
							result = result && !e.IsError;
							Logger.AddLog(e.IsError ? NLog.LogLevel.Warn : NLog.LogLevel.Info, $"Summary: {e.Summary} | message: {e.Message}", e.IsError ? 1 : 0);
						};

						sender.ShouldContinueWithSending += (s, e) => result;

						result = sender.CheckAndSendCampaigns();
					}
				}
			}

			if (result)
			{
				token?.Delete();
				Factory.Save();
			}

			return result;
		}

		protected override BusinessObject GetNewDataSource()
		{
			if (queryToken == null)
			{
				return null;
			}

			using (Db.DisposableActionForDbConnection())
			{
				var user = WebAppEnvironment.GetWebUserFromDatabase();
				using (Env.SetTemporaryUserContext(new UserContext(user, DataRegistry.Instance.WebBranch, DataRegistry.Instance.WebDepartment)))
				{
					userAgreementManager.FromContact = Factory.Load<EDIOrgContact>(queryToken.FromContact);
					userAgreementManager.Assignments = Array.Empty<EdiUserAgreementAssignment>();

					if (userAgreementManager.FromContact != null)
					{
						var assignmentsQuery = new ZQuery(EdiUserAgreementAssignmentSchema.EAE_OH_ClientAgreementOrg, userAgreementManager.FromContact.OC_OH);
						AddDatabaseAssignmentFilters(assignmentsQuery);
						userAgreementManager.Assignments = Factory.Load<EdiUserAgreementAssignment>(assignmentsQuery).Where(x => x.LastAcceptedDateUtc.IsEmpty).ToArray();
					}
					else if (!queryToken.Database.IsEmpty)
					{
						var databaseAssignmentQuery = new ZQuery(EdiUserAgreementAssignmentSchema.EAE_ParentID, queryToken.Database);
						AddDatabaseAssignmentFilters(databaseAssignmentQuery);
						var databaseAssignment = Factory.LoadTop1<EdiUserAgreementAssignment>(databaseAssignmentQuery);
						if (databaseAssignment != null)
						{
							var assignmentsQuery = new ZQuery(EdiUserAgreementAssignmentSchema.EAE_OH_ClientAgreementOrg, databaseAssignment.EAE_OH_ClientAgreementOrg);
							AddDatabaseAssignmentFilters(assignmentsQuery);
							userAgreementManager.Assignments = Factory.Load<EdiUserAgreementAssignment>(assignmentsQuery).Where(x => x.LastAcceptedDateUtc.IsEmpty).ToArray();
						}
					}

					OrgHeader orgForBinding = null;
					if (userAgreementManager.IsDatabaseLevelAgreement)
					{
						userAgreementManager.ClientAgreementOrg = userAgreementManager.FromContact?.Header ?? userAgreementManager.Assignments.First().ClientAgreementOrg;
						orgForBinding = userAgreementManager.ClientAgreementOrg;

						if (!queryToken.Database.IsEmpty)
						{
							userAgreementManager.Database = Factory.Load<LicenceDatabase>(queryToken.Database);
						}
					}
					else
					{
						userAgreementManager.Enterprise = Factory.Load<LicenceEnterprise>(queryToken.Enterprise) ?? Factory.Load<LicenceDatabase>(queryToken.Database)?.LicEnterprise ?? (userAgreementManager.FromContact?.ParentOrg as EDIOrgHeader)?.LicEnterprise;
						if (userAgreementManager.Enterprise == null)
						{
							return null;
						}

						orgForBinding = userAgreementManager.Enterprise.Organisation;
					}

					var cusCode = EdiUserAgreementWrapper.GetPrimaryCusCodeForCountry(orgForBinding);
					var address = orgForBinding?.MainAddress;
					var clientAddress = string.Empty;
					if (address != null)
					{
						clientAddress = new ZStringBuilder().AppendIfNotEmpty(address.Address1).AppendIfNotEmpty(address.Address2)
		.AppendIfNotEmpty(address.OA_City).AppendIfNotEmpty(address.State).AppendIfNotEmpty(address.Postcode).AppendIfNotEmpty(address.CountryName).ToStringWithDelimiterBetweenAppends(", ");
					}

					userAgreementManager.AgreementInfo = new EnterpriseUserAgreementInfo
					{
						ClientAgreementOrgFullName = orgForBinding?.OH_FullName ?? ZString.Empty,
						ClientMainAddressInSingleLine = clientAddress,
						ClientBusinessRegistration = cusCode != null
						? $"{cusCode.OK_CodeType} {cusCode.SecuredCustomsRegNo}"
						: "Not on file",
						UserCountry = orgForBinding?.CountryCode ?? string.Empty,
						UserAgreementType = queryToken.AgreementType,
						Email = queryToken.Type == UserAgreementTokenTypes.Verify ? userAgreementManager.FromContact?.Email ?? queryToken.RecipientEmail : queryToken.RecipientEmail,
						FullName = queryToken.Type == UserAgreementTokenTypes.Verify ? userAgreementManager.FromContact?.Name ?? queryToken.RecipientName : queryToken.RecipientName,
						JobTitle = queryToken.Type == UserAgreementTokenTypes.Verify ? userAgreementManager.FromContact?.OC_Title ?? queryToken.RecipientJobTitle : queryToken.RecipientJobTitle,

						ShouldSendAgreementCopy = queryToken.SendAgreementCopy,
					};

					var responseWrapper = userAgreementManager.IsDatabaseLevelAgreement
						? UserAgreementHelper.GetEnterpriseUserAgreementResponse(userAgreementManager.Assignments, userAgreementManager.AgreementInfo, LinkHelper, queryTokenString)
						: UserAgreementHelper.GetEnterpriseUserAgreementResponse(userAgreementManager.Enterprise, userAgreementManager.AgreementInfo, LinkHelper, queryTokenString);
					userAgreementManager.CurrentUserAgreementResponse = responseWrapper.ResponseData;
					userAgreementManager.Agreement = responseWrapper.Agreement;

					return userAgreementManager;
				}
			}
		}

		readonly UserAgreementManager userAgreementManager = new UserAgreementManager();

		void AddDatabaseAssignmentFilters(ZQuery databaseAssignmentQuery)
		{
			databaseAssignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_AgreementType, queryToken.AgreementType);
			databaseAssignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_ParentTableCode, LicenceDatabaseSchema.Constants.Prefix);
			databaseAssignmentQuery.AddToFilter(EdiUserAgreementAssignmentSchema.EAE_AllowOnlineAcceptance, true);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (userAgreementManager.Enterprise == null && userAgreementManager.ClientAgreementOrg == null)
			{
				Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.UrlExpiredMessage + "(Missing Lic.Enterprise/Instance/Client Agreement Organization)", Notification.NotificationType.Warning);
				Response.End();
				return;
			}

			if (!userAgreementManager.CurrentUserAgreementResponse.AllowOnlineClickthrough)
			{
				Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.CannotOnlineClickthrough, Notification.NotificationType.Info);
				Response.End();
				return;
			}
			else if (!userAgreementManager.CurrentUserAgreementResponse.Required)
			{
				using (Db.DisposableActionForDbConnection())
				{
					((ITokenizedAccessControl)new TokenizedAccessControl()).TryConsume(queryTokenString, AccessTokenTypes.MyAccountUserAgreement, out _);
				}

				Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.AgreementCompletedMessage, Notification.NotificationType.Info);
				Response.End();
				return;
			}

			if (!IsPostBack)
			{
				if (queryToken.Type == UserAgreementTokenTypes.Verify)
				{
					VerifyEmailButton.Visible = true;
					AcceptButton.Visible = false;
					CancelButton.Visible = false;
					VerifyPanel.Style["display"] = "none";
				}
				else if (queryToken.Type == UserAgreementTokenTypes.Accept)
				{
					VerifyEmailButton.Visible = false;
					AcceptButton.Visible = true;
					CancelButton.Visible = true;
					EmailAddressTextBox.ReadOnly = true;
					EmailAddressTextBox.CssClass += " readonly";
					VerifyPanel.Style["display"] = "flex";

					JobTitleTextBox.Text = string.IsNullOrEmpty(JobTitleTextBox.Text) ? userAgreementManager.AgreementInfo.JobTitle : JobTitleTextBox.Text;
					FullNameTextBox.Text = string.IsNullOrEmpty(FullNameTextBox.Text) ? userAgreementManager.AgreementInfo.FullName : FullNameTextBox.Text;
					EmailAddressTextBox.Text = string.IsNullOrEmpty(EmailAddressTextBox.Text) ? userAgreementManager.AgreementInfo.Email : EmailAddressTextBox.Text;
				}
			}

			DatabaseList.DataSource = userAgreementManager.Databases.Where(x => x.LD_IsActive && ProductTypes.CargoWiseNextAgreementCompatibleProducts.Contains(x.LD_Product.ToString(), StringComparer.OrdinalIgnoreCase)).ToList();
			DatabaseList.DataBind();
			EDocList.DataSource = userAgreementManager.EDocs;
			EDocList.DataBind();

			DocumentPanel.Visible = userAgreementManager.EDocs.Any();

			AgreementContent.Controls.Add(new LiteralControl(userAgreementManager.CurrentUserAgreementResponse.Content));
		}

		#region Event Handlers

		protected void AcceptButton_Click(object sender, EventArgs e)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var agreementInfo = userAgreementManager.AgreementInfo;
				agreementInfo.JobTitle = JobTitleTextBox.Text;
				agreementInfo.FullName = FullNameTextBox.Text;
				agreementInfo.Email = EmailAddressTextBox.Text;
				agreementInfo.MajorVersion = userAgreementManager.Agreement.ERA_VersionNumber.ToString();
				agreementInfo.MinorVersion = userAgreementManager.Agreement.ERA_MinorVersion.ToString();
				agreementInfo.Variant = userAgreementManager.Agreement.ERA_VariantCode;

				var acknowledgeResult = true;

				if (userAgreementManager.IsDatabaseLevelAgreement)
				{
					foreach (var database in userAgreementManager.Databases)
					{
						acknowledgeResult &= UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(database, agreementInfo);
					}
				}
				else
				{
					acknowledgeResult = UserAgreementHelper.AcknowledgeEnterpriseUserAgreement(userAgreementManager.Enterprise, agreementInfo);
				}

				if (acknowledgeResult)
				{
					Factory.Save();
					((ITokenizedAccessControl)new TokenizedAccessControl()).TryConsume(queryTokenString, AccessTokenTypes.MyAccountUserAgreement, out _);

					var message = string.Format(Constants.AcknowledgedAgreement, userAgreementManager.AgreementInfo.ShouldSendAgreementCopy ? Constants.SendCopy : string.Empty);
					Notification.ShowMessage(Response, Constants.NotificationTitle, message, Notification.NotificationType.Success);
				}
				else
				{
					Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.CannotAcceptAgreement, Notification.NotificationType.Error);
				}
			}
		}

		protected void CancelButton_Click(object sender, EventArgs e)
		{
			Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.CancelledAgreement, Notification.NotificationType.Info);
		}

		protected void VerifyEmailButton_Click(object sender, EventArgs e)
		{
			var verifyEmailToken = new UserAgreementQueryToken()
			{
				Source = queryToken.Source,
				Type = UserAgreementTokenTypes.Accept,
				AgreementType = queryToken.AgreementType,
				FromContact = queryToken.FromContact,
				RecipientEmail = EmailAddressTextBox.Text,
				RecipientName = FullNameTextBox.Text,
				RecipientJobTitle = JobTitleTextBox.Text,
				SendAgreementCopy = queryToken.SendAgreementCopy
			};

			if (userAgreementManager.IsDatabaseLevelAgreement)
			{
				verifyEmailToken.Enterprise = ZGuid.Empty;
				verifyEmailToken.Database = userAgreementManager.Database?.PK ?? ZGuid.Empty;
			}
			else
			{
				verifyEmailToken.Enterprise = userAgreementManager.Enterprise.PK;
				verifyEmailToken.Database = ZGuid.Empty;
			}

			var tokenControl = new TokenizedAccessControl();
			var tokenParent = (BusinessObject)userAgreementManager.FromContact ?? (BusinessObject)userAgreementManager.Database ?? userAgreementManager.Enterprise;
			var verifyTokenInfo = new AccessTokenInfo(verifyEmailToken.ToJson(), tokenParent.PK.ToGuid(), tokenParent.TablePrefix);

			using (Db.DisposableActionForDbConnection())
			{
				var user = WebAppEnvironment.GetWebUserFromDatabase();
				using (Env.SetTemporaryUserContext(new UserContext(user, DataRegistry.Instance.WebBranch, DataRegistry.Instance.WebDepartment)))
				{
					var verifyToken = tokenControl.CreateLimitedToken(AccessTokenTypes.MyAccountUserAgreement, verifyTokenInfo, TimeSpan.FromDays(90), maxUses: 1);

					((ITokenizedAccessControl)tokenControl).TryConsume(queryTokenString, AccessTokenTypes.MyAccountUserAgreement, out _);

					var acceptUrl = EDIDataRegistry.Instance.MyAccountSiteRootUrl.Value.Trim('/') + $"/Admin/UserAgreement.aspx?data={WebUtility.UrlEncode(verifyToken)}";
					SendAcceptEmail(acceptUrl, userAgreementManager.CurrentUserAgreementResponse.Title);
				}
			}

			Notification.ShowMessage(Response, Constants.NotificationTitle, Constants.VerifyEmailSentMessage, Notification.NotificationType.Success);
		}

		void SendAcceptEmail(string acceptUrl, string agreementTitle)
		{
			var emailBody = EDIDataRegistry.Instance.MyAccountUserVerifyAgreementEmailTemplate.Value;
			emailBody = emailBody.Replace(EDIDataRegistry.MyAccountUserVerifyAgreementUrlMacro, acceptUrl);

			Env.OutgoingCustomsMailManager.CreateAndSaveSimple(
				$"Verify your email - {agreementTitle}",
				emailBody,
				EmailAddressTextBox.Text);
		}

		protected void EDocListRepeater_ItemDataBound(object sender, RepeaterItemEventArgs e)
		{
			if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem || e.Item.ItemType == ListItemType.SelectedItem)
			{
				var edoc = e.Item.DataItem as IeDoc;
				var edocLink = e.Item.FindControl("EDocLink") as HyperLink;

				if (edoc != null && edocLink != null)
				{
					edocLink.NavigateUrl = LinkHelper.GetHandlerUrl(userAgreementManager.Agreement.PK, edoc.UniqueKey.ToGuid(), queryTokenString);
				}
			}
		}

		UserAgreementEDocRequestHelper LinkHelper
		{
			get { return linkHelper ?? (linkHelper = new UserAgreementEDocRequestHelper()); }
		}
		UserAgreementEDocRequestHelper linkHelper;

		#endregion
		public string CurrentAgreementTitle => userAgreementManager.CurrentUserAgreementResponse.Title;

		public string CompanyName => userAgreementManager.IsDatabaseLevelAgreement ? userAgreementManager.ClientAgreementOrg?.OH_FullName : userAgreementManager.Enterprise.Organisation?.OH_FullName ?? string.Empty;

		public string PageFooter
		{
			get
			{
				if (queryToken.Type == UserAgreementTokenTypes.Verify)
				{
					return "An email verification step is required to accept this agreement. By clicking VERIFY MY EMAIL, you will be sent a new secure link to the email address specified in the declaration. Simply follow that link to confirm your acceptance.";
				}
				else if (queryToken.Type == UserAgreementTokenTypes.Accept)
				{
					return "By pressing ACCEPT, you are confirming that you are authorized to accept these legally binding terms and conditions on behalf of your organization for the user of CargoWise Next and its sub-products ";
				}

				return string.Empty;
			}
		}
	}
}
