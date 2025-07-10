using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	sealed class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestLookups()
		{
			var additionalInfo = GetNewBusinessObject(Factory);
			AssertType<AdditionalInfoLookups>(additionalInfo.Lookups);
		}

		public void TestCSI_StatusDefaultValue()
		{
			var additionalInfo = GetNewBusinessObject(Factory);
			AssertEquals("Status is empty by default", ZString.Empty, additionalInfo.CSI_Status);
		}

		public void TestCSI_ItemNumber_Tags()
		{
			CombineAssertions(() =>
			{
				var additionalInfo = GetNewBusinessObject(Factory);
				AssertEquals("Sequence Number", DataBoundResourceStrings.GetDataForProperty(additionalInfo.CSI_ItemNumberInfo).Caption);
				AssertEquals("Seq Number", DataBoundResourceStrings.GetDataForProperty(additionalInfo.CSI_ItemNumberInfo).MediumCaption);
				AssertEquals("Seq Num.", DataBoundResourceStrings.GetDataForProperty(additionalInfo.CSI_ItemNumberInfo).ShortCaption);
			});
		}

		public void TestStatusIsMissing()
		{
			CombineAssertions(() =>
			{
				var additionalInfo = GetNewBusinessObject(Factory);
				AssertEquals("When CSI_Status is empty", false, additionalInfo.StatusIsMissing);

				additionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.Missing;
				AssertEquals("When CSI_Status is MIS", true, additionalInfo.StatusIsMissing);

				additionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				AssertEquals("When CSI_Status is DIF", false, additionalInfo.StatusIsMissing);
			});
		}

		public void TestStatusIsDifferencesToDeclared()
		{
			CombineAssertions(() =>
			{
				var additionalInfo = GetNewBusinessObject(Factory);
				AssertEquals("When CSI_Status is empty", false, additionalInfo.StatusIsDifferencesToDeclared);

				additionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				AssertEquals("When CSI_Status is DIF", true, additionalInfo.StatusIsDifferencesToDeclared);

				additionalInfo.CSI_Status = DiscrepanciesStatusCodeList.Codes.Missing;
				AssertEquals("When CSI_Status is MIS", false, additionalInfo.StatusIsDifferencesToDeclared);
			});
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
			return (AdditionalInfo)report.AdditionalInfos.AddNew();
		}
	}
}

