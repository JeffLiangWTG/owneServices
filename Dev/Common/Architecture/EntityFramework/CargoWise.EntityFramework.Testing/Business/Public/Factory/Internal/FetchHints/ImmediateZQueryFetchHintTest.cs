using System.Linq;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ImmediateZQueryFetchHintTest : TestCase
	{
		public void TestIsNeeded()
		{
			RowFactory factory = new RowFactory();
			QueryHistoryProvider historyProvider = new QueryHistoryProvider(factory);

			IFetchHint hint = new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.AddFetchHint(hint);
			AssertEquals(true, hint.IsNeeded(historyProvider));

			factory.ExecuteAllFetchHints();
			AssertEquals(false, hint.IsNeeded(historyProvider));
		}

		public void TestConstructor()
		{
			IFetchHint fetchHint = new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals("DummyBizo", fetchHint.TableName);
		}

		public void TestGetHashStringIncludingLoadWithBlobs()
		{
			IFetchHint fetchHint = new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals("Z0_Code = '123'", fetchHint.GetHashKeyObject().KeyParts.Single());
		}

		public void TestGetQuery()
		{
			IFetchHint fetchHint = new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals("Z0_Code = '123'", fetchHint.GetQuery().LiteralTextADO);
		}

		public void TestIsDataHintLoaded()
		{
			IFetchHint fetchHint = new ImmediateZQueryFetchHint(typeof(DummyBusinessObject), new ZQuery(DummyBizoSchema.Z0_Code, "123"));
			AssertEquals(false, fetchHint.IsDataHintLoaded);
			fetchHint.IsDataHintLoaded = true;
			AssertEquals(true, fetchHint.IsDataHintLoaded);
		}
	}
}
