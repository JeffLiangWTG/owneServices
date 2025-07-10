using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.Business.AccountingIServices.Testing
{
	class FactoryCountForNumberFountainUsageTest : TestCaseWithFactory
	{
		public void TestSetGetDataRowRelatedValue()
		{
			var bizo1Instance1 = Factory.New<DummyBusinessObject>();
			var bizo1Instance2 = Factory.Load<DummyBusinessObjectForAnotherInstance>(bizo1Instance1.PK);
			var bizo2 = Factory.New<DummyBusinessObject>();
			var bizo3 = Factory.New<DummyBusinessObjectForAnotherInstance>();

			string flag1Name = "flag1Name";
			int flag1Value = 0;
			string flag2Name = "flag2Name";
			int flag2Value = 0;
			string notExistedFlag = "NotExistedFlag";

			AssertEquals("bizo1Instance1, flag1Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance1, flag1Name));
			AssertEquals("bizo1Instance1, flag2Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance1, flag2Name));
			AssertEquals("bizo1Instance1, NotExistedFlag", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance1, notExistedFlag));

			AssertEquals("bizo1Instance2, flag1Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance2, flag1Name));
			AssertEquals("bizo1Instance2, flag2Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance2, flag2Name));
			AssertEquals("bizo1Instance2, NotExistedFlag", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance2, notExistedFlag));

			AssertEquals("bizo2, flag1Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo2, flag1Name));
			AssertEquals("bizo2, flag2Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo2, flag2Name));
			AssertEquals("bizo2, NotExistedFlag", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo2, notExistedFlag));

			AssertEquals("bizo3, flag1Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo3, flag1Name));
			AssertEquals("bizo3, flag2Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo3, flag2Name));
			AssertEquals("bizo3, NotExistedFlag", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo3, notExistedFlag));

			Action assertValues = () =>
			{
				AccountingIServices.FactoryCountForNumberFountainUsage.SetDataRowRelatedValue(bizo1Instance1, flag1Name, flag1Value);
				AccountingIServices.FactoryCountForNumberFountainUsage.SetDataRowRelatedValue(bizo1Instance2, flag2Name, flag2Value);

				AssertEquals("bizoInstance1, flag1Name", flag1Value, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance1, flag1Name));
				AssertEquals("bizoInstance1, flag2Name", flag2Value, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance1, flag2Name));
				AssertEquals("bizoInstance1, NotExistedFlag", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance1, notExistedFlag));

				AssertEquals("bizoInstance2, flag1Name", flag1Value, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance2, flag1Name));
				AssertEquals("bizoInstance2, flag2Name", flag2Value, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance2, flag2Name));
				AssertEquals("bizoInstance2, NotExistedFlag", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo1Instance2, notExistedFlag));

				AssertEquals("bizo2, flag1Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo2, flag1Name));
				AssertEquals("bizo2, flag2Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo2, flag2Name));
				AssertEquals("bizo2, NotExistedFlag", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo2, notExistedFlag));

				AssertEquals("bizo3, flag1Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo3, flag1Name));
				AssertEquals("bizo3, flag2Name", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo3, flag2Name));
				AssertEquals("bizo3, NotExistedFlag", 0, AccountingIServices.FactoryCountForNumberFountainUsage.GetDataRowRelatedValue(bizo3, notExistedFlag));
			};

			flag1Value = 1;
			flag2Value = 1;
			assertValues();

			flag1Value = 1;
			flag2Value = 0;
			assertValues();

			flag1Value = 0;
			flag2Value = 1;
			assertValues();

			flag1Value = 0;
			flag2Value = 0;
			assertValues();
		}

		class DummyBusinessObjectForAnotherInstance : DummyBusinessObject
		{
			public DummyBusinessObjectForAnotherInstance(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}
	}
}
