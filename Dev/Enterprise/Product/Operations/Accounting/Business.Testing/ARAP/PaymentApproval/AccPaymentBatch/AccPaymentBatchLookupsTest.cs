using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval.Testing
{
	public class AccPaymentBatchLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPaymentTypeList()
		{
			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, true))
			{
				var accPaymentBatch = Factory.New<AccPaymentBatch>();
				var expectedValues = new string[] {
									"CHQ", "CSH", "CCD",
									"DDR", "EFT", "SFT",
									"CRQ", "END", "ECC",
									"AMF", "BDT", "BDP",
									"INT", "INR", "PPY",
									"STD", "MSR", "MSF",
									"EPA" };

				AssertContainsExactElementsInAnyOrder(expectedValues, accPaymentBatch.Lookups.PaymentTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray());
			}

			using (TestObjectCreator.SetupEnableEPaymentFunctionalityRegistry(Env.CurrentCompanyPK, false))
			{
				var accPaymentBatch = Factory.New<AccPaymentBatch>();
				var expectedValues = new string[] {
									"CHQ", "CSH", "CCD",
									"DDR", "EFT", "SFT",
									"CRQ", "END", "ECC",
									"AMF", "BDT", "BDP",
									"INT", "INR", "PPY",
									"STD", "MSR", "MSF" };

				AssertContainsExactElementsInAnyOrder(expectedValues, accPaymentBatch.Lookups.PaymentTypeList.Cast<CodeDescriptionPair>().Select(x => x.Code).ToArray());
			}
		}
	}
}


