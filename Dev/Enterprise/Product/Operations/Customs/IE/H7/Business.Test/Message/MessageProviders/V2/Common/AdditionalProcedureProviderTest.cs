using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	sealed class AdditionalProcedureProviderTest : DataProviderTestCase<AdditionalProcedureProvider>
	{
		public void TestSequenceNumber()
		{
			AssertEquals("Sequence Number", "1", additionalProcedureProvider.SequenceNumber);
		}

		public void TestCcQualifier()
		{
			AssertNull("CcQualifier", additionalProcedureProvider.CcQualifier);
		}

		public void TestAdditionalProcedure()
		{
			AssertEquals("Additional Procedure", "C07", additionalProcedureProvider.AdditionalProcedure);
		}

		protected override void SetUp()
		{
			base.SetUp();

			sequenceNumber = 1;
			additionalProcedureCode = "C07";
			additionalProcedureProvider = new AdditionalProcedureProvider(sequenceNumber, additionalProcedureCode);
		}

		AdditionalProcedureProvider additionalProcedureProvider;
		int sequenceNumber;
		string additionalProcedureCode;

		protected sealed override AdditionalProcedureProvider GetProvider()
		{
			return new AdditionalProcedureProvider(sequenceNumber, additionalProcedureCode);
		}
	}
}
