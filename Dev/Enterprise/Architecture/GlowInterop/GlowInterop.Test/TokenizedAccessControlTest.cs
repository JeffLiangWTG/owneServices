using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GlowInterop.Test
{
	class TokenizedAccessControlTest : TransactionedTestCase
	{
		public void TestCreatesConsumableToken()
		{
			var info = new AccessTokenInfo("AA", Guid.Empty, "Z0");
			var token = accessControl.CreateLimitedToken(DummyAccessTokenTypes.Dummy1, info, maxUses: 2);

			AccessTokenInfo consumptionInfo;
			var result = accessControl.TryConsume(token, DummyAccessTokenTypes.Dummy1, out consumptionInfo);
			AssertEquals(true, result);
			AssertEquals(info.Scope, consumptionInfo.Scope);
			AssertEquals(info.ParentId, consumptionInfo.ParentId);
			AssertEquals(info.ParentTableCode, consumptionInfo.ParentTableCode);

			result = accessControl.TryConsume(token, DummyAccessTokenTypes.Dummy1, out consumptionInfo);
			AssertEquals(true, result);
			AssertEquals(info.Scope, consumptionInfo.Scope);
			AssertEquals(info.ParentId, consumptionInfo.ParentId);
			AssertEquals(info.ParentTableCode, consumptionInfo.ParentTableCode);

			result = accessControl.TryConsume(token, DummyAccessTokenTypes.Dummy1, out consumptionInfo);
			AssertEquals(false, result);
		}

		public void TestUnableToCreateToken()
		{
			var accessControlMock = new Mock<ITokenizedAccessControl>();
			accessControlMock.Setup(a => a.TryCreate(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<DateTime?>(), It.IsAny<int>(), It.IsAny<AccessTokenInfo>())).Returns(false);
			AssertExceptionThrown<TokenGenerationException>(() => accessControlMock.Object.CreateLimitedToken(DummyAccessTokenTypes.Dummy1, new AccessTokenInfo("AA", Guid.Empty, "Z0")));
		}

		[ExpectNoExceptions]
		public void TestCreatesUniqueTokens()
		{
			var tokens = new HashSet<string>();
			for (int i = 0; i < 100; i++)
			{
				var token = accessControl.CreateLimitedToken(DummyAccessTokenTypes.Dummy1, new AccessTokenInfo("AA", Guid.Empty, "Z0"));

				if (!tokens.Add(token))
				{
					Assert(string.Format(CultureInfo.InvariantCulture, "Duplicate token generated: \"{0}\"", token), false);
				}
			}
		}

		public void TestCannotConsumeTokenCreatedForADifferentType()
		{
			var info = new AccessTokenInfo("AA", Guid.Empty, "Z0");
			var token = accessControl.CreateLimitedToken(DummyAccessTokenTypes.Dummy1, info, maxUses: 2);

			AccessTokenInfo consumptionInfo;
			var result = accessControl.TryConsume(token, DummyAccessTokenTypes.Dummy2, out consumptionInfo);
			AssertEquals(false, result);
		}

		public void TestCanConsumeTokenCreatedWithStoredProcedureDirectly()
		{
			var parentId = new Guid();

			CreateDirectly("MyFancyToken", DummyAccessTokenTypes.Dummy1, "drwxrwxrwx", parentId, "ZD1", false, null, 1, "8-)");

			AccessTokenInfo info;
			var result = accessControl.TryConsume("MyFancyToken", DummyAccessTokenTypes.Dummy1, out info);
			AssertEquals(true, result);
			AssertEquals("drwxrwxrwx", info.Scope);
			AssertEquals(parentId, info.ParentId);
			AssertEquals("ZD1", info.ParentTableCode);

			result = accessControl.TryConsume("MyFancyToken", DummyAccessTokenTypes.Dummy1, out info);
			AssertEquals("Should not be able to consume a consumed token.", false, result);
		}

		public void TestCreatesTokenThatCanBeConsumedWithStoredProcedureDirectly()
		{
			var info = new AccessTokenInfo("SomeScopeData", Guid.NewGuid(), "Z0");
			var token = accessControl.CreateLimitedToken(DummyAccessTokenTypes.Dummy1, info, maxUses: 1);

			AccessTokenInfo consumptionInfo;
			var result = TryConsumeDirectly(token, DummyAccessTokenTypes.Dummy1, out consumptionInfo);

			AssertEquals(true, result);
			AssertEquals(info.Scope, consumptionInfo.Scope);
			AssertEquals(info.ParentId, consumptionInfo.ParentId);
			AssertEquals(info.ParentTableCode, consumptionInfo.ParentTableCode);
		}

		public void TestPeekTokenDoesNotConsume()
		{
			var info = new AccessTokenInfo("SomeScopeData", Guid.NewGuid(), "Z0");
			var token = accessControl.CreateLimitedToken(DummyAccessTokenTypes.Dummy1, info, maxUses: 1);

			bool result;

			for (var i = 0; i < 5; i++)
			{
				result = accessControl.TryPeek(token, DummyAccessTokenTypes.Dummy1, out var consumptionInfo);

				AssertEquals(true, result);
				AssertEquals(info.Scope, consumptionInfo.Scope);
				AssertEquals(info.ParentId, consumptionInfo.ParentId);
				AssertEquals(info.ParentTableCode, consumptionInfo.ParentTableCode);
			}

			result = accessControl.TryConsume(token, DummyAccessTokenTypes.Dummy1, out _);
			AssertEquals(true, result);

			result = accessControl.TryPeek(token, DummyAccessTokenTypes.Dummy1, out _);
			AssertEquals(false, result);

			result = accessControl.TryConsume(token, DummyAccessTokenTypes.Dummy1, out _);
			AssertEquals(false, result);
		}

		public void TestCreatesTokenFromDesiredCharacterSetAndLength()
		{
			var characterSet = new List<char>("az");
			var length = 8;

			var info = new AccessTokenInfo("SomeScopeData", Guid.NewGuid(), "Z0");
			var token = accessControl.CreateLimitedToken(DummyAccessTokenTypes.Dummy1, info, null, 1, characterSet, length);

			AssertEquals(length, token.Length);

			foreach (var character in token)
			{
				AssertCollectionContains(character, characterSet);
			}
		}

		#region Implementation

		static class DummyAccessTokenTypes
		{
			public const string Dummy1 = @"\o/";
			public const string Dummy2 = @"-_-";
		}

		static void CreateDirectly(string token, string type, string scope, Guid parentId, string parentTableCode, bool isPermanent, DateTime? expiresAtUtc, int useCount, string createUser)
		{
			bool result;
			using (var command = Db.Connection.Command("CreateAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddParameter("@Scope", SqlDbType.VarChar, scope);
				command.AddParameter("@ParentId", SqlDbType.UniqueIdentifier, parentId);
				command.AddParameter("@ParentTableCode", SqlDbType.VarChar, parentTableCode);
				command.AddParameter("@IsPermanent", SqlDbType.Bit, isPermanent);
				command.AddParameter("@ExpiresAtUtc", SqlDbType.DateTime, (object)expiresAtUtc ?? DBNull.Value);
				command.AddParameter("@UseCount", SqlDbType.Int, useCount);
				command.AddParameter("@CreateUser", SqlDbType.VarChar, createUser);
				command.AddOutputParameter("@CATResult", SqlDbType.Bit, 0, 0, 0, DBNull.Value);

				command.ExecuteNonQuery();
				result = (bool)command.GetParameterValue("@CATResult");
			}

			AssertEquals(true, result);
		}

		static bool TryConsumeDirectly(string token, string type, out AccessTokenInfo info)
		{
			using (var command = Db.Connection.Command("TryConsumeAccessToken"))
			{
				command.CommandType = CommandType.StoredProcedure;

				command.AddParameter("@Token", SqlDbType.VarChar, token);
				command.AddParameter("@Type", SqlDbType.VarChar, type);
				command.AddOutputParameter("@Scope", SqlDbType.VarChar, int.MaxValue, 0, 0, null);
				command.AddOutputParameter("@ParentId", SqlDbType.UniqueIdentifier, 0, 0, 0, null);
				command.AddOutputParameter("@ParentTableCode", SqlDbType.VarChar, 3, 0, 0, null);
				command.AddOutputParameter("@TCATResult", SqlDbType.Bit, 0, 0, 0, null);

				command.ExecuteNonQuery();
				var result = (bool)command.GetParameterValue("@TCATResult");

				if (!result)
				{
					info = default(AccessTokenInfo);
					return false;
				}

				info = new AccessTokenInfo(
						(string)command.GetParameterValue("@Scope"),
						(Guid)command.GetParameterValue("@ParentId"),
						(string)command.GetParameterValue("@ParentTableCode"));
				return true;
			}
		}

		ITokenizedAccessControl accessControl;

		protected override void SetUp()
		{
			base.SetUp();

			accessControl = new TokenizedAccessControl();
		}

		protected override void TearDown()
		{
			accessControl = null;
			base.TearDown();
		}

		#endregion
	}
}
