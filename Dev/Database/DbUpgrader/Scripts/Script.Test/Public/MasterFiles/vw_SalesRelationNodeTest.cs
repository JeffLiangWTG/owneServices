using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Testing
{
	[TestedType(typeof(vw_SalesRelationNode))]
	class vw_SalesRelationNodeTest : DbCreateScriptTest
	{
		public void TestGeneralUsage()
		{
			var insertPivotsSql = @"
			INSERT INTO dbo.RelatedActivityPivot
			(
				RAP_PK,
				RAP_ParentActivityTableCode,
				RAP_ParentActivityID,
				RAP_ChildActivityTableCode,
				RAP_ChildActivityID,
				RAP_SalesRelationTreeID,
				RAP_SystemCreateTimeUtc,
				RAP_SystemCreateUser,
				RAP_SystemLastEditTimeUtc,
				RAP_SystemLastEditUser
			)
			VALUES
				(newid(), 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'G8', '4e33c461-ed10-465c-a064-f8e5a52054cc', '4e33c461-ed10-465c-a064-f8e5a52054cc', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(newid(), 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'O1', '3e1fc5e3-b779-4177-b273-1d36461089e1', '3e1fc5e3-b779-4177-b273-1d36461089e1', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(newid(), 'O1', '3e1fc5e3-b779-4177-b273-1d36461089e1', 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', '3e1fc5e3-b779-4177-b273-1d36461089e1', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(newid(), 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', 'P8', 'eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c', 'eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(newid(), 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', 'TH', 'bd41879b-62ea-406f-8549-9c48c21da95b', 'bd41879b-62ea-406f-8549-9c48c21da95b', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(newid(), 'VB', 'eecba5d5-2390-4361-b982-cd38dad9d2bd', 'OQ', 'ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8', 'eecba5d5-2390-4361-b982-cd38dad9d2bd', GetUtcDate(), 'E', GetUtcDate(), 'E'),
				(newid(), 'AH', '7dc9fe58-15bb-4d5c-88f9-2fd5ab3f24ca', 'VB', 'eecba5d5-2390-4361-b982-cd38dad9d2bd', NULL, GetUtcDate(), 'E', GetUtcDate(), 'E'), -- non sales-relation pivot
				(newid(), 'G0', 'fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626', 'IM', 'd2054803-3a6c-4ba9-84dd-d5acd3b1c17a', 'd2054803-3a6c-4ba9-84dd-d5acd3b1c17a', GetUtcDate(), 'E', GetUtcDate(), 'E')
			";
			/*
			 * 	  G0    AH
			 * 	  /|\    |
			 * 	 / | \   |
			 * IM G8 O1 VB
			 * 	      | /
			 *	      |/
			 *	     OQ
			 *	      |\
			 *	      | \
			 *	     P8 TH
			 * 
			 * */

			using (var command = TestConnection.Command(insertPivotsSql))
			{
				command.ExecuteNonQuery();
			}

			var trees = ReadAllTrees();
			var g8Tree = trees[Guid.Parse("4e33c461-ed10-465c-a064-f8e5a52054cc")];
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"G0 fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626 CAM",
					"G8 4e33c461-ed10-465c-a064-f8e5a52054cc CAM",
				},
				g8Tree.Activities.Select(x => string.Format("{0} {1} {2}", x.TableCode, x.ID, x.Type)));

			var o1Tree = trees[Guid.Parse("3e1fc5e3-b779-4177-b273-1d36461089e1")];
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"G0 fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626 CAM",
					"O1 3e1fc5e3-b779-4177-b273-1d36461089e1 INQ",
					"OQ ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8 COM",
				},
				o1Tree.Activities.Select(x => string.Format("{0} {1} {2}", x.TableCode, x.ID, x.Type)));

			var p8Tree = trees[Guid.Parse("eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c")];
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"OQ ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8 COM",
					"P8 eef57e46-f43e-43c4-bc3b-2cf82e6e3a0c OPP",
				},
				p8Tree.Activities.Select(x => string.Format("{0} {1} {2}", x.TableCode, x.ID, x.Type)));

			var thTree = trees[Guid.Parse("bd41879b-62ea-406f-8549-9c48c21da95b")];
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"OQ ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8 COM",
					"TH bd41879b-62ea-406f-8549-9c48c21da95b QTE",
				},
				thTree.Activities.Select(x => string.Format("{0} {1} {2}", x.TableCode, x.ID, x.Type)));

			var vbTree = trees[Guid.Parse("eecba5d5-2390-4361-b982-cd38dad9d2bd")];
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"VB eecba5d5-2390-4361-b982-cd38dad9d2bd QBK",
					"OQ ce6e6b1e-81b2-4750-9d0a-a97e556bb2a8 COM",
				},
				vbTree.Activities.Select(x => string.Format("{0} {1} {2}", x.TableCode, x.ID, x.Type)));

			var imTree = trees[Guid.Parse("d2054803-3a6c-4ba9-84dd-d5acd3b1c17a")];
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					"G0 fb1b3bd0-6bdc-48fc-9ca5-175d94e9c626 CAM",
					"IM d2054803-3a6c-4ba9-84dd-d5acd3b1c17a INC",
				},
				imTree.Activities.Select(x => string.Format("{0} {1} {2}", x.TableCode, x.ID, x.Type)));

			AssertEquals("trees.Count", 6, trees.Count);
		}

		#region Implementation

		Dictionary<Guid, Tree> ReadAllTrees()
		{
			var selectSql = @"SELECT * FROM dbo.vw_SalesRelationNode";
			using (var command = TestConnection.Command(selectSql))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, Tree>();

					while (reader.Read())
					{
						var treeID = (Guid)reader["SalesRelationTreeID"];

						Tree tree;
						if (!result.TryGetValue(treeID, out tree))
						{
							tree = new Tree(treeID);
							result[treeID] = tree;
						}

						var activityTableCode = (string)reader["ActivityTableCode"];
						var activityId = (Guid)reader["ActivityID"];
						var activityType = reader["ActivityType"] == DBNull.Value ? "" : (string)reader["ActivityType"];

						tree.Activities.Add(new Activity(activityTableCode, activityId, activityType));
					}

					return result;
				}
			}
		}

		class Tree
		{
			public Tree(Guid id)
			{
				ID = id;
			}

			public readonly Guid ID;

			public List<Activity> Activities = new List<Activity>();
		}

		class Activity
		{
			public Activity(string tableCode, Guid id, string type)
			{
				TableCode = tableCode;
				ID = id;
				Type = type;
			}

			public readonly string TableCode;
			public readonly Guid ID;
			public readonly string Type;
		}
		#endregion
	}
}

