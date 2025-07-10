using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentItem))]
	sealed class CusExitConsignmentItemTest : EnterpriseBusinessObjectTestCase
	{
		public static (CusExitHeader header, CusExitConsignment consignment, CusExitConsignmentItem item) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			(var consignment, var header) = CusExitConsignmentTest.GetNewBusinessObject(factory);
			var item = consignment.CusExitConsignmentItems.AddNew();
			item.CCI_LineNumber = 1;
			return (header, consignment, item);
		}

		public void TestLookups()
		{
			var (_, _, item) = GetNewBusinessObject(Factory);
			AssertType<CusExitConsignmentItemUcc6Lookups>(item.Lookups);
		}

		public void TestValidation()
		{
			var (_, _, item) = GetNewBusinessObject(Factory);
			AssertType<CusExitConsignmentItemValidation>(item.Validation);
		}

		public void TestCusExitReportItems()
		{
			var (_, _, item) = GetNewBusinessObject(Factory);
			AssertType<CusExitReportItemCollection<CusExitReportItem>>(item.CusExitReportItems);
		}

		public void TestCusExitConsignmentPivots()
		{
			var (_, _, item) = GetNewBusinessObject(Factory);
			AssertType<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(item.CusExitConsignmentPivots);
		}

		public void TestCusExitConsignmentPackagePivots()
		{
			CombineAssertions(() =>
			{
				var (_, _, item) = GetNewBusinessObject(Factory);
				var cusExitConsignmentPackagePivots = item.CusExitConsignmentPackagePivots;
				AssertType<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(cusExitConsignmentPackagePivots);
				AssertEquals("SupportsSorting", true, cusExitConsignmentPackagePivots.SupportsSorting);
				AssertEquals("SortDirection", System.ComponentModel.ListSortDirection.Ascending, cusExitConsignmentPackagePivots.SortDirection);
				AssertEquals("SortInformation.PropertyName", nameof(CusExitConsignmentPivot.SequenceNumber), cusExitConsignmentPackagePivots.SortInformation.PropertyName);
			});
		}

		public void TestCusExitConsignmentContainerPivots()
		{
			var (_, _, item) = GetNewBusinessObject(Factory);
			AssertType<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(item.CusExitConsignmentContainerPivots);
		}

		public void TestAdditionalInfos() => CombineAssertions(() =>
		{
			var (_, _, item) = GetNewBusinessObject(Factory);
			AssertType<AdditionalInfoCollection>("AdditionalInfoCollection", item.AdditionalInfos);
			AssertType<AdditionalInfo>("AdditionalInfoCollection should use ES AdditionalInfo", item.AdditionalInfos.AddNew());
		});

		public void TestGetCusSupportingInfoTypes_AdditionalInfo()
		{
			var (_, _, item) = GetNewBusinessObject(Factory);
			var cusSupportingInfoTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)item).GetCusSupportingInfoTypes();
			AssertEquals(typeof(AdditionalInfo), cusSupportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo]);
		}

		public void TestClearStatusAndSetSealsReadOnlyIfStatusIsMissing()
		{
			CombineAssertions(() =>
			{
				var (_, _, item) = GetNewBusinessObject(Factory);
				var addDoc = item.AdditionalInfos.AddNew();
				addDoc.CSI_Status = "MIS";
				var package = item.CusExitConsignmentPackagePivots.AddNew().Package;
				package.CXP_MarksAndNumbers = "DIF";

				AssertEquals("addDoc is not ReadOnly", false, addDoc.ReadOnly);
				AssertEquals("package is not ReadOnly", false, package.ReadOnly);

				item.CCI_DiscrepancyStatus = "MIS";
				AssertEquals("addDoc is ReadOnly", true, addDoc.ReadOnly);
				AssertEquals("package is ReadOnly", true, package.ReadOnly);
				AssertEquals("addDoc Status is empty", ZString.Empty, addDoc.CSI_Status);
				AssertEquals("package Status is empty", ZString.Empty, package.CXP_MarksAndNumbersStatus);

				item.CCI_DiscrepancyStatus = "DIF";
				AssertEquals("addDoc is not ReadOnly", false, addDoc.ReadOnly);
				AssertEquals("package is not ReadOnly", false, package.ReadOnly);

				item.CCI_DiscrepancyStatus = "MIS";
				Factory.Save();

				var newFactory = Factory.CreateNewFactory();
				var itemReloaded = newFactory.Load<CusExitConsignmentItem>(item.PK);
				var addDocReloaded = itemReloaded.AdditionalInfos[0];
				var packageReloaded = itemReloaded.CusExitConsignmentPackagePivots[0].Package;
				AssertEquals("addDocReloaded is ReadOnly", true, addDocReloaded.ReadOnly);
				AssertEquals("packageReloaded is ReadOnly", true, packageReloaded.ReadOnly);
			});
		}

		public void TestStatusIsMissing()
		{
			CombineAssertions(() =>
			{
				var (_, _, item) = GetNewBusinessObject(Factory);
				AssertEquals("When CCI_DiscrepancyStatus is empty", false, item.StatusIsMissing);

				item.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				AssertEquals("When CCI_DiscrepancyStatus is MIS", true, item.StatusIsMissing);

				item.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				AssertEquals("When CCI_DiscrepancyStatus is DIF", false, item.StatusIsMissing);
			});
		}

		public void TestStatusIsDifferencesToDeclared()
		{
			CombineAssertions(() =>
			{
				var (_, _, item) = GetNewBusinessObject(Factory);
				AssertEquals("When CCI_DiscrepancyStatus is empty", false, item.StatusIsDifferencesToDeclared);

				item.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				AssertEquals("When CCI_DiscrepancyStatus is DIF", true, item.StatusIsDifferencesToDeclared);

				item.CCI_DiscrepancyStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				AssertEquals("When CCI_DiscrepancyStatus is MIS", false, item.StatusIsDifferencesToDeclared);
			});
		}

		public void TestUCRStatusIsDifferencesToDeclared()
		{
			CombineAssertions(() =>
			{
				var (_, _, item) = GetNewBusinessObject(Factory);
				AssertEquals("When CCI_UniqueConsignmentReferenceStatus is empty", false, item.UCRStatusIsDifferencesToDeclared);

				item.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.DifferencesToDeclared;
				AssertEquals("When CCI_UniqueConsignmentReferenceStatus is DIF", true, item.UCRStatusIsDifferencesToDeclared);

				item.CCI_UniqueConsignmentReferenceStatus = EU.ExitControl.Business.DiscrepanciesStatusCodeList.Codes.Missing;
				AssertEquals("When CCI_UniqueConsignmentReferenceStatus is MIS", false, item.UCRStatusIsDifferencesToDeclared);
			});
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory).item;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory).item;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).item;
	}
}
