using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Statistics;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using WTG.Statistics;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SmartParameterisationCommentGeneratorTest : TestCase
	{
		public void TestNoParameters()
		{
			var commentGenerator = new SmartParameterisationCommentGenerator();
			AssertEquals(string.Empty, commentGenerator.Generate(Array.Empty<ZSqlParameter>()));
		}

		public void TestTableValuedParametersAreBucketized()
		{
			var commentGenerator = new SmartParameterisationCommentGenerator();
			var parameters = new[]
			{
				ZSqlParameter.New("@p1", new [] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J" }, OrgHeaderSchema.OH_Code, true)
			};
			var result = commentGenerator.Generate(parameters);
			AssertEquals(@"

/* Parameter Stats
OH_Code = Bucket:2
End Parameter Stats */
", result);
		}

		public void TestCommentContainsAllNonNumericElements()
		{
			var commentGenerator = new SmartParameterisationCommentGenerator();
			var parameters = new[]
			{
				ZSqlParameter.New("@p1", ZDateTime.UtcNow.AddYears(10), OrgHeaderSchema.OH_SystemCreateTimeUtc),
				ZSqlParameter.New("@p1", ZDateTime.UtcNow.AddMonths(10), OrgHeaderSchema.OH_SystemCreateTimeUtc),
				ZSqlParameter.New("@p1", ZDateTime.UtcNow.AddDays(10), OrgHeaderSchema.OH_SystemCreateTimeUtc)
			};

			var result = commentGenerator.Generate(parameters);
			AssertEquals(@"

/* Parameter Stats
OH_SystemCreateTimeUtc = FutureQuarters,FutureWeeks,FutureYears
End Parameter Stats */
", result);
		}

		public void TestCommentRemovesDuplicates()
		{
			var histogram = new[]
						{
							new SqlHistogram("dbo", "OrgHeader", "OH_Code",
								new []
								{
									new SqlHistogramStep("ABIGAS", 1, 1),
									new SqlHistogramStep("OHLOLD", 1, 1),
								}
							)
						};

			var mock = new Mock<IStatisticsPersister>();
			mock
				.Setup(persister => persister.TryRetrieve("dbo", "OrgHeader", out histogram))
				.Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			using (ParameterSuffixer.Instance.TemporaryUseNewCache_ForTest())
			{
				var commentGenerator = new SmartParameterisationCommentGenerator();
				var parameters = new[]
				{
						ZSqlParameter.New("@p1", "ABIGAS", OrgHeaderSchema.OH_Code),
						ZSqlParameter.New("@p1", "OHLOLD", OrgHeaderSchema.OH_Code),
					};

				var result = commentGenerator.Generate(parameters);
				AssertEquals(@"

/* Parameter Stats
OH_Code = 0
End Parameter Stats */
", result);
			}
		}

		public void TestCommentRemovesLowerExponents()
		{
			var histogram = new[]
			{
				new SqlHistogram("dbo", "OrgHeader", "OH_Code",
					new []
					{
						new SqlHistogramStep("ABIGAS", 1, 1),
						new SqlHistogramStep("OHLOLD", 1000, 1),
					}
				)
			};

			var mock = new Mock<IStatisticsPersister>();
			mock
				.Setup(persister => persister.TryRetrieve("dbo", "OrgHeader", out histogram))
				.Returns(true);

			using (ObjectFactory.Substitute(mock.Object))
			using (ParameterSuffixer.Instance.TemporaryUseNewCache_ForTest())
			{
				var commentGenerator = new SmartParameterisationCommentGenerator();
				var parameters = new[]
				{
						ZSqlParameter.New("@p1", "ABIGAS", OrgHeaderSchema.OH_Code),
						ZSqlParameter.New("@p1", "OHLOLD", OrgHeaderSchema.OH_Code),
					};

				var result = commentGenerator.Generate(parameters);
				AssertEquals(@"

/* Parameter Stats
OH_Code = 3
End Parameter Stats */
", result);
			}
		}
	}
}
