using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class WebUserAdminManager
	{
		public WebUserAdminManager(OrgContact contact)
		{
			this.contact = contact;
		}

		protected readonly OrgContact contact;

		GlbPerson Person => contact.Person;

		WebUserConfirmationEmailSender mailSender;
		WebUserConfirmationEmailSender MailSender
		{
			get
			{
				if (mailSender == null)
				{
					mailSender = CreateMailSender();
				}
				return	mailSender;
			}
		}

		protected virtual WebUserConfirmationEmailSender CreateMailSender() => new WebUserConfirmationEmailSender(contact);

		#region Change Password

		public ChangePasswordResult ChangePassword(string newPassword, string newPasswordConfirm, PasswordInstructionType instructionType = PasswordInstructionType.Reset, bool ignoreEmailOverride = false, string currentPassword = null)
		{
			ChangePasswordResult result;
			if (string.IsNullOrEmpty(newPassword))
			{
				return ChangePasswordResult.Failure(EmptyNewPasswordFailure);
			}

			var fullNames = new List<string>() { Person.PER_FullName };
			var emails = new List<string>() { Person.PER_EmailAddress };

			if (contact != null)
			{
				fullNames.Add(contact.OC_ContactName);
				emails.Add(contact.OC_Email);
			}

			var (isValid, message) = ContactPasswordValidator.IsValidPassword((IGlbPasswordHistoryParent)contact ?? Person, newPassword, fullNames, emails);
			if (!isValid)
			{
				result = ChangePasswordResult.Failure(message);
			}
			else if (!newPassword.Equals(newPasswordConfirm))
			{
				result = ChangePasswordResult.Failure(PasswordConfirmationFailure);
			}
			else
			{
				try
				{
					contact.SetHashedPassword(newPassword, currentPassword);

					if (!contact.HasErrors)
					{
						contact.Logs.AddNew(Events.WebAccessPasswordChanged);
						contact.Factory.Save();
						var emailSendSuccess = true;

						try
						{
							MailSender.SendConfirmationEmail(instructionType, ignoreEmailOverride);
						}
						catch (Exception e) when (!e.IsCriticalException())
						{
							emailSendSuccess = false;
						}

						var followOnResult = GetFollowOnPasswordChangeResult(contact);

						if (!emailSendSuccess)
						{
							result = ChangePasswordResult.PartialSuccess(PasswordChangeSuccessWithEmailSendFailure);
						}
						else if (followOnResult != null && !followOnResult.IsSuccess)
						{
							result = ChangePasswordResult.PartialSuccess(PasswordChangeSuccess);
						}
						else
						{
							result = ChangePasswordResult.Success(PasswordChangeSuccess);
						}
					}
					else
					{
						result = ChangePasswordResult.Failure(PasswordChangeFailure);
						LogPasswordChangeFailure(contact, null, null);
					}
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					result = ChangePasswordResult.Failure(PasswordChangeFailure);
					LogPasswordChangeFailure(contact, null, e);
				}
			}

			return result;
		}

		protected virtual ChangePasswordResult GetFollowOnPasswordChangeResult(IPasswordStored record, Guid[] contactPks = default)
		{
			return null;
		}

		public string ChangePassword(string currentPassword, string newPassword, string newPasswordConfirm, bool ignoreEmailOverride = false)
		{
			return contact.VerifyPassword(currentPassword) ? ChangePassword(newPassword, newPasswordConfirm, ignoreEmailOverride: ignoreEmailOverride, currentPassword: currentPassword).Message : CurrentPasswordMismatch;
		}

		protected virtual string AccountInfo
		{
			get
			{
				var companyName = contact.ParentOrg != null ? contact.ParentOrg.OH_FullName : ZString.Empty;
				return Res.GetString("d153fcb3-114b-46fb-89e5-f669ae8b121f", "{0} / {1} account", companyName, contact.OrgCode);
			}
		}

		#region SetMasterPassword

		public string SetMasterPassword(string newPassword, string newPasswordConfirm, PasswordInstructionType instructionType = PasswordInstructionType.Reset, string currentPassword = null)
		{
			string result;

			if (string.IsNullOrEmpty(newPassword))
			{
				return EmptyNewPasswordFailure;
			}

			var fullNames = new List<string>() { Person.PER_FullName };
			var emails = new List<string>() { Person.PER_EmailAddress };

			if (contact != null)
			{
				fullNames.Add(contact.OC_ContactName);
				emails.Add(contact.OC_Email);
			}

			var (isValid, message) = ContactPasswordValidator.IsValidPassword(Person, newPassword, fullNames, emails);
			if (!isValid)
			{
				result = message;
			}
			else if (!newPassword.Equals(newPasswordConfirm))
			{
				result = PasswordConfirmationFailure;
			}
			else
			{
				try
				{
					Person.SetHashedPassword(newPassword, currentPassword);
					result = PostSetMasterPasswordAction(instructionType);
				}
				catch (Exception e) when (!e.IsCriticalException())
				{
					result = PasswordChangeFailure;
					LogPasswordChangeFailure(null, Person, e);
				}
			}

			return result;
		}

		string PostSetMasterPasswordAction(PasswordInstructionType instructionType)
		{
			if (Person.HasErrors)
			{
				LogPasswordChangeFailure(null, Person, null);
				return PasswordChangeFailure;
			}

			Person.Logs.AddNew(AutoEvents.WebAccessPasswordChanged);
			Person.ContactCollection.Cast<OrgContact>().ForEach(x => x.RemovePasswordAndHash());
			Person.Factory.Save();

			var contactPks = Person.ContactCollection.Select(c => c.PK.ToGuid()).ToArray();

			_ = GetFollowOnPasswordChangeResult(Person, contactPks);

			try
			{
				MailSender.SendConfirmationEmail(instructionType);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				LogPasswordChangeFailure(null, Person, e);
				return PasswordChangeSuccessWithEmailSendFailure;
			}
			
			return PasswordChangeSuccess;
		}

		public string ChangeMasterPassword(string currentPassword, string newPassword, string newPasswordConfirm)
		{
			return Person.VerifyPassword(currentPassword) ? SetMasterPassword(newPassword, newPasswordConfirm, currentPassword: currentPassword) : CurrentPasswordMismatch;
		}

		public string CopyMasterPasswordFromContact(OrgContact passwordSource)
		{
			Person.PER_PasswordHash = passwordSource.OC_PasswordHash;
			Person.PER_PasswordHashIterations = passwordSource.OC_PasswordHashIterations;
			Person.PER_PasswordSalt = passwordSource.OC_PasswordSalt;

			string result;

			try
			{
				result = PostSetMasterPasswordAction(PasswordInstructionType.Set);
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				result = PasswordChangeFailure;
				LogPasswordChangeFailure(null, Person, e);
			}

			return result;
		}

		public const string SetMasterPasswordKey = TrackingConstants.QueryStringKeys.SetPasswordKey; // Key used for url queries;
		#endregion

		public virtual string PasswordChangeSuccess => Res.GetString("ac959f2a-91dc-4ef3-99f6-ce785d6bc5cd", @"Your password has been changed.");

		public virtual string PasswordChangeSuccessWithEmailSendFailure => Res.GetString("a777f6fb-3fae-464c-974d-2cd63bfdf238", @"Your password has been updated but we encountered a problem sending the confirmation email. Please contact your system administrator");

		protected virtual string PasswordChangeFailure => Res.GetString("7b0a97b4-6410-49e8-9729-753588bd50d5", @"Unable to update your password. Please contact your Customer Service Representative.");

		protected virtual string CurrentPasswordMismatch => Res.GetString("55f1e4b1-0652-46b5-9aa5-7325a34cbeb4", @"The password supplied does not match your current password.");

		protected virtual string EmptyNewPasswordFailure => Res.GetString("3217d764-a9ca-4ccf-8381-523f0741b8a0", @"The new password cannot be empty.");

		protected virtual string PasswordConfirmationFailure => Res.GetString("2a106144-0e51-4d48-9c36-6baa09be579a", @"The password and confirmation do not match.");

		#region Logging

		static void LogPasswordChangeFailure(OrgContact contact, GlbPerson person, Exception ex)
		{
			var logBuilder = new ZStringBuilder();
			logBuilder.AppendLine(nameof(PasswordChangeFailure));

			foreach (var bizObj in new BusinessObject[] { contact, person }.Where(x => x != null && x.HasErrors()))
			{
				logBuilder.AppendLine(bizObj.TableName);
				logBuilder.AppendLine(bizObj.Notifications.ToUniqueMessageListString());
			}

			if (ex != null)
			{
				logBuilder.AppendLine(ex.Message);
				logBuilder.AppendLine(ex.StackTrace);
			}

			var factory = new BusinessObjectFactory() { RefreshEnabled = false };
			if (contact != null)
			{
				factory.Load<OrgContact>(contact.PK)?.Logs.AddNew(Events.ErrorReport, logBuilder.ToString());
			}
			else if (person != null)
			{
				factory.Load<GlbPerson>(person.PK)?.Logs.AddNew(Events.ErrorReport, logBuilder.ToString());
				if (!SystemDataRegistry.Instance.PersonIntelligenceModuleEnabled.Value)
				{
					foreach (var personContact in factory.Load<OrgContact>(new ZQuery(OrgContactSchema.PK, person.ContactCollection.Select(x => x.PK))))
					{
						personContact.Logs.AddNew(Events.ErrorReport, logBuilder.ToString());
					}
				}
			}
			factory.Save();
		}

		#endregion

		#endregion
	}
}
