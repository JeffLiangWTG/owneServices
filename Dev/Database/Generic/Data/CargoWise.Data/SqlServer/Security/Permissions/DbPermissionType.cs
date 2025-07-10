namespace CargoWise.Data
{
	public sealed class DbPermissionType
	{
		#region Constants

		public static class Constants
		{
			public static class Select
			{
				public const string Code = "SL";
				public const string Description = "SELECT";
			}

			public static class Impersonate
			{
				public const string Code = "IM";
				public const string Description = "IMPERSONATE";
			}
		}

		#endregion // Constants

		DbPermissionType(string code, string description)
		{
			Code = code;
			Description = description;
		}

		public string Code { get; }
		public string Description { get; }

		public static DbPermissionType Select => new DbPermissionType(Constants.Select.Code, Constants.Select.Description);

		public static DbPermissionType Impersonate => new DbPermissionType(Constants.Impersonate.Code, Constants.Impersonate.Description);
	}
}
