using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Testing;
using Enterprise.Client.JAS.Business.JXC.Import;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.JAS.Business
{
	[TestedType(typeof(JASJob))]
	internal class JASJobTest : JobTest
	{
		protected override string TypeForErrorMsg => "JASJob";

		[ExpectNoExceptions]
		public void TestAddChargeFromNullChargeData()
		{
			JASJob.AddCharges();
		}

		public void TestAddChargeFromChargeData()
		{
			AssertEquals("Pre-condition", 0, JASJob.Charges.Count);
			JASJob.JH_GB = GlbBranch.CurrentBranch.PK;
			ZGuid localAgentGuid = Factory.NewWithValidTestData<OrgHeader>().PK;
			JASJob.LocalChargesPK = localAgentGuid;
			Guid defaultChargeCodeGuid = Guid.NewGuid();
			JASDataRegistry.Instance.DefaultChargeCodeForAccrualsImportItem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, defaultChargeCodeGuid);
			JobChargeData chargeData1 = new JobChargeData("690", "SAFe(Security Admin Fee)", "X01", 1m);
			JobChargeData chargeData2 = new JobChargeData("FRT", "This is the freight", "IDR", 125.5m);
			JobChargeData chargeData3 = new JobChargeData("ADINS", "This is not a DSB or MRG", "USD", 200m);
			JobChargeData chargeData4 = new JobChargeData("CUSDSB", "This is DSB", "AUD", 800m);
			CHGSRecord cHGSRecord = new CHGSRecord("CHGS3100", "111;Something else;23.00;P;923");
			JASJob.AddCharges(chargeData1, chargeData2, chargeData3, chargeData4, cHGSRecord);
			AssertEquals("Only collect charges should be imported", 4, JASJob.Charges.Count);
			AssertJobCharge(JASJob.Charges[0], defaultChargeCodeGuid, "SAFe(Security Admin Fee)", GlbCompany.CurrentCompany.LocalCurrency.RX_Code, localAgentGuid, 1m);
			AssertJobCharge(JASJob.Charges[1], new ZGuid(Env.Registry.FreightChargeCode), "This is the freight", "IDR", localAgentGuid, 126m);
			AssertJobCharge(JASJob.Charges[2], defaultChargeCodeGuid, "This is not a DSB or MRG", "USD", localAgentGuid, 200m);
			ZQuery filter = new ZQuery(AccChargeCodeSchema.AC_Code, "CUSDSB");
			filter.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			AccChargeCode expectedChargeCode = Factory.LoadTop1<AccChargeCode>(filter);
			AssertJobCharge(JASJob.Charges[3], expectedChargeCode.PK, "This is DSB", "AUD", localAgentGuid, 800m);
		}

		[ExpectNoExceptions]
		public void TestAddChargeFromChargeData_ShouldBeMergedWithUncommittedExistingCharges()
		{
			JASJob.FillWithValidTestData();
			Factory.Save();
			AccChargeCode chargeCode690 = CreateNewValidChargeCode("690");
			AccChargeCode chargeCode888 = CreateNewValidChargeCode("888");
			AccChargeCode chargeCode111 = CreateNewValidChargeCode("111");
			AccChargeCode chargeCode222 = CreateNewValidChargeCode("222");
			JobCharge charge888 = CreateNewJobCharge(chargeCode888.PK);
			Factory.Save();
			CreateNewJobCharge(chargeCode690.PK);
			CreateNewJobCharge(chargeCode111.PK);
			CreateNewJobCharge(new ZGuid(Env.Registry.FreightChargeCode));
			JobChargeData chargeData1 = new JobChargeData("690", "SAFe(Security Admin Fee)", "X01", 1m);
			JobChargeData chargeData2 = new JobChargeData("FRT", "This is the freight", "IDR", 125.5m);
			JobChargeData chargeData3 = new JobChargeData("111", "This is 111", "USD", 200m);
			JobChargeData chargeData4 = new JobChargeData("888", "This is DSB", "AUD", 800m);
			JobChargeData chargeData5 = new JobChargeData("FRT", "Another Freight", "AUD", 300m);
			JobChargeData chargeData6 = new JobChargeData("222", "Charge 222", "AUD", 600m);
			JobChargeData chargeData7 = new JobChargeData("222", "Charge 222-2", "AUD", 15m);
			AssertEquals("Pre-condition", 4, JASJob.Charges.Count);
			JASJob.AddCharges(chargeData1, chargeData2, chargeData3, chargeData4, chargeData5, chargeData6, chargeData7);
			AssertEquals("Uncommitted Existing charges should be merged", 8, JASJob.Charges.Count);
			JASJob.Charges.Sort(JobChargeSchema.Constants.JR_Desc, ListSortDirection.Ascending);
			RefCurrency sGD = RefCurrency.LoadFromCurrencyCode(Factory, "SGD");
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, "AUD");
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, "USD");
			RefCurrency iDR = RefCurrency.LoadFromCurrencyCode(Factory, "IDR");
			ZGuid freightChargeCodePK = Env.Registry.FreightChargeCode;
			AssertJobCharge(JASJob.Charges[0], chargeCode888.PK, "888", sGD.RX_Code, ZGuid.Empty, 101m);
			AssertJobCharge(JASJob.Charges[1], freightChargeCodePK, "Another Freight", aUD.RX_Code, JASJob.LocalChargesPK, 300m);
			AssertJobCharge(JASJob.Charges[2], chargeCode222.PK, "Charge 222", aUD.RX_Code, JASJob.LocalChargesPK, 600m);
			AssertJobCharge(JASJob.Charges[3], chargeCode222.PK, "Charge 222-2", aUD.RX_Code, JASJob.LocalChargesPK, 15m);
			AssertJobCharge(JASJob.Charges[4], chargeCode690.PK, "SAFe(Security Admin Fee)", GlbCompany.CurrentCompany.LocalCurrency.RX_Code, JASJob.LocalChargesPK, 1m);
			AssertJobCharge(JASJob.Charges[5], chargeCode111.PK, "This is 111", uSD.RX_Code, JASJob.LocalChargesPK, 200m);
			AssertJobCharge(JASJob.Charges[6], chargeCode888.PK, "This is DSB", aUD.RX_Code, JASJob.LocalChargesPK, 800m);
			AssertJobCharge(JASJob.Charges[7], freightChargeCodePK, "This is the freight", iDR.RX_Code, JASJob.LocalChargesPK, 126m);
			Factory.Save();
		}

		[ExpectNoExceptions]
		public void TestNewJobChargeCanBeSaved()
		{
			JASJob.FillWithValidTestData();
			CHGSRecord record = new CHGSRecord("CHGS3100", "FRT;This is the freight;125.50;C;IDR");
			JASJob.AddCharges(record);
			Factory.Save();
		}

		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewJobForTesting<Job>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (fJASJob != null)
			{
				JASJob.Dispose();
			}
		}

		JASJob JASJob
		{
			get
			{
				if (fJASJob == null)
				{
					JASForwardingShipment shipment = Factory.New<JASForwardingShipment>();
					fJASJob = (JASJob)new Job.Loader(shipment).TryCreate();
				}

				return fJASJob;
			}
		}

		JobCharge CreateNewJobCharge(ZGuid chargeCodePK)
		{
			JobCharge result = JASJob.Charges.AddNew();
			result.JR_AC = chargeCodePK;
			result.JR_Desc = result.ChargeCode.AC_Code;
			result.JR_RX_NKSellCurrency = "SGD";
			result.JR_OH_SellAccount = ZGuid.Empty;
			result.JR_OSSellAmt = 101;
			return result;
		}

		AccChargeCode CreateNewValidChargeCode(ZString chargeCode)
		{
			AccChargeCode result = Factory.NewWithValidTestData<AccChargeCode>();
			result.AC_ChargeType = Core.Constants.ChargeType.Margin;
			result.AC_Code = chargeCode;
			return result;
		}

		void AssertJobCharge(JobCharge jobCharge, ZGuid expectedChargeCodePK, ZString expectedDesc, ZString expectedSellCurrencyCode, ZGuid expectedSellAccountPK, ZDecimal expectedSellAmount)
		{
			AssertEquals(expectedChargeCodePK, jobCharge.JR_AC);
			AssertEquals(expectedDesc, jobCharge.JR_Desc);
			AssertEquals(expectedSellCurrencyCode, jobCharge.JR_RX_NKSellCurrency);
			AssertEquals(expectedSellAccountPK, jobCharge.JR_OH_SellAccount);
			AssertEquals(expectedSellAmount, jobCharge.JR_OSSellAmt);
		}

		JASJob fJASJob;
		#endregion
	}
}
