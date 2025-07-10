#if DEBUG
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CargoWise.Data;
using CargoWise.Data.Utils;
using NUnit.Framework;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[ThreadSafe]
	public class FountainTestListener : BaseTestListener
	{
		internal FountainTestListener()
		{
		}

		public static void AddFountainAccess(string fountainName, Guid ownerPk)
		{
			if (fountainName == null)
			{
				throw new ArgumentNullException(nameof(fountainName));
			}

			Instance.AddFountainAccessCore(fountainName, ownerPk);
		}

		public static void SetLossyFountainAccess()
		{
			Instance.SetLossyFountainAccessCore();
		}

		protected void AddFountainAccessCore(string fountainName, Guid ownerPk)
		{
			if (fountainName == null)
			{
				throw new ArgumentNullException(nameof(fountainName));
			}

			lock (fountainIdsLocker)
			{
				fountainIds = fountainIds.Add(new FountainPair { Name = fountainName, Owner = ownerPk });
			}
		}

		protected void SetLossyFountainAccessCore()
		{
			lossyFountainAccessed = true;
			lossyFountainAccessedLastSetStacktrace = new StackTrace().ToString();
		}

		public override void BeforeEachTest(DateTime startTime)
		{
			if (previouslyExistedFountains == null)
			{
				previouslyExistedFountains = ReadExistingFountains();
			}
		}

		public override void AfterEachTest(DateTime endTime)
		{
			base.AfterEachTest(endTime);
			FountainPair[] ids;
			lock (fountainIdsLocker)
			{
				ids = fountainIds.ToArray();
				fountainIds = ImmutableList<FountainPair>.Empty;
			}
			DeleteFountains(previouslyExistedFountains, ids);
			ResetLossyFountains();
		}

		internal const string DeleteStmNumsCreatedInTestScript = "DELETE FROM dbo.StmNums WHERE SN_Id IN (SELECT SN_Id FROM [dbo].[StmNums] TestStmNumsData WHERE ";
		internal static void DeleteFountains(IReadOnlyCollection<int> existingFountainIds, IReadOnlyCollection<FountainPair> fountainIdsToDelete)
		{
			if (fountainIdsToDelete == null || fountainIdsToDelete.Count == 0)
			{
				return;
			}

			using (var connection = Db.NewExtraConnectionToMainDb())
			{
				var sql = DeleteStmNumsCreatedInTestScript + $"{string.Join(" OR ", fountainIdsToDelete.Select(pair => $"SN_Name='{pair.Name}' AND SN_Owner='{pair.Owner}'"))})";
				if (existingFountainIds?.Count > 0)
				{
					sql += $" AND SN_Id NOT IN ({string.Join(",", existingFountainIds)})";
				}
				using (var cmd = connection.Command(sql))
				{
					cmd.ExecuteScalar();
				}
			}
		}

		static int[] ReadExistingFountains()
		{
			using (var cmd = Db.Connection.Command("SELECT SN_Id FROM dbo.StmNums"))
			{
				using (var reader = cmd.ExecuteReader())
				{
					var result = new List<int>();
					while (reader.Read())
					{
						result.Add(reader.GetValue<int>("SN_Id"));
					}
					return result.ToArray();
				}
			}
		}

		void ResetLossyFountains()
		{
			if (lossyFountainAccessed)
			{
				lossyFountainAccessed = false;
				var fountains = GetLossyFountainNamesToReset();
				var resetCommandBuilder = new StringBuilder();
				foreach (var fountain in fountains)
				{
					resetCommandBuilder.Append($@"
IF OBJECT_ID('{fountain}', N'SO') IS NOT NULL
ALTER SEQUENCE [{fountain}] RESTART
");
				}

				var resetCommand = resetCommandBuilder.ToString();
				if (!string.IsNullOrEmpty(resetCommand))
				{
					using (var adminConnection = Db.NewAdminConnection())
					{
						adminConnection.ExecuteNonQuery(resetCommand);
					}
				}
			}
		}

		List<string> GetLossyFountainNamesToReset()
		{
			using (var cmd = Db.Connection.Command($@"SELECT name FROM sys.sequences WHERE last_used_value IS NOT NULL /* lossyFountainAccessed Last Set Stacktrace: {lossyFountainAccessedLastSetStacktrace}*/"))
			{
				using (var reader = cmd.ExecuteReader())
				{
					var result = new List<string>();
					while (reader.Read())
					{
						result.Add(reader.GetValue<string>("name"));
					}
					return result;
				}
			}
		}

		public static readonly FountainTestListener Instance = new FountainTestListener();
		readonly object fountainIdsLocker = new object();

		int[] previouslyExistedFountains;
		internal protected int[] PreviouslyExistedFountains
		{
			get
			{
				if (previouslyExistedFountains == null)
				{
					return ReadExistingFountains();
				}
				return previouslyExistedFountains;
			}
			set
			{
				previouslyExistedFountains = value;
			}
		}

		bool lossyFountainAccessed;
		ImmutableList<FountainPair> fountainIds = ImmutableList<FountainPair>.Empty;
		string lossyFountainAccessedLastSetStacktrace;

		internal class FountainPair
		{
			public string Name { get; set; }
			public Guid Owner { get; set; }
		}
	}
}

#endif
