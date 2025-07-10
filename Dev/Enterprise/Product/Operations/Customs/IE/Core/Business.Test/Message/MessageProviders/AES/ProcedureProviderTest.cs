using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class ProcedureProviderTest : DataProviderTestCase<ProcedureProvider>
	{
		#region Public test methods

		public void TestRequestedProcedure()
		{
			invoiceLine.JI_Procedure = "1234567";
			AssertEquals("RequestedProcedure", "12", Provider.RequestedProcedure);
		}

		public void TestPreviousProcedure()
		{
			invoiceLine.JI_Procedure = "1234567";
			AssertEquals("PreviousProcedure", "34", Provider.PreviousProcedure);
		}

		public void TestAdditionalProcedures()
		{
			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "20";
			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "10";
			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "30";
			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "10";
			invoiceLine.JI_Procedure = "1000E51";
			var additionalProcedures = GetProvider().AdditionalProcedures.ToArray();
			AssertEquals("additionalProcedures.Length", 4, additionalProcedures.Length);
			AssertEquals("additionalProcedures[0]", "10", additionalProcedures[0]);
			AssertEquals("additionalProcedures[1]", "20", additionalProcedures[1]);
			AssertEquals("additionalProcedures[2]", "30", additionalProcedures[2]);
			AssertEquals("additionalProcedures[3]", "E51", additionalProcedures[3]);

			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "E51";
			additionalProcedures = GetProvider().AdditionalProcedures.ToArray();
			AssertEquals("additionalProcedures.Length", 4, additionalProcedures.Length);
			AssertEquals("additionalProcedures[0]", "10", additionalProcedures[0]);
			AssertEquals("additionalProcedures[1]", "20", additionalProcedures[1]);
			AssertEquals("additionalProcedures[2]", "30", additionalProcedures[2]);
			AssertEquals("additionalProcedures[3]", "E51", additionalProcedures[3]);

			invoiceLine.AdditionalProcedureCodes.AddNew().CY_Code = "1000E52";
			additionalProcedures = GetProvider().AdditionalProcedures.ToArray();
			AssertEquals("additionalProcedures.Length", 5, additionalProcedures.Length);
			AssertEquals("additionalProcedures[0]", "10", additionalProcedures[0]);
			AssertEquals("additionalProcedures[1]", "20", additionalProcedures[1]);
			AssertEquals("additionalProcedures[2]", "30", additionalProcedures[2]);
			AssertEquals("additionalProcedures[3]", "E51", additionalProcedures[3]);
			AssertEquals("additionalProcedures[4]", "E52", additionalProcedures[4]);
		}

		#endregion

		#region Overridings & inherits

		protected override ProcedureProvider GetProvider() => new ProcedureProvider(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory).entryLineWrapper.RandomInvoiceLine;
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
