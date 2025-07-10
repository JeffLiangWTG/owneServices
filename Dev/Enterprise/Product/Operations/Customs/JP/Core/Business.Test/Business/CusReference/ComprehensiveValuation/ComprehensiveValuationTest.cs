using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ComprehensiveValuation))]
	sealed class ComprehensiveValuationTest : CusReferenceAbstractTest<ComprehensiveValuation>
	{
		public void TestGetValidation()
		{
			AssertType<ComprehensiveValuationValidation>(Factory.New<ComprehensiveValuation>().Validation);
		}

		public void TestCFR_Reference()
		{
			var comprehensiveValuation = Factory.New<ComprehensiveValuation>();
			AssertEquals("Max length of CFR_Reference", 9, comprehensiveValuation.CFR_ReferenceInfo.MaxLength);
		}

		public void TestDefaultValue()
		{
			var comprehensiveValuation = Factory.New<ComprehensiveValuation>();
			CombineAssertions(() =>
			{
				AssertEquals("CFR_Type", ComprehensiveValuation.ComprehensiveValuationSchema.Type, comprehensiveValuation.CFR_Type);
				AssertEquals("CFR_Code", ComprehensiveValuation.ComprehensiveValuationSchema.Code, comprehensiveValuation.CFR_Code);
			});
		}

		protected override IEnumerable<ComprehensiveValuation> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var bizObj = factory.New<JobComInvoiceHeader>().ComprehensiveValuations.AddNew();
			bizObj.CFR_Reference = "111";
			yield return bizObj;
		}
	}
}
