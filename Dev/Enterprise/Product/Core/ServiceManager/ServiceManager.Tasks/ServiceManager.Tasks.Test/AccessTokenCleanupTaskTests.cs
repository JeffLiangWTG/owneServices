using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using CargoWise.Data;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.ServiceManager.Tasks.SystemServices.Test
{
	[TestedType(typeof(AccessTokenCleanupTask))]
	public class AccessTokenCleanupTaskTests : ServiceTaskTestCase<AccessTokenCleanupTask>
	{
		public void TestCleansesOldExpiredTokens()
		{
			var scope = new AccessTokenInfo("some arbitrary data", Guid.Empty, "^_^");

			var permanentToken = accessControl.CreateLimitedToken("XXX", scope);
			MakePermanent(permanentToken);

			var validTokens = new[]
			{
				accessControl.CreateLimitedToken("AAA", scope, time: TimeSpan.FromMinutes(30)),
				accessControl.CreateLimitedToken("BBB", scope, maxUses: 1),
				accessControl.CreateLimitedToken("CCC", scope, time: TimeSpan.FromMinutes(30), maxUses: 1),
				permanentToken,
			};

			var expiredTokensToRemain = new[]
			{
				accessControl.CreateLimitedToken("DDD", scope, time: TimeSpan.FromDays(-7) + TimeSpan.FromMinutes(-30)),
				accessControl.CreateLimitedToken("EEE", scope, maxUses: -1),
				accessControl.CreateLimitedToken("FFF", scope, time: TimeSpan.FromDays(-7) + TimeSpan.FromMinutes(-30), maxUses: -1),
				accessControl.CreateLimitedToken("GGG", scope, time: TimeSpan.FromDays(-7) + TimeSpan.FromMinutes(-30), maxUses: 5),
				accessControl.CreateLimitedToken("HHH", scope),
			};

			var expiredTokensToRemove = new[]
			{
				accessControl.CreateLimitedToken("III", scope, time: TimeSpan.FromDays(-7) + TimeSpan.FromMinutes(-30)),
				accessControl.CreateLimitedToken("JJJ", scope, maxUses: 0),
				accessControl.CreateLimitedToken("KKK", scope, time: TimeSpan.FromDays(-7) + TimeSpan.FromMinutes(-30), maxUses: -1),
				accessControl.CreateLimitedToken("LLL", scope, time: TimeSpan.FromDays(-7) + TimeSpan.FromMinutes(-30), maxUses: 5),
				accessControl.CreateLimitedToken("MMM", scope),
			};

			foreach (var token in expiredTokensToRemove)
			{
				MakeOld(token, TimeSpan.FromDays(-7) + TimeSpan.FromHours(-1));
			}

			var task = new AccessTokenCleanupTask { BatchSize = 2 };
			InitialiseTaskSchedule(task);
			RunTaskSchedule(task, CancellationToken.None);

			foreach (var token in validTokens)
			{
				AssertTokenExistsAndIsConsumable(token);
			}

			foreach (var token in expiredTokensToRemain)
			{
				AssertTokenExistsAndIsNotConsumable(token);
			}

			foreach (var token in expiredTokensToRemove)
			{
				AssertTokenDoesNotExist(token);
			}
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes => Array.Empty<TaskNudgeInformationForTest>();

		ITokenizedAccessControl accessControl;

		protected override void SetUpCore()
		{
			accessControl = new TokenizedAccessControl();
		}

		static void MakePermanent(string token)
			=> Db.Connection.ExecuteNonQuery(
				FormattableString.Invariant(
					$"UPDATE {StmAccessTokenSchema.Constants.SqlSchemaName}.{StmAccessTokenSchema.Constants.TableName} SET {StmAccessTokenSchema.Constants.SAT_IsPermanentToken} = 1 WHERE {StmAccessTokenSchema.Constants.SAT_Token} = @token"),
				cmd => cmd.AddParameterBasedOnDbColumn("@token", token, StmAccessTokenSchema.SAT_Token));

		static void MakeOld(string token, TimeSpan age)
			=> Db.Connection.ExecuteNonQuery(
				FormattableString.Invariant(
					$"UPDATE {StmAccessTokenSchema.Constants.SqlSchemaName}.{StmAccessTokenSchema.Constants.TableName} SET {StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc} = DATEADD(MINUTE, @minutes, {StmAccessTokenSchema.Constants.SAT_SystemCreateTimeUtc}) WHERE {StmAccessTokenSchema.Constants.SAT_Token} = @token"),
				cmd =>
				{
					cmd.AddParameter("@minutes", SqlDbType.Int, age.TotalMinutes);
					cmd.AddParameterBasedOnDbColumn("@token", token, StmAccessTokenSchema.SAT_Token);
				});

		static string GetTokenType(string token)
			=> (string)Db.Connection.ExecuteScalar(
				FormattableString.Invariant(
					$"SELECT {StmAccessTokenSchema.Constants.SAT_Type} FROM {StmAccessTokenSchema.Constants.SqlSchemaName}.{StmAccessTokenSchema.Constants.TableName} WHERE {StmAccessTokenSchema.Constants.SAT_Token} = @token"),
				cmd => cmd.AddParameterBasedOnDbColumn("@token", token, StmAccessTokenSchema.SAT_Token));

		void AssertTokenExistsAndIsConsumable(string token)
		{
			var tokenType = GetTokenType(token);
			AssertNotNull(tokenType);
			Assert($"Should be able to peek at token {token} - is consumable", accessControl.TryPeek(token, tokenType, out _));
		}

		void AssertTokenExistsAndIsNotConsumable(string token)
		{
			var tokenType = GetTokenType(token);
			AssertNotNull(tokenType);
			Assert($"Should not be able to peek at token {token} - is not consumable", !accessControl.TryPeek(token, tokenType, out _));
		}

		static void AssertTokenDoesNotExist(string token)
		{
			var tokenType = GetTokenType(token);
			AssertNull("Token record should not exist.", tokenType);
		}
	}
}
