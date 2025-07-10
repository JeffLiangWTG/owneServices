using System;
using System.Globalization;
using System.Security.Principal;
using CargoWise.Data;
using CargoWise.DataProtection;
using CargoWise.DataProtection.DefaultSecrets;
using Enterprise.DbUpgrader.Transformation.DataModification;

namespace Enterprise.DbUpgrader.Transformations.Transforms.Architecture.DataProtection
{
	public class ProtectedDataStatusSeedForDefaults : DataTransformation
	{
		public override string UserDescription => "Create status records for default database credentials";

		protected override void OfflinePostUpgradeTransform()
		{
			base.OfflinePostUpgradeTransform();

			(Guid id, string comment)[] toRegister =
			{
				(SystemProtectedDataServiceWithDefaults.DefaultCargoWiseReaderLoginCredentialsId, "Default password for Reader account"),
				(SystemProtectedDataServiceWithDefaults.DefaultCargoWiseWriterLoginCredentialsId, "Default password for Writer account"),
				(SystemProtectedDataServiceWithDefaults.DefaultRestrictedReaderLoginCredentialsId, "Default password for RestrictedReader account"),
				(SystemProtectedDataServiceWithDefaults.DefaultRestrictedWriterLoginCredentialsId, "Default password for RestrictedWriter account"),
				(SystemProtectedDataServiceWithDefaults.DefaultUnrestrictedWriterLoginCredentialsId, "Default password for UnrestrictedWriter account"),
			};

			var pds = ProtectedDataService.SystemProtectedDataService;
			var connection = Db.Connection;

			foreach (var item in toRegister)
			{
				var protectedData = pds.LoadSecret(item.id);

				if (!connection.Exists($"FROM [dbo].[StmProtectedDataState] WHERE PDS_ProtectedDataID = '{item.id}'"))
				{
					RegisterProtectedData(connection, protectedData, item.comment);
					ActivateProtectedData(connection, protectedData, "Default Item Activation");
				}
			}
		}

		public void RegisterProtectedData(DbConnection connection, IProtectedData protectedData, string comments)
		{
			var registrationInfo = $"Registered at '{DateTimeOffset.Now.ToString(CultureInfo.InvariantCulture)}' on '{GetMachineName()}' by '{GetWindowsUserInfo()}'. Comments: {comments}";

			using var cmd = connection.Command("[dbo].[PDSRegisterProtectedData]");
			cmd.CommandType = System.Data.CommandType.StoredProcedure;
			cmd.AddParameter("@PDS_ProtectedDataID", System.Data.SqlDbType.UniqueIdentifier, protectedData.Id);
			cmd.AddParameter("@PDS_Type ", System.Data.SqlDbType.VarChar, 100, protectedData.Type);
			cmd.AddParameter("@PDS_ValidFrom", System.Data.SqlDbType.DateTimeOffset, protectedData.ValidFrom);
			cmd.AddParameter("@PDS_ValidTo", System.Data.SqlDbType.DateTimeOffset, protectedData.ValidTo);
			cmd.AddParameter("@PDS_RegistrationInfo", System.Data.SqlDbType.NVarChar, 1000, registrationInfo);
			cmd.ExecuteNonQuery();
		}
		public void ActivateProtectedData(DbConnection connection, IProtectedData protectedData, string comments)
		{
			var activationInfo = $"Activated at '{DateTimeOffset.Now.ToLocalTime().ToString("u", CultureInfo.InvariantCulture)}' on '{GetMachineName()}' by '{GetWindowsUserInfo()}'. Comments: {comments}";

			using var cmd = connection.Command("[dbo].[PDSActivateProtectedData]");
			cmd.CommandType = System.Data.CommandType.StoredProcedure;
			cmd.AddParameter("@PDS_ProtectedDataID", System.Data.SqlDbType.UniqueIdentifier, protectedData.Id);
			cmd.AddParameter("@PDS_ActivationInfo", System.Data.SqlDbType.NVarChar, 1000, activationInfo);

			cmd.ExecuteNonQuery();
		}

		string GetWindowsUserInfo()
		{
			return WindowsIdentity.GetCurrent(false) is WindowsIdentity wid ? wid.Name : $"{Environment.UserDomainName}\\{Environment.UserName}";
		}
		string GetMachineName()
		{
			return Environment.MachineName;
		}
	}
}
