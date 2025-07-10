using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.ClusterKey.Testing
{
	[TestsSubclassesOf(typeof(IClusterKeyWorker), RequireTestOnlyInFirstSubLevel = true, IncludeAbstractClasses = true)]
	public abstract class ClusterKeyWorkerMandatoryTest : ClusterKeyEntityTest
	{
		public void TestSetClusterKeyOnWorkerObject()
		{
			if (IsFkToParentMandatory())
			{
				AssertSetClusterKeyOnWorkerObjectWithMandatoryParent();
			}
			else
			{
				AssertSetClusterKeyOnWorkerObjectWithNullableParent();
			}
		}

		void AssertSetClusterKeyOnWorkerObjectWithMandatoryParent()
		{
			CombineAssertions("[PRE-CONDITION]", () =>
			{
				AssertEquals("[FK to parent NOT NULL] Is FK valid?", true, ClusterKeyEntityToTest.FkToParentPty.Value.IsValid);
				AssertEquals("Initial ClusterKey value", 0, ClusterKeyEntityToTest.ClusterKeyPty.Value);
				AssertEquals("Initial Parent ClusterKey value", 0, GetClusterKeyParent(ClusterKeyEntityToTest).ClusterKeyPty.Value);
			});

			Factory.Save();

			CombineAssertions("After Save", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.ClusterKeyPty.Value);
				AssertEquals("Parent ClusterKey", 1, GetClusterKeyParent(ClusterKeyEntityToTest).ClusterKeyPty.Value);
			});

			ClusterKeyEntityToTest.ClusterKeyPty.Value = 999;

			Factory.Save();

			CombineAssertions("ClusterKey value tampered with. Saving should fix it.", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.ClusterKeyPty.Value);
				AssertEquals("Parent ClusterKey", 1, GetClusterKeyParent(ClusterKeyEntityToTest).ClusterKeyPty.Value);
			});

			var oldParent = GetClusterKeyParent(ClusterKeyEntityToTest);
			var parent2 = NewParentObject();
			PrepareNewParentBeforeChangingFk(parent2);
			ClusterKeyEntityToTest.FkToParentPty.Value = parent2.PK;

			Factory.Save();

			CombineAssertions("After setting FK to another Cluster Parent.", () =>
			{
				AssertEquals("Old (no longer) Parent ClusterKey", 1, oldParent.ClusterKeyPty.Value);

				AssertEquals("ClusterKey", 2, ClusterKeyEntityToTest.ClusterKeyPty.Value);
				AssertEquals("Parent ClusterKey", 2, GetClusterKeyParent(ClusterKeyEntityToTest).ClusterKeyPty.Value);
			});
		}

		/// <summary>
		/// Override this if parent entity has default values preventing setting this entity to reference it.
		/// E.G.:
		///   KR.JobComInvoiceLine automatically creates a new JobKRComInvoiceLine setting it as its child.
		///   The automatically created child must be deleted to prevent a unique index violation.
		/// </summary>
		protected virtual void PrepareNewParentBeforeChangingFk(EnterpriseBusinessObject newParent)
		{
			if (newParent is IAddInfoChildSupporter supporter)
			{
				supporter.AddInfoChild?.Delete();
			}
		}

		void AssertSetClusterKeyOnWorkerObjectWithNullableParent()
		{
			Assert("[PRE-CONDITION] Please ensure NewClusterKeyEntity returns an object attached to its Cluster Key parent.", ClusterKeyEntityToTest.FkToParentPty.Value.IsValid);
			Factory.Save();

			CombineAssertions("Entity with valid parent", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.ClusterKeyPty.Value);
				AssertEquals("Parent ClusterKey", 1, GetClusterKeyParent(ClusterKeyEntityToTest).ClusterKeyPty.Value);
			});

			var parentPk = ClusterKeyEntityToTest.FkToParentPty.Value;
			ClusterKeyEntityToTest.FkToParentPty.Value = ZGuid.Empty;
			Factory.Save();

			if (ClusterKeyEntityToTest is IClusterKeyMaster)
			{
				AssertEquals("[Worker-or-Master entity with no parent] ClusterKey", 2, ClusterKeyEntityToTest.ClusterKeyPty.Value);
			}
			else
			{
				AssertEquals("[Worker entity with no parent] ClusterKey", 0, ClusterKeyEntityToTest.ClusterKeyPty.Value);
			}

			ClusterKeyEntityToTest.FkToParentPty.Value = parentPk;
			Factory.Save();

			CombineAssertions("Entity re-attached to parent", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.ClusterKeyPty.Value);
				AssertEquals("Parent ClusterKey", 1, GetClusterKeyParent(ClusterKeyEntityToTest).ClusterKeyPty.Value);
			});
		}

		public void TestHasClusterKeyMasterAncestor()
		{
			var parent = NewParentObject();
			ClusterKeyEntityToTest.FkToParentPty.Value = parent.PK;

			var ancestorEntity = GetClusterKeyParent(ClusterKeyEntityToTest);

			while (ancestorEntity != null && !(ancestorEntity is IClusterKeyMaster))
			{
				ancestorEntity = GetClusterKeyParent((IClusterKeyWorker)ancestorEntity);
			}

			Assert("No IClusterKeyMaster ancestor found.", ancestorEntity is IClusterKeyMaster);
		}

		public void TestFkToParentPty()
		{
			AssertEquals("Is Worker-or-Master Entity FK to parent mandatory?", IsParentMandatoryButFkParentPtyOptional, ClusterKeyEntityToTest is IClusterKeyMaster && IsFkToParentMandatory());
			AssertEquals("FK to parent is valid or optional.", true, ClusterKeyEntityToTest.FkToParentPty.Value.IsValid || !IsFkToParentMandatory());
		}

		public void TestClusterKeyChildren()
		{
			var expectedResults = PrepareDataAndGetExpectedClusterKeyChildren();
			var clusterKeyChildList = ClusterKeyEntityToTest.ClusterKeyChildList;

			if (expectedResults == null)
			{
				AssertNull("ClusterKeyChildList", clusterKeyChildList);
			}
			else
			{
				var clusterKeyChildren = ClusterKeyChildInfoTest.LoadChildClusterKeyEntities(clusterKeyChildList, ClusterKeyEntityToTestAsBizObj);

				CombineAssertions("Cluster Key Children contain:", () =>
				{
					foreach (var expectedChild in expectedResults)
					{
						AssertEquals(expectedChild.GetType().Name, true, clusterKeyChildren.Contains(expectedChild));
					}
				});
			}
		}

		public void TestClusterKeyChildListEnlistsAllChildren()
		{
			var tableName = ClusterKeyEntityToTestAsBizObj.TableName;

			AssertCandidateChildrenAreListed(tableName);

			if (ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk != null && ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk.Any())
			{
				AssertExemptedChildrenPointToValidClusterKeyParent(tableName, string.Join(",", ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk.Select(c => $"('{c.Name}')")));
			}
		}

		void AssertCandidateChildrenAreListed(string tableName)
		{
			string GetSqlValuesTable(IEnumerable<SchemaGuidColumn> ckChildList)
			{
				return (ckChildList == null || !ckChildList.Any()) ? "(NULL)" : string.Join(",", ckChildList.Select(c => $"('{c.TableName}')"));
			}

			var enlistedChildren = GetSqlValuesTable((ClusterKeyEntityToTest.ClusterKeyChildList?.Select(c => c.FkColumn) ?? Enumerable.Empty<SchemaGuidColumn>()).Union(ExemptedClusterKeyChildList));
			var exemptedChildren = GetSqlValuesTable(ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk);

			var sql = $@"
				SELECT
					ChildTable = fktab.name,
					ClusterKeyColumn = ckcol.name
				FROM
					sys.tables fktab
					INNER JOIN sys.foreign_keys fk
						ON fk.parent_object_id = fktab.object_id
					INNER JOIN sys.tables pktab
						ON pktab.object_id = fk.referenced_object_id
					INNER JOIN sys.columns ckcol
						ON ckcol.object_id = fktab.object_id
						AND ckcol.name like '%[_]ClusterKey'
					LEFT JOIN (VALUES {enlistedChildren}) EnlistedChildren (TableName) ON EnlistedChildren.TableName = fktab.name
					LEFT JOIN (VALUES {exemptedChildren}) ExemptedCandidates (TableName) ON ExemptedCandidates.TableName = fktab.name
				WHERE
					pktab.name = '{tableName}'
					AND fktab.name <> '{tableName}'
					AND EnlistedChildren.TableName is null
					AND ExemptedCandidates.TableName is null";

			var nonListedCandidateChildren = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			CombineAssertions(
				"The following child tables not included in ClusterKeyChildList.\r\n"
				+ "If cluster key comes from another parent please add them to ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk.",
				() =>
				{
					foreach (DataRow row in nonListedCandidateChildren.Rows)
					{
						Fail($"{row["ChildTable"]}.{row["ClusterKeyColumn"]}");
					}
				}
			);

			Assert("All candidate cluster key children should be listed.", nonListedCandidateChildren.Rows.Count == 0);
		}

		void AssertExemptedChildrenPointToValidClusterKeyParent(string tableName, string appointedFksToClusterKeyParent)
		{
			var sql = $@"
				SELECT
					AppointedFkToClusterKeyParent = ExemptedCandidates.FkColumn,
					ParentTable = fk.PkTable
				FROM
					(VALUES {appointedFksToClusterKeyParent}) AS ExemptedCandidates (FkColumn)
					LEFT JOIN dbo.vw_FkReferences AS fk ON ExemptedCandidates.FkColumn = fk.FkColumn
					LEFT JOIN sys.tables AS ptab ON ptab.name = fk.PkTable AND ptab.name <> '{tableName}'
					LEFT JOIN sys.columns AS pcol ON pcol.object_id = ptab.object_id AND pcol.name like '%[_]ClusterKey'
				WHERE
					pcol.name is null";

			var invalidAppointedClusterKeyParents = DataUtils.GetDataTableFromQuery(TestConnection, sql);

			CombineAssertions(
				"Invalid appointed cluster key parents.\r\n"
				+ "FK in ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk must point to another parent table with a ClusterKey column.",
				() =>
				{
					foreach (DataRow row in invalidAppointedClusterKeyParents.Rows)
					{
						var parentTable = row["ParentTable"].ToString();
						var reasonParentIsInvalid = parentTable.Equals(tableName, StringComparison.OrdinalIgnoreCase)
							? "is the very entity being tested"
							: "has no ClusterKey field";
						Fail($"[{row["AppointedFkToClusterKeyParent"]}] links to table [{parentTable}] which {reasonParentIsInvalid}.");
					}
				}
			);
		}

		protected virtual IEnumerable<SchemaGuidColumn> ExemptedCandidateChildrenRepresentedByActualParentClusterKeyFk { get; }

		protected virtual IEnumerable<SchemaGuidColumn> ExemptedClusterKeyChildList => Enumerable.Empty<SchemaGuidColumn>();

		public void TestClusterKeyPropagationToChildren()
		{
			var clusterKeyChildList = ClusterKeyEntityToTest.ClusterKeyChildList;
			var testChildObjects = PrepareDataAndGetExpectedClusterKeyChildren();

			if (clusterKeyChildList.IsNullOrEmpty())
			{
				AssertNull("Expected child objects when Cluster Key entity has no listed children.", testChildObjects);
			}
			else
			{
				Factory.Save();

				CombineAssertions("1st Save - Should set child object cluster key values.", () =>
				{
					AssertEquals("Parent ClusterKey", 1, GetClusterKeyParent(ClusterKeyEntityToTest).ClusterKeyPty.Value);
					AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.ClusterKeyPty.Value);

					foreach (var childObj in testChildObjects)
					{
						AssertEquals($"Child business object [{childObj.ClusterKeyPty.BizObj.TableName}] ClusterKey", 1, childObj.ClusterKeyPty.Value);
					}
				});

				var oldParent = GetClusterKeyParent(ClusterKeyEntityToTest);
				var parent2 = NewParentObject();
				PrepareNewParentBeforeChangingFk(parent2);
				ClusterKeyEntityToTest.FkToParentPty.Value = parent2.PK;

				Factory.Save();

				CombineAssertions("Change Cluster Parent - Should propagate new parent cluster key to all child objects.", () =>
				{
					AssertEquals("Parent ClusterKey", 2, GetClusterKeyParent(ClusterKeyEntityToTest).ClusterKeyPty.Value);
					AssertEquals("ClusterKey", 2, ClusterKeyEntityToTest.ClusterKeyPty.Value);

					foreach (var childObj in testChildObjects)
					{
						AssertEquals($"Child business object [{childObj.ClusterKeyPty.BizObj.TableName}] ClusterKey", 2, childObj.ClusterKeyPty.Value);
					}
				});
			}
		}

		public void TestDatabaseConstraint()
		{
			Assert("[PRE-CONDITION] Please ensure NewClusterKeyEntity returns an object attached to its Cluster Key parent.", ClusterKeyEntityToTest.FkToParentPty.Value.IsValid);
			Factory.Save();

			CombineAssertions("After Save", () =>
			{
				AssertEquals("ClusterKey", 1, ClusterKeyEntityToTest.ClusterKeyPty.Value);
				AssertNotNull("Parent Entity", GetClusterKeyParent(ClusterKeyEntityToTest));
			});

			var bizObj = ClusterKeyEntityToTestAsBizObj;

			const string assertSuffix = "with FK to ClusterKey Parent NOT NULL";
			AssertUpdateClusterKeyException(bizObj, 0, assertSuffix);
			AssertUpdateClusterKeyException(bizObj, -1, assertSuffix);

			if (!IsFkToParentMandatory())
			{
				AssertExceptionThrown(
					"Attempt to set Master-or-Worker entity ClusterKey = -1 in the database with FK to ClusterKey Parent NULL",
					typeof(SqlException),
					$"The UPDATE statement conflicted with the CHECK constraint \"Constraint_{ClusterKeyEntityToTest.ClusterKeyPty.Name}\"",
					() => UpdateClusterKeyAndSetParentFkToDefault(bizObj, -1),
					assertStartsWith: true);

				if (ClusterKeyEntityToTest is IClusterKeyMaster)
				{
					AssertExceptionThrown(
						"Attempt to set Master-or-Worker entity ClusterKey = 0 in the database with FK to ClusterKey Parent NULL",
						typeof(SqlException),
						$"The UPDATE statement conflicted with the CHECK constraint \"Constraint_{ClusterKeyEntityToTest.ClusterKeyPty.Name}\"",
						() => UpdateClusterKeyAndSetParentFkToDefault(bizObj, 0),
						assertStartsWith: true);

					AssertNoExceptionThrown(
						"Setting Master-or-Worker entity ClusterKey > 0 in the database with FK to ClusterKey Parent NULL",
						() => UpdateClusterKeyAndSetParentFkToDefault(bizObj, 1));
				}
				else
				{
					AssertExceptionThrown(
						"Attempt to set Worker-only entity ClusterKey > 0 in the database with FK to ClusterKey Parent NULL",
						typeof(SqlException),
						$"The UPDATE statement conflicted with the CHECK constraint \"Constraint_{ClusterKeyEntityToTest.ClusterKeyPty.Name}\"",
						() => UpdateClusterKeyAndSetParentFkToDefault(bizObj, 1),
						assertStartsWith: true);

					AssertNoExceptionThrown(
						"Setting Worker-only entity ClusterKey = 0 in the database with FK to ClusterKey Parent NULL",
						() => UpdateClusterKeyAndSetParentFkToDefault(bizObj, 0));
				}
			}
		}

		protected virtual void UpdateClusterKeyAndSetParentFkToDefault(EnterpriseBusinessObject bizObj, int clusterKeyValue)
		{
			var indexOfUnderscore = ClusterKeyEntityToTest.FkToParentPty.Name.IndexOf('_');
			var prefix = ClusterKeyEntityToTest.FkToParentPty.Name.Substring(0, indexOfUnderscore);
			var parentPk = DefaultParentFk == null ? "NULL" : $"'{DefaultParentFk}'";
			var sql = $@"
				UPDATE {bizObj.TableName}
				SET
					{ClusterKeyEntityToTest.ClusterKeyPty.Name} = {clusterKeyValue}
					, {ClusterKeyEntityToTest.FkToParentPty.Name} = {parentPk}
					, {prefix}_SystemLastEditTimeUtc = GETUTCDATE()
					, {prefix}_SystemLastEditUser = '~BP'
				WHERE {bizObj.PKSchemaColumn.Name} = '{bizObj.PK}'";
			TestConnection.ExecuteNonQuery(sql);
		}

		protected abstract IEnumerable<IClusterKeyWorker> PrepareDataAndGetExpectedClusterKeyChildren();

		protected abstract EnterpriseBusinessObject NewParentObject();

		protected virtual bool IsFkToParentMandatory() => !IsColumnNullable(ClusterKeyEntityToTest.FkToParentPty);

		protected virtual ZGuid? DefaultParentFk => null;

		protected virtual bool IsParentMandatoryButFkParentPtyOptional => false;

		IClusterKeyEntity GetClusterKeyParent(IClusterKeyWorker clusterKeyWorker) => (IClusterKeyEntity)Factory.Load(clusterKeyWorker.ParentBizObjType, clusterKeyWorker.FkToParentPty.Value);

		new IClusterKeyWorker ClusterKeyEntityToTest => (IClusterKeyWorker)base.ClusterKeyEntityToTest;
	}
}
