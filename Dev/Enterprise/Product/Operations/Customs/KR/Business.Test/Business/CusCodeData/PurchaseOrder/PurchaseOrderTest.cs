using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(PurchaseOrder))]
	sealed class PurchaseOrderTest : CusCodeDataTest<PurchaseOrder>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.PurchaseOrder, purchaseOrder.CY_Type);
		}

		public void TestValidation()
		{
			AssertType<PurchaseOrderValidation>(purchaseOrder.Validation);
		}

		public void TestCY_Data()
		{
			AssertEquals(40, purchaseOrder.CY_DataInfo.MaxLength);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return purchaseOrder;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<PurchaseOrder>();

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			purchaseOrder = (PurchaseOrder)GetNewBusinessObject();
			purchaseOrder.Parent = invoice;
			Factory.Save();
		}
		PurchaseOrder purchaseOrder;
	}
}
