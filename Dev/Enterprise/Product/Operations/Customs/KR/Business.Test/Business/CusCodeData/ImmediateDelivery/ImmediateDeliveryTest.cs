using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.KR.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(ImmediateDelivery))]
	sealed class ImmediateDeliveryTest : CusCodeDataTest<ImmediateDelivery>
	{
		public void TestSetDefaultValues()
		{
			AssertEquals(CusCodeDataTypeList.Codes.ImmediateDelivery, immediateDelivery.CY_Type);
		}

		public void TestValidation()
		{
			AssertType<ImmediateDeliveryValidation>(immediateDelivery.Validation);
		}

		public void TestCY_Data()
		{
			immediateDelivery.CY_Data = "Y";
			AssertEquals("Y", immediateDelivery.CY_Data);
			AssertEquals(18, immediateDelivery.CY_DataInfo.MaxLength);
		}

		protected override IEnumerable<ImmediateDelivery> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			yield return immediateDelivery;
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<ImmediateDelivery>();

		protected override void SetUp()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			immediateDelivery = invoiceLine.ImmediateDeliveries.AddNew();
			Factory.Save();
		}
		ImmediateDelivery immediateDelivery;
	}
}
