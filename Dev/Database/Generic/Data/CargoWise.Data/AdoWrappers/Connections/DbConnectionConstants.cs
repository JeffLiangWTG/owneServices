using System.Collections.Generic;

namespace CargoWise.Data
{
	public static class DbConnectionConstants
	{
		#region Application Names

		public static class ApplicationNames
		{
			public const string CargoWiseOne = "CargoWiseOne";
			public const string ServiceHost = "CargoWiseOneServiceHost";
			public const string ServiceRunner = "CargoWiseOneServiceRunner";
		}

		public static List<string> ApplicationNamesList = new List<string>
		{
			ApplicationNames.CargoWiseOne,
			ApplicationNames.ServiceHost,
			ApplicationNames.ServiceRunner,
		};

		#endregion

	}
}
