using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(ProductConditionCollection))]
	sealed class ProductConditionCollectionTest : CusCodeDataCollectionTest<ProductCondition>
	{
		public void TestMaxCount()
		{
			var collection = GetCusCodeDataCollection();
			for (int i = 0; i < 10; i++)
			{
				collection.AddNew();
			}

			CombineAssertions(() =>
			{
				var errors = collection.GetErrors();
				Assert("8 objects", !errors.Contains("Error - CusCodeData: You are only allowed a maximum of 10 CusCodeDatas here."));
				collection.AddNew();
				errors = collection.GetErrors();
				Assert("To many objects", errors.Contains("Error - CusCodeData: You are only allowed a maximum of 10 CusCodeDatas here."));
			});
		}

		public void TestSetDefaultsForNewChild_Order()
		{
			var collection = (ProductConditionCollection)Collection;
			var obj1 = collection.AddNew();
			var obj2 = collection.AddNew();
			var obj3 = collection.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("Obj1 Order", new ZShort(1), obj1.CY_Order);
				AssertEquals("Obj2 Order", new ZShort(2), obj2.CY_Order);
				AssertEquals("Obj3 Order", new ZShort(3), obj3.CY_Order);
			});
		}

		protected override CusCodeDataCollection<ProductCondition> GetCusCodeDataCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			var invoiceHeader = declaration.Invoices.AddNew();
			_ = invoiceHeader.QuarantineExDocHeader;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var eXDOCLine = invoiceLine.QuarantineExDocLine;
			return new ProductConditionCollection(eXDOCLine);
		}
	}
}
