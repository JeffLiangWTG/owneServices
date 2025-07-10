using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.CusTempStorage;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.CusTempStorage.Testing
{
	sealed class Declaration14ProviderTest : DataProviderTestCase<Declaration14Provider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("TemporaryStorageHeader missing", () => GenerateProvider(null));
				AssertNoExceptionThrown(() => GenerateProvider(header));
			});
		}

		public void TestMRN()
		{
			header.MRN = "123";
			AssertEquals("MRN", "123", Provider.MRN);
		}

		public void TestInvalidationReason()
		{
			AssertNull(Provider.InvalidationReason);
		}

		[TestDate(2022, 5, 15, 16, 45, 35)]
		public void TestInvalidationRequestDateAndTime()
		{
			AssertEquals("InvalidationRequestDateAndTime", new DateTime(2022, 5, 15, 16, 45, 35), Provider.InvalidationRequestDateAndTime);
		}

		public void TestFallbackProcedure()
		{
			AssertNull(Provider.FallbackProcedure);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<TemporaryStorageHeader>();
		}
		TemporaryStorageHeader header;

		Declaration14Provider GenerateProvider(TemporaryStorageHeader header) => new Declaration14Provider(header);

		protected sealed override Declaration14Provider GetProvider()
		{
			return GenerateProvider(header);
		}
	}
}
