using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Environment
{
	public class EmailGroupUtility
	{
		public StringCollection GetCompanyNotificationGroupEmails()
		{
			StringCollection groupEmails;
			Guid groupPK;
			GetCompanyNotificationGroupEmails(out groupEmails, out groupPK);
			return groupEmails;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public StringCollection GetCompanyNotificationGroupEmails(out string groupDetail)
		{
			StringCollection groupEmails;
			Guid groupPK;
			GetCompanyNotificationGroupEmails(out groupEmails, out groupPK);

			groupDetail = "";
			if (groupPK != Guid.Empty)
			{
				var sqlSelect = string.Format(CultureInfo.InvariantCulture,
	@"
SELECT {0}, {1}
FROM {2}
WHERE {3} = @groupPK
",
					GlbGroupSchema.Constants.GG_Code,
					GlbGroupSchema.Constants.GG_Desc,
					GlbGroupSchema.Constants.TableName,
					GlbGroupSchema.Constants.PK);

				using (Db.DisposableActionForDbConnection())
				using (var cmd = Db.Connection.Command(sqlSelect))
				{
					cmd.AddParameter("@groupPK", SqlDbType.UniqueIdentifier, groupPK);
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							var groupCode = reader[0] as string;
							var groupDesc = reader[1] as string;
							groupDetail = string.Format(CultureInfo.InvariantCulture, "'{0} - {1}'", groupCode.Trim(), groupDesc.Trim());
						}
					}
				}
			}
			return groupEmails;
		}

		void GetCompanyNotificationGroupEmails(out StringCollection groupEmails, out Guid groupPK)
		{
			IEnvironment env = EnvProxy.Instance;
			groupPK = env.Registry.NotificationGroup(env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			groupEmails = GetGroupEmailCollection(groupPK, false);
			if (groupEmails.Count == 0)
			{
				var systemGroupPK = env.Registry.NotificationGroup(Guid.Empty, Guid.Empty, Guid.Empty);
				if (systemGroupPK != groupPK)
				{
					groupEmails = GetGroupEmailCollection(systemGroupPK, false);
					if (groupEmails.Count > 0)
					{
						groupPK = systemGroupPK;
					}
				}
			}
		}

#if DEBUG
		public void SetNotificationGroup(Guid groupPK)
		{
			IEnvironment env = EnvProxy.Instance;
			env.Registry.RawRegistry.NotificationGroup.SetValue(env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupPK);
		}
#endif

		#region Notification email addresses

		public StringCollection PostMasters
		{
			get { return GetGroupEmailCollection(EnvProxy.Instance.Registry.PostMasterGroup, false); }
		}

		public StringCollection AdminStaff
		{
			get { return GetStaffEmailCollection(true); }
		}

		public StringCollection AllStaff
		{
			get { return GetStaffEmailCollection(false); }
		}

		#endregion

		public StringCollection SendNotificationToDatabaseAdministrator(Guid groupPK, bool throwExceptionIfEmptyGroup)
		{
			return EnvProxy.IsHostedWithCargowise ?
				GetHostedNotificationsEmailOverride() :
				GetGroupEmailCollection(groupPK, throwExceptionIfEmptyGroup);
		}

		public StringCollection GetHostedNotificationsEmailOverride()
		{
			return new StringCollection() { EnvProxy.Instance.Registry.HostedNotificationsEmailOverride };
		}

		public StringCollection GetGroupEmailCollection(Guid groupPK, bool throwExceptionIfEmptyGroup)
		{
			string language;
			return GetGroupEmailCollection(groupPK, throwExceptionIfEmptyGroup, out language);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public StringCollection GetGroupEmailCollection(Guid groupPK, bool throwExceptionIfEmptyGroup, out string language)
		{
			language = null;
			var groupEmails = new StringCollection();

			string sqlSelect = string.Format(CultureInfo.InvariantCulture,
@"
select {0}, {1}
  from {2}
 inner join {3} on {4} = {5}
 where {6} = @GroupPK
   and {7} = 1
",
				GlbStaffSchema.Constants.GS_EmailAddress,
				GlbStaffSchema.Constants.GS_WorkingLanguage,
				GlbGroupLinkSchema.Constants.TableName,
				GlbStaffSchema.Constants.TableName,
				GlbGroupLinkSchema.Constants.GK_GS,
				GlbStaffSchema.Constants.PK,
				GlbGroupLinkSchema.Constants.GK_GG,
				GlbStaffSchema.Constants.GS_IsActive);

			bool userInGroup = false;
			using (Db.DisposableActionForDbConnection())
			{
				var command = Db.Connection.Command(sqlSelect);
				command.AddParameterBasedOnDbColumn("@GroupPK", groupPK, GlbGroupLinkSchema.GK_GG);
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						userInGroup = true;
						string email = reader[0] as string;
						if (email != null && email.Trim().Length > 0)
						{
							groupEmails.Add(email);

							var staffLanguage = reader[1] as string;
							if (language == null)
							{
								language = staffLanguage;
							}
							else if (language != staffLanguage)
							{
								language = Res.DefaultLanguage;
							}
						}
					}
					reader.Close();
				}
			}

			if (throwExceptionIfEmptyGroup)
			{
				if (!userInGroup)
				{
					throw new EmailHasNoRecipientsException("No staff members were found in the staff group.");
				}
				else if (groupEmails.Count == 0)
				{
					throw new EmailHasNoRecipientsException("No members of the group have setup an email address.");
				}
			}

			if (string.IsNullOrEmpty(language))
			{
				language = Res.DefaultLanguage;
			}

			if (Constants.Groups.AllPK == groupPK)
			{
				RemoveHostNotificationEmails(groupEmails);
			}

			return groupEmails;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public StringCollection GetStaffEmailCollection(bool adminOnly)
		{
			var groupEmails = new StringCollection();

			var sqlSelect = "SELECT " + GlbStaffSchema.Constants.GS_EmailAddress + " FROM "
				+ GlbStaffSchema.Constants.SqlSchemaName + "." + GlbStaffSchema.Constants.TableName
				+ " WHERE " + GlbStaffSchema.Constants.GS_IsSystemAccount + " = 0"
				+ " AND " + GlbStaffSchema.Constants.GS_IsActive + " = 1";

			var hostingNotificationEmail = EnvProxy.Instance.Registry.HostedNotificationsEmailOverride;

			if (adminOnly)
			{
				sqlSelect += " AND " + GlbStaffSchema.Constants.GS_IsController + " = 1"; // SQL Statement
			}

			using (Db.DisposableActionForDbConnection())
			using (var reader = Db.Connection.Command(sqlSelect).ExecuteReader())
			{
				while (reader.Read())
				{
					var email = reader[0] as string;

					if (email != null && email.Trim().Length > 0)
					{
						groupEmails.Add(email);
					}
				}

				reader.Close();
			}

			if (!adminOnly)
			{
				RemoveHostNotificationEmails(groupEmails);
			}

			return groupEmails;
		}

		IEnumerable<string> HostEmails
		{
			get
			{
				return new string[]
				{
					EnvProxy.Instance.Registry.HostedNotificationsEmailOverride,
					"Hosting.Notifications@cargowise.com",
					"Hosting.Notifications@corporate.cargowise.com"
				};
			}
		}

		public bool IsHostNotificationEmail(string email)
		{
			return HostEmails.Any(x => x.Equals(email.Trim(), StringComparison.OrdinalIgnoreCase));
		}

		void RemoveHostNotificationEmails(StringCollection emails)
		{
			var removeList = new List<string>();

			foreach (var email in emails)
			{
				if (IsHostNotificationEmail(email))
				{
					removeList.Add(email);
				}
			}

			foreach (var email in removeList)
			{
				emails.Remove(email);
			}
		}

		public Guid GetNotificationGroup(Guid branchPK, Guid departmentPK)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var result = Db.Connection.ExecuteScalar("select " + GlbBranchSchema.GB_GC.Name + " from " + GlbBranchSchema.Constants.SqlSchemaName + "." + GlbBranchSchema.Constants.TableName + " where " + GlbBranchSchema.PK.Name + " = '" + branchPK + "'");
				var companyPK = result != null ? (Guid)result : Guid.Empty;
				return EnvProxy.Instance.Registry.NotificationGroup(companyPK, branchPK, departmentPK);
			}
		}

		public StringCollection GetNotificationMailList(Guid branchPK, Guid departmentPK)
		{
			StringCollection result = new StringCollection();
			Guid notificationGroup = GetNotificationGroup(branchPK, departmentPK);
			if (notificationGroup != Guid.Empty)
			{
				result = new EmailGroupUtility().GetGroupEmailCollection(notificationGroup, true);
			}
			return result;
		}
	}
}
