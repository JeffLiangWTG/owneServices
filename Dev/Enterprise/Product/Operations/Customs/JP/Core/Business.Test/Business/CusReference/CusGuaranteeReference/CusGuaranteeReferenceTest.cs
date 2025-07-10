using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(CusGuaranteeReference))]
	sealed public class CusGuaranteeReferenceTest : CusReferenceAbstractTest<CusGuaranteeReference>
	{
		public void TestCFR_ReferenceMaxLength()
		{
			AssertEquals(9, cusGuranteeReference.CFR_ReferenceInfo.MaxLength);
		}

		public void TestSetDefaultValues()
		{
			AssertEquals(GuaranteeCusCodeDataTypeList.Codes.GRN, cusGuranteeReference.CFR_Type);
			AssertEquals(CusEntryInstructionSchema.Constants.Prefix, cusGuranteeReference.CFR_ParentTableCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			cusGuranteeReference = Factory.New<CusGuaranteeReference>();
		}

		CusGuaranteeReference cusGuranteeReference;

		protected override IEnumerable<CusGuaranteeReference> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var bizObj = factory.New<JobDeclaration>().CustomsEntryInstructions.AddNew().Guarantees.AddNew();
			bizObj.CFR_Reference = "111";
			yield return bizObj;
		}
	}
}
