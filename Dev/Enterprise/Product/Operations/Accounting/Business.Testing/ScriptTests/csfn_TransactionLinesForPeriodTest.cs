using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class csfn_TransactionLinesForPeriodTest : ScriptTest
	{
		[TestDate(2021, 9, 1)]
		public void TestTransactionLinesForPeriod_FilterREVCSTByAL_JH()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S1");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);

			var shipment2 = TestObjectCreator.CreateShipment("S2");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);

			var arInv1 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine1 = TestObjectCreator.CreateInvoiceLine(arInv1, TestObjectCreator.AUD, 1m, 100m);
			arInvLine1.AL_JH = job1.PK;
			var charge1 = TestObjectCreator.CreateJobCharge(arInvLine1, job1, TestObjectCreator.CC3, TestObjectCreator.AUD);

			var apInv1 = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv1", TestObjectCreator.AUD, 1m, 200m, 0m, 0m, 200m, 0m, 0m);
			var apInvLine1 = apInv1.Lines[0];
			apInvLine1.AL_JH = job1.PK;
			var charge2 = TestObjectCreator.CreateJobCharge(apInv1.Lines[0], job1, TestObjectCreator.CC3, TestObjectCreator.AUD);

			var arInv2 = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv2", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine2 = TestObjectCreator.CreateInvoiceLine(arInv2, TestObjectCreator.AUD, 1m, 300m);
			arInvLine2.AL_JH = job2.PK;
			var charge3 = TestObjectCreator.CreateJobCharge(arInvLine2, job2, TestObjectCreator.CC3, TestObjectCreator.AUD);

			var apInv2 = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv2", TestObjectCreator.AUD, 1m, 400m, 0m, 0m, 400m, 0m, 0m);
			var apInvLine2 = apInv2.Lines[0];
			apInvLine2.AL_JH = job2.PK;
			var charge4 = TestObjectCreator.CreateJobCharge(apInv2.Lines[0], job2, TestObjectCreator.CC3, TestObjectCreator.AUD);
			Factory.Save();

			var rev1 = Factory.Load<AccTransactionLines>(charge1.JR_AL_ARLine);
			var cst1 = Factory.Load<AccTransactionLines>(charge2.JR_AL_APLine);

			var rev2 = Factory.Load<AccTransactionLines>(charge3.JR_AL_ARLine);
			var cst2 = Factory.Load<AccTransactionLines>(charge4.JR_AL_APLine);

			var expectedReverseDate = new ZDateTime(2021, 9, 1);
			AssertEquals("[REV 1] Reverse Date: 2021/9/1", expectedReverseDate, rev1.AL_ReverseDate);
			AssertEquals("[CST 1] Reverse Date: 2021/9/1", expectedReverseDate, cst1.AL_ReverseDate);
			AssertEquals("[REV 2] Reverse Date: 2021/9/1", expectedReverseDate, rev2.AL_ReverseDate);
			AssertEquals("[CST 2] Reverse Date: 2021/9/1", expectedReverseDate, cst2.AL_ReverseDate);

			var headers = new[] { "AL_PK", "AL_JH" };

			//Filter by AL_PostDate
			var result = RunScript(expectedReverseDate.ToISO8601String(), expectedReverseDate.AddDays(1).ToISO8601String(), job1.PK);
			var lines = new object[][]
			{
				new object[] {	rev1.PK, job1.PK },
				new object[] {	cst1.PK, job1.PK },
			};
			AssertDataTableAllRowsByKeyColumns("Contain rev1 & cst1 for job1", result, headers, lines, headers);

			result = RunScript(expectedReverseDate.ToISO8601String(), expectedReverseDate.AddDays(1).ToISO8601String(), job2.PK);
			lines = new object[][]
			{
				new object[] {	rev2.PK, job2.PK },
				new object[] {	cst2.PK, job2.PK },
			};
			AssertDataTableAllRowsByKeyColumns("Contain rev2 & cst2 for job2", result, headers, lines, headers);
		}

		[TestDate(2021, 9, 1)]
		public void TestTransactionLinesForPeriod_FilterACRWIPByAL_JH()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S1");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 100m, 200m);

			var shipment2 = TestObjectCreator.CreateShipment("S2");
			var job2 = TestObjectCreator.CreateJob(shipment2, false);
			var charge2 = TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, 300m, 400m);
			Factory.Save();

			var acr1 = charge1.Accrual;
			var wip1 = charge1.WIP;

			var acr2 = charge2.Accrual;
			var wip2 = charge2.WIP;

			charge1.JR_LocalCostAmt = charge1.JR_LocalSellAmt = charge2.JR_LocalCostAmt = charge2.JR_LocalSellAmt = 0m;
			acr1.AL_PostDate = wip1.AL_PostDate = acr2.AL_PostDate = wip2.AL_PostDate = new ZDateTime(2021, 8, 1); // post date manually changed for test purpose
			Factory.Save();

			var expectedPostDate = new ZDateTime(2021, 8, 1);
			AssertEquals("[ACR 1] Post Date: 2021/8/1", expectedPostDate, acr1.AL_PostDate);
			AssertEquals("[WIP 1] Post Date: 2021/8/1", expectedPostDate, wip1.AL_PostDate);
			AssertEquals("[ACR 2] Post Date: 2021/8/1", expectedPostDate, acr2.AL_PostDate);
			AssertEquals("[WIP 2] Post Date: 2021/8/1", expectedPostDate, wip2.AL_PostDate);

			var expectedReverseDate = new ZDateTime(2021, 9, 1);
			AssertEquals("[ACR 1] Reverse Date: 2021/9/1", expectedReverseDate, acr1.AL_ReverseDate);
			AssertEquals("[WIP 1] Reverse Date: 2021/9/1", expectedReverseDate, wip1.AL_ReverseDate);
			AssertEquals("[ACR 2] Reverse Date: 2021/9/1", expectedReverseDate, acr2.AL_ReverseDate);
			AssertEquals("[WIP 2] Reverse Date: 2021/9/1", expectedReverseDate, wip2.AL_ReverseDate);

			var headers = new[] { "AL_PK", "AL_JH" };

			//Filter by AL_PostDate
			var result = RunScript(expectedPostDate.ToISO8601String(), expectedPostDate.AddDays(1).ToISO8601String(), job1.PK);
			var lines = new object[][]
			{
				new object[] {	acr1.PK, job1.PK },
				new object[] {	wip1.PK, job1.PK },
			};
			AssertDataTableAllRowsByKeyColumns("Contain acr1 & wip1 for job1", result, headers, lines, headers);

			result = RunScript(expectedPostDate.ToISO8601String(), expectedPostDate.AddDays(1).ToISO8601String(), job2.PK);
			lines = new object[][]
			{
				new object[] {	acr2.PK, job2.PK },
				new object[] {	wip2.PK, job2.PK },
			};
			AssertDataTableAllRowsByKeyColumns("Contain acr2 & wip2 for job2", result, headers, lines, headers);

			//Filter by AL_ReverseDate
			result = RunScript(expectedReverseDate.ToISO8601String(), expectedReverseDate.AddDays(1).ToISO8601String(), job1.PK);
			lines = new object[][]
			{
				new object[] {	acr1.PK, job1.PK },
				new object[] {	wip1.PK, job1.PK },
			};
			AssertDataTableAllRowsByKeyColumns("Contain acr1 & wip1 for job1", result, headers, lines, headers);

			result = RunScript(expectedReverseDate.ToISO8601String(), expectedReverseDate.AddDays(1).ToISO8601String(), job2.PK);
			lines = new object[][]
			{
				new object[] {	acr2.PK, job2.PK },
				new object[] {	wip2.PK, job2.PK },
			};
			AssertDataTableAllRowsByKeyColumns("Contain acr2 & wip2 for job2", result, headers, lines, headers);
		}

		[TestDate(2012, 10, 2)]
		public void TestTransactionLinesForPeriodByTransactionRecognitionDates()
		{
			#region Recognized ACR, WIP, CST and REV

			var shipment = TestObjectCreator.CreateShipment("S1");
			var job = TestObjectCreator.CreateJob(shipment, false);
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 200m);

			var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("ARInv1", TestObjectCreator.AUD, 1m, TestObjectCreator.AALSHI);
			var arInvLine = TestObjectCreator.CreateInvoiceLine(arInv, TestObjectCreator.AUD, 1m, 400m);
			arInvLine.AL_JH = job.PK;
			var charge2 = TestObjectCreator.CreateJobCharge(arInvLine, job, TestObjectCreator.CC3, TestObjectCreator.AUD);

			var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("APInv1", TestObjectCreator.AUD, 1m, 300m, 0m, 0m, 300m, 0m, 0m);
			var apInvLine = apInv.Lines[0];
			apInvLine.AL_JH = job.PK;
			var charge3 = TestObjectCreator.CreateJobCharge(apInv.Lines[0], job, TestObjectCreator.CC3, TestObjectCreator.AUD);
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

			DataTable result = RunScript(new ZDateTime(2012, 3, 11).ToISO8601String(), new ZDateTime(2012, 3, 12).ToISO8601String(), job.PK);
			var headers = new[] { "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var lines = new object[][]
						{
							new object[] {	-100m,	0m,		0m,		0m	},
							new object[] {	0m,		200m,	0m,		0m	},
						};
			AssertDataTableAllRowsByKeyColumns("Recognized ACR and WIP's Post Date in Range", result, headers, lines, headers);

			result = RunScript(new ZDateTime(2012, 10, 2).ToISO8601String(), new ZDateTime(2012, 10, 3).ToISO8601String(), job.PK);
			lines = new object[][]
						{
							new object[] {	100m,	0m,		0m,		0m	 },
							new object[] {	0m,		-200m,	0m,		0m   },
							new object[] {	0m,		0m,		0m,		400m },
							new object[] {	0m,		0m,		-300m,	0m   },
						};
			AssertDataTableAllRowsByKeyColumns("Recognized ACR and WIP's Post Date in Range", result, headers, lines, headers);

			result = RunScript(new ZDateTime(2012, 3, 11).ToISO8601String(), new ZDateTime(2012, 10, 3).ToISO8601String(), job.PK);
			lines = new object[][]
						{
							new object[] {	-100m,	0m,		0m,		0m	 },
							new object[] {	0m,		200m,	0m,		0m	 },
							new object[] {	100m,	0m,		0m,		0m	 },
							new object[] {	0m,		-200m,	0m,		0m   },
							new object[] {	0m,		0m,		0m,		400m },
							new object[] {	0m,		0m,		-300m,  0m   },
						};
			AssertDataTableAllRowsByKeyColumns("Recognized ACR and WIP's Post Date in Range", result, headers, lines, headers);

			result = RunScript(MinDateTime, MaxDateTime, job.PK);
			AssertDataTableAllRowsByKeyColumns("Range Not Provided from Filter", result, headers, lines, headers);

			result = RunScript(new ZDateTime(2012, 1, 1).ToISO8601String(), new ZDateTime(2012, 3, 10).ToISO8601String(), job.PK);
			AssertEquals("Out of Range", 0, result.Rows.Count);
		}

		[TestDate(2022, 1, 2, 3, 4, 0)]
		public void TestExcludeReversedWIPAndACRFiltering()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 100m, 100m);

			charge1.JR_OSCostAmt = 10m;
			charge1.JR_OSSellAmt = 10m;

			Factory.Save();
			charge1.Accrual.AL_PostDate = new ZDateTime(2022, 10, 01);
			charge1.WIP.AL_PostDate = new ZDateTime(2022, 10, 02);
			charge1.ReverseAccrual(new ZDateTime(2022, 10, 03));
			charge1.ReverseWIP(new ZDateTime(2022, 10, 04));

			Factory.Save();

			var headers = new[] { "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };
			var keyColumns = new[] { "ACRAmount", "WIPAmount", "CSTAmount", "REVAmount" };

			var result =  RunScript(new ZDateTime(2022, 10, 01).ToISO8601String(), new ZDateTime(2022, 10, 05).ToISO8601String(), job1.PK, "Y");
			var result1 = RunScript(new ZDateTime(2022, 10, 01).ToISO8601String(), new ZDateTime(2022, 10, 02).ToISO8601String(), job1.PK, "Y");
			var result2 = RunScript(new ZDateTime(2022, 10, 02).ToISO8601String(), new ZDateTime(2022, 10, 03).ToISO8601String(), job1.PK, "Y");
			var result3 = RunScript(new ZDateTime(2022, 10, 03).ToISO8601String(), new ZDateTime(2022, 10, 04).ToISO8601String(), job1.PK, "Y");
			var result4 = RunScript(new ZDateTime(2022, 10, 04).ToISO8601String(), new ZDateTime(2022, 10, 05).ToISO8601String(), job1.PK, "Y");

			var line1 = new object[][]
						{
							new object[] {  -10m,       0m,     0m,     0m },
						};
			var line2 = new object[][]
						{
							new object[] {  0m,       10m,     0m,     0m },
						};
			var line3 = new object[][]
						{
							new object[] {  10m,       0m,     0m,     0m  },
						};
			var line4 = new object[][]
						{
							new object[] {  0m,       -10m,     0m,     0m },
						};

			AssertEquals("Filtering reversed WIP and ACR1", 0, result.Rows.Count);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR2", result1, headers, line1, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR3", result2, headers, line2, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR4", result3, headers, line3, keyColumns);
			AssertDataTableAllRowsByKeyColumns("Filtering reversed WIP and ACR5", result4, headers, line4, keyColumns);
		}

		[TestDate(2012, 09, 24)]
		public void TestWhenExcludeReversedWIPACRIsNull()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S0001");
			var job1 = TestObjectCreator.CreateJob(shipment1, false);
			var charge1 = TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, 10m, 20m);
			Factory.Save();

			charge1.ReverseAccrual(ZDateTime.Today);
			charge1.ReverseWIP(ZDateTime.Today);
			Factory.Save();

			var headers = new[] { "ACRAmount", "WIPAmount" };
			var keyColumns = new[] { "ACRAmount", "WIPAmount" };
			object[][] lines =
			[
				[-10m, 0m],
				[10m, 0m],
				[-10m, 0m],
				[0m, 20m],
				[0m, -20m],
				[0m, 20m],
			];

			var result = RunScript(MinDateTime, MaxDateTime, job1.PK, null);
			AssertDataTableAllRowsByKeyColumns("Should contains reversed lines when AL_ExcludeReversedWIPACR is null", result, headers, lines, keyColumns);
		}

		DataTable RunScript(string transactionFrom, string transactionTo, ZGuid jobPK, string excludeReversedWIPACR = "")
		{
			var paramExcludeReversedWIPACR = excludeReversedWIPACR == null ? "NULL" : $"'{excludeReversedWIPACR}'";
			var sql = string.Format(@"SELECT *
							FROM csfn_TransactionLinesForPeriod('{0}','{1}','N','N','{2}', {3})", transactionFrom, transactionTo, jobPK, paramExcludeReversedWIPACR);

			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}
	}
}


