using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_CFSJobProfitChargeTest : ScriptTest
	{
		[TestDate(2018,03,31)]
		public void TestReport_CFSJobProfitCharge()
		{
			var shipment = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSShipment);

			var loadListConsol1 = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList);
			var loadListConsol2 = TestObjectCreator.CreateJobPlugIn(JobInvoicingConsumerTypes.CFSLoadList);

			var link1 = Factory.NewWithValidTestData(typeof(JobConShipLink)) as JobConShipLink;
			link1.JN_JS = shipment.PK;
			link1.JN_JK = loadListConsol1.PK;

			var link2 = Factory.NewWithValidTestData(typeof(JobConShipLink)) as JobConShipLink;
			link2.JN_JS = shipment.PK;
			link2.JN_JK = loadListConsol2.PK;

			var job = TestObjectCreator.CreateJob(shipment, false);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			Factory.Save();

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 10M, 20M);
			Factory.Save();

			var table = RunScript();

			AssertEquals("1 row should be found, the charge line is not duplicated because multiple consol is linked to the CFS shipment through Load List", 1, table.Rows.Count);
		}

		DataTable RunScript()
		{
			var sqlString = string.Format(@"SELECT *
FROM Report_CFSJobProfitCharge
(
'{0}'		--@CurrentCountry
,'{1}'		--@CompanyPK
,'{2}'		--@TransactionFrom
,'{3}'		--@TransactionTo
,'{4}'		--@OutstandingWIP
,'{5}'		--@OutstandingACR
,'{6}'		--@ChargeCode
,'{7}'		--@ExcludedChargeCode
,'{8}'		--@ChargeGroup
,null		--@SalesGroup
,null		--@ExpenseGroup
,'{9}'		--@TransactionBranch
,'{10}'		--@TransactionDepartment
,'{11}'		--@TransactionDebtor
,'{12}'		--@TransactionCreditor
,'{13}'		--@JW_FromDT
,'{14}'		--@JW_ToDT
,'{15}'		--@JW_FromAT
,'{16}'		--@JW_ToAT
,'{17}'		--@JW_RL_NKLoad
,'{18}'		--@JW_RL_NKDischarge
,'{19}'		--@RevRecogFrom 
,'{20}'		--@RevRecogTo
,'{21}'		--@CFSJobType
)",
GlbCompany.CurrentCompany.GC_RN_NKCountryCode		//@CurrentCountry
, GlbCompany.CurrentCompany.PK.ToGuid()				//@CompanyPK
, GetMinDateTimeString(ZDateTime.Now.AddDays(-1))   //TransactionFrom
, GetMaxDateTimeString(ZDateTime.Now.AddDays(1))    //TransactionTo
, string.Empty										//OutstandingWIP
, string.Empty                                      //OutstandingACR
, string.Empty                                      //ChargeCode
, string.Empty                                      //ExcludedChargeCode
, string.Empty                                      //ChargeGroup
, string.Empty                                      //TransactionBranch
, string.Empty                                      //TransactionDepartment
, string.Empty                                      //TransactionDebtor
, string.Empty                                      //TransactionCreditor
, GetMinDateTimeString(ZDateTime.Empty)				//JW_FromDT
, GetMaxDateTimeString(ZDateTime.Empty)				//JW_ToDT
, GetMinDateTimeString(ZDateTime.Empty)				//JW_FromAT
, GetMaxDateTimeString(ZDateTime.Empty)				//JW_ToAT
, null                                              //JW_RL_NKLoad
, null                                              //JW_RL_NKDischarge
, GetMinDateTimeString(ZDateTime.Empty)				//RevRecogFrom
, GetMaxDateTimeString(ZDateTime.Empty)				//RevRecogTo
, string.Empty                                      //CFSJobType
);
			return DataUtils.GetDataTableFromQuery(Db.Connection, sqlString);
		}
	}
}
