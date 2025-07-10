namespace CargoWise.Data
{
	public sealed class DbPermissionClass
	{
		#region Constants

		public static class Constants
		{
			public static class Database
			{
				public const byte Code = 0;
				public const string Description = "DATABASE";
			}

			public static class Schema
			{
				public const byte Code = 3;
				public const string Description = "SCHEMA";
			}

			public static class User
			{
				public const byte Code = 4;
				public const string Description = "USER";
			}

			public static class Login
			{
				public const byte Code = 101;
				public const string Description = "LOGIN";
			}
		}

		#endregion // Constants

		DbPermissionClass(byte code, string description)
		{
			Code = code;
			Description = description;
		}

		public byte Code { get; }
		public string Description { get; }

		public static DbPermissionClass Database => new DbPermissionClass(Constants.Database.Code, Constants.Database.Description);
		public static DbPermissionClass Schema => new DbPermissionClass(Constants.Schema.Code, Constants.Schema.Description);
		public static DbPermissionClass User => new DbPermissionClass(Constants.User.Code, Constants.User.Description);
		public static DbPermissionClass Login => new DbPermissionClass(Constants.Login.Code, Constants.Login.Description);
	}
}
