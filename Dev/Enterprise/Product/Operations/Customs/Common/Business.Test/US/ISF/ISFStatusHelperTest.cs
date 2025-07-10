using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.US.ISF.Testing
{
	class ISFStatusHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestGetISFBillDataForTimeFroma6Months()
		{
			var newFactory = new BusinessObjectFactory();
			var headerOut = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			headerOut.BF_JobReference = "JOB3";
			headerOut.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);

			var billOut = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			billOut.BB_BF = headerOut.PK;
			billOut.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			billOut.BB_BillNum = "BIOUT";
			billOut.BB_CustomsStatus = "S9";

			var headerIn = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			headerIn.BF_JobReference = "JOB5";
			headerIn.BF_SystemCreateTimeUtc = ZDate.Today.AddMonths(2);

			var billIn = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			billIn.BB_BF = headerIn.PK;
			billIn.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			billIn.BB_BillNum = "BILIN";
			billIn.BB_CustomsStatus = "S1";

			newFactory.Save();

			var isfData = ISFStatusHelper.GetISFBillData(Factory, false, "BIOUT", ZDateTime.Now);
			NUnit.Framework.Assert.That(isfData, Is.EqualTo(default(ISFBillData)));

			isfData = ISFStatusHelper.GetISFBillData(Factory, true, "BILIN", ZDateTime.Now);
			NUnit.Framework.Assert.That(isfData.BillNumber, Is.EqualTo("BILIN").Using(CustomComparers.TypeComparison), "BillNumber");
			NUnit.Framework.Assert.That(DispositionCodeList.Codes.S1, Is.EqualTo("S1"), "Status");
		}

		[TestDate(2011, 01, 01)]
		[ExpectNoExceptions]
		public void TestGetISFBillData()
		{
			var newFactory = new BusinessObjectFactory();

			//create an isf job with created date outside the range
			var header1 = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header1.BF_JobReference = "JOB1";
			header1.BF_SystemCreateTimeUtc = new ZDateTime(2008, 10, 9);

			var bill1 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill1.BB_BF = header1.PK;
			bill1.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill1.BB_BillNum = "BILL1";
			bill1.BB_CustomsStatus = "S1";

			var bill2 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill2.BB_BF = header1.PK;
			bill2.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill2.BB_BillNum = "BILL2";
			bill2.BB_CustomsStatus = "S2";

			var bill3 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill3.BB_BF = header1.PK;
			bill3.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill3.BB_BillNum = "BILL3";
			bill3.BB_CustomsStatus = "S3";

			//create an isf job with created date within range

			var header2 = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header2.BF_JobReference = "JOB2";
			header2.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);

			var bill4 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill4.BB_BF = header2.PK;
			bill4.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill4.BB_BillNum = "BILL4";
			bill4.BB_CustomsStatus = "S4";

			var bill5 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill5.BB_BF = header2.PK;
			bill5.BB_BillType = BillTypeList.Codes.MasterBillOfLading;
			bill5.BB_BillNum = "BILL5";
			bill5.BB_CustomsStatus = "S5";

			var bill6 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill6.BB_BF = header2.PK;
			bill6.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill6.BB_BillNum = "BILL6";
			bill6.BB_CustomsStatus = "S6";
			newFactory.Save();

			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, false, "BILL1", ZDateTime.Now), Is.EqualTo(default(ISFBillData)));
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, true, "BILL1", ZDateTime.Now), Is.EqualTo(default(ISFBillData)));
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, false, "BILL2", ZDateTime.Now), Is.EqualTo(default(ISFBillData)));
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, true, "BILL2", ZDateTime.Now), Is.EqualTo(default(ISFBillData)));
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, false, "BILL3", ZDateTime.Now), Is.EqualTo(default(ISFBillData)));
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, true, "BILL3", ZDateTime.Now), Is.EqualTo(default(ISFBillData)));

			AssertISFBillData(ISFStatusHelper.GetISFBillData(Factory, false, "BILL4", ZDateTime.Now), "BILL4", DispositionCodeList.Codes.S4, DispositionCodeList.Descriptions.S4);
			AssertISFBillData(ISFStatusHelper.GetISFBillData(Factory, true, "BILL4", ZDateTime.Now), "BILL4", DispositionCodeList.Codes.S4, DispositionCodeList.Descriptions.S4);
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, true, "BILL4", ZDateTime.Now.AddYears(2)), Is.EqualTo(default(ISFBillData)), "Should not match as it's outside the date range - should be [null]");
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, true, "BILL4", ZDateTime.Empty), Is.EqualTo(default(ISFBillData)), "Should not match as the createTime is invalid - should be [null]");
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, false, "BILL5", ZDateTime.Now), Is.EqualTo(default(ISFBillData)), "MasterBill (MB) do not have disposition in ISF - should be [null]");
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, true, "BILL5", ZDateTime.Now), Is.EqualTo(default(ISFBillData)), "MasterBill (MB) do not have disposition in ISF - should be [null]");
			AssertISFBillData(ISFStatusHelper.GetISFBillData(Factory, false, "BILL6", ZDateTime.Now), "BILL6", DispositionCodeList.Codes.S6, DispositionCodeList.Descriptions.S6);
			AssertISFBillData(ISFStatusHelper.GetISFBillData(Factory, false, new ZString[] { "BILL6" }, ZDateTime.Now), "BILL6", DispositionCodeList.Codes.S6, DispositionCodeList.Descriptions.S6);
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillData(Factory, true, "BILL6", ZDateTime.Now), Is.EqualTo(default(ISFBillData)));
			AssertISFBillData(ISFStatusHelper.GetISFBillData(Factory, false, new ZString[] { "BILL6", "BILL4" }, ZDateTime.Now), "BILL4, BILL6", ISFStatusHelper.Multiple, ISFStatusHelper.BillFoundOnMultipleISF);
			AssertISFBillData(ISFStatusHelper.GetISFBillData(Factory, true, new ZString[] { "BILL4", "BILL6" }, ZDateTime.Now), "BILL4", DispositionCodeList.Codes.S4, DispositionCodeList.Descriptions.S4);
			AssertISFBillData(ISFStatusHelper.GetISFBillData(Factory, false, new ZString[] { "BILL7", "BILL6" }, ZDateTime.Now), "BILL6", DispositionCodeList.Codes.S6, DispositionCodeList.Descriptions.S6);

			var bill7 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill7.BB_BF = header2.PK;
			bill7.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill7.BB_BillNum = "BILL6";
			bill7.BB_CustomsStatus = "S6";
			newFactory.Save();

			newFactory = new BusinessObjectFactory();
			var value1 = ISFStatusHelper.GetISFBillData(newFactory, false, "BILL6", ZDateTime.Now);
			AssertISFBillData(value1, "BILL6, BILL6", ISFStatusHelper.Multiple, ISFStatusHelper.BillFoundOnMultipleISF);
			var value2 = ISFStatusHelper.GetISFBillData(newFactory, false, "BILL6", ZDateTime.Now);
			NUnit.Framework.Assert.That(value2.BillNumber, Is.EqualTo(value1.BillNumber), "BillNumber");
			NUnit.Framework.Assert.That(value2.Status, Is.EqualTo(value1.Status), "Status");
			var tableSelect = newFactory.TableSelects.FirstOrDefault(x => x.TableName == CusISFBillSchema.Constants.TableName);
			var value3 = ISFStatusHelper.GetISFBillData(newFactory, false, "BILL6", ZDateTime.Now);
			NUnit.Framework.Assert.That(value2.BillNumber, Is.EqualTo(value3.BillNumber), "BillNumber");
			NUnit.Framework.Assert.That(value2.Status, Is.EqualTo(value3.Status), "Status");
			var tableSelect2 = newFactory.TableSelects.FirstOrDefault(x => x.TableName == CusISFBillSchema.Constants.TableName);
			NUnit.Framework.Assert.That(tableSelect2.Value, Is.EqualTo(tableSelect.Value));
		}

		[ExpectNoExceptions]
		void AssertISFBillData(ISFBillData isfBillData, ZString billNumber, ZString status, ZString statusDesc)
		{
			NUnit.Framework.Assert.That(isfBillData.BillNumber, Is.EqualTo(billNumber), "BillNumber");
			NUnit.Framework.Assert.That(isfBillData.Status, Is.EqualTo(status), "Status");
			NUnit.Framework.Assert.That(isfBillData.StatusDescription, Is.EqualTo(statusDesc), "StatusDescription");
		}

		[ExpectNoExceptions]
		public void TestGetISFBillStatusDescription()
		{
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillStatusDescription(Factory, ISFStatusHelper.Multiple), Is.EqualTo(ISFStatusHelper.BillFoundOnMultipleISF).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(ISFStatusHelper.GetISFBillStatusDescription(Factory, DispositionCodeList.Codes.S2), Is.EqualTo(DispositionCodeList.Descriptions.S2).Using(CustomComparers.TypeComparison));
		}
	}
}
