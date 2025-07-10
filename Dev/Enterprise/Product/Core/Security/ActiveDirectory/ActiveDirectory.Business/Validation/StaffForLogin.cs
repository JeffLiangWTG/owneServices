using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public class StaffForLogin : NonPersistentBusinessObject, IADLinkedEntity
	{
		public StaffForLogin() : base()
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static StaffForLogin LoadByStaffPK(Guid staffPK)
		{
			var sqlText = FormattableString.Invariant($@"SELECT {GlbStaffSchema.GS_LoginName.Name}, {GlbStaffSchema.GS_DomainName.Name}, {GlbStaffSchema.GS_IsActive.Name}, {GlbStaffSchema.GS_ActiveDirectoryObjectGuid.Name} FROM {GlbStaffSchema.Constants.SqlSchemaName}.{GlbStaffSchema.Constants.TableName} WHERE {GlbStaffSchema.PK.Name} = @staffPK");

			using (var command = Db.Connection.Command(sqlText)) // Have to use Sql query here as we can't use BizO during login authentication
			{
				command.AddParameter("@staffPK", System.Data.SqlDbType.UniqueIdentifier, staffPK);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						return new StaffForLogin
						{
							LoginName = reader[GlbStaffSchema.GS_LoginName.Name].ToString(),
							DomainName = reader[GlbStaffSchema.GS_DomainName.Name].ToString(),
							IsActive = Convert.ToBoolean(reader[GlbStaffSchema.GS_IsActive.Name], CultureInfo.InvariantCulture),
							ActiveDirectoryObjectGuid = new ZGuid(reader[GlbStaffSchema.GS_ActiveDirectoryObjectGuid.Name])
						};
					}
					else
					{
						return null;
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public static StaffForLogin LoadByADObjectGuid(Guid adObjectGuid)
		{
			var sqlText = FormattableString.Invariant($@"SELECT {GlbStaffSchema.GS_LoginName.Name}, {GlbStaffSchema.GS_DomainName.Name}, {GlbStaffSchema.GS_IsActive.Name} FROM {GlbStaffSchema.Constants.SqlSchemaName}.{GlbStaffSchema.Constants.TableName} WHERE {GlbStaffSchema.GS_ActiveDirectoryObjectGuid.Name} = @adObjectGuid");

			using (var command = Db.Connection.Command(sqlText)) // Have to use Sql query here as we can't use BizO during login authentication
			{
				command.AddParameter("adObjectGuid", System.Data.SqlDbType.UniqueIdentifier, adObjectGuid);
				using (var reader = command.ExecuteReader())
				{
					if (reader.Read())
					{
						return new StaffForLogin
						{
							ActiveDirectoryObjectGuid = adObjectGuid,
							LoginName = reader[GlbStaffSchema.GS_LoginName.Name].ToString(),
							DomainName = reader[GlbStaffSchema.GS_DomainName.Name].ToString(),
							IsActive = Convert.ToBoolean(reader[GlbStaffSchema.GS_IsActive.Name], CultureInfo.InvariantCulture)
						};
					}
					else
					{
						return null;
					}
				}
			}
		}

		public ZString LoginName { get; set; }

		public ZString DomainName
		{
			get => domainName;
			set => SetNonPersistentPropertyValue(DomainNameInfo, ref domainName, value);
		}
		ZString domainName;

		public ZPropertyInfo DomainNameInfo => GetZPropertyInfo(nameof(DomainName));

		public bool IsActive { get; set; }

		public ZGuid ActiveDirectoryObjectGuid
		{
			get => activeDirectoryObjectGuid;
			set => SetNonPersistentPropertyValue(ActiveDirectoryObjectGuidInfo, ref activeDirectoryObjectGuid, value);
		}
		ZGuid activeDirectoryObjectGuid;

		public ZPropertyInfo ActiveDirectoryObjectGuidInfo => GetZPropertyInfo(nameof(ActiveDirectoryObjectGuid));

		public ZBool IsADLinked => ActiveDirectoryObjectGuid.IsValid;

		public ZPropertyInfo IsADLinkedInfo => GetZPropertyInfo(nameof(IsADLinked));

		bool IADLinkedEntity.IsADLinkable => true;

		bool IADLinkedEntity.IsADIntegrationEnabled => ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled;

		ZDateTime IADLinkedEntity.SystemCreateTimeUtc => throw new NotImplementedException();

		ZDateTime IADLinkedEntity.SystemLastEditTimeUtc => throw new NotImplementedException();

		void IADLinkedEntity.SynchroniseWithAD() => throw new InvalidOperationException("Do not use this class for AD operation.");

		void IADLinkedEntity.DisconnectFromAD() => throw new InvalidOperationException("Do not use this class for AD operation.");
	}
}
