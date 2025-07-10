using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeader))]
	class CusGuaranteeHeaderTest : EnterpriseBusinessObjectTestCase
	{
		public void TestICusGuaranteeHeaderIsCorrectlySetup()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			Factory.Save();

			CombineAssertions(() =>
			{
				var loadedGuaranteeHeader = (BusinessObject)NewFactory().Load<Integration.Customs.AsycudaCustoms.ICusGuaranteeHeader>(guaranteeHeader.PK);
				AssertType<CusGuaranteeHeader>("Load by ICusGuaranteeHeader", loadedGuaranteeHeader);

				loadedGuaranteeHeader = NewFactory().Load<Customs.Business.BaseCusGuaranteeHeader>(guaranteeHeader.PK);
				AssertType<CusGuaranteeHeader>("Load by BaseCusGuaranteeHeader", loadedGuaranteeHeader);

				loadedGuaranteeHeader = NewFactory().Load<Customs.Business.CommonCusPermitHeader>(guaranteeHeader.PK);
				AssertType<CusGuaranteeHeader>("Load by CommonCusPermitHeader", loadedGuaranteeHeader);
			});
		}
	}
}
