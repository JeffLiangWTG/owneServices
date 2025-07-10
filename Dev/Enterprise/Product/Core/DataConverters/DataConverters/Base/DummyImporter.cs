using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DataConverters
{
	public class DummyImporter : NonPersistentBusinessObject, IObsoleteValidation
	{
		public DummyImporter(BusinessObjectFactory factory, ZString dataLocation)
			: base(factory)
		{
			this.DataLocation = dataLocation;
			SetDefaults();
		}

		public DummyImporter(BusinessObjectFactory factory)
			: this(factory, "")
		{
		}

		#region Schema

		public static class Schema
		{
			public const string ConnectionText = "ConnectionText";
			public const string DataLocation = "DataLocation";
			public const string ServerName = "ServerName";
			public const string UserId = "UserId";
			public const string Password = "Password";
		}

		#endregion

		#region SetDefaults

		void SetDefaults()
		{
			using (SuspendSettingHasChanges())
			{
				UserId = "SYSDBA";
				Password = "masterkey";
				ServerName = "Cyber2";
			}
		}

		#endregion

		#region DataLocation

		[CargoWise.ComponentModel.MaxLength(512)]
		public ZString DataLocation
		{
			get { return fDataLocation; }
			set
			{
				CheckMaximumLength(DataLocationInfo, value);
				fDataLocation = value;
				DataLocationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo DataLocationInfo
		{
			get { return GetZPropertyInfo(Schema.DataLocation); }
		}
		ZString fDataLocation;

		#endregion

		#region Connection Text

		[CargoWise.ComponentModel.MaxLength(1024)]
		public ZString ConnectionText
		{
			get { return fConnectionText; }
			set
			{
				CheckMaximumLength(ConnectionTextInfo, value);
				fConnectionText = value;
				ConnectionTextInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ConnectionTextInfo
		{
			get { return GetZPropertyInfo(Schema.ConnectionText); }
		}
		ZString fConnectionText;

		#endregion

		#region ServerName

		[CargoWise.ComponentModel.MaxLength(512)]
		public ZString ServerName
		{
			get { return serverName; }
			set
			{
				CheckMaximumLength(ServerNameInfo, value);
				SetNonPersistentPropertyValue(ServerNameInfo, ref serverName, value);
			}
		}

		public ZPropertyInfo ServerNameInfo
		{
			get { return GetZPropertyInfo(Schema.ServerName); }
		}

		ZString serverName;

		#endregion

		#region UserId

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString UserId
		{
			get { return userId; }
			set
			{
				CheckMaximumLength(UserIdInfo, value);
				SetNonPersistentPropertyValue(UserIdInfo, ref userId, value);
			}
		}

		public ZPropertyInfo UserIdInfo
		{
			get { return GetZPropertyInfo(Schema.UserId); }
		}
		ZString userId;

		#endregion

		#region Password

		[CargoWise.ComponentModel.MaxLength(100)]
		public ZString Password
		{
			get { return fPassword; }
			set
			{
				CheckMaximumLength(PasswordInfo, value);
				fPassword = value;
				PasswordInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo PasswordInfo
		{
			get { return GetZPropertyInfo(Schema.Password); }
		}
		ZString fPassword;

		#endregion

		public void SetConnectionText()
		{
			ConnectionText = @"Driver={INTERSOLV InterBase ODBC Driver (*.gdb)}" +
				";" + "Server=" + ServerName.Trim() +
				";" + "Database=" + DataLocation.Trim() +
				";" + "Uid=" + UserId.Trim() +
				";" + "Pwd=" + Password.Trim();

			//Firebird connection:											
			//			ConnectionText = @"Database=" + DataLocation.Trim() + 
			//				";" + "User=" + UserId.Trim()  +
			//				";" + "Password=" + Password.Trim() + 
			//				";" + "Dialect=3" + 
			//				";" + "Server=" + ServerName.Trim();
		}
	}
}
