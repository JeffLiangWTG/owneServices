using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.OrgPatternMatching;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DataTransfer.Native.Common.PostUpdateProcesses.OrgPatternMatchRegen
{
	class PatternMatchDataManager : IPatternMatchDataManager
	{
		internal PatternMatchDataManager(MatchingOrganisation organisation, WrapperFactory factory, DbConnection dbConnection)
		{
			this.organisation = Argument.NotNull(organisation, "MatchingOrganisation organisation");
			this.factory = Argument.NotNull(factory, "WrapperFactory factory");
			this.dbConnection = Argument.NotNull(dbConnection, "DbConnection dbConnection");
		}

		readonly MatchingOrganisation organisation;
		readonly WrapperFactory factory;
		readonly DbConnection dbConnection;

		public ZBool AllowPatternMatchesWithEmptyAddress
		{
			get { return false; }
		}

		public IMatchingOrganisation Organisation
		{
			get { return organisation; }
		}

		public IEnumerable<IOrgPatternMatch> GetPatternMatchesAlreadyLoaded()
		{
			return new List<IOrgPatternMatch>();
		}

		public IEnumerable<IOrgPatternMatch> LoadPatternMatches(ZQuery query)
		{
			return factory.Load<MatchingPattern>(query);
		}

		public IOrgPatternMatch CreateNewPatternMatch()
		{
			var patternMatch = factory.New<MatchingPattern>();
			patternMatch.OS_OH = organisation.PK;
			return patternMatch;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		public void DeletePatternMatches(ZQuery query)
		{
			var tableName = TableNameAttribute.GetTableName<MatchingPattern>();

			var deleteCommand = "delete from " + tableName + " " + query.GetAsWhereClause(false);
			using (var command = dbConnection.Command(deleteCommand))
			{
				foreach (var param in query.Params)
				{
					command.AddParameter(param);
				}
				command.ExecuteNonQuery();
			}
		}
	}
}
