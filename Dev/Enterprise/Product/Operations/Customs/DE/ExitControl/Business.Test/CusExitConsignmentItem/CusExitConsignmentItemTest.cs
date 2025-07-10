using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusExitConsignmentItem))]
	sealed class CusExitConsignmentItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCCI_ReferenceNumber_Caption()
		{
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(consignmentItem.CCI_ReferenceNumberInfo);
			AssertEquals("Caption", "Registration Number (ext.)", resourceStringData.Caption);
			AssertEquals("MediumCaption", "Rego. No. (ext.)", resourceStringData.MediumCaption);
			AssertEquals("ShortCaption", "Rego. No.", resourceStringData.ShortCaption);
		}

		public void TestCusExitConsignmentPivots()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(consignmentItem.CusExitConsignmentPivots);
		}

		public void TestCusExitConsignmentPackagePivots()
		{
			AssertType<ExitControlBase.Business.CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>(consignmentItem.CusExitConsignmentPackagePivots);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(Factory).Item;

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => consignmentItem;

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory).Item;

		protected override void SetUp()
		{
			base.SetUp();
			(consignmentItem, _) = GetNewBusinessObject(Factory);
		}
		CusExitConsignmentItem consignmentItem;

		(CusExitConsignmentItem Item, CusExitConsignment Consignment) GetNewBusinessObject(BusinessObjectFactory factory)
		{
			var header = factory.New<CusExitHeader>();
			header.CXH_JobReference = header.PK.ToString().Substring(0, 35);
			var consignment = header.CusExitConsignments.AddNew();
			var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
			consignmentItem.CCI_LineNumber = 1;
			return (consignmentItem, consignment);
		}
	}
}
