using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.MasterFiles.Org;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.MasterFiles.Org.Testing
{
	[TestedType(typeof(GetAllowedOrgPKs))]
	class GetAllowedOrgPKsTest : DbCreateScriptTest
	{
		public void TestDirectRelation()
		{
			var parentPK = Guid.NewGuid();
			var childPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(parentPK, childPK)}
{InsertOrgRelatedParty(childPK, parentPK)}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(parentPK);
			AssertResult(childPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(2, result.Count);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(childPK, result);
			}
		}

		public void TestSiblingRelation()
		{
			var parentPK = Guid.NewGuid();
			var child1PK = Guid.NewGuid();
			var child2PK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(parentPK, child1PK, child2PK)}
{InsertOrgRelatedParty(child1PK, parentPK)}
{InsertOrgRelatedParty(child2PK, parentPK)}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(parentPK);
			AssertResult(child1PK);
			AssertResult(child2PK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(3, result.Count);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(child1PK, result);
				AssertCollectionContains(child2PK, result);
			}
		}

		public void TestTwoLevelRelation()
		{
			var grandParentPK = Guid.NewGuid();
			var parentPK = Guid.NewGuid();
			var childPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(grandParentPK, parentPK, childPK)}
{InsertOrgRelatedParty(parentPK, grandParentPK)}
{InsertOrgRelatedParty(childPK, parentPK)}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(grandParentPK);
			AssertResult(parentPK);
			AssertResult(childPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(3, result.Count);
				AssertCollectionContains(grandParentPK, result);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(childPK, result);
			}
		}

		public void TestNephewRelation()
		{
			var grandParentPK = Guid.NewGuid();
			var unclePK = Guid.NewGuid();
			var parentPK = Guid.NewGuid();
			var childPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(grandParentPK, unclePK, parentPK, childPK)}
{InsertOrgRelatedParty(unclePK, grandParentPK)}
{InsertOrgRelatedParty(parentPK, grandParentPK)}
{InsertOrgRelatedParty(childPK, parentPK)}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(grandParentPK);
			AssertResult(unclePK);
			AssertResult(parentPK);
			AssertResult(childPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(4, result.Count);
				AssertCollectionContains(grandParentPK, result);
				AssertCollectionContains(unclePK, result);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(childPK, result);
			}
		}

		public void TestCousinRelation()
		{
			var grandParentPK = Guid.NewGuid();
			var unclePK = Guid.NewGuid();
			var parentPK = Guid.NewGuid();
			var cousinPK = Guid.NewGuid();
			var childPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(grandParentPK, unclePK, parentPK, cousinPK, childPK)}
{InsertOrgRelatedParty(unclePK, grandParentPK)}
{InsertOrgRelatedParty(parentPK, grandParentPK)}
{InsertOrgRelatedParty(cousinPK, unclePK)}
{InsertOrgRelatedParty(childPK, parentPK)}");
			Db.Connection.ExecuteNonQuery(commandText);

			AssertResult(grandParentPK);
			AssertResult(unclePK);
			AssertResult(parentPK);
			AssertResult(cousinPK);
			AssertResult(childPK);

			void AssertResult(Guid currentOrgPK)
			{
				var result = GetResult(currentOrgPK);
				AssertEquals(5, result.Count);
				AssertCollectionContains(grandParentPK, result);
				AssertCollectionContains(unclePK, result);
				AssertCollectionContains(parentPK, result);
				AssertCollectionContains(cousinPK, result);
				AssertCollectionContains(childPK, result);
			}
		}

		public void TestUnrelated()
		{
			var parentPK = Guid.NewGuid();
			var childPK = Guid.NewGuid();
			var anotherPK = Guid.NewGuid();

			var commandText = FormattableString.Invariant(
$@"{InsertOrgHeaders(parentPK, childPK, anotherPK)}
{InsertOrgRelatedParty(childPK, parentPK)}
{InsertOrgRelatedParty(childPK, anotherPK, "ABC")}");
			Db.Connection.ExecuteNonQuery(commandText);

			var result = GetResult(anotherPK);
			AssertEquals(1, result.Count);
			AssertCollectionContains(anotherPK, result);
		}

		static string InsertOrgHeaders(params Guid[] pks)
		{
			var lines = pks.Select(pk => FormattableString.Invariant($"INSERT INTO dbo.OrgHeader(OH_PK, OH_Code) VALUES('{pk}', '{pk.ToString("n").Substring(0, OrgHeaderSchema.OH_Code.MaxLength)}')"));
			return string.Join(System.Environment.NewLine, lines);
		}

		static string InsertOrgRelatedParty(Guid parentPK, Guid relatedPK, string partyType = "MNG")
		{
			return FormattableString.Invariant($"INSERT INTO dbo.OrgRelatedParty (PR_PK, PR_OH_Parent, PR_OH_RelatedParty, PR_PartyType) VALUES (newid(), '{parentPK}', '{relatedPK}', '{partyType}');");
		}

		IReadOnlyCollection<Guid> GetResult(Guid currentOrgPK)
		{
			return DataUtils.GetListOfValuesFromQuery(TestConnection, FormattableString.Invariant($"SELECT OrgPK FROM GetAllowedOrgPKs('{currentOrgPK}')")).Select(Guid.Parse).ToList();
		}
	}
}

