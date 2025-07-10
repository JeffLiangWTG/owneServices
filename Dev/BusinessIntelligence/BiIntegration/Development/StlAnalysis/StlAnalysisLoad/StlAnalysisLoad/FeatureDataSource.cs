namespace Enterprise.StlAnalysis.Load
{
	using System;
	using System.Collections.Generic;

	public enum SourceLocation
	{
		CLIENT,
		EDIPROD,
		EROUTER,
		EDIFAX,
		DENIEDPARTY,
		EHUB,
	}

	class FeatureDataSource
	{
		public FeatureDataSource(string strLocation)
		{
			var matchedLocation = SourceLocation.CLIENT;

			foreach (var location in GetAllLocations())
			{
				if (strLocation.Equals(location.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					matchedLocation = location;
					break;
				}
			}

			sourceLocation = matchedLocation;
		}

		public SourceLocation Location
		{
			get { return sourceLocation; }
		}
		readonly SourceLocation sourceLocation;

		public bool IsMultitenanted
		{
			get { return (sourceLocation != SourceLocation.CLIENT); }
		}

		public static SqlServerInfo GetConnectionInfo(SourceLocation location, HostedClient client)
		{
			if (location == SourceLocation.CLIENT)
			{
				return client.DatabaseInfo;
			}
			else if (location == SourceLocation.EDIPROD)
			{
				return SqlServerInfo.NewEdiProdSqlServerInfo();
			}
			else if (location == SourceLocation.EDIFAX)
			{
				return SqlServerInfo.NewEdiFaxDbSqlServerInfo();
			}
			else if (location == SourceLocation.DENIEDPARTY)
			{
				return SqlServerInfo.NewDeniedPartyScreeningSqlServerInfo();
			}

			return null;
		}

		public static IEnumerable<SourceLocation> GetAllLocations()
		{
			return (IEnumerable<SourceLocation>)Enum.GetValues(typeof(SourceLocation));
		}
	}
}
