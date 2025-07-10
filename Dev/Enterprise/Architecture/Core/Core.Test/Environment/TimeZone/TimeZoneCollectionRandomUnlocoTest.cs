using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class TimeZoneCollectionRandomUnlocoTest : TestCase
	{
		public void TestRandomUnlocoTimeToAndFroUtc()
		{
			TimeZoneCollectionForTesting testCollection = new TimeZoneCollectionForTesting();
			DateTime utcDateTime = EnvProxy.Instance.Time.CurrentUtcDateTime;

			string defaultPort = EnvProxy.Instance.CurrentBranch.NKUNLOCO;
			DateTime localDateTime_DefaultPort = testCollection.ToLocationTimeFromUtc(defaultPort, utcDateTime);
			AssertEquals("UTC (from Default UNLOCO local time)", utcDateTime, testCollection.ToUtcFromLocationTime(defaultPort, localDateTime_DefaultPort));
			TimeSpan utcOffset_DefaultPort = testCollection.GetUtcOffsetBasedOnUtc(defaultPort, utcDateTime);
			AssertEquals("UTC offset (Default UNLOCO)", localDateTime_DefaultPort.Subtract(utcDateTime), utcOffset_DefaultPort);

			string randomPort = GetRandomPort();
			DateTime localDateTime_RandomPort = testCollection.ToLocationTimeFromUtc(randomPort, utcDateTime);
			AssertEquals("UTC (from Random UNLOCO local time)", utcDateTime, testCollection.ToUtcFromLocationTime(randomPort, localDateTime_RandomPort));
			TimeSpan utcOffset_RandomPort = testCollection.GetUtcOffsetBasedOnUtc(randomPort, utcDateTime);
			AssertEquals("UTC offset (Random UNLOCO)", localDateTime_RandomPort.Subtract(utcDateTime), utcOffset_RandomPort);

			TimeSpan localTimeDiff = localDateTime_RandomPort.Subtract(localDateTime_DefaultPort);
			TimeSpan utcOffsetDiff = utcOffset_RandomPort.Subtract(utcOffset_DefaultPort);
			AssertEquals("RTandomPort-DefaultPort: Local time diff should be the same as UTC Offset diff", localTimeDiff, utcOffsetDiff);
		}

		string GetRandomPort()
		{
			Random rand = new Random();
			char[] randomChar5Array = new char[] { (char)rand.Next(65, 90), (char)rand.Next(65, 90), (char)rand.Next(65, 90), (char)rand.Next(65, 90), (char)rand.Next(65, 90) };
			string randomPortCode = new string(randomChar5Array);

			string sqlText = @"
				DECLARE @Port char(5)
				SELECT @Port = RL_Code FROM dbo.RefUNLOCO WHERE RL_Code = @code0
				IF (@Port is null) SELECT TOP 1 @Port = RL_Code FROM dbo.RefUNLOCO WHERE RL_Code like @code1
				IF (@Port is null) SELECT TOP 1 @Port = RL_Code FROM dbo.RefUNLOCO WHERE RL_Code like @code2
				IF (@Port is null) SELECT TOP 1 @Port = RL_Code FROM dbo.RefUNLOCO WHERE RL_Code like @code3
				IF (@Port is null) SELECT TOP 1 @Port = RL_Code FROM dbo.RefUNLOCO WHERE RL_Code like @code4
				IF (@Port is null) SELECT TOP 1 @Port = RL_Code FROM dbo.RefUNLOCO
				SELECT @Port";

			var cmd = Db.Connection.Command(sqlText);

			for (var i = 0; i < 5; i++)
			{
				cmd.AddParameterBasedOnDbColumn($"@code{i}", randomPortCode.Substring(0, 5 - i) + new string('_', i), RefUNLOCOSchema.RL_Code);
			}

			return cmd.ExecuteScalar().ToString();
		}
	}
}
