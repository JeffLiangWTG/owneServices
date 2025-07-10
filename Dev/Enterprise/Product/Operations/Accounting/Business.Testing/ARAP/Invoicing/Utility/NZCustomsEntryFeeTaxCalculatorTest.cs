using System;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class NZCustomsEntryFeeTaxCalculatorTest : TestCaseWithFactory
	{
		public void TestGetEntryFeeGST_HardCodedNumbers()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var gst = NZCustomsEntryFeeTaxCalculator.GetEntryFeeGST(NZCustomsEntryFeeTaxCalculator.EntryFeeAmount_ForTestOnly, NZCustomsEntryFeeTaxCalculator.IncorrectEntryFeeGST_ForTestOnly);
				AssertEquals(NZCustomsEntryFeeTaxCalculator.IncorrectEntryFeeGST_ForTestOnly, gst);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var gst = NZCustomsEntryFeeTaxCalculator.GetEntryFeeGST(NZCustomsEntryFeeTaxCalculator.EntryFeeAmount_ForTestOnly, NZCustomsEntryFeeTaxCalculator.IncorrectEntryFeeGST_ForTestOnly);
				AssertEquals(NZCustomsEntryFeeTaxCalculator.CorrectEntryFeeGST_ForTestOnly, gst);

				gst = NZCustomsEntryFeeTaxCalculator.GetEntryFeeGST(-NZCustomsEntryFeeTaxCalculator.EntryFeeAmount_ForTestOnly, -NZCustomsEntryFeeTaxCalculator.IncorrectEntryFeeGST_ForTestOnly);
				AssertEquals(-NZCustomsEntryFeeTaxCalculator.CorrectEntryFeeGST_ForTestOnly, gst);

				gst = NZCustomsEntryFeeTaxCalculator.GetEntryFeeGST(100m, 10m);
				AssertEquals(10m, gst);
			}
		}

		#region TestUpdateEntryFeeGST

		[TestDate(2018, 01, 03)]
		public void TestUpdateEntryFeeGST_For2015Fee()
		{
			AssertUpdateEntryFeeGST();
		}

		public void TestUpdateEntryFeeGST()
		{
			AssertUpdateEntryFeeGST();
		}

		void AssertUpdateEntryFeeGST()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var gstRatePK = AccTaxRate.Helper.FindTaxRatePK(Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
				var rate = new BusinessObjectFactory().Load<AccTaxRate>(gstRatePK);
				rate.SetRateNumerator_ForTestOnly(15);
				rate.Factory.Save();

				var entryFeeChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				entryFeeChargeCode.AC_Code = "ENTRYFEE";
				entryFeeChargeCode.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
				entryFeeChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
				entryFeeChargeCode.AC_AT_GSTRate = gstRatePK;

				var chargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var typeAndCode1 = chargeTypesAndCodes.AddNew();
				typeAndCode1.ChargeType = NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee;
				typeAndCode1.AC_ChargeCode = entryFeeChargeCode.PK;

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeTypesAndCodes);

				var invoicingLineBase = Factory.NewWithValidTestData<APInvoiceLine>();
				invoicingLineBase.AL_AC = entryFeeChargeCode.PK;
				invoicingLineBase.AL_AT = gstRatePK;
				invoicingLineBase.AL_RX_NKTransactionCurrency = "NZD";

				AssertEquals(false, NZCustomsEntryFeeTaxCalculator.IsEntryFeeChargeWithCorrectAmount(invoicingLineBase));

				invoicingLineBase.AL_OSExTaxAmount = EntryFeeAmount;
				AssertEquals(CorrectEntryFeeGST, invoicingLineBase.AL_OSTaxAmount);
				AssertEquals(CorrectEntryFeeGST, invoicingLineBase.AL_OSGSTAmount);

				invoicingLineBase.AL_OSGSTAmount = TotalEntryFeeGST;
				invoicingLineBase.AL_OSTaxAmount = TotalEntryFeeGST;

				NZCustomsEntryFeeTaxCalculator.UpdateEntryFeeGST(invoicingLineBase);
				AssertEquals(CorrectEntryFeeGST, invoicingLineBase.AL_OSTaxAmount);
				AssertEquals(CorrectEntryFeeGST, invoicingLineBase.AL_OSGSTAmount);
				AssertEquals(true, NZCustomsEntryFeeTaxCalculator.IsEntryFeeChargeWithCorrectAmount(invoicingLineBase));
			}
		}

		#endregion

		#region TestUpdateEntryFeeGSTForRevenueCharge

		[TestDate(2018, 01, 03)]
		public void TestUpdateEntryFeeGSTForRevenueCharge_For2015Fee()
		{
			AssertUpdateEntryFeeGSTForRevenueCharge();
		}

		public void TestUpdateEntryFeeGSTForRevenueCharge()
		{
			AssertUpdateEntryFeeGSTForRevenueCharge();
		}

		void AssertUpdateEntryFeeGSTForRevenueCharge()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var gstRatePK = AccTaxRate.Helper.FindTaxRatePK(Factory, AccTaxRate.Helper.MainGSTTaxRegistryID, GlbCompany.CurrentCompany.PK.ToGuid());
				var rate = new BusinessObjectFactory().Load<AccTaxRate>(gstRatePK);
				rate.SetRateNumerator_ForTestOnly(15);
				rate.Factory.Save();

				var entryFeeChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				entryFeeChargeCode.AC_Code = "ENTRYFEE";
				entryFeeChargeCode.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
				entryFeeChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
				entryFeeChargeCode.AC_AT_GSTRate = gstRatePK;

				var chargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var typeAndCode1 = chargeTypesAndCodes.AddNew();
				typeAndCode1.ChargeType = NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee;
				typeAndCode1.AC_ChargeCode = entryFeeChargeCode.PK;

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeTypesAndCodes);

				var charge = Factory.NewWithValidTestData<Charge>();
				charge.JR_AC = entryFeeChargeCode.PK;
				charge.JR_AT_SellGSTRate = gstRatePK;
				charge.JR_RX_NKSellCurrency = "NZD";

				AssertEquals(false, NZCustomsEntryFeeTaxCalculator.IsEntryFeeRevenueChargeWithCorrectAmount(charge));

				charge.JR_OSSellAmt = EntryFeeAmount;

				AssertEquals(CorrectEntryFeeGST, charge.JR_OSSellGSTAmt_Calc);
				AssertEquals(true, NZCustomsEntryFeeTaxCalculator.IsEntryFeeRevenueChargeWithCorrectAmount(charge));
			}
		}

		#endregion

		#region TestUpdateEntryFeeGSTForCostCharge

		[TestDate(2018, 01, 03)]
		public void TestUpdateEntryFeeGSTForCostCharge_For2015Fee()
		{
			AssertUpdateEntryFeeGSTForCostCharge();
		}

		public void TestUpdateEntryFeeGSTForCostCharge()
		{
			AssertUpdateEntryFeeGSTForCostCharge();
		}

		void AssertUpdateEntryFeeGSTForCostCharge()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				var gSTRate = AccTaxRate.CreateTaxRate_ForTestOnly(Factory);
				gSTRate.SetRateNumerator_ForTestOnly(15);

				var entryFeeChargeCode = Factory.NewWithValidTestData<AccChargeCode>();
				entryFeeChargeCode.AC_Code = "ENTRYFEE";
				entryFeeChargeCode.AC_ChargeType = Enterprise.Core.Constants.ChargeType.Disbursement;
				entryFeeChargeCode.AC_ChargeGroup = ChargeCodeGroupList.Codes.CustomsDuty;
				entryFeeChargeCode.AC_AT_GSTRate = gSTRate.PK;

				var chargeTypesAndCodes = RatingDataRegistry.Instance.EntryChargeTypesAndCodes.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
				var typeAndCode1 = chargeTypesAndCodes.AddNew();
				typeAndCode1.ChargeType = NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee;
				typeAndCode1.AC_ChargeCode = entryFeeChargeCode.PK;

				RatingDataRegistry.Instance.EntryChargeTypesAndCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, chargeTypesAndCodes);

				var charge = Factory.NewWithValidTestData<Charge>();
				charge.JR_AC = entryFeeChargeCode.PK;
				charge.JR_AT_CostGSTRate = gSTRate.PK;
				charge.JR_RX_NKCostCurrency = "NZD";
				charge.JR_OSCostAmt = EntryFeeAmount;

				AssertEquals(CorrectEntryFeeGST, charge.JR_OSCostGSTAmt_Calc);
			}
		}

		#endregion

		[TestDate(2018, 01, 03)]
		public void TestHardCodedNumbersIntegrity_For2015Fee()
		{
			AssertEquals(EntryFeeAmount, NZCustomsEntryFeeTaxCalculator.EntryFeeAmount_ForTestOnly);
			AssertEquals(TotalEntryFeeGST, NZCustomsEntryFeeTaxCalculator.IncorrectEntryFeeGST_ForTestOnly);
			AssertEquals(CorrectEntryFeeGST, NZCustomsEntryFeeTaxCalculator.CorrectEntryFeeGST_ForTestOnly);
		}

		#region TestHardCodedNumbersIntegrity

		[TestDate(2018, 01, 03)]
		public void TestGetEntryFeeGST_For2015Fee()
		{
			AssertGetEntryFeeGST();
		}

		public void TestGetEntryFeeGST()
		{
			AssertGetEntryFeeGST();
		}

		void AssertGetEntryFeeGST()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("NZ"))
			{
				AssertEquals(CorrectEntryFeeGST, NZCustomsEntryFeeTaxCalculator.GetEntryFeeGST(EntryFeeAmount, TotalEntryFeeGST));
			}
		}

		public void TestValidNZChargeTypesSameAsNZ()
		{
			var nzList = EntryChargeTypeList.GetList(Core.Constants.CountryCodes.NewZealand);

			var desc = nzList.GetDescriptionFromCode(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Duty);
			AssertNotNull(desc);
			AssertEquals("Duty", desc);

			desc = nzList.GetDescriptionFromCode(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.EntryFee);
			AssertNotNull(desc);
			AssertEquals("Entry Fee", desc);

			desc = nzList.GetDescriptionFromCode(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.GST);
			AssertNotNull(desc);
			AssertEquals("GST", desc);

			desc = nzList.GetDescriptionFromCode(NZCustomsEntryFeeTaxCalculator.ValidNZChargeTypes.Default);
			AssertNull(desc);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			var now = ZDateTime.Now;

			var binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var assembly = Assembly.LoadFile(Path.Combine(binPath, "Enterprise.Customs.NZ.Business.dll"));
			var feeChargeCalculatorType = assembly.GetType("Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.EntryFeeCalculator+FeeChargeCalculator");
			var feeChargeCalculator = Activator.CreateInstance(feeChargeCalculatorType, now, Factory);

			var testAssembly = Assembly.LoadFile(Path.Combine(binPath, "Enterprise.Customs.NZ.Business.Test.dll"));
			var taxOrFeeTestHelperType = testAssembly.GetType("Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.Testing.TaxOrFeeTestHelper");
			var setUpMethod = taxOrFeeTestHelperType.GetMethod("SetUp", BindingFlags.Public | BindingFlags.Static);
			setUpMethod?.Invoke(null, parameters: new object[] { Factory });

			var importEntryTransactionFee = (decimal)feeChargeCalculator.GetType().GetProperty("ImportEntryTransactionFee").GetValue(feeChargeCalculator, null);
			var biosecuritySystemEntryLevy = (decimal)feeChargeCalculator.GetType().GetProperty("BiosecuritySystemEntryLevy", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(feeChargeCalculator, null);

			var gstCalculatorType = assembly.GetType("Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators.GSTCalculator");
			var getGSTMethod = gstCalculatorType.GetMethod("GetGST", BindingFlags.Static | BindingFlags.NonPublic);

			EntryFeeAmount = importEntryTransactionFee + biosecuritySystemEntryLevy;
			TotalEntryFeeGST = (decimal)getGSTMethod.Invoke(obj: null, parameters: new object[] { Factory, EntryFeeAmount, now });
			CorrectEntryFeeGST = (decimal)getGSTMethod.Invoke(obj: null, parameters: new object[] { Factory, new ZDecimal(importEntryTransactionFee), now })
				+ (decimal)getGSTMethod.Invoke(obj: null, parameters: new object[] { Factory, new ZDecimal(biosecuritySystemEntryLevy), now });
			EntryFeeAmount = EntryFeeAmount.Round(2);
		}

		ZDecimal EntryFeeAmount;
		ZDecimal TotalEntryFeeGST;
		ZDecimal CorrectEntryFeeGST;
	}
}
