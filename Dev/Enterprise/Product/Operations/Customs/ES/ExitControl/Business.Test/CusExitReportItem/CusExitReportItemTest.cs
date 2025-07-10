using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitReportItem))]
	sealed class CusExitReportItemTest : EnterpriseBusinessObjectTestCase
	{
		public static (CusExitReportItem reportItem, CusExitReport report, CusExitConsignmentItem consignmentItem, CusExitConsignment consignment, CusExitHeader header) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var report, var consignment, var header) = CusExitReportTest.GetNewBusinessObject(factory);
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			var reportItem = report.CusExitReportItems.AddNew();
			reportItem.ERI_CCI_ConsignmentItem = consignmentItem.PK;
			return (reportItem, report, consignmentItem, consignment, header);
		}

		public void TestAdditionalInfos()
		{
			AssertType<AdditionalInfoCollection>(GetNewBusinessObject(Factory).reportItem.AdditionalInfos);
		}

		public void TestCusSupportingInfoTypes()
		{
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)reportItem).GetCusSupportingInfoTypes();
			AssertEquals(typeof(AdditionalInfo), cusSupportingInfoTypes[CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestAdditionalSealNumbersSequenceNumberDictionary()
		{
			reportItem.AdditionalInfos.AddNew();
			reportItem.AdditionalInfos.AddNew();
			var additionalInfo1 = reportItem.AdditionalInfos.AddNew();
			additionalInfo1.CSI_ItemNumber = 1;

			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			reportItem = newFactory.Load<CusExitReportItem>(reportItem.PK);

			var dictionary = new System.Collections.Generic.Dictionary<ZInt, ZInt>();
			dictionary.Add(1, 2);
			dictionary.Add(2, 1);
			AssertEquals("Dictionary", true, reportItem.AdditionalInfosItemNumberDictionary.ContainsSameElementsInAnyOrder(dictionary));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).reportItem;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => reportItem;

		protected override BusinessObject GetNewBusinessObject() => reportItem;

		protected override void SetUp()
		{
			base.SetUp();
			reportItem = GetNewBusinessObject(Factory).reportItem;
		}
		CusExitReportItem reportItem;
	}
}
