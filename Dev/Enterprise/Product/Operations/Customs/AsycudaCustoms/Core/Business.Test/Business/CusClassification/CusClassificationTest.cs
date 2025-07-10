using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusClassification))]
	class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
	{
		public void TestDefaultValues()
		{
			AssertEquals("Country is set", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Classification.CC_RN_NKCountryCode);
			AssertEquals("Type is set", CusClassification.ClassificationType.Both, Classification.CC_ClassificationType);
		}

		public void TestTypeDecider()
		{
			Assert("Update Customs.Business.BaseCusClassification to include a decider for this class", Factory.New(typeof(Customs.Business.BaseCusClassification)).GetType() == GetExpectedBusinessObjectType());
		}

		protected new CusClassification Classification => (CusClassification)base.Classification;
	}
}
