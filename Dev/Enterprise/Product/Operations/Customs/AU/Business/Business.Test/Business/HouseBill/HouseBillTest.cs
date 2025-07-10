using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(Bill))]
	sealed class HouseBillTest : Customs.Business.Testing.BaseHouseBillTest<Bill, JobDeclaration>
	{
		public void TestCodeDescriptionProperty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var masterBill = declaration.Bills.AddNew();
			AssertEquals("PreCondition:MasterBill Type", Customs.Business.BillTypeList.Codes.MasterBill, masterBill.CU_BillType);
			masterBill.CU_BillNum = "2";

			masterBill = declaration.Bills.AddNew();
			masterBill.CU_BillType = Customs.Business.BillTypeList.Codes.MasterBill;
			masterBill.CU_BillNum = "1";

			var bill1 = declaration.Bills.AddNew();
			bill1.CU_HouseBill = "1";
			bill1.CU_MasterBill = "2";

			var bill2 = declaration.Bills.AddNew();
			bill2.CU_HouseBill = "2";
			bill2.CU_MasterBill = "1";
			AssertEquals("HB:1 (MB:2)", CodePropertyAttribute.CodeFromBusinessObject(bill1));
			AssertEquals("HB:2 (MB:1)", CodePropertyAttribute.CodeFromBusinessObject(bill2));
			AssertEquals("HB:1 (MB:2)", DescriptionPropertyAttribute.DescriptionFromBusinessObject(bill1));
			AssertEquals("HB:2 (MB:1)", DescriptionPropertyAttribute.DescriptionFromBusinessObject(bill2));
		}

		public void TestCU_fPartShipConsignmentReference()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "H1";

			var primaryHouseBill = declaration.PrimaryHouseBill;
			AssertNotNull(primaryHouseBill);

			primaryHouseBill.CU_fPartShipConsignmentReference = "X1";
			AssertEquals("X1", primaryHouseBill.CU_fPartShipConsignmentReference);
			AssertEquals("X1", declaration.JE_PartShipConsignmentReference);
			Assert(!primaryHouseBill.CU_fPartShipConsignmentReferenceInfo.ReadOnly);

			primaryHouseBill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			Assert(primaryHouseBill.CU_fPartShipConsignmentReferenceInfo.ReadOnly);
			AssertEquals(ZString.Empty, primaryHouseBill.CU_fPartShipConsignmentReference);
		}

		[TestDate(2013, 02, 01, 01, 09, 09)]
		public void TestSetConsignmentReferenceIfRequired()
		{
			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			var mawb = Factory.New<CusMAWB>();
			var hawb = mawb.ChildBills.AddNew();
			hawb.CS_HAWB = "H1";
			hawb.CS_fPartShipConsignmentReference = "XXX2013010112345678";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "H1";

			var primaryHouseBill = declaration.PrimaryHouseBill;
			AssertNotNull(primaryHouseBill);
			AssertEquals("Con Ref not set when registry setting not active", ZString.Empty, primaryHouseBill.CU_fPartShipConsignmentReference);

			AUCustomsDataRegistry.Instance.ActivateAlternatePartShipmentModel.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			declaration.JE_HouseBill = ZString.Empty;
			declaration.JE_HouseBill = "H1";
			AssertEquals("Con Ref now set", "XXX2013010112345678", primaryHouseBill.CU_fPartShipConsignmentReference);
		}

		public void TestValidation()
		{
			var bill = (Bill)GetNewBusinessObject();
			AssertEquals("Normal validation", typeof(BillValidation), bill.Validation.GetType());
			bill.CU_JE = JobDeclaration.New(Factory).PK;
			bill.Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("Normal validation", typeof(BillValidation), bill.Validation.GetType());
			bill.Declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals("ExWarehouse validation", typeof(CusDecHouseBillValidationForExWarehouse), bill.Validation.GetType());
		}
	}
}
