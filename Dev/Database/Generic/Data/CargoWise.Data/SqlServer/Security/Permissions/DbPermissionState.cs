using System;

namespace CargoWise.Data
{
	public sealed class DbPermissionState
	{
		#region Constants

		public static class Constants
		{
			public static class Deny
			{
				public const string Code = "D";
				public const string Description = "DENY";
			}

			public static class Grant
			{
				public const string Code = "G";
				public const string Description = "GRANT";
			}
		}

		#endregion // Constants

		DbPermissionState(string code, string description)
		{
			Code = code;
			Description = description;
		}

		public string Code { get; }
		public string Description { get; }

		public static DbPermissionState Deny => new DbPermissionState(Constants.Deny.Code, Constants.Deny.Description);
		public static DbPermissionState Grant => new DbPermissionState(Constants.Grant.Code, Constants.Grant.Description);

		public static DbPermissionState GetState(string state)
		{
			if (state == Deny.Code)
			{
				return Deny;
			}
			else if (state == Grant.Code)
			{
				return Grant;
			}
			else
			{
				throw new NotImplementedException($"Unknown DbPermissionState code: '{state}'");
			}
		}
	}
}
