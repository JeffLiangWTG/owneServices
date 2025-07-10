using System;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Integration.Licensing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security
{
	public class TemporaryPassword
	{
		public static TemporaryPassword CreateAndSendTemporaryPassword(IUser user)
		{
			if (CanSendTemporaryPassword())
			{
				return new TemporaryPassword(user);
			}
			return null;

			bool CanSendTemporaryPassword()
			{
				return !ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled && user.LocalPasswordMustBeReset && IsValidEmailAddress() && IsEmailAddressUnique();
			}

			bool IsValidEmailAddress()
			{
				return !string.IsNullOrEmpty(user.EmailAddress) && Regex.IsMatch(user.EmailAddress, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");
			}

			bool IsEmailAddressUnique()
			{
				var query = new ZQuery(GlbStaffSchema.GS_EmailAddress, user.EmailAddress);
				query.AddToFilter(JoinCondition.And, GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, user.PK);
				return !(new BusinessObjectFactory().ExistsInDatabase(GlbStaffSchema.Constants.TableName, query));
			}
		}

		TemporaryPassword(IUser user)
		{
			User = user;
			Password = GetNewTemporaryPassword();
			Attempt = 0;
			CreatedOn = ZDateTime.UtcNow.ToDateTime();
			SendTemporaryPassword();
		}

		internal IUser User { get; set; }

		internal string Password { get; set; }

		internal ZDateTime CreatedOn { get; set; }

		internal int Attempt { get; set; }

		string GetNewTemporaryPassword()
		{
			using (var rng = RandomNumberGenerator.Create())
			{
				var tokenData = new byte[9];
				rng.GetBytes(tokenData);
				return Convert.ToBase64String(tokenData);
			}
		}

		void SendTemporaryPassword()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().Key;
			var enterpriseCode = registrationKey.EnterpriseCode;
			var databaseCode = registrationKey.ServerCode;

			var systemCode = !string.IsNullOrEmpty(enterpriseCode) && !string.IsNullOrEmpty(databaseCode) ? enterpriseCode + databaseCode + "." : string.Empty;

			var email = new EmailDef();
			email.AddRecipientForSystemCommunication(User.EmailAddress, true);
			email.Subject = Res.GetString("3607FCF7-86B3-47FC-A79B-9608421576EE", "Your {0} Temporary Password for system {1}", Enterprise.Core.Constants.ProductName, systemCode);
			var bodyBuilder = new ZStringBuilder();
			bodyBuilder.Append(Password);
			bodyBuilder.AppendLine();
			bodyBuilder.AppendLine();
			bodyBuilder.Append(Res.GetString("F7D0C99D-7153-426A-8872-518AB36F94E4", "This temporary password is only valid for the current login attempt for system {0}", systemCode));
			email.Body = bodyBuilder.ToString();
			Env.OutgoingMailManager.CreateAndSave(email);
		}

		const int MaxAttempts = 3;

		const int ExpiredMinutes = 10;

		bool HasExpired() => ZDateTime.UtcNow.ToDateTime().Subtract(CreatedOn.ToDateTime()).TotalMinutes > ExpiredMinutes || Attempt > MaxAttempts;

		public TemporaryPasswordValidationResult Validate(string userLoginName, string userPassword)
		{
			Attempt++;
			if (HasExpired())
			{
				return TemporaryPasswordValidationResult.Expired;
			}
			else
			{
				return string.Equals(User.LoginName, userLoginName, StringComparison.OrdinalIgnoreCase) && userPassword.Equals(Password) ? TemporaryPasswordValidationResult.OK : TemporaryPasswordValidationResult.Invalid;
			}
		}
	}

	public enum TemporaryPasswordValidationResult
	{
		OK,
		Invalid,
		Expired
	}
}
