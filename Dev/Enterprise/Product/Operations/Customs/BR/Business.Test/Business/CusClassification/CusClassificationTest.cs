using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(CusClassification))]
	public class CusClassificationTest : Customs.Business.Testing.BaseCusClassificationTest
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

		public void TestFormatTariffForSaving()
		{
			var classification = Factory.New<CusClassification>();
			classification.CC_TariffNum = "1234.56.78";
			AssertEquals("Keep Numeric Only", "12345678", classification.CC_TariffNum);
		}

		#region Implementation

		protected new CusClassification Classification
		{
			get
			{
				return (CusClassification)base.Classification;
			}
		}

		#endregion
	}
}
