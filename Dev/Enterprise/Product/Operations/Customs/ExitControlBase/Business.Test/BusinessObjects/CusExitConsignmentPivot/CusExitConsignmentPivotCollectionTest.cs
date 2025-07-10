using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ExitControlBase.Business.Testing;

[TestedType(typeof(CusExitConsignmentPivotCollection<CusExitConsignmentPivot>))]
sealed class CusExitConsignmentPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<CusExitConsignmentPivotCollection<CusExitConsignmentPivot>>
{
	public void TestCusExitConsignmentItemAsMaster()
	{
		var consignmentItem = Factory.New<CusExitConsignmentItem>();
		var collection = new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(consignmentItem);
		AssertEquals(CusExitConsignmentPivotSchema.CNP_CCI_ConsignmentItem, ((DependentRelationship)collection.Relationship).FKSchemaColumnInDependent);
	}

	public void TestMaxCountValidation_ConsignmentItemPackagePivot()
	{
		var consignmentItem = Factory.New<CusExitConsignmentItem>();
		var collection = new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(consignmentItem, ConsignmentItemPivotType.Package) as ISupportMaxCountValidation;
		CombineAssertions(() =>
		{
			var validator = collection.MaxCountValidator;
			var notification = validator.Notification;
			AssertEquals("MaxCount", 9, validator.MaxCount);
			AssertEquals("WarnAtHalfway", false, validator.WarnAtHalfway);
			AssertEquals("Notification Type", CargoWise.ComponentModel.NotificationType.Error, notification.Type);
			AssertEquals("Notification Message", "Maximum number of Packing Details is 9.", notification.Message);
		});
	}

	public void TestSetDefaultsForNewElement_ConsignmentItemPackagePivot()
	{
		var header = Factory.New<CusExitHeader>();
		var consignment = header.CusExitConsignments.AddNew();
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		var collection = new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(consignmentItem, ConsignmentItemPivotType.Package);
		var packagePivot = collection.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("CNP_CXP_Package", header.CusExitConsignmentPackages.Single().PK, packagePivot.CNP_CXP_Package);
			AssertEquals("CNP_CXN_Container", ZGuid.Empty, packagePivot.CNP_CXN_Container);
		});
	}

	public void TestMaxCountValidation_ConsignmentItemContainerPivot()
	{
		var consignmentItem = Factory.New<CusExitConsignmentItem>();
		var collection = new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(consignmentItem, ConsignmentItemPivotType.Container) as ISupportMaxCountValidation;
		AssertEquals("MaxCount", -1, collection.MaxCountValidator.MaxCount);
	}

	public void TestSetDefaultsForNewElement_ConsignmentItemContainerPivot()
	{
		var header = Factory.New<CusExitHeader>();
		var consignment = header.CusExitConsignments.AddNew();
		var consignmentItem = consignment.CusExitConsignmentItems.AddNew();
		consignmentItem.CCI_LineNumber = 1;
		var collection = new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(consignmentItem, ConsignmentItemPivotType.Container);
		var containerPivot = collection.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("CNP_CXN_Container", header.CusExitContainers.Single().PK, containerPivot.CNP_CXN_Container);
			AssertEquals("CNP_CXP_Package", ZGuid.Empty, containerPivot.CNP_CXP_Package);
		});
	}

	public void TestCusExitContainerAsMaster()
	{
		var container = Factory.New<CusExitContainer>();
		var collection = new CusExitConsignmentPivotCollection<CusExitConsignmentPivot>(container);
		AssertEquals(CusExitConsignmentPivotSchema.CNP_CXN_Container, ((DependentRelationship)collection.Relationship).FKSchemaColumnInDependent);
	}

	protected override CusExitConsignmentPivotCollection<CusExitConsignmentPivot> GetCollectionToTest()
		=> (CusExitConsignmentPivotCollection<CusExitConsignmentPivot>)cusExitConsignmentItem.CusExitConsignmentPackagePivots;

	protected override BusinessObject GetNewElementToAddToTheCollection()
	{
		var newPackage = cusExitConsignmentItem.Consignment.Header.CusExitConsignmentPackages.AddNew();
		var newItem = cusExitConsignmentItem.CusExitConsignmentPackagePivots.AddNew();
		newItem.CNP_CXP_Package = newPackage.PK;
		return newItem;
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusExitConsignmentItem = CusExitConsignmentItemTest.GetNewBusinessObject(Factory).consignmentItem;
	}
	CusExitConsignmentItem cusExitConsignmentItem;
}
