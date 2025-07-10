using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestLookups()
		{
			var additionalInfo = GetNewBusinessObject(Factory);
			AssertType<AdditionalInfoLookups>(additionalInfo.Lookups);
		}

		public void TestValidation()
		{
			var additionalInfo = GetNewBusinessObject(Factory);
			AssertType<AdditionalInfoValidation>(additionalInfo.Validation);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var alternativeEvidence = GetNewBusinessObject(Factory);
			return alternativeEvidence;
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

		public static AdditionalInfo GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<CusExitHeader>();
			var consignment = header.CusExitConsignments.AddNew();
			var report = header.CusExitReports.AddNew();
			report.CER_CXC_Consignment = consignment.PK;
			report.CER_DateTime = ZDateTimeOffset.Now;
			report.CER_OfficeOfExit = "IE001";
			return (AdditionalInfo)report.AdditionalInfos.AddNew();
		}
	}
}

