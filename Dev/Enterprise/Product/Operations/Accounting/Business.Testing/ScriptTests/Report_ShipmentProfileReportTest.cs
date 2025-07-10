
using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ShipmentProfileReportTest : ScriptTest
	{
		[TestDate(2012, 10, 2)]
		public void TestShipmentProfileReportByTransactionRecognitionDates()
		{
			#region Recognized ACR, WIP, CST and REV

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 200m);

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine = TestObjectCreator.CreateInvoiceLine(arInv, TestObjectCreator.AUD, 1m, 400m);
			arInvLine.AL_JH = job.PK;
			var charge2 = TestObjectCreator.CreateJobCharge(arInvLine, job, TestObjectCreator.CC6, TestObjectCreator.AUD);
			charge2.JR_OSCostAmt = 0m;

			var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv1", TestObjectCreator.AUD, 1m, 300m, 0m, 0m, 300m, 0m, 0m);
			var apInvLine = apInv.Lines[0];
			apInvLine.AL_JH = job.PK;
			var charge3 = TestObjectCreator.CreateJobCharge(apInv.Lines[0], job, TestObjectCreator.CC6, TestObjectCreator.AUD);
			charge3.JR_OSSellAmt = 0m;
			Factory.Save();

			var acr = charge.Accrual;
			var wip = charge.WIP;
			var rev = Factory.Load<AccTransactionLines>(charge2.JR_AL_ARLine);
			var cst = Factory.Load<AccTransactionLines>(charge3.JR_AL_APLine);

			AssertNotEquals("ACR created", null, acr);
			AssertNotEquals("WIP created", null, wip);
			AssertNotEquals("REV created", null, rev);
			AssertNotEquals("CST created", null, cst);

			charge.JR_LocalCostAmt = charge.JR_LocalSellAmt = 0m;
			acr.AL_PostDate = wip.AL_PostDate = new ZDateTime(2012, 3, 11); // post date manually changed for test purpose
			Factory.Save();

			AssertEquals("[ACR] Post Date: 2012/3/11", new ZDateTime(2012, 3, 11), acr.AL_PostDate);
			AssertEquals("[ACR] Reverse Date: 2012/10/2", new ZDateTime(2012, 10, 2), acr.AL_ReverseDate);
			AssertEquals("[WIP] Post Date: 2012/3/11", new ZDateTime(2012, 3, 11), wip.AL_PostDate);
			AssertEquals("[WIP] Reverse Date: 2012/10/2", new ZDateTime(2012, 10, 2), wip.AL_ReverseDate);

			#endregion

			#region Unrecognized ACR, WIP, CST and REV

			RevenueRecognitionCollection registryCollection =  new RevenueRecognitionCollection();
			RevenueRecognition registryValue = registryCollection.AddNew();
			registryValue.JobType = "SHP";
			registryValue.DirectionCode = Enterprise.Core.Constants.FreightShipmentDirection.Code.All;
			registryValue.Mode = RevenueRecognitionLookups.ModeAdditionalCodes.All;
			registryValue.RecognitionDateOptionCode = RevenueRecognitionLookups.RecognitionDateOptionCodes.JobClosure;
			AccountingConfigurationRegistry.Instance.RevenueRecognitionSetup.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, registryCollection);
			AccountingConfigurationRegistry.Instance.RecognizeProfitOnWIPsAccrualsBeforePosting.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			job.ClearCacheOfRevenueRecognitionRegistryItems_ForTestsOnly();
			var charge4 = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC3, 500m, 600m);

			var arInv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv2", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine2 = TestObjectCreator.CreateInvoiceLine(arInv2, TestObjectCreator.AUD, 1m, 700m);
			arInvLine2.AL_JH = job.PK;
			var charge5 = TestObjectCreator.CreateJobCharge(arInvLine2, job, TestObjectCreator.CC4, TestObjectCreator.AUD);
			charge5.JR_OSCostAmt = 0m;

			var apInv2 = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv2", TestObjectCreator.AUD, 1m, 800m, 0m, 0m, 800m, 0m, 0m);
			var apInvLine2 = apInv2.Lines[0];
			apInvLine2.AL_JH = job.PK;
			var charge6 = TestObjectCreator.CreateJobCharge(apInv2.Lines[0], job, TestObjectCreator.CC5, TestObjectCreator.AUD);
			charge6.JR_OSSellAmt = 0m;
			Factory.Save();

			Db.Connection.ExecuteNonQuery(string.Format("update dbo.AccTransactionLines set AL_ReverseDate = null, AL_SystemLastEditTimeUtc = GETUTCDATE(), AL_SystemLastEditUser = 'TST' where AL_PK in ('{0}', '{1}')", arInvLine2.PK, apInvLine2.PK));
			arInvLine2.Reload();
			apInvLine2.Reload();
			Factory.Save();

			AssertEquals("Should be Unrecognized ACR", null, charge4.Accrual);
			AssertEquals("Should be Unrecognized WIP", null, charge4.WIP);
			AssertEquals("Should be Unrecognized CST", ZDateTime.Empty, apInvLine2.AL_ReverseDate);
			AssertEquals("Should be Unrecognized REV", ZDateTime.Empty, arInvLine2.AL_ReverseDate);

			#endregion

			DataTable result = RunScript(new ZDateTime(2012, 3, 11), new ZDateTime(2012, 3, 12));
			var headers = new[] { "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount", "UnRecogWIPAmount", "UnRecogACRAmount", "UnRecogCSTAmount", "UnRecogREVAmount" };
			var lines = new object[][]
						{
							new object[] {	-100m,  200m,  0m,  0m,  0m,  0m,  0m,  0m  }
						};
			AssertDataTableAllRows("Recognized ACR and WIP's Post Date in Range", result, headers, lines);

			result = RunScript(new ZDateTime(2012, 10, 2), new ZDateTime(2012, 10, 3));
			lines = new object[][]
						{
							new object[] {	100m,  -200m,  -300m,  400m,  0m,  0m,  0m,  0m  }
						};
			AssertDataTableAllRows("Recognized ACR and WIP's Reverse Date in Range", result, headers, lines);

			result = RunScript(new ZDateTime(2012, 3, 11), new ZDateTime(2012, 10, 3));
			lines = new object[][]
						{
							new object[] {	0m,  0m,  -300m,  400m,  0m,  0m,  0m,  0m  }
						};
			AssertDataTableAllRows("Recognized ACR and WIP's Post And Reverse Date in Range", result, headers, lines);

			result = RunScript();
			lines = new object[][]
						{
							new object[] {	0m,  0m,  -300m,  400m,  600m,  -500m,  -800m,  700m  }
						};
			AssertDataTableAllRows("Range Not Provided from Filter", result, headers, lines);

			result = RunScript(new ZDateTime(2012, 1, 1), new ZDateTime(2012, 3, 10));
			AssertEquals("Out of Range", 0, result.Rows.Count);
		}

		public void TestShipmentProfileReportPrintsShipmentWhereJobIncludingUnrecognizedLines()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var charge = job2.Charges.AddNew();
			charge.JR_AC = TestObjectCreator.CC1.PK;
			Factory.Save();

			DataTable result = RunScript();
			AssertEquals("Result Rows", 2, result.Rows.Count);
		}

		public void TestJobInactive()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1);

			var shipment2 = TestObjectCreator.CreateShipment("S0002");
			var job2 = TestObjectCreator.CreateJob(shipment2);
			Factory.Save();

			job2.MarkAsInactive();
			Factory.Save();

			var result = RunScript();
			AssertEquals("Result Rows", 1, result.Select("HeaderExists = 'Y'").Length);
		}

			DataTable RunScript()
		{
			return RunScript(ZDateTime.Empty, ZDateTime.Empty);
		}

		DataTable RunScript(ZDateTime transactionFrom, ZDateTime transactionTo)
		{
			return DataUtils.GetDataTableFromQuery(Db.Connection, string.Format(@"
SELECT * 
FROM Report_ShipmentProfileReport(	
'{0}', --@CurrentCountry
'{1}', --@CompanyPK
'', --@OrderedStorageClassCodesList
NULL, --@Origin
NULL, --@Destination
'ALL', --@Direction
'', --@RelatedClientType
'', --@RelatedPartyType
NULL, --@RelatedParty
'ALL', --@CoLoadType
'N', --@IncludeInactive
'{2}', --@JobRevFrom
'{3}', --@JobRevTo
'{4}', --@TransactionFrom
'{5}'  --@TransactionTo
)
",
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode,
			GlbCompany.CurrentCompany.PK,
			"1900-01-01 00:00:00",
			"2079-06-06 23:59:29",
			GetMinDateTimeString(transactionFrom),
			GetMaxDateTimeString(transactionTo)
			));
		}
	}
}


